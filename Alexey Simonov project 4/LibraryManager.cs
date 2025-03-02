using Spectre.Console;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Alexey_Simonov_project_4
{
    public class LibraryManager : IEnumerable<Book>
    {
        private List<Book> _books = new List<Book>();
        private string _filePath;
    
        public string FilePath => _filePath;
    
        public int Count => _books.Count;
        public delegate void BooksDisplayHandler(List<Book> books, string text);
    
        /// <summary>
        /// Инициализирует новый экземпляр класса LibraryManager.
        /// </summary>
        /// <param name="path">Путь к файлу библиотеки.</param>
        /// <param name="needToLoad">Указывает, нужно ли загружать книги при создании.</param>
        public LibraryManager(string path, bool needToLoad = true)
        {
            // Устанавливаем путь к файлу
            _filePath = path;
            if (needToLoad)
            {
                LoadBooks();
            }
        }
        
        /// <summary>
        /// Отображает список книг с использованием заданного делегата.
        /// </summary>
        /// <param name="displayMethod">Метод отображения книг.</param>
        /// <param name="text">Текст для отображения вместе с книгами.</param>
        public void DisplayBooks(BooksDisplayHandler? displayMethod, string text)
        {
            // Вызываем делегат, если он не null
            displayMethod?.Invoke(_books, text);
        }
    
        /// <summary>
        /// Отображает указанную часть списка книг.
        /// </summary>
        /// <param name="booksToDisplay">Список книг для отображения.</param>
        /// <param name="displayMethod">Метод отображения.</param>
        /// <param name="text">Сопроводительный текст.</param>
        public void DisplayPartBooks(List<Book> booksToDisplay, BooksDisplayHandler displayMethod, string text)
        {
            // Передаем указанный список книг в метод отображения
            displayMethod(booksToDisplay, text);
        }

        // Индексатор для доступа к книгам по индексу
        public Book this[int index]
        {
            get => _books[index];
            set => _books[index] = value;
        }

        private void LoadBooks()
        {
            if (!File.Exists(_filePath))
            {
                AnsiConsole.MarkupLine("[yellow]Файл не найден, создан новый список книг.[/]");
                _books = new List<Book>();
                return;
            }

            List<Book> rezerverBooks = new List<Book>();
            rezerverBooks.AddRange(_books);
    
            // Попытка загрузки данных из файла
            try
            {
                string json = File.ReadAllText(_filePath, Encoding.UTF8); // Используем UTF-8 для поддержки кириллицы
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    PropertyNameCaseInsensitive = true
                };
                _books = JsonSerializer.Deserialize<List<Book>>(json, options) ?? new List<Book>();

                // Нормализация данных после десериализации
                foreach (Book book in _books)
                {
                    if (book.Title == "")
                    {
                        book.Title = "__NOT_STATED__"; // Устанавливаем значение по умолчанию для пустого заголовка
                    }

                    if (book.Author == "")
                    {
                        book.Author = "__NOT_STATED__"; // Устанавливаем значение по умолчанию для пустого автора
                    }

                    if (book.Genre == "")
                    {
                        book.Genre = "__NOT_STATED__"; // Устанавливаем значение по умолчанию для пустого жанра
                    }

                    if (book.Isbn == "")
                    {
                        book.Isbn = "__NOT_STATED__"; // Устанавливаем значение по умолчанию для пустого ISBN
                    }

                    if (book.Year == 0)
                    {
                        book.Year = -1; // 0 заменяем на -1 для отсутствия года
                    }

                    if (book.Rating == 0)
                    {
                        continue; // Сохраняем рейтинг 0, если он был явно указан
                    }

                    if (book.Rating == -1 && JsonSerializer.Serialize(book.Rating) == "0")
                    {
                        book.Rating = 0; // Исправляем ошибку сериализации рейтинга
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Ошибка чтения JSON: {ex.Message}.");
                _books = new List<Book>();
                _books.AddRange(rezerverBooks); // Восстанавливаем резервные данные при ошибке
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}.");
                _books = new List<Book>();
                _books.AddRange(rezerverBooks); // Общая обработка ошибок
            }
        }
 
        /// <summary>
        /// Сохраняет текущий список книг в файл.
        /// </summary>
        public void SaveBooks()
        {
            try
            {
                string json = JsonSerializer.Serialize(_books, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json, Encoding.UTF8); 
                AnsiConsole.MarkupLine("[green]Библиотека сохранена в {0}.[/]", _filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}.");
            }
        }
      
        public IEnumerator<Book> GetEnumerator()
        {
            return _books.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Добавляет новую книгу в библиотеку.
        /// </summary>
        /// <param name="book">Книга для добавления.</param>
        public void AddBook(Book book)
        {
            _books.Add(book); // Добавляем книгу в список
        }

        /// <summary>
        /// Удаляет книгу из библиотеки.
        /// </summary>
        /// <param name="book">Книга для удаления.</param>
        public void RemoveBook(Book book)
        {
            _books.Remove(book); // Удаляем указанную книгу
        }

        // AI_CODE_1
        // Новый метод для добавления книги по уникальному коду (ISBN) с таймаутом 10 секунд
/// Асинхронно добавляет или обновляет книгу по ISBN, используя данные из OpenLibrary.
/// <summary>
/// Асинхронно добавляет или обновляет книгу по ISBN, используя данные из OpenLibrary.
/// </summary>
/// <param name="isbn">Уникальный код книги (ISBN).</param>
/// <returns>Задача, представляющая асинхронную операцию.</returns>
public async Task AddBookFromOpenLibrary(string? isbn)
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

    BookData? bookData = null;
    bool dataLoaded = false;
    bool coverLoaded = false;
    string coverPath;

    // Создаем CancellationTokenSource и извлекаем токен заранее
    using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
    {
        CancellationToken token = cts.Token; // Извлекаем токен для использования в задачах

        // Запускаем задачу получения данных о книге
        Task<BookData?> dataTask = GetBookDataFromOpenLibraryAsync(isbn);
        Task coverTask;

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
            }, token); // Используем извлеченный токен

            dataWaitTask.Wait(token); // Передаем токен в Wait

            if (!dataLoaded || bookData == null)
            {
                AnsiConsole.MarkupLine("[yellow]Данные о книге с ISBN {0} не загрузились за 10 секунд. Скачивание прекращено. Нажмите любую клавишу...[/]", isbn);
                Console.ReadKey(true);
                return;
            }

            // Проверяем, удалось ли получить реальные данные (не дефолтные)
            if (bookData.Title == "__NOT_STATED__" && 
                bookData.Author == "__NOT_STATED__" && 
                bookData.Genre == "__NOT_STATED__" && 
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
                bookData.Isbn,
                -1, // Устанавливаем -1 как значение по умолчанию для рейтинга
                bookData.CoverUrl ?? "" // Если null, используем пустую строку
            );

            // Проверяем, есть ли книга с таким ISBN в библиотеке
            Book? existingBook = _books.FirstOrDefault(b =>
            {
                Debug.Assert(newBook.Isbn != null, "newBook.Isbn != null");
                Debug.Assert(b.Isbn != null, "b.Isbn != null");
                return b.Isbn.Replace("-", "") == newBook.Isbn.Replace("-", "") && b.Isbn != "__NOT_STATED__";
            });

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
                    string? ratingInput = Console.ReadLine();
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

                if (bookData.Isbn != "__NOT_STATED__" && existingBook.Isbn != bookData.Isbn)
                {
                    text.Add($"ISBN обновлен с '{existingBook.Isbn}' на '{bookData.Isbn}' из OpenLibrary.");
                    existingBook.Isbn = bookData.Isbn;
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
                            ProgressTask task = ctx.AddTask("[cyan] Парсинг данных...[/]");

                            coverTask = Task.Run(async () =>
                            {
                                using (HttpClient client = new HttpClient())
                                {
                                    HttpResponseMessage coverResponse = await client.GetAsync(bookData.CoverUrl, token); // Используем извлеченный токен
                                    coverResponse.EnsureSuccessStatusCode();
                                    byte[] coverBytes = await coverResponse.Content.ReadAsByteArrayAsync();
                                    await File.WriteAllBytesAsync(coverPath, coverBytes, token); // Используем токен
                                    coverLoaded = true;
                                }
                            });

                            try
                            {
                                while (!coverTask.IsCompleted)
                                {
                                    task.Increment(10);
                                    await Task.Delay(200, token); // Используем токен в задержке
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
                _books.Add(newBook);
                SaveBooks(); // Сохраняем изменения в файл

                // Предлагаем ввести рейтинг только если его нет в новой книге
                if (newBook.Rating == -1)
                {
                    AnsiConsole.MarkupLine("[bold blue]Введите рейтинг книги (1-10, пустая строка для пропуска, 0 для оценки 0):[/]");
                    string? ratingInput = Console.ReadLine();
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
                            ProgressTask task = ctx.AddTask("[cyan]Скачивание обложки...[/]");

                            coverTask = Task.Run(async () =>
                            {
                                using (HttpClient client = new HttpClient())
                                {
                                    HttpResponseMessage coverResponse = await client.GetAsync(bookData.CoverUrl, token); // Используем извлеченный токен
                                    coverResponse.EnsureSuccessStatusCode();
                                    byte[] coverBytes = await coverResponse.Content.ReadAsByteArrayAsync();
                                    await File.WriteAllBytesAsync(coverPath, coverBytes, token); // Используем токен
                                    coverLoaded = true;
                                }
                            });

                            try
                            {
                                while (!coverTask.IsCompleted)
                                {
                                    task.Increment(10);
                                    await Task.Delay(200, token); // Используем токен в задержке
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
    } // Здесь cts автоматически освобождается благодаря using
}
        // AI_CODE_1
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

        // AI_CODE_1
        // Нормализация жанров
        /// <summary>
        /// Нормализует строку жанров, очищая и маппинг их на известные значения.
        /// </summary>
        /// <param name="genre">Исходная строка жанров.</param>
        /// <returns>Нормализованная строка жанров.</returns>
        private static string NormalizeGenre(string? genre)
        {
            if (string.IsNullOrEmpty(genre) || genre == "__NOT_STATED__")
            {
                return "__NOT_STATED__";
            }

            // Разбиваем строку на отдельные жанры, нормализуем, фильтруем и мапим
            List<string> normalizedGenres = genre.ToLowerInvariant()
                .Split(',')
                .Select(g => g.Trim())
                .Where(g => !string.IsNullOrEmpty(g) && !g.Contains("nyt:") && !g.Contains("award:") && !g.Contains("(imaginary place)"))
                .Select(g => GenreMapping.ContainsKey(g) ? GenreMapping[g] : g) // Маппим или оставляем как есть
                .Distinct() // Убираем дубликаты после маппинга
                .Take(3) // Ограничиваем до 3 уникальных жанров
                .ToList();

            return normalizedGenres.Any() ? string.Join(", ", normalizedGenres) : "__NOT_STATED__";
        }

        // AI_CODE_1
        // Асинхронный метод для получения данных о книге из OpenLibrary
        /// <summary>
        /// Асинхронно получает данные о книге из OpenLibrary по ISBN.
        /// </summary>
        /// <param name="isbn">Уникальный код книги (ISBN).</param>
        /// <returns>Данные книги или null в случае ошибки.</returns>
        private static async Task<BookData?> GetBookDataFromOpenLibraryAsync(string? isbn)
        {
            using (HttpClient client = new HttpClient())
            {
                Debug.Assert(isbn != null, nameof(isbn) + " != null");
                isbn = isbn.Replace("-", "").Replace(" ", ""); // Убираем дефисы и пробелы
                string isbnUrl = $"https://openlibrary.org/isbn/{isbn}.json";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(isbnUrl);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    Dictionary<string, object>? bookData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                    if (bookData == null)
                    {
                        Console.WriteLine($"Пустой ответ от OpenLibrary для ISBN {isbn}.");
                        return new BookData { Isbn = isbn };
                    }

                    BookData result = new BookData { Isbn = isbn };

                    // Извлекаем название книги
                    result.Title = (bookData.ContainsKey("title") && !string.IsNullOrEmpty(bookData["title"].ToString())
                        ? bookData["title"].ToString()
                        : null) ?? "__NOT_STATED__";

                    // Извлекаем год
                    result.Year = bookData.ContainsKey("publish_date") && !string.IsNullOrEmpty(bookData["publish_date"].ToString())
                        ? int.TryParse(Regex.Match(bookData["publish_date"].ToString() ?? string.Empty, @"\d{4}").Value, out int year) ? year : -1
                        : -1;

                    // Извлекаем авторов
                    if (bookData.ContainsKey("authors") && bookData["authors"] is JsonElement authorsElement)
                    {
                        List<string?> authorNames = new List<string?>(); // Для нескольких авторов

                        if (authorsElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement author in authorsElement.EnumerateArray())
                            {
                                if (author.ValueKind == JsonValueKind.Object && author.TryGetProperty("key", out JsonElement key))
                                {
                                    string authorUrl = $"https://openlibrary.org{key.GetString()}.json";
                                    try
                                    {
                                        HttpResponseMessage authorResponse = await client.GetAsync(authorUrl);
                                        authorResponse.EnsureSuccessStatusCode();

                                        string authorJson = await authorResponse.Content.ReadAsStringAsync();
                                        Dictionary<string, object>? authorData = JsonSerializer.Deserialize<Dictionary<string, object>>(authorJson);
                                        if (authorData != null && authorData.ContainsKey("name") && !string.IsNullOrEmpty(authorData["name"].ToString()))
                                        {
                                            authorNames.Add(authorData["name"].ToString());
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Автор для URL {authorUrl} не найден или пуст.");
                                        }
                                    }
                                    catch (HttpRequestException ex)
                                    {
                                        Console.WriteLine($"Ошибка при запросе автора для URL {authorUrl}: {ex.Message}");
                                    }
                                }
                            }
                        }

                        // Если есть хотя бы один автор, соединяем их в строку
                        result.Author = authorNames.Any() ? string.Join(", ", authorNames) : "__NOT_STATED__";
                    }
                    else
                    {
                        Console.WriteLine($"Поле 'authors' отсутствует или некорректно для ISBN {isbn}.");
                        result.Author = "__NOT_STATED__";
                    }

                    // Извлекаем жанры
                    if (bookData.ContainsKey("works") && bookData["works"] is JsonElement works)
                    {
                        JsonElement work = works.EnumerateArray().FirstOrDefault();
                        if (work.ValueKind == JsonValueKind.Object && work.TryGetProperty("key", out JsonElement workKey))
                        {
                            string workUrl = $"https://openlibrary.org{workKey.GetString()}.json";
                            try
                            {
                                HttpResponseMessage workResponse = await client.GetAsync(workUrl);
                                workResponse.EnsureSuccessStatusCode();

                                string workJson = await workResponse.Content.ReadAsStringAsync();
                                Dictionary<string, object>? workData = JsonSerializer.Deserialize<Dictionary<string, object>>(workJson);

                                if (workData != null)
                                {
                                    string? rawGenre = workData.ContainsKey("subjects") && workData["subjects"] is JsonElement subjects
                                        ? string.Join(", ", subjects.EnumerateArray().Select(s => s.ToString()))
                                        : null;
                                    result.Genre = rawGenre != null ? NormalizeGenre(rawGenre) : "__NOT_STATED__";

                                    if (workData.ContainsKey("covers") && workData["covers"] is JsonElement covers)
                                    {
                                        JsonElement coverId = covers.EnumerateArray().FirstOrDefault();
                                        if (coverId.ValueKind != JsonValueKind.Undefined && coverId.ValueKind != JsonValueKind.Null)
                                        {
                                            result.CoverUrl = $"https://covers.openlibrary.org/b/id/{coverId.GetInt32()}-M.jpg";
                                        }
                                        else
                                        {
                                            result.CoverUrl = "__NOT_STATED__";
                                        }
                                    }
                                    else
                                    {
                                        result.CoverUrl = "__NOT_STATED__";
                                    }
                                }
                                else
                                {
                                    result.Genre = "__NOT_STATED__";
                                    result.CoverUrl = "__NOT_STATED__";
                                }
                            }
                            catch (HttpRequestException ex)
                            {
                                Console.WriteLine($"Ошибка при запросе работы для URL {workUrl}: {ex.Message}");
                                result.Genre = "__NOT_STATED__";
                                result.CoverUrl = "__NOT_STATED__";
                            }
                        }
                        else
                        {
                            result.Genre = "__NOT_STATED__";
                            result.CoverUrl = "__NOT_STATED__";
                        }
                    }
                    else
                    {
                        result.Genre = "__NOT_STATED__";
                        result.CoverUrl = "__NOT_STATED__";
                    }

                    return result;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Ошибка запроса к OpenLibrary: {ex.Message}");
                    return new BookData { Isbn = isbn, Author = "__NOT_STATED__", Genre = "__NOT_STATED__", CoverUrl = "__NOT_STATED__" };
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Ошибка парсинга JSON: {ex.Message}");
                    return new BookData { Isbn = isbn, Author = "__NOT_STATED__", Genre = "__NOT_STATED__", CoverUrl = "__NOT_STATED__" };
                }
            }
        }
    }
}