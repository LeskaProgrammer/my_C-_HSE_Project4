
using System.Text.Json;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;

public class LibraryManager : IEnumerable<Book>
{
    private List<Book> books = new List<Book>();
    private string _filePath;
    
    public string FilePath { get; private set; }
    
    public int Count => books.Count;
    public delegate void BooksDisplayHandler(List<Book> books, string text);
    
    public LibraryManager(string path)
    {
        _filePath = path;
        LoadBooks();
    }
    
    public void DisplayBooks(BooksDisplayHandler displayMethod, string text)
    {
        displayMethod?.Invoke(books, text);
    }
    
    public void DisplayPartBooks(List<Book> booksToDisplay, BooksDisplayHandler displayMethod, string text)
    {
        displayMethod?.Invoke(booksToDisplay, text);
    }

    // Индексатор для доступа к книгам по индексу
    public Book this[int index]
    {
        get => books[index];
        set => books[index] = value;
    }

 private void LoadBooks()
{
    if (!File.Exists(_filePath))
    {
        AnsiConsole.MarkupLine("[yellow]Файл не найден, создан новый список книг.[/]");
        books = new List<Book>();
        return;
    }

    List<Book> rezerverBooks = new List<Book>();
    rezerverBooks.AddRange(books);
    
    
    try
    {
        string json = File.ReadAllText(_filePath, Encoding.UTF8); // Используем UTF-8 для поддержки кириллицы
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true
        };
        books = JsonSerializer.Deserialize<List<Book>>(json, options) ?? new List<Book>();

        // Нормализуем значения, но не сбрасываем рейтинг, если он указан
        foreach (var book in books)
        {
            if (book.Title == null || book.Title == "") book.Title = "__NOT_STATED__";
            if (book.Author == null || book.Author == "") book.Author = "__NOT_STATED__";
            if (book.Genre == null || book.Genre == "") book.Genre = "__NOT_STATED__";
            if (book.ISBN == null || book.ISBN == "") book.ISBN = "__NOT_STATED__";
            if (book.Year == 0) book.Year = -1; // 0 заменяем на -1 для отсутствия года
            // Не меняем рейтинг, если он уже есть (оставляем существующее значение, даже если 0)
            if (book.Rating == 0) continue; // Не трогаем 0, если он есть
            if (book.Rating == -1 && JsonSerializer.Serialize(book.Rating) == "0") book.Rating = 0; // Корректируем, если JSON интерпретировал 0 как -1
            if (book.CoverUrl == null) book.CoverUrl = ""; // Убедимся, что CoverUrl не null
        }
        // AnsiConsole.MarkupLine("[green]Загружено {0} книг из файла {1}[/]", books.Count, _filePath);
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Ошибка чтения JSON: {ex.Message}.");
        books = new List<Book>();
        books.AddRange(rezerverBooks);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}.");
        books = new List<Book>();
        books.AddRange(rezerverBooks);
        
    }
}
 
 
 
public void SaveBooks()
{
    try
    {
        string json = JsonSerializer.Serialize(books, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json, Encoding.UTF8); // Сохраняем в UTF-8 для поддержки кириллицы
        AnsiConsole.MarkupLine("[green]Библиотека сохранена в {0}.[/]", _filePath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка сохранения: {ex.Message}.");
    }
}
    // Реализация IEnumerable<Book> для итерации по книгам
    public IEnumerator<Book> GetEnumerator()
    {
        return books.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // Примеры методов для B-side
    public void AddBook(Book book) => books.Add(book);
    public void RemoveBook(Book book) => books.Remove(book);

    // Новый метод для добавления книги по уникальному коду (ISBN) с таймаутом 10 секунд
public async Task AddBookFromOpenLibrary(string isbn)
{
    List<string> text = new();
    if (string.IsNullOrEmpty(isbn) || isbn == "__NOT_STATED__")
    {
        AnsiConsole.MarkupLine("[red]Неверный уникальный код книги. Нажмите любую клавишу...[/]");
        Console.ReadKey(true);
        return;
    }

    // Загружаем текущие книги перед добавлением, чтобы не потерять данные
    LoadBooks();
    
    

    BookData bookData = null;
    bool dataLoaded = false;
    bool coverLoaded = false;
    string coverPath = "";
    CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // Таймаут 10 секунд

    // Запускаем задачу получения данных о книге
    Task<BookData> dataTask = GetBookDataFromOpenLibraryAsync(isbn);
    Task coverTask = null;

    try
    {
        // Ждём данные о книге с таймаутом
        Task dataWaitTask = dataTask.ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully)
            {
                bookData = t.Result;
                dataLoaded = true;
            }
        }, cts.Token);

        dataWaitTask.Wait(cts.Token);

        if (!dataLoaded || bookData == null)
        {
            AnsiConsole.MarkupLine("[yellow]Данные о книге с ISBN {0} не загрузились за 10 секунд. Скачивание прекращено. Нажмите любую клавишу...[/]", isbn);
            Console.ReadKey(true);
            return;
        }

        // Проверяем, удалось ли получить реальные данные (не дефолтные)
        if ((bookData.Title == null || bookData.Title == "__NOT_STATED__") && 
            (bookData.Author == null || bookData.Author == "__NOT_STATED__") && 
            (bookData.Genre == null || bookData.Genre == "__NOT_STATED__") && 
            bookData.Year == -1 && string.IsNullOrEmpty(bookData.CoverUrl))
        {
            AnsiConsole.MarkupLine("[red]Книга с ISBN {0} не найдена в OpenLibrary. Нажмите любую клавишу...[/]", isbn);
            Console.ReadKey(true);
            return;
        }

        // Создаём новую книгу с данными из OpenLibrary
        Book newBook = new Book(
            bookData.Title ?? "__NOT_STATED__",
            bookData.Author ?? "__NOT_STATED__",
            bookData.Genre ?? "__NOT_STATED__",
            bookData.Year == -1 ? -1 : bookData.Year,
            bookData.ISBN,
            -1, // Устанавливаем -1 как значение по умолчанию для рейтинга
            bookData.CoverUrl ?? "" // Если null, используем пустую строку
        );

        // Проверяем, есть ли книга с таким ISBN в библиотеке
       var existingBook = books.FirstOrDefault(b => b.ISBN.Replace("-", "") == newBook.ISBN.Replace("-", "") && b.ISBN != "__NOT_STATED__");
        if (existingBook != null)
        {
            // Обновляем существующую книгу, только если данные из OpenLibrary существуют и отличаются
            bool changesMade = false;

            
            
            
            
            // Сохраняем существующий рейтинг, если он не -1, и не запрашиваем заново
            if (existingBook.Rating != -1)
            {
                newBook.Rating = existingBook.Rating; // Сохраняем существующий рейтинг
            }
            else if (newBook.Rating == -1)
            {
                // Предлагаем ввести рейтинг только если его нет в библиотеке
                AnsiConsole.MarkupLine("[bold blue]Введите рейтинг книги (0-10, пустая строка для пропуска):[/]");
                string ratingInput = Console.ReadLine();
                if (!string.IsNullOrEmpty(ratingInput))
                {
                    if (int.TryParse(ratingInput, out int userRating) && userRating >= 0 && userRating <= 10)
                    {
                        newBook.Rating = userRating; // Устанавливаем введённый рейтинг
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Неверный формат рейтинга, оставлен без изменений (-1).[/]");
                        newBook.Rating = -1; // Оставляем -1, если ввод некорректен
                    }
                }
                else
                {
                    newBook.Rating = -1; // Пустая строка означает отсутствие рейтинга
                }
            }
            
            
            
            
            
            

            // Проверяем и обновляем каждое поле, показывая конкретные старые и новые значения
            if (bookData.Title != null && bookData.Title != "__NOT_STATED__" && existingBook.Title != bookData.Title)
            {
                text.Add($"Название обновлено с '{existingBook.Title}' на '{bookData.Title}' из OpenLibrary.");
             
                existingBook.Title = bookData.Title;
                changesMade = true;
            }

            if (bookData.Author != null && bookData.Author != "__NOT_STATED__" && existingBook.Author != bookData.Author)
            {
                text.Add($"Автор обновлен с '{existingBook.Author}' на '{bookData.Author}' из OpenLibrary.");
          
                existingBook.Author = bookData.Author;
                changesMade = true;
            }

            if (bookData.Genre != null && bookData.Genre != "__NOT_STATED__" && existingBook.Genre != bookData.Genre)
            {
                text.Add($"Жанр обновлен с '{existingBook.Genre}' на '{bookData.Genre}' из OpenLibrary.");
                
                
                existingBook.Genre = bookData.Genre;
                changesMade = true;
            }

            if (bookData.Year != -1 && existingBook.Year != bookData.Year)
            {
                text.Add($"год обновлен с '{existingBook.Year}' на '{bookData.Year}' из OpenLibrary.");
         
                
                existingBook.Year = bookData.Year;
                changesMade = true;
            }

            if (bookData.ISBN != null && bookData.ISBN != "__NOT_STATED__" && existingBook.ISBN != bookData.ISBN)
            {
                text.Add($"ISBN обновлен с '{existingBook.ISBN}' на '{bookData.ISBN}' из OpenLibrary.");

                
                existingBook.ISBN = bookData.ISBN;
                changesMade = true;
            }

            // Обновляем рейтинг, если он был изменён, показывая конкретные значения
            if (newBook.Rating != -1 && existingBook.Rating != newBook.Rating)
            {
                AnsiConsole.MarkupLine("[green]Оценка обновлена с '{0}' на '{1}' пользователем при добавлении.[/]", existingBook.Rating, newBook.Rating);
                existingBook.Rating = newBook.Rating;
                changesMade = true;
            }

            // Обновляем обложку, если есть, показывая конкретные значения
            if (bookData.CoverUrl != null && !string.IsNullOrEmpty(bookData.CoverUrl))
            {
                string coversDir = Path.Combine(Path.GetDirectoryName(FilePath) ?? "", "covers");
                Directory.CreateDirectory(coversDir); // Создаём папку, если её нет
                coverPath = Path.Combine(coversDir, $"{isbn.Replace("-", "").Replace(" ", "")}.jpg");

                // Добавляем индикатор скачивания обложки
                await AnsiConsole.Progress()
                    .StartAsync(async ctx =>
                    {
                        var task = ctx.AddTask("[cyan] Парсинг данных...[/]", true, 100);
                        double progressValue = 0;

                        coverTask = Task.Run(async () =>
                        {
                            using (var client = new HttpClient())
                            {
                                var coverResponse = await client.GetAsync(bookData.CoverUrl, cts.Token);
                                coverResponse.EnsureSuccessStatusCode();
                                byte[] coverBytes = await coverResponse.Content.ReadAsByteArrayAsync();
                                await File.WriteAllBytesAsync(coverPath, coverBytes, cts.Token);
                                coverLoaded = true;
                            }
                        });

                        try
                        {
                            while (!coverTask.IsCompleted)
                            {
                                progressValue += 10; // Симулируем прогресс (можно улучшить для реального прогресса)
                                task.Increment(10);
                                await Task.Delay(200, cts.Token); // Задержка для имитации загрузки
                            }
                            task.Value = 100; // Завершаем на 100%
                        }
                        catch (OperationCanceledException)
                        {
                            if (!coverLoaded)
                            {
                                task.Value = 0; // Останавливаем прогресс
                                AnsiConsole.MarkupLine("[yellow]Обложка для ISBN {0} не загрузилась за 10 секунд. Скачивание прекращено.[/]", isbn);
                            }
                        }
                        catch (Exception ex)
                        {
                            task.Value = 0; // Останавливаем прогресс
                            Console.WriteLine($"Ошибка скачивания обложки для ISBN {isbn}: {ex.Message}");
                        }
                    });

                if (coverLoaded)
                {
                    if (existingBook.CoverUrl != coverPath && !string.IsNullOrEmpty(coverPath))
                    {
                        AnsiConsole.MarkupLine("[green]Путь к обложке обновлён с '{0}' на '{1}' из OpenLibrary.[/]", existingBook.CoverUrl, coverPath);
                        existingBook.CoverUrl = coverPath;
                        changesMade = true;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(existingBook.CoverUrl))
            {
                // Сохраняем существующий путь к обложке, если он есть
            }

            if (changesMade)
            {
                SaveBooks(); // Сохраняем изменения в файл
                
                    
                foreach (string i in text)
                {
                    AnsiConsole.MarkupLine("[green]{0}.[/]", i);
                }

                
                AnsiConsole.MarkupLine("[green]Книга с ISBN {0} обновлена данными из OpenLibrary.[/]", isbn);
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Книга с ISBN {0} уже актуальна, изменения не требуются.[/]", isbn);
            }
        }
        else
        {
            // Добавляем новую книгу
            books.Add(newBook);
            SaveBooks(); // Сохраняем изменения в файл

            // Предлагаем ввести рейтинг только если его нет в новой книге
            if (newBook.Rating == -1)
            {
                AnsiConsole.MarkupLine("[bold blue]Введите рейтинг книги (1-10, пустая строка для пропуска, 0 для оценки 0):[/]");
                string ratingInput = Console.ReadLine();
                if (!string.IsNullOrEmpty(ratingInput))
                {
                    if (int.TryParse(ratingInput, out int userRating) && userRating >= 0 && userRating <= 10)
                    {
                        newBook.Rating = userRating; // Устанавливаем введённый рейтинг
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Неверный формат рейтинга, оставлен без изменений (-1).[/]");
                        newBook.Rating = -1; // Оставляем -1, если ввод некорректен
                    }
                }
                else
                {
                    newBook.Rating = -1; // Пустая строка означает отсутствие рейтинга
                }

                // Обновляем книгу в списке с новым рейтингом, если он был введён
                if (newBook.Rating != -1)
                {
                    SaveBooks(); // Сохраняем обновлённую книгу
                    AnsiConsole.MarkupLine("[green]Оценка для книги с ISBN {0} установлена на {1}.[/]", isbn, newBook.Rating);
                }
            }

            // Скачиваем обложку, если есть URL
            if (bookData.CoverUrl != null && !string.IsNullOrEmpty(bookData.CoverUrl))
            {
                string coversDir = Path.Combine(Path.GetDirectoryName(FilePath) ?? "", "covers");
                Directory.CreateDirectory(coversDir); // Создаём папку, если её нет
                coverPath = Path.Combine(coversDir, $"{isbn.Replace("-", "").Replace(" ", "")}.jpg");

                // Добавляем индикатор скачивания обложки
                await AnsiConsole.Progress()
                    .StartAsync(async ctx =>
                    {
                        var task = ctx.AddTask("[cyan]Скачивание обложки...[/]", true, 100);
                        double progressValue = 0;

                        coverTask = Task.Run(async () =>
                        {
                            using (var client = new HttpClient())
                            {
                                var coverResponse = await client.GetAsync(bookData.CoverUrl, cts.Token);
                                coverResponse.EnsureSuccessStatusCode();
                                byte[] coverBytes = await coverResponse.Content.ReadAsByteArrayAsync();
                                await File.WriteAllBytesAsync(coverPath, coverBytes, cts.Token);
                                coverLoaded = true;
                            }
                        });

                        try
                        {
                            while (!coverTask.IsCompleted)
                            {
                                progressValue += 10; // Симулируем прогресс (можно улучшить для реального прогресса)
                                task.Increment(10);
                                await Task.Delay(200, cts.Token); // Задержка для имитации загрузки
                            }
                            task.Value = 100; // Завершаем на 100%
                        }
                        catch (OperationCanceledException)
                        {
                            if (!coverLoaded)
                            {
                                task.Value = 0; // Останавливаем прогресс
                                AnsiConsole.MarkupLine("[yellow]Обложка для ISBN {0} не загрузилась за 10 секунд. Скачивание прекращено.[/]", isbn);
                            }
                        }
                        catch (Exception ex)
                        {
                            task.Value = 0; // Останавливаем прогресс
                            Console.WriteLine($"Ошибка скачивания обложки для ISBN {isbn}: {ex.Message}");
                        }
                    });

                if (coverLoaded)
                {
                    newBook.CoverUrl = coverPath;
                    SaveBooks(); // Сохраняем обновлённую книгу с локальным путём
                    AnsiConsole.MarkupLine("[green]Обложка для ISBN {0} сохранена в: {1}[/]", isbn, Path.GetFullPath(coverPath));
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Книга с ISBN {0} добавлена, но обложка не загрузилась за 10 секунд.[/]", isbn);
                }
            }
            else if (!string.IsNullOrEmpty(newBook.CoverUrl))
            {
                // Сохраняем существующий путь к обложке, если он есть (хотя для новой книги он пустой)
            }

        
            
            AnsiConsole.MarkupLine("[green]Книга с ISBN {0} добавлена из OpenLibrary.[/]", isbn);
        }
    }
    catch (OperationCanceledException)
    {
        AnsiConsole.MarkupLine("[yellow]Данные о книге с ISBN {0} не загрузились за 10 секунд. Скачивание прекращено. Нажмите любую клавишу...[/]", isbn);
        Console.ReadKey(true);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Общая ошибка при добавлении книги с ISBN {isbn}: {ex.Message} Нажмите любую клавишу...");
        Console.ReadKey(true);
    }
    finally
    {
        cts.Dispose();
    }
    
    
    
}















    // Маппинг жанров для нормализации
    private static readonly Dictionary<string, string> GenreMapping = new()
    {
        { "science fiction", "Science Fiction" },
        { "fiction", "Fiction" },
        { "fantasy", "Fantasy" },
        { "american literature", "Literature" },
        { "science-fiction", "Science Fiction" }, // Добавляем синоним
        { "dune (imaginary place)", "Science Fiction" }
    };

    // Нормализация жанров
    private static string NormalizeGenre(string genre)
    {
        if (string.IsNullOrEmpty(genre) || genre == "__NOT_STATED__") return "__NOT_STATED__";
        
        // Разбиваем строку на отдельные жанры, нормализуем, фильтруем и мапим
        var normalizedGenres = genre.ToLowerInvariant()
            .Split(',')
            .Select(g => g.Trim())
            .Where(g => !string.IsNullOrEmpty(g) && !g.Contains("nyt:") && !g.Contains("award:") && !g.Contains("(imaginary place)"))
            .Select(g => GenreMapping.ContainsKey(g) ? GenreMapping[g] : g) // Маппим или оставляем как есть
            .Distinct() // Убираем дубликаты после маппинга
            .Take(3) // Ограничиваем до 3 уникальных жанров
            .ToList();

        return normalizedGenres.Any() ? string.Join(", ", normalizedGenres) : "__NOT_STATED__";
    }

    // Асинхронный метод для получения данных о книге из OpenLibrary
private static async Task<BookData> GetBookDataFromOpenLibraryAsync(string isbn)
{
    using (var client = new HttpClient())
    {
        isbn = isbn.Replace("-", "").Replace(" ", ""); // Убираем дефисы и пробелы
        string isbnUrl = $"https://openlibrary.org/isbn/{isbn}.json";

        try
        {
            var response = await client.GetAsync(isbnUrl);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            var bookData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (bookData == null) return new BookData { ISBN = isbn, Rating = -1 };

            var result = new BookData { ISBN = isbn, Rating = -1 }; // Устанавливаем Rating по умолчанию как -1

            // Извлекаем данные, возвращая null для отсутствующих полей
            result.Title = bookData.ContainsKey("title") && !string.IsNullOrEmpty(bookData["title"]?.ToString())
                ? bookData["title"].ToString()
                : null;
            result.Year = bookData.ContainsKey("publish_date") && !string.IsNullOrEmpty(bookData["publish_date"]?.ToString())
                ? int.TryParse(Regex.Match(bookData["publish_date"].ToString(), @"\d{4}").Value, out int year) ? year : -1
                : -1;

            if (bookData.ContainsKey("authors") && bookData["authors"] is JsonElement authors)
            {
                var author = authors.EnumerateArray().FirstOrDefault();
                if (author.ValueKind == JsonValueKind.Object && author.TryGetProperty("key", out JsonElement key))
                {
                    string authorUrl = $"https://openlibrary.org{key.GetString()}.json";
                    var authorResponse = await client.GetAsync(authorUrl);
                    authorResponse.EnsureSuccessStatusCode();

                    string authorJson = await authorResponse.Content.ReadAsStringAsync();
                    var authorData = JsonSerializer.Deserialize<Dictionary<string, object>>(authorJson);
                    if (authorData != null && authorData.ContainsKey("name") && !string.IsNullOrEmpty(authorData["name"]?.ToString()))
                    {
                        result.Author = authorData["name"].ToString();
                    }
                    else
                    {
                        result.Author = null; // Возвращаем null, если автора нет
                    }
                }
                else
                {
                    result.Author = null; // Возвращаем null, если автор не найден
                }
            }
            else
            {
                result.Author = null; // Возвращаем null, если авторы отсутствуют
            }

            if (bookData.ContainsKey("works") && bookData["works"] is JsonElement works)
            {
                var work = works.EnumerateArray().FirstOrDefault();
                if (work.ValueKind == JsonValueKind.Object && work.TryGetProperty("key", out JsonElement workKey))
                {
                    string workUrl = $"https://openlibrary.org{workKey.GetString()}.json";
                    var workResponse = await client.GetAsync(workUrl);
                    workResponse.EnsureSuccessStatusCode();

                    string workJson = await workResponse.Content.ReadAsStringAsync();
                    var workData = JsonSerializer.Deserialize<Dictionary<string, object>>(workJson);

                    if (workData != null)
                    {
                        string rawGenre = workData.ContainsKey("subjects") && workData["subjects"] is JsonElement subjects
                            ? string.Join(", ", subjects.EnumerateArray().Select(s => s.ToString()))
                            : null;
                        result.Genre = rawGenre != null ? NormalizeGenre(rawGenre) : null;

                        if (workData.ContainsKey("covers") && workData["covers"] is JsonElement covers)
                        {
                            var coverId = covers.EnumerateArray().FirstOrDefault();
                            if (coverId.ValueKind != JsonValueKind.Undefined && coverId.ValueKind != JsonValueKind.Null)
                            {
                                result.CoverUrl = $"https://covers.openlibrary.org/b/id/{coverId.GetInt32()}-M.jpg";
                            }
                            else
                            {
                                result.CoverUrl = null;
                            }
                        }
                        else
                        {
                            result.CoverUrl = null;
                        }
                    }
                    else
                    {
                        result.Genre = null;
                        result.CoverUrl = null;
                    }
                }
                else
                {
                    result.Genre = null;
                    result.CoverUrl = null;
                }
            }
            else
            {
                result.Genre = null;
                result.CoverUrl = null;
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка запроса к OpenLibrary: {ex.Message}");
            return new BookData { ISBN = isbn, Rating = -1 };
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка парсинга JSON: {ex.Message}");
            return new BookData { ISBN = isbn, Rating = -1 };
        }
    }
}
    
    
    
}