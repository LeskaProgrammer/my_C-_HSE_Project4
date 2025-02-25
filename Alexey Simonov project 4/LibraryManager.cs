using System;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


public class LibraryManager : IEnumerable<Book>
{
    private List<Book> books = new List<Book>();
    private string _filePath;
    
    public string FilePath { get; private set; }
    public delegate void BooksDisplayHandler(List<Book> books);
    
    public LibraryManager(string path)
    {
        _filePath = path;
        LoadBooks();
    }
    
    public void DisplayBooks(BooksDisplayHandler displayMethod)
    {
        displayMethod?.Invoke(books);
    }
    
    public void DisplayPartBooks(List<Book> booksToDisplay, BooksDisplayHandler displayMethod)
    {
        displayMethod?.Invoke(booksToDisplay);
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
            Console.WriteLine("Файл не найден, создан новый.");
            return;
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true
            };
            books = JsonSerializer.Deserialize<List<Book>>(json, options) ?? new List<Book>();

            // Проверь, что все книги имеют корректные значения
            foreach (var book in books)
            {
                if (book.Title == null || book.Title == "") book.Title = "__NOT_STATED__";
                if (book.Author == null || book.Author == "") book.Author = "__NOT_STATED__";
                if (book.Genre == null || book.Genre == "") book.Genre = "__NOT_STATED__";
                if (book.ISBN == null || book.ISBN == "") book.ISBN = "__NOT_STATED__";
                if (book.Year == 0) book.Year = -1; // 0 — стандартное значение для int
                if (book.Rating == 0) book.Rating = -1; // 0 — стандартное значение для int
                if (book.CoverUrl == null) book.CoverUrl = ""; // Убедимся, что CoverUrl не null
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка чтения JSON: {ex.Message}. Создан пустой список.");
            books = new List<Book>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}. Создан пустой список.");
            books = new List<Book>();
        }
    }

    public void SaveBooks()
    {
        try
        {
            string json = JsonSerializer.Serialize(books, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения: {ex.Message}");
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
    public void AddBookByUniqueCode(string uniqueCode)
    {
        if (string.IsNullOrEmpty(uniqueCode) || uniqueCode == "__NOT_STATED__")
        {
            Console.WriteLine("Неверный уникальный код книги. Нажмите любую клавишу...");
            Console.ReadKey(true);
            return;
        }

        BookData bookData = null;
        bool dataLoaded = false;
        bool coverLoaded = false;
        string coverPath = "";
        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // Таймаут 10 секунд

        // Запускаем задачу получения данных о книге
        Task<BookData> dataTask = GetBookDataFromOpenLibraryAsync(uniqueCode);
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
                Console.WriteLine($"Данные о книге с ISBN {uniqueCode} не загрузились за 10 секунд. Скачивание прекращено. Нажмите любую клавишу...");
                Console.ReadKey(true);
                return;
            }

            // Проверяем, удалось ли получить реальные данные (не дефолтные)
            if (bookData.Title == "__NOT_STATED__" && bookData.Author == "__NOT_STATED__" && 
                bookData.Genre == "__NOT_STATED__" && bookData.Year == -1 && string.IsNullOrEmpty(bookData.CoverUrl))
            {
                Console.WriteLine($"Книга с ISBN {uniqueCode} не найдена в OpenLibrary. Нажмите любую клавишу...");
                Console.ReadKey(true);
                return;
            }

            // Создаём новую книгу с данными из OpenLibrary
            Book newBook = new Book(
                bookData.Title,
                bookData.Author,
                bookData.Genre,
                bookData.Year,
                bookData.ISBN,
                bookData.Rating, // Установлено случайное значение для теста
                bookData.CoverUrl
            );

            // Проверяем, нет ли дубликата по ISBN
            if (!books.Any(b => b.ISBN == newBook.ISBN && b.ISBN != "__NOT_STATED__"))
            {
                books.Add(newBook);
                SaveBooks(); // Сохраняем изменения в файл

                // Если есть URL обложки, запускаем скачивание
                if (!string.IsNullOrEmpty(bookData.CoverUrl))
                {
                    string coversDir = Path.Combine(Path.GetDirectoryName(_filePath) ?? "", "covers");
                    Directory.CreateDirectory(coversDir); // Создаём папку, если её нет
                    coverPath = Path.Combine(coversDir, $"{uniqueCode.Replace("-", "").Replace(" ", "")}.jpg");

                    coverTask = Task.Run(async () =>
                    {
                        using (var client = new HttpClient())
                        {
                            var coverResponse = await client.GetAsync(bookData.CoverUrl, cts.Token);
                            coverResponse.EnsureSuccessStatusCode();
                            await File.WriteAllBytesAsync(coverPath, await coverResponse.Content.ReadAsByteArrayAsync(), cts.Token);
                            coverLoaded = true;
                        }
                    });

                    try
                    {
                        coverTask.Wait(cts.Token); // Ждём скачивание обложки с тем же таймаутом 10 секунд
                    }
                    catch (OperationCanceledException)
                    {
                        if (!coverLoaded)
                        {
                            Console.WriteLine($"Обложка для ISBN {uniqueCode} не загрузилась за 10 секунд. Скачивание прекращено. Нажмите любую клавишу...");
                            Console.ReadKey(true);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка скачивания обложки для ISBN {uniqueCode}: {ex.Message}");
                    }

                    if (coverLoaded)
                    {
                        Console.WriteLine($"Обложка для ISBN {uniqueCode} сохранена в: {Path.GetFullPath(coverPath)}");
                    }
                    else
                    {
                        Console.WriteLine($"Книга с ISBN {uniqueCode} добавлена, но обложка не загрузилась за 10 секунд. Нажмите любую клавишу...");
                        Console.ReadKey(true);
                        return;
                    }
                }
                else
                {
                    Console.WriteLine($"Книга с ISBN {uniqueCode} добавлена, но обложка отсутствует. Нажмите любую клавишу...");
                    Console.ReadKey(true);
                    return;
                }

                Console.WriteLine($"Книга с ISBN {uniqueCode} добавлена. Нажмите любую клавишу...");
                Console.ReadKey(true);
            }
            else
            {
                Console.WriteLine($"Книга с ISBN {uniqueCode} уже существует. Нажмите любую клавишу...");
                Console.ReadKey(true);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Данные о книге с ISBN {uniqueCode} не загрузились за 10 секунд. Скачивание прекращено. Нажмите любую клавишу...");
            Console.ReadKey(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Общая ошибка при добавлении книги с ISBN {uniqueCode}: {ex.Message}. Нажмите любую клавишу...");
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
                // Запрос по ISBN
                var response = await client.GetAsync(isbnUrl);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var bookData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                if (bookData == null) return new BookData { ISBN = isbn };

                var result = new BookData { ISBN = isbn };

                // Извлекаем основные данные из ISBN JSON
                result.Title = bookData.ContainsKey("title") ? bookData["title"].ToString() ?? "__NOT_STATED__" : "__NOT_STATED__";
                result.Year = bookData.ContainsKey("publish_date")
                    ? int.TryParse(Regex.Match(bookData["publish_date"].ToString(), @"\d{4}").Value, out int year) ? year : -1
                    : -1;

                // Получаем автора
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
                        if (authorData != null && authorData.ContainsKey("name"))
                        {
                            result.Author = authorData["name"].ToString() ?? "__NOT_STATED__";
                        }
                    }
                }

                // Получаем жанры и обложку через works
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
                            string rawGenre = workData.ContainsKey("subjects")
                                ? string.Join(", ", ((JsonElement)workData["subjects"]).EnumerateArray().Select(s => s.ToString()))
                                : "__NOT_STATED__";
                            result.Genre = NormalizeGenre(rawGenre);

                            // Обработка обложки
                            if (workData.ContainsKey("covers") && workData["covers"] is JsonElement covers)
                            {
                                var coverId = covers.EnumerateArray().FirstOrDefault();
                                if (coverId.ValueKind != JsonValueKind.Undefined && coverId.ValueKind != JsonValueKind.Null)
                                {
                                    result.CoverUrl = $"https://covers.openlibrary.org/b/id/{coverId.GetInt32()}-M.jpg";
                                }
                                else
                                {
                                    result.CoverUrl = "";
                                }
                            }
                            else
                            {
                                result.CoverUrl = "";
                            }
                        }
                    }
                }

                // Временное решение для рейтинга (OpenLibrary не предоставляет рейтинги)
                Random random = new Random();
                result.Rating = random.Next(1, 11); // Случайный рейтинг от 1 до 10 для тестов

                return result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка запроса к OpenLibrary: {ex.Message}");
                return new BookData { ISBN = isbn, Rating = -1 }; // Устанавливаем -1 для рейтинга в случае ошибки
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Ошибка парсинга JSON: {ex.Message}");
                return new BookData { ISBN = isbn, Rating = -1 }; // Устанавливаем -1 для рейтинга в случае ошибки
            }
        }
    }
}