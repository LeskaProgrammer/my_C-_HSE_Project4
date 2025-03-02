using Spectre.Console;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Alexey_Simonov_project_4
{
    /// <summary>
    /// Статический класс, содержащий вспомогательные методы для управления библиотекой книг.
    /// </summary>
    public static class Metods
    {
        /// <summary>
        /// Ссылка на менеджер библиотеки, может быть null, если библиотека не инициализирована.
        /// </summary>
        public static LibraryManager? Library;

        /// <summary>
        /// Получает количество книг в библиотеке.
        /// Предполагается, что Library не null, иначе вызывается исключение через Debug.Assert.
        /// </summary>
        public static int LibCount
        {
            get
            {
                // Убеждаемся, что Library не null, чтобы избежать исключения NullReferenceException
                Debug.Assert(Library != null, nameof(Library) + " != null");
                return Library.Count;
            }
        }
        
        /// <summary>
        /// Получает путь к файлу библиотеки.
        /// Предполагается, что Library не null, иначе вызывается исключение через Debug.Assert.
        /// </summary>
        public static string LibraryPath
        {
            get
            {
                // Убеждаемся, что Library не null, чтобы избежать исключения NullReferenceException
                Debug.Assert(Library != null, nameof(Library) + " != null");
                return Library.FilePath;
            }
        }

        // Список допустимых жанров для проверки при вводе или редактировании книг
        private static string[] _validGenre =
        {
            "Фантастика", "Детектив", "Роман", "История", "Научная литература", "Приключения", "Фэнтези", "Боевик",
            "Триллер", "Комедия", "Драма", "Хоррор",
            "Science Fiction", "Mystery", "Romance", "History", "Nonfiction", "Adventure", "Fantasy", "Thriller",
            "Comedy", "Drama", "Horror", "Biography", "Autobiography", "Poetry", "Science", "Technology",
            "Dystopian Fiction", "Political Fiction", "Literary Fiction"
        };
        
        /// <summary>
        /// Отображает список книг с обложками в виде таблиц с панелями, используя библиотеку Spectre.Console.
        /// </summary>
        /// <param name="books">Список книг для отображения, может быть null.</param>
        /// <param name="text">Заголовок или текст для отображения, по умолчанию содержит описание списка книг с обложками.</param>
        public static void DisplayBooksWithCoversInPanels(List<Book>? books,
            string text = "\n=== Список книг в библиотеке с обложками ===")
        {
            if (books != null && !books.Any())
            {
                AnsiConsole.MarkupLine("[red]Список книг пуст.[/]");
                return;
            }

            int selectedIndex = 0;
            if (books != null)
            {
                List<Book> sortableBooks = new List<Book>(books);
                int sortColumn = -1;
                bool sortAscending = true;
                string filterText = "";

                while (true)
                {
                    List<Book> filteredBooks = FilterBooks(sortableBooks, filterText).ToList();
                    // Фильтрует книги по тексту и возвращает отфильтрованный список для отображения


                    if (sortColumn != -1)
                    {
                        filteredBooks = SortBooks(filteredBooks, sortColumn, sortAscending).ToList();
                        selectedIndex = Math.Min(selectedIndex, Math.Max(0, filteredBooks.Count - 1));
                        // Сортирует книги по указанному столбцу и направлению, корректируя индекс выбранной книги
                    }


                    Console.WriteLine(new string('\n', 50));
                    Console.Clear();
                    Table table = new Table()
                        .Border(TableBorder.Rounded)
                        .Alignment(Justify.Center)
                        .Title($"[bold blue]{text}[/]")
                        .Caption(
                            "[grey]Стрелки для навигации, Enter для обложки, Q для календаря, F для фильтра (по любому полю, Esc для отмены), Esc для выхода, 1-7 для сортировки[/]");

                    table.AddColumns(
                        CreateSortableColumn("Название", 25, sortColumn == 0, sortAscending)
                            .Alignment(Justify.Center),
                        CreateSortableColumn("Автор", 15, sortColumn == 1, sortAscending).Alignment(Justify.Center),
                        CreateSortableColumn("Жанр", 25, sortColumn == 2, sortAscending).Alignment(Justify.Center),
                        CreateSortableColumn("Год", 6, sortColumn == 3, sortAscending).Alignment(Justify.Center),
                        CreateSortableColumn("ISBN", 15, sortColumn == 4, sortAscending).Alignment(Justify.Center),
                        CreateSortableColumn("Оценка", 7, sortColumn == 5, sortAscending).Alignment(Justify.Center),
                        CreateSortableColumn("Обложка", 20, sortColumn == 6, sortAscending).Alignment(Justify.Center)
                    );

                    for (int i = 0; i < filteredBooks.Count; i++)
                    {
                        if (i > 0)
                        {
                            table.AddEmptyRow();
                        }

                        Book book = filteredBooks[i];
                        string? displayTitle = book.Title == "__NOT_STATED__" ? "not stated" : Truncate(book.Title, 50);
                        string? displayAuthor =
                            book.Author == "__NOT_STATED__" ? "not stated" : Truncate(book.Author, 15);
                        string? displayGenre = book.Genre == "__NOT_STATED__" ? "not stated" : Truncate(book.Genre, 50);
                        string displayYear = book.Year == -1 ? "not stated" : book.Year.ToString();
                        string? displayIsbn = book.Isbn == "__NOT_STATED__" ? "not stated" : Truncate(book.Isbn, 15);
                        string displayRating = book.Rating == -1 ? "not stated" : book.Rating.ToString();
                        string displayCover = !string.IsNullOrEmpty(book.CoverUrl) && File.Exists(book.CoverUrl)
                            ? $"[link={book.CoverUrl}]{Path.GetFileName(book.CoverUrl)}[/]"
                            : !string.IsNullOrEmpty(book.CoverUrl)
                                ? "[yellow]Не найдена[/]"
                                : "Без обложки";

                        string? titleMarkup = i == selectedIndex ? $"[green]-> {displayTitle}[/]" : displayTitle;
                        table.AddRow(
                            new Markup(titleMarkup ?? string.Empty),
                            new Markup(displayAuthor ?? string.Empty),
                            new Markup(displayGenre ?? string.Empty),
                            new Markup(displayYear),
                            new Markup(displayIsbn ?? string.Empty),
                            new Markup(displayRating),
                            new Markup(displayCover)
                        );
                    }


                    AnsiConsole.Write(table);

                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.UpArrow:
                            selectedIndex = Math.Max(0, selectedIndex - 1);
                            break;
                        case ConsoleKey.DownArrow:
                            selectedIndex = Math.Min(filteredBooks.Count - 1, selectedIndex + 1);
                            break;
                        case ConsoleKey.Enter:
                            DisplayCoverForSelectedBook(filteredBooks, selectedIndex);
                            break;
                        case ConsoleKey.Q:
                            DisplayCalendar(books);
                            break;
                        case ConsoleKey.Escape:
                            return;
                        case ConsoleKey.F:
                            filterText = ApplyFilter();
                            selectedIndex = 0;
                            break;
                        default:
                            if (keyInfo.KeyChar >= '1' && keyInfo.KeyChar <= '7')
                            {
                                int column = keyInfo.KeyChar - '1';
                                if (sortColumn == column)
                                {
                                    sortAscending = !sortAscending;
                                }
                                else
                                {
                                    sortColumn = column;
                                    sortAscending = true;
                                }
                            }

                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Создаёт сортируемый столбец для таблицы с указанным заголовком, шириной и параметрами сортировки.
        /// </summary>
        /// <param name="header">Текст заголовка столбца.</param>
        /// <param name="width">Ширина столбца в символах.</param>
        /// <param name="isSorted">Флаг, указывающий, отсортирован ли столбец в текущий момент.</param>
        /// <param name="ascending">Направление сортировки (по возрастанию или убыванию).</param>
        /// <returns>Объект TableColumn с настроенным заголовком и шириной.</returns>
        private static TableColumn CreateSortableColumn(string header, int width, bool isSorted,
            bool ascending)
        {
            string headerText = $"[bold blue]{header}[/]";
            if (isSorted)
            {
                headerText +=
                    ascending ? "[red] Dw[/]" : "[red] Up[/]"; // Индикация направления сортировки (стрелка вниз/вверх)
            }

            return new TableColumn(new Markup(headerText)).Width(width);
        }

        /// <summary>
        /// Укорачивает строку до указанной максимальной длины, добавляя троеточие, если строка длиннее.
        /// </summary>
        /// <param name="value">Исходная строка, может быть null.</param>
        /// <param name="maxLength">Максимальная длина строки после укорачивания.</param>
        /// <returns>Укороченная строка или исходная, если она короче или равна maxLength.</returns>
        private static string? Truncate(string? value, int maxLength)
        {
            return value != null && value.Length > maxLength ? value.Substring(0, maxLength - 3) + "..." : value;
        }

        /// <summary>
        /// Позволяет пользователю ввести текст для фильтрации списка книг через интерактивный ввод.
        /// </summary>
        /// <returns>Введённый пользователем текст фильтра или пустая строка при отмене (Esc).</returns>
        private static string ApplyFilter()
        {
            Console.WriteLine(new string('\n', 50));
            Console.Clear();
            AnsiConsole.MarkupLine("[bold blue]Введите текст для фильтрации (по любому полю, Esc для отмены):[/]");
            AnsiConsole.MarkupLine("[grey]Фильтрация чувствительна к регистру. Нажмите Enter для применения.[/]");
            string filterText = "";
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    return "";
                }

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return filterText;
                }

                if (keyInfo.Key == ConsoleKey.Backspace && filterText.Length > 0)
                {
                    filterText = filterText.Substring(0, filterText.Length - 1);
                    Console.WriteLine(new string('\n', 50));
                    Console.Clear();
                    AnsiConsole.MarkupLine(
                        "[bold blue]Введите текст для фильтрации (по любому полю, Esc для отмены):[/]");
                    AnsiConsole.MarkupLine("[grey]{0}[/]", filterText);
                }
                else if (char.IsLetterOrDigit(keyInfo.KeyChar) || char.IsWhiteSpace(keyInfo.KeyChar))
                {
                    filterText += keyInfo.KeyChar;
                    Console.WriteLine(new string('\n', 50));
                    Console.Clear();
                    AnsiConsole.MarkupLine(
                        "[bold blue]Введите текст для фильтрации (по любому полю, Esc для отмены):[/]");
                    AnsiConsole.MarkupLine("[grey]{0}[/]", filterText);
                }
            }
        }

        /// <summary>
        /// Фильтрует список книг по указанному тексту, игнорируя тире в ISBN для поиска.
        /// </summary>
        /// <param name="books">Список книг для фильтрации.</param>
        /// <param name="filterText">Текст для поиска в полях книги (название, автор, жанр, ISBN, год, рейтинг, обложка).</param>
        /// <returns>Фильтрованный список книг, соответствующий условиям поиска.</returns>
        private static IEnumerable<Book> FilterBooks(List<Book> books, string filterText)
        {
            if (string.IsNullOrEmpty(filterText))
            {
                return books;
            }

            // Нормализуем текст поиска, удаляя тире для корректного сравнения с ISBN
            string normalizedFilterText = filterText.Replace("-", "");
            return books.Where(b =>
            {
                // Убеждаемся, что свойства книги не null, чтобы избежать исключений при доступе
                Debug.Assert(b.Title != null, "b.Title != null");
                Debug.Assert(b.Isbn != null, "b.Isbn != null");
                Debug.Assert(b.Author != null, "b.Author != null");
                Debug.Assert(b.Genre != null, "b.Genre != null");
                return (b.Title != "__NOT_STATED__" &&
                        b.Title.Contains(filterText, StringComparison.OrdinalIgnoreCase)) ||
                       (b.Author != "__NOT_STATED__" &&
                        b.Author.Contains(filterText, StringComparison.OrdinalIgnoreCase)) ||
                       (b.Genre != "__NOT_STATED__" &&
                        b.Genre.Contains(filterText, StringComparison.OrdinalIgnoreCase)) ||
                       (b.Isbn != "__NOT_STATED__" && b.Isbn.Replace("-", "")
                           .Contains(normalizedFilterText, StringComparison.OrdinalIgnoreCase)) ||
                       (b.Year != -1 && b.Year.ToString().Contains(filterText)) ||
                       (b.Rating != -1 && b.Rating.ToString().Contains(filterText)) ||
                       (!string.IsNullOrEmpty(b.CoverUrl) && Path.GetFileName(b.CoverUrl)
                           .Contains(filterText, StringComparison.OrdinalIgnoreCase));
            });
        }

        /// <summary>
        /// Сортирует список книг по указанному столбцу и направлению сортировки.
        /// </summary>
        /// <param name="books">Список книг для сортировки.</param>
        /// <param name="column">Индекс столбца для сортировки (0 — Название, 1 — Автор, 2 — Жанр, 3 — Год, 4 — ISBN, 5 — Оценка, 6 — Обложка).</param>
        /// <param name="ascending">Флаг, указывающий, должна ли сортировка быть по возрастанию (true) или убыванию (false).</param>
        /// <returns>Отсортированный список книг в виде List&lt;Book&gt;.</returns>
        private static List<Book> SortBooks(List<Book> books, int column, bool ascending)
        {
            switch (column)
            {
                case 0:
                    return ascending
                        ? books.OrderBy(b => b.Title == "__NOT_STATED__" ? "" : b.Title).ToList()
                        : books.OrderByDescending(b => b.Title == "__NOT_STATED__" ? "" : b.Title).ToList();
                case 1:
                    return ascending
                        ? books.OrderBy(b => b.Author == "__NOT_STATED__" ? "" : b.Author).ToList()
                        : books.OrderByDescending(b => b.Author == "__NOT_STATED__" ? "" : b.Author).ToList();
                case 2:
                    return ascending
                        ? books.OrderBy(b => b.Genre == "__NOT_STATED__" ? "" : b.Genre).ToList()
                        : books.OrderByDescending(b => b.Genre == "__NOT_STATED__" ? "" : b.Genre).ToList();
                case 3:
                    return ascending
                        ? books.OrderBy(b => b.Year == -1 ? int.MinValue : b.Year).ToList()
                        : books.OrderByDescending(b => b.Year == -1 ? int.MinValue : b.Year).ToList();
                case 4:
                    return ascending
                        ? books.OrderBy(b => b.Isbn == "__NOT_STATED__" ? "" : b.Isbn).ToList()
                        : books.OrderByDescending(b => b.Isbn == "__NOT_STATED__" ? "" : b.Isbn).ToList();
                case 5:
                    return ascending
                        ? books.OrderBy(b => b.Rating == -1 ? int.MinValue : b.Rating).ToList()
                        : books.OrderByDescending(b => b.Rating == -1 ? int.MinValue : b.Rating).ToList();
                case 6:
                    return ascending
                        ? books.OrderBy(b => string.IsNullOrEmpty(b.CoverUrl) || !File.Exists(b.CoverUrl) ? 1 : 0)
                            .ToList()
                        : books.OrderByDescending(b =>
                            string.IsNullOrEmpty(b.CoverUrl) || !File.Exists(b.CoverUrl) ? 1 : 0).ToList();
                default: return books;
            }
        }

        /// <summary>
        /// Отображает обложку выбранной книги, если она существует и доступна локально.
        /// </summary>
        /// <param name="books">Список книг, из которого выбирается книга для отображения обложки.</param>
        /// <param name="selectedIndex">Индекс выбранной книги в списке.</param>
        private static void DisplayCoverForSelectedBook(List<Book> books, int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= books.Count)
            {
                return;
            }

            Book book = books[selectedIndex];
            Console.WriteLine(new string('\n', 50));
            Console.Clear();
            if (!string.IsNullOrEmpty(book.CoverUrl) && File.Exists(book.CoverUrl))
            {
                AnsiConsole.MarkupLine($"[bold blue]Обложка для \"{book.Title}\":[/]");
                CanvasImage image = new CanvasImage(book.CoverUrl).MaxWidth(20);
                AnsiConsole.Write(image);
            }
            else
            {
                return;
            }

            AnsiConsole.MarkupLine("[grey]Нажмите Esc для возврата к списку книг...[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }
        }

        /// <summary>
        /// Отображает календарь годов издания книг с количеством книг для каждого года.
        /// </summary>
        /// <param name="books">Список книг, из которых извлекаются годы издания, может быть null.</param>
        /// <param name="text">Заголовок календаря, по умолчанию "=== Календарь книг ===".</param>
        private static void DisplayCalendar(List<Book>? books, string text = "=== Календарь книг ===")
        {
            if (books == null || !books.Any())
            {
                AnsiConsole.MarkupLine("[red]Список книг пуст.[/]");
                return;
            }

            List<int> years = books.Where(b => b.Year != -1).Select(b => b.Year).Distinct().OrderByDescending(y => y)
                .ToList();
            if (!years.Any())
            {
                AnsiConsole.MarkupLine("[red]Нет книг с указанными годами издания.[/]");
                AnsiConsole.MarkupLine("[grey]Нажмите Esc для выхода...[/]");
                while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }

                return;
            }

            int selectedIndex = 0;

            while (true)
            {
                Console.WriteLine(new string('\n', 50));
                Console.Clear();

                Table table = new Table()
                    .Border(TableBorder.Rounded)
                    .Alignment(Justify.Left)
                    .Title($"[bold blue]{text}[/]") // Исправляем синтаксис Title
                    .Caption("[grey]Стрелки для навигации, Enter для книг, Esc для выхода[/]")
                    .AddColumn(new TableColumn("Год").Width(20).Alignment(Justify.Left));

                Dictionary<int, int> yearCounts = years.ToDictionary(y => y, y => books.Count(b => b.Year == y));
                int maxCount = yearCounts.Values.Max();

                for (int i = 0; i < years.Count; i++)
                {
                    int year = years[i];
                    int count = yearCounts[year];
                    int intensity = 128 + (127 * count / maxCount); // Вычисляем интенсивность цвета от 128 до 255
                    string color = $"#{intensity:X2}{intensity:X2}{intensity:X2}"; // Формируем цвет в формате #RRGGBB
                    string yearMarkup = i == selectedIndex
                        ? $"[green]-> [{color}]{year} ({count} книг)[/][/] "
                        : $"[{color}]{year} ({count} книг)[/]";
                    table.AddRow(new Markup(yearMarkup));
                }

                AnsiConsole.Write(table);

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = Math.Max(0, selectedIndex - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = Math.Min(years.Count - 1, selectedIndex + 1);
                        break;
                    case ConsoleKey.Enter:
                        DisplayBooksForYear(books, years[selectedIndex]);
                        break;
                    case ConsoleKey.Escape:
                        return;
                }
            }
        }

        /// <summary>
        /// Отображает список книг, изданных в указанном году, в таблице.
        /// </summary>
        /// <param name="books">Список всех книг, из которых фильтруются книги по году, может быть null.</param>
        /// <param name="year">Год, для которого отображаются книги.</param>
        private static void DisplayBooksForYear(List<Book>? books, int year)
        {
            Console.WriteLine(new string('\n', 50));
            Console.Clear();
            if (books != null)
            {
                List<Book> booksInYear = books.Where(b => b.Year == year).ToList();
                if (!booksInYear.Any())
                {
                    AnsiConsole.MarkupLine("[red]Книги за этот год не найдены.[/]");
                    AnsiConsole.MarkupLine("[grey]Нажмите Esc для возврата к календарю...[/]");
                    while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }

                    return;
                }

                // Создаём таблицу для отображения книг с закруглёнными границами и центрированием
                Table table = new Table()
                    .Border(TableBorder.Rounded) // Устанавливаем закруглённые границы таблицы
                    .Alignment(Justify.Center) // Центрируем таблицу по горизонтали
                    .Title($"[bold blue]Книги за {year} год[/]") // Устанавливаем заголовок таблицы
                    .Caption("[grey]Нажмите Esc для возврата к календарю[/]");

                // Добавляем столбцы с фиксированными ширинами и выравниванием по центру
                table.AddColumns(
                    new TableColumn("Название").Width(25).Alignment(Justify.Center),
                    new TableColumn("Автор").Width(15).Alignment(Justify.Center),
                    new TableColumn("Жанр").Width(25).Alignment(Justify.Center),
                    new TableColumn("Год").Width(6).Alignment(Justify.Center),
                    new TableColumn("ISBN").Width(15).Alignment(Justify.Center),
                    new TableColumn("Оценка").Width(7).Alignment(Justify.Center),
                    new TableColumn("Обложка").Width(20).Alignment(Justify.Center)
                );

                // Добавляем строки с данными книг, добавляя пустую строку как разделитель между записями
                for (int i = 0; i < booksInYear.Count; i++)
                {
                    if (i > 0)
                    {
                        table.AddEmptyRow(); // Добавляем пустую строку для визуального разделения книг
                    }

                    Book book = booksInYear[i];
                    Debug.Assert(book.Title != null, "book.Title != null");
                    string? displayTitle = book.Title == "__NOT_STATED__" ? "not stated"
                        : book.Title.Length > 25 ? book.Title.Substring(0, 22) + "..." : book.Title;
                    Debug.Assert(book.Author != null, "book.Author != null");
                    string? displayAuthor = book.Author == "__NOT_STATED__" ? "not stated"
                        : book.Author.Length > 15 ? book.Author.Substring(0, 12) + "..." : book.Author;
                    Debug.Assert(book.Genre != null, "book.Genre != null");
                    string? displayGenre = book.Genre == "__NOT_STATED__" ? "not stated"
                        : book.Genre.Length > 25 ? book.Genre.Substring(0, 22) + "..." : book.Genre;
                    string displayYear = book.Year == -1 ? "not stated" : book.Year.ToString();
                    Debug.Assert(book.Isbn != null, "book.Isbn != null");
                    string? displayIsbn = book.Isbn == "__NOT_STATED__" ? "not stated"
                        : book.Isbn.Length > 15 ? book.Isbn.Substring(0, 12) + "..." : book.Isbn;
                    string displayRating = book.Rating == -1 ? "not stated" : book.Rating.ToString();
                    string displayCover = !string.IsNullOrEmpty(book.CoverUrl) && File.Exists(book.CoverUrl)
                        ? $"[link={book.CoverUrl}]{Path.GetFileName(book.CoverUrl)}[/]"
                        : !string.IsNullOrEmpty(book.CoverUrl)
                            ? "[yellow]Не найдена[/]"
                            : "Без обложки";

                    table.AddRow(
                        new Markup(displayTitle),
                        new Markup(displayAuthor),
                        new Markup(displayGenre),
                        new Markup(displayYear),
                        new Markup(displayIsbn),
                        new Markup(displayRating),
                        new Markup(displayCover)
                    );
                }

                // Выводим таблицу в консоль с использованием Spectre.Console
                AnsiConsole.Write(table);
            }

            while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }
        }

        /// <summary>
        /// Отображает список книг в текстовом формате в консоли.
        /// </summary>
        /// <param name="books">Список книг для отображения.</param>
        /// <param name="text">Заголовок или текст для отображения, по умолчанию содержит описание списка книг.</param>
        private static void DisplayBooksInConsole(List<Book> books, string text = "\n=== Список книг в библиотеке ===")
        {
            if (!books.Any())
            {
                Console.WriteLine("Список книг пуст.");
                return;
            }

            // Устанавливаем заголовок таблицы, выделяя его зелёным цветом
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ResetColor();

            // Определяем фиксированные ширины для столбцов, чтобы обеспечить аккуратный вывод
            int titleWidth = 25; // Ограничение до 30 символов для Название
            int authorWidth = 20; // Ограничение до 20 символов для Автор
            int genreWidth = 30; // Ограничение до 30 символов для Жанр
            int yearWidth = 4; // Фиксированная ширина для Год (4 символа)
            int isbnWidth = 15; // Ограничение до 20 символов для ISBN
            int ratingWidth = 6; // Фиксированная ширина для Оценка (7 символов)

            // Выводим заголовки таблицы, выделяя их зелёным цветом
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(
                $"| {"Название".PadRight(titleWidth)} | {"Автор".PadRight(authorWidth)} | " +
                $"{"Жанр".PadRight(genreWidth)} | {"Год".PadRight(yearWidth)} | " +
                $"{"ISBN".PadRight(isbnWidth)} | {"Оценка".PadRight(ratingWidth)} |");
            Console.ResetColor();

            // Выводим разделитель между заголовком и данными
            Console.WriteLine(new string('-',
                titleWidth + authorWidth + genreWidth + yearWidth + isbnWidth + ratingWidth + 7));

            // Проходим по каждой книге в списке и выводим её данные
            foreach (Book book in books)
            {
                Debug.Assert(book.Title != null, "book.Title != null");
                string displayTitle = book.Title == "__NOT_STATED__" ? "not stated"
                    : book.Title.Length > titleWidth ? book.Title.Substring(0, titleWidth - 3) + "..."
                    : book.Title.PadRight(titleWidth);
                Debug.Assert(book.Author != null, "book.Author != null");
                string displayAuthor = book.Author == "__NOT_STATED__" ? "not stated"
                    : book.Author.Length > authorWidth ? book.Author.Substring(0, authorWidth - 3) + "..."
                    : book.Author.PadRight(authorWidth);
                Debug.Assert(book.Genre != null, "book.Genre != null");
                string displayGenre = book.Genre == "__NOT_STATED__" ? "not stated"
                    : book.Genre.Length > genreWidth ? book.Genre.Substring(0, genreWidth - 3) + "..."
                    : book.Genre.PadRight(genreWidth);
                string displayYear = book.Year == -1
                    ? "not stated".PadRight(yearWidth).Substring(0, yearWidth - 3) + "..."
                    : book.Year.ToString().PadRight(yearWidth).Substring(0, yearWidth);

                Debug.Assert(book.Isbn != null, "book.Isbn != null");
                string displayIsbn = book.Isbn == "__NOT_STATED__" ? "not stated"
                    : book.Isbn.Length > isbnWidth ? book.Isbn.Substring(0, isbnWidth - 3) + "..."
                    : book.Isbn.PadRight(isbnWidth);
                string displayRating = book.Rating == -1
                    ? "not stated".PadRight(ratingWidth).Substring(0, ratingWidth - 3) + "..."
                    : book.Rating.ToString().PadRight(ratingWidth).Substring(0, ratingWidth);

                Console.WriteLine(
                    $"| {displayTitle} | {displayAuthor} | " +
                    $"{displayGenre} | {displayYear} | " +
                    $"{displayIsbn} | {displayRating} |");
            }

            // Выводим завершающий разделитель после списка книг
            Console.WriteLine(new string('-',
                titleWidth + authorWidth + genreWidth + yearWidth + isbnWidth + ratingWidth + 7));
        }

        /// <summary>
        /// Показывает рекомендации на основе рейтинга книг (рейтинг >= 7).
        /// </summary>
        public static void ShowRecs()
        {
            // Создаём список для хранения рекомендуемых книг с рейтингом не менее 7
            List<Book> recommendedBooks = new List<Book>();
            Debug.Assert(Library != null, nameof(Library) + " != null");
            foreach (Book book in Library)
            {
                if (book.Rating >= 7 && book.Rating != -1) // Игнорируем книги с рейтингом -1 (not stated)
                {
                    recommendedBooks.Add(book);
                }
            }

            // Если есть рекомендуемые книги, отображаем их в консоли
            if (recommendedBooks.Count > 0)
            {
                Console.WriteLine(new string('\n', 50));
                Console.Clear();
                Library.DisplayPartBooks(recommendedBooks, DisplayBooksInConsole,
                    "Рекомендуемые книги (рейтинг 7 и выше):");
                Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
                Console.ReadKey(true);
            }
            else
            {
                Console.WriteLine(new string('\n', 50));
                Console.Clear();
                Console.WriteLine("Нет рекомендаций: книги с рейтингом 7 и выше не найдены.");
                Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// Удаляет книгу из библиотеки на основе поискового запроса пользователя.
        /// </summary>
        public static void DeleteBook()
        {
            // Инициализируем список для хранения книг, соответствующих поиску
            List<Book> matchingBooks = new List<Book>();
            string searchQuery = "";

            while (true)
            {
                // Очищаем консоль для обновления интерфейса
                Console.WriteLine(new string('\n', 50));
                Console.Clear();

                // Выводим форму для ввода поискового запроса
                Console.Write(
                    $"Поиск книги для удаления, введите часть любого поля (пробел это отобразить всех): {searchQuery}");
                Console.WriteLine();
                Console.WriteLine("Нажмите ENTER для выбора, ESC для выхода");

                // Фильтруем и отображаем книги, соответствующие поисковому запросу
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    matchingBooks.Clear();
                    Debug.Assert(Library != null, nameof(Library) + " != null");
                    foreach (Book book in Library)
                    {
                        Debug.Assert(book.Genre != null, "book.Genre != null");
                        Debug.Assert(book.Isbn != null, "b.Isbn != null");
                        Debug.Assert(book.Title != null, "book.Title != null");
                        Debug.Assert(book.Author != null, "book.Author != null");
                        if (book.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            book.Author.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            book.Genre.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            book.Isbn.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            (book.Year != -1 && book.Year.ToString().Contains(searchQuery)) ||
                            (book.Rating != -1 && book.Rating.ToString().Contains(searchQuery)))
                        {
                            matchingBooks.Add(book);
                        }
                    }

                    if (matchingBooks.Count > 0)
                    {
                        Library.DisplayPartBooks(matchingBooks, DisplayBooksInConsole, "\nНайденные книги:");
                    }
                    else
                    {
                        Console.WriteLine("\nКниги не найдены.");
                    }
                }

                // Обрабатываем ввод пользователя для выбора книги или отмены
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter && !string.IsNullOrEmpty(searchQuery)) // Выбор книги для удаления
                {
                    if (matchingBooks.Count == 1)
                    {
                        // Если найдена только одна книга, удаляем её сразу
                        DeleteSelectedBook(matchingBooks[0]);
                        break;
                    }
                    else if (matchingBooks.Count > 1)
                    {
                        // Если найдено несколько книг, предоставляем пользователю выбор
                        Book selectedBook = SelectBookFromList(matchingBooks);
                        DeleteSelectedBook(selectedBook);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Книга не найдена. Нажмите любую клавишу...");
                        Console.ReadKey(true);
                    }
                }
                else if (key.Key == ConsoleKey.Escape) // Выход без удаления
                {
                    Console.WriteLine("Удаление отменено.");
                    Console.WriteLine("Нажмите любую клавишу для выхода");
                    Console.ReadKey(true);
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace && searchQuery.Length > 0) // Удаление последнего символа запроса
                {
                    searchQuery = searchQuery.Substring(0, searchQuery.Length - 1);
                }
                else if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar) ||
                         key.KeyChar == '-') // Добавление символа в запрос
                {
                    searchQuery += key.KeyChar;
                }
            }
        }

        // Вспомогательная функция для удаления выбранной книги
        /// <summary>
        /// Удаляет выбранную книгу из библиотеки и сохраняет изменения.
        /// </summary>
        /// <param name="bookToDelete">Книга, которую нужно удалить из библиотеки.</param>
        private static void DeleteSelectedBook(Book bookToDelete)
        {
            Debug.Assert(Library != null, nameof(Library) + " != null");
            Library.RemoveBook(bookToDelete);
            Library.SaveBooks(); // Сохраняем изменения в файл
            Console.WriteLine(new string('\n', 50));
            Console.Clear();
            Console.WriteLine("Книга удалена. Нажмите любую клавишу для возврата в меню...");
            Console.ReadKey(true);
        }

        /// <summary>
        /// Позволяет пользователю изменить путь к файлу библиотеки, создавая новый файл, если он не существует.
        /// </summary>
        public static void ChangeFilePath()
        {
            while (true)
            {
                Console.WriteLine("Введите новый путь к файлу:");
                string? newPath = Console.ReadLine();

                Debug.Assert(newPath != null, nameof(newPath) + " != null");
                if (!newPath.Contains('.'))
                {
                    newPath += ".txt";
                }
                if (TryLoadOrCreateFile(newPath))
                {
                    Debug.Assert(Library != null, nameof(Library) + " != null");
                    Library.SaveBooks();
                    Debug.Assert(newPath != null, nameof(newPath) + " != null");
                    Library = new LibraryManager(newPath);
                    Console.WriteLine("Путь обновлён.");
                    break;
                }

                Console.WriteLine("Ошибка: Невозможно открыть или создать файл. Попробуйте снова.");
            }
        }

        /// <summary>
        /// Импортирует книги из CSV-файла в библиотеку, создавая резервную копию текущего файла.
        /// </summary>
        public static void ImportFromCsv()
        {
            // Сохраняем текущий файл библиотеки как резервную копию перед импортом
            if (File.Exists(Library?.FilePath))
            {
                string backupPath = $"{Library.FilePath}.bak";
                try
                {
                    File.Copy(Library.FilePath, backupPath, true);
                    Console.WriteLine($"Предыдущий файл сохранён как {backupPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка создания резервной копии: {ex.Message}");
                    Console.ReadKey(true);
                    return;
                }
            }

            // Запрашиваем путь к CSV-файлу для импорта
            Console.WriteLine("Введите путь к CSV-файлу для импорта (например, C:\\books.csv):");
            string csvPath = EnsureCsvExtension(Console.ReadLine() ?? ""); // Автоматически добавляем .csv, если нет

            if (string.IsNullOrEmpty(csvPath))
            {
                Console.WriteLine("Путь не указан. Нажмите любую клавишу...");
                Console.ReadKey(true);
                return;
            }

            if (!File.Exists(csvPath))
            {
                Console.WriteLine("Файл не найден. Нажмите любую клавишу...");
                Console.ReadKey(true);
                return;
            }

            try
            {
                List<Book> importedBooks = new List<Book>();
                // Читаем содержимое CSV-файла с кодировкой UTF-8 для совместимости с Excel
                string[] lines = File.ReadAllLines(csvPath, Encoding.UTF8);
                if (lines.Length == 0)
                {
                    Console.WriteLine("CSV-файл пуст. Нажмите любую клавишу...");
                    Console.ReadKey(true);
                    return;
                }

                // Проверяем, есть ли заголовок в CSV, предполагая разделитель ;
                bool hasHeader = lines[0].Contains("Title") && lines[0].Contains(";");
                int startIndex = hasHeader ? 1 : 0;

                for (int i = startIndex; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        Console.WriteLine($"Строка {i + 1} пропущена: пустая строка.");
                        continue;
                    }

                    // Разбиваем строку на поля, используя точку с запятой как разделитель
                    string[] parts = line.Split(';');

                    // Пропускаем строки с количеством полей больше 6
                    if (parts.Length > 6)
                    {
                        Console.WriteLine($"Строка {i + 1} пропущена: слишком много полей (больше 6).");
                        continue;
                    }

                    // Пропускаем строки с количеством полей меньше 6 и некорректными данными
                    if (parts.Length < 6)
                    {
                        bool hasInvalidData = false;
                        for (int j = 3; j < parts.Length && j < 6; j++) // Проверяем Year, ISBN, Rating
                        {
                            if (j == 3 && !int.TryParse(parts[j].Trim(), out _)) // Проверяем, является ли Year числом
                            {
                                hasInvalidData = true;
                                break;
                            }

                            if (j == 5 && !int.TryParse(parts[j].Trim(), out _)) // Проверяем, является ли Rating числом
                            {
                                hasInvalidData = true;
                                break;
                            }
                        }

                        if (hasInvalidData)
                        {
                            Console.WriteLine(
                                $"Строка {i + 1} пропущена: некорректные данные (например, буквы в Year или Rating).");
                            continue;
                        }
                    }

                    // Выводим отладочное сообщение для проверки парсинга строки
                    Console.WriteLine($"Строка {i + 1} распарсена: {string.Join(", ", parts)}");

                    // Извлекаем и нормализуем данные из строки CSV, используя значения по умолчанию при отсутствии
                    string title = parts.Length > 0
                        ? string.IsNullOrEmpty(parts[0].Trim()) ? "__NOT_STATED__" : parts[0].Trim()
                        : "__NOT_STATED__";
                    string author = parts.Length > 1
                        ? string.IsNullOrEmpty(parts[1].Trim()) ? "__NOT_STATED__" : parts[1].Trim()
                        : "__NOT_STATED__";
                    string genre = parts.Length > 2
                        ? string.IsNullOrEmpty(parts[2].Trim()) ? "__NOT_STATED__" : parts[2].Trim()
                        : "__NOT_STATED__";
                    int year = parts.Length > 3 && int.TryParse(parts[3].Trim(), out int y) ? y : -1;
                    string isbn = parts.Length > 4
                        ? string.IsNullOrEmpty(parts[4].Trim()) ? "__NOT_STATED__" : parts[4].Trim()
                        : "__NOT_STATED__";
                    int rating = parts.Length > 5 && int.TryParse(parts[5].Trim(), out int r) ? r : -1;

                    Book newBook = new Book(title, author, genre, year, isbn, rating);

                    // Проверяем, не дублируется ли книга по ISBN, игнорируя тире
                    Debug.Assert(Library != null, nameof(Library) + " != null");
                    if (!Library.Any(b =>
                        {
                            Debug.Assert(b.Isbn != null, "b.Isbn != null");
                            Debug.Assert(newBook.Isbn != null, "newBook.Isbn != null");
                            return b.Isbn.Replace("-", "").Trim() == newBook.Isbn.Replace("-", "").Trim() &&
                                   b.Isbn != "__NOT_STATED__";
                        }))
                    {
                        importedBooks.Add(newBook);
                    }
                    else
                    {
                        Console.WriteLine(
                            $"Строка {i + 1} пропущена: книга с ISBN {newBook.Isbn} уже существует в библиотеке.");
                    }
                }

                // Добавляем импортированные книги в библиотеку и сохраняем изменения
                if (Library != null && importedBooks.Any())
                {
                    foreach (Book book in importedBooks)
                    {
                        Library.AddBook(book);
                    }

                    Library.SaveBooks();
                    Console.WriteLine(
                        $"Импортировано {importedBooks.Count} новых книг из CSV. Нажмите любую клавишу...");
                }
                else if (!importedBooks.Any())
                {
                    Console.WriteLine(
                        "Нет новых книг для импорта, существующие книги пропущены. Нажмите любую клавишу...");
                }

                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка импорта из CSV: {ex.Message}. Нажмите любую клавишу...");
                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// Обеспечивает добавление расширения .csv к пути, если оно отсутствует или неверно.
        /// </summary>
        /// <param name="path">Исходный путь к файлу.</param>
        /// <returns>Путь с добавленным или исправленным расширением .csv.</returns>
        private static string EnsureCsvExtension(string path)
        {
            if (!Path.HasExtension(path) || Path.GetExtension(path).ToLower() != ".csv")
            {
                return Path.ChangeExtension(path, ".csv");
            }

            return path;
        }

        /// <summary>
        /// Экспортирует текущий список книг в JSON-файл по указанному пользователем пути.
        /// </summary>
        public static void ExportToJson()
        {
            Console.WriteLine("Введите путь для сохранения JSON-файла (например, C:\\exported_books):");
            string jsonPath = EnsureJsonExtension(Console.ReadLine() ?? ""); // Автоматически добавляем .json, если нет

            if (string.IsNullOrEmpty(jsonPath))
            {
                Console.WriteLine("Путь не указан. Нажмите любую клавишу...");
                Console.ReadKey(true);
                return;
            }

            try
            {
                if (Library != null)
                {
                    List<Book> booksToExport = Library.ToList(); // Преобразуем IEnumerable в List для сериализации
                    string json = JsonSerializer.Serialize(booksToExport,
                        new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(jsonPath, json, Encoding.UTF8); // Сохраняем с кодировкой UTF-8 для совместимости
                    Console.WriteLine($"Книги экспортированы в: {Path.GetFullPath(jsonPath)}");
                    Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
                    Console.ReadKey(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка экспорта в JSON: {ex.Message}. Нажмите любую клавишу...");
                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// Экспортирует текущий список книг в CSV-файл по указанному пользователем пути.
        /// </summary>
        public static void ExportToCsv()
        {
            Console.WriteLine("Введите путь для сохранения CSV-файла (например, C:\\exported_books):");
            string csvPath = EnsureCsvExtension(Console.ReadLine() ?? ""); // Автоматически добавляем .csv, если нет

            if (string.IsNullOrEmpty(csvPath))
            {
                Console.WriteLine("Путь не указан. Нажмите любую клавишу...");
                Console.ReadKey(true);
                return;
            }

            try
            {
                if (Library != null)
                {
                    using (StreamWriter
                           writer = new StreamWriter(csvPath, false, new UTF8Encoding(true))) // Используем UTF-8 с BOM для Excel
                    {
                        writer.WriteLine("Title;Author;Genre;Year;ISBN;Rating"); // Записываем заголовок CSV
                        foreach (Book book in Library)
                        {
                            string title = book.Title == "__NOT_STATED__" ? "" : EscapeCsv(book.Title);
                            string author = book.Author == "__NOT_STATED__" ? "" : EscapeCsv(book.Author);
                            string genre = book.Genre == "__NOT_STATED__" ? "" : EscapeCsv(book.Genre);
                            string year = book.Year == -1 ? "" : book.Year.ToString();
                            string isbn = book.Isbn == "__NOT_STATED__" ? "" : EscapeCsv(book.Isbn);
                            string rating = book.Rating == -1 ? "" : book.Rating.ToString();

                            string line = $"{title};{author};{genre};{year};{isbn};{rating}";
                            writer.WriteLine(line);
                        }
                    }

                    Console.WriteLine($"Книги экспортированы в: {Path.GetFullPath(csvPath)}");
                    Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
                    Console.ReadKey(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка экспорта в CSV: {ex.Message}. Нажмите любую клавишу...");
                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// Обеспечивает добавление расширения .json к пути, если оно отсутствует или неверно.
        /// </summary>
        /// <param name="path">Исходный путь к файлу.</param>
        /// <returns>Путь с добавленным или исправленным расширением .json.</returns>
        private static string EnsureJsonExtension(string path)
        {
            if (!Path.HasExtension(path) || Path.GetExtension(path).ToLower() != ".json")
            {
                return Path.ChangeExtension(path, ".json");
            }

            return path;
        }

        /// <summary>
        /// Экранирует специальные символы в строке для корректного формата CSV.
        /// </summary>
        /// <param name="value">Исходная строка, может быть null.</param>
        /// <returns>Экранированная строка для CSV или пустая строка, если значение null или пусто.</returns>
        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            if (value.Contains("\t") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        /// <summary>
        /// Проверяет валидность ISBN (10 или 13 символов) по стандартному алгоритму проверки.
        /// </summary>
        /// <param name="isbn">Строка с ISBN, может быть null.</param>
        /// <returns>Логическое значение, указывающее, валиден ли ISBN.</returns>
        public static bool IsValidIsbn(string? isbn)
        {
            Debug.Assert(isbn != null, nameof(isbn) + " != null");
            isbn = isbn.Replace("-", "").Replace(" ", ""); // Удаляем дефисы и пробелы для проверки

            if (string.IsNullOrEmpty(isbn))
            {
                return false;
            }

            if (isbn.Length == 10) // Проверяем ISBN-10
            {
                // Убеждаемся, что первые 9 символов — цифры, а последний — цифра или 'X'
                if (!isbn.Substring(0, 9).All(char.IsDigit))
                {
                    return false;
                }

                if (!char.IsDigit(isbn[9]) && isbn[9] != 'X')
                {
                    return false;
                }

                int sum = 0;
                for (int i = 0; i < 9; i++)
                {
                    sum += (10 - i) * (isbn[i] - '0');
                }

                int checkDigit = isbn[9] == 'X' ? 10 : isbn[9] - '0';
                return (sum + checkDigit) % 11 == 0;
            }
            else if (isbn.Length == 13) // Проверяем ISBN-13
            {
                // Убеждаемся, что все 13 символов — цифры
                if (!isbn.All(char.IsDigit))
                {
                    return false;
                }

                int sum = 0;
                for (int i = 0; i < 12; i++)
                {
                    int digit = isbn[i] - '0';
                    sum += (i % 2 == 0 ? 1 : 3) * digit;
                }

                int checkDigit = isbn[12] - '0';
                return (10 - (sum % 10)) % 10 == checkDigit;
            }

            return false; // Возвращаем false, если длина ISBN некорректна
        }

        /// <summary>
        /// Добавляет новую книгу в библиотеку через интерактивный ввод от пользователя.
        /// </summary>
        public static void AddBook()
        {
            Book newBook = new Book(); // Создаём новую книгу с значениями по умолчанию
            string[] fields = { "Title", "Author", "Genre", "Year", "ISBN", "Rating" }; // Список полей для ввода
            string?[] values =
            {
                newBook.Title, newBook.Author, newBook.Genre, newBook.Year.ToString(), newBook.Isbn,
                newBook.Rating.ToString()
            };
            int currentField = 0; // Индекс текущего поля для редактирования
            bool isIsbnInvalid = false; // Флаг, указывающий на некорректный ISBN

            // Инициализируем значения массива values, заменяя "__NOT_STATED__" или "not stated" на пустую строку
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == "__NOT_STATED__" || values[i] == "not stated")
                {
                    values[i] = "";
                }
                else if (i >= 3) // Обрабатываем числовые поля Year и Rating
                {
                    values[i] = values[i] == "-1" ? "" : values[i];
                }
            }

            while (true)
            {
                // Очищаем консоль для обновления интерфейса
                Console.WriteLine(new string('\n', 50));
                Console.Clear();

                // Выводим все поля для ввода, выделяя текущее поле цветом
                for (int i = 0; i < fields.Length; i++)
                {
                    Console.Write($"Введите {fields[i]} книги: ");
                    if (i == currentField)
                    {
                        // Если текущее поле — это ISBN и оно некорректно, выделяем красным, иначе зелёным
                        if (i == 4 && !string.IsNullOrEmpty(values[i]) && values[i] != "not stated" && isIsbnInvalid)
                        {
                            Console.ForegroundColor = ConsoleColor.Red; // Указываем на ошибку ISBN
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green; // Обычное выделение активного поля
                        }

                        Console.Write(values[i] == ""
                            ? "not stated"
                            : values[i]); // Показываем "not stated", если поле пустое
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(values[i] == "" ? "not stated" : values[i]); // Неактивное поле без цвета
                    }

                    Console.WriteLine();
                }

                Console.WriteLine("Нажмите ENTER для выхода");

                // Обрабатываем ввод пользователя для навигации и редактирования
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter) // Завершение ввода и добавление книги
                {
                    // Проверяем, все ли поля остались пустыми или по умолчанию
                    bool allNotStated = true;
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (i < 3 || i == 4) // Проверяем строковые поля (Title, Author, Genre, ISBN)
                        {
                            if (values[i] != "" && values[i] != "not stated")
                            {
                                allNotStated = false;
                                break;
                            }
                        }
                        else // Проверяем числовые поля (Year, Rating)
                        {
                            if (int.TryParse(values[i], out int num) && num != -1)
                            {
                                allNotStated = false;
                                break;
                            }
                        }
                    }

                    if (allNotStated)
                    {
                        Console.WriteLine(
                            "Книга не добавлена: все поля пустые или по умолчанию. Нажмите любую клавишу...");
                        Console.ReadKey(true);
                        break;
                    }

                    // Проверяем валидность ISBN, если он указан
                    if (values[4] != "" && values[4] != "not stated" && !IsValidIsbn(values[4]))
                    {
                        Console.WriteLine("Неверный ISBN. Исправьте перед сохранением. Нажмите любую клавишу...");
                        Console.ReadKey(true);
                        continue; // Возвращаемся к вводу для исправления
                    }

                    // Проверяем, принадлежит ли жанр списку допустимых значений
                    if (values[2] != "" && values[2] != "not stated" && !_validGenre.Contains(values[2]))
                    {
                        Console.WriteLine("Неверный Genre. Используйте один жанр из списка. Нажмите любую клавишу...");
                        foreach (string i in _validGenre)
                        {
                            Console.Write(i + (i == _validGenre[_validGenre.Length - 1] ? "" : ", "));
                        }

                        Console.ReadKey(true);
                        continue; // Возвращаемся к вводу для исправления
                    }

                    // Преобразуем введённые значения в свойства книги, используя "__NOT_STATED__" для пустых строк
                    newBook.Title = values[0] == "" ? "__NOT_STATED__" : values[0];
                    newBook.Author = values[1] == "" ? "__NOT_STATED__" : values[1];
                    newBook.Genre = values[2] == "" ? "__NOT_STATED__" : values[2];
                    newBook.Year = int.TryParse(values[3], out int year) ? year : -1;
                    newBook.Isbn = values[4] == "" ? "__NOT_STATED__" : values[4];
                    newBook.Rating = int.TryParse(values[5], out int rating) ? rating : -1;
                    newBook.CoverUrl = ""; // Устанавливаем пустую обложку, так как она добавляется через ISBN, если доступна

                    Debug.Assert(Library != null, nameof(Library) + " != null");
                    Library.AddBook(newBook);
                    Console.WriteLine("Книга добавлена. Нажмите любую клавишу...");
                    Console.ReadKey(true);
                    break;
                }
                else if (key.Key == ConsoleKey.UpArrow) // Навигация вверх по полям
                {
                    currentField = (currentField - 1 + fields.Length) % fields.Length;
                }
                else if (key.Key == ConsoleKey.DownArrow) // Навигация вниз по полям
                {
                    currentField = (currentField + 1) % fields.Length;
                }
                else if (key.Key == ConsoleKey.Backspace && values[currentField] is { Length: > 0 }) // Удаление символа
                {
                    // Предполагаем, что values[currentField] не null в этом контексте, так как проверка Length > 0 гарантирует существование значения
                    values[currentField] = values[currentField]?.Substring(0, values[currentField]!.Length - 1);
                    if (currentField == 4)
                    {
                        isIsbnInvalid = false; // Сбрасываем флаг некорректности ISBN при изменении
                    }
                }
                else if (currentField >= 3 && char.IsDigit(key.KeyChar)) // Разрешены только цифры для Year и Rating
                {
                    if (values[currentField] == "")
                    {
                        values[currentField] = ""; // Убеждаемся, что не добавляем символы к "not stated"
                    }

                    values[currentField] += key.KeyChar;
                }
                else if
                    (currentField < 3 ||
                     currentField == 4) // Разрешены буквы, цифры, пробелы и дефис для Title, Author, Genre, ISBN
                {
                    if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar) || key.KeyChar == '-')
                    {
                        if (values[currentField] == "")
                        {
                            values[currentField] = ""; // Убеждаемся, что не добавляем символы к "not stated"
                        }

                        values[currentField] += key.KeyChar;
                        if (currentField == 4)
                        {
                            isIsbnInvalid = false; // Сбрасываем флаг некорректности ISBN при изменении
                        }
                    }
                }
            }

            Console.WriteLine(new string('\n', 50));
            Console.Clear();
        }

        /// <summary>
        /// Позволяет пользователю редактировать существующую книгу на основе поискового запроса.
        /// </summary>
        public static void EditBook()
        {
            // Создаём список для хранения книг, соответствующих поиску
            List<Book> matchingBooks = new List<Book>();
            string searchQuery = "";

            while (true)
            {
                // Очищаем консоль для обновления интерфейса
                Console.WriteLine(new string('\n', 50));
                Console.Clear();

                // Выводим форму для ввода поискового запроса
                Console.Write(
                    $"Поиск книги для удаления, введите часть любого поля (пробел это отобразить всех): {searchQuery}");
                Console.WriteLine();
                Console.WriteLine("Нажмите ENTER для выбора, ESC для выхода");

                // Фильтруем и отображаем книги, соответствующие поисковому запросу
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    matchingBooks.Clear();
                    Debug.Assert(Library != null, nameof(Library) + " != null");
                    foreach (Book book in Library)
                    {
                        Debug.Assert(book.Title != null, "book.Title != null");
                        Debug.Assert(book.Genre != null, "book.Genre != null");
                        Debug.Assert(book.Author != null, "book.Author != null");
                        Debug.Assert(book.Isbn != null, "book.Isbn != null");
                        if (book.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            book.Author.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            book.Genre.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            book.Isbn.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            (book.Year != -1 && book.Year.ToString().Contains(searchQuery)) ||
                            (book.Rating != -1 && book.Rating.ToString().Contains(searchQuery)))
                        {
                            matchingBooks.Add(book);
                        }
                    }

                    if (matchingBooks.Count > 0)
                    {
                        Library.DisplayPartBooks(matchingBooks, DisplayBooksInConsole, "\nНайденные книги:");
                    }
                    else
                    {
                        Console.WriteLine("\nКниги не найдены.");
                    }
                }

                // Обрабатываем ввод пользователя для выбора книги или отмены
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter && !string.IsNullOrEmpty(searchQuery)) // Выбор книги для редактирования
                {
                    if (matchingBooks.Count == 1)
                    {
                        // Если найдена только одна книга, начинаем её редактирование
                        EditSelectedBook(matchingBooks[0]);
                        break;
                    }
                    else if (matchingBooks.Count > 1)
                    {
                        // Если найдено несколько книг, предоставляем пользователю выбор
                        Book selectedBook = SelectBookFromList(matchingBooks);
                        EditSelectedBook(selectedBook);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Книга не найдена. Нажмите любую клавишу...");
                        Console.ReadKey(true);
                    }
                }
                else if (key.Key == ConsoleKey.Escape) // Выход без редактирования
                {
                    Console.WriteLine("Редактирование отменено.");
                    Console.WriteLine("Нажмите любую кнопку для выхода");
                    Console.ReadKey(true);
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace && searchQuery.Length > 0) // Удаление последнего символа запроса
                {
                    searchQuery = searchQuery.Substring(0, searchQuery.Length - 1);
                }
                else if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar) ||
                         key.KeyChar == '-') // Добавление символа в запрос
                {
                    searchQuery += key.KeyChar;
                }
            }
        }

        /// <summary>
        /// Предоставляет пользователю возможность выбрать книгу из списка для дальнейших действий.
        /// </summary>
        /// <param name="books">Список книг, из которых выбирается одна.</param>
        /// <returns>Выбранная книга или новая пустая книга при отмене выбора.</returns>
        private static Book SelectBookFromList(List<Book> books)
        {
            int selectedIndex = 0;

            while (true)
            {
                Console.WriteLine(new string('\n', 50));
                Console.Clear();
                Console.WriteLine("Выберите конкретную книгу:");

                for (int i = 0; i < books.Count; i++)
                {
                    Console.Write($"{i + 1}. {books[i].Title} ({books[i].Author}, {books[i].Year})");
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" <- Текущий выбор");
                        Console.ResetColor();
                    }

                    Console.WriteLine();
                }

                Console.WriteLine("Используйте стрелки вверх/вниз для выбора, ENTER для подтверждения, ESC для отмены");

                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter) // Подтверждение выбора книги
                {
                    return books[selectedIndex];
                }
                else
                {
                    if (key.Key == ConsoleKey.Escape) // Отмена выбора
                    {
                        Console.WriteLine("Выбор отменён. Нажмите любую клавишу...");
                        Console.ReadKey(true);
                        return new Book();
                    }
                    if (key.Key == ConsoleKey.UpArrow) // Навигация вверх по списку
                    {
                        selectedIndex = (selectedIndex - 1 + books.Count) % books.Count;
                    }
                    else if (key.Key == ConsoleKey.DownArrow) // Навигация вниз по списку
                    {
                        selectedIndex = (selectedIndex + 1) % books.Count;
                    }
                }
            }
        }
        
            
        public static bool TryLoadOrCreateFile(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false; // Дополнительная проверка на null/пустую строку
            }

            try
            {
                if (File.Exists(path))
                {
                    return true;
                }
                else
                {
                    string? directory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.Create(path).Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Редактирует выбранную книгу через интерактивный ввод от пользователя.
        /// </summary>
        /// <param name="bookToEdit">Книга, которую нужно отредактировать.</param>
        private static void EditSelectedBook(Book bookToEdit)
        {
            // Подготавливаем массив полей для редактирования книги
            string[] fields = { "Title", "Author", "Genre", "Year", "ISBN", "Rating" }; // Поля для ввода
            string?[] values =
            {
                bookToEdit.Title == "__NOT_STATED__" ? "" : bookToEdit.Title,
                bookToEdit.Author == "__NOT_STATED__" ? "" : bookToEdit.Author,
                bookToEdit.Genre == "__NOT_STATED__" ? "" : bookToEdit.Genre, bookToEdit.Year.ToString(),
                bookToEdit.Isbn == "__NOT_STATED__" ? "" : bookToEdit.Isbn, bookToEdit.Rating.ToString()
            };
            int currentField = 0; // Индекс текущего поля для редактирования
            bool isIsbnInvalid = false; // Флаг, указывающий на некорректный ISBN

            // Нормализуем значения, заменяя "__NOT_STATED__" или "not stated" на пустую строку, а числовые поля (-1) на пустую строку
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == "__NOT_STATED__" || values[i] == "not stated")
                {
                    values[i] = "";
                }
                else if (i >= 3) // Обрабатываем числовые поля Year и Rating
                {
                    values[i] = values[i] == "-1" ? "" : values[i];
                }
            }

            while (true)
            {
                // Очищаем консоль для обновления интерфейса
                Console.WriteLine(new string('\n', 50));
                Console.Clear();

                // Выводим все поля для редактирования, выделяя текущее поле цветом
                for (int i = 0; i < fields.Length; i++)
                {
                    Console.Write($"Текущее {fields[i]}: ");
                    if (i == currentField)
                    {
                        // Если текущее поле — это ISBN и оно некорректно, выделяем красным, иначе зелёным
                        if (i == 4 && !string.IsNullOrEmpty(values[i]) && values[i] != "not stated" && isIsbnInvalid)
                        {
                            Console.ForegroundColor = ConsoleColor.Red; // Указываем на ошибку ISBN
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green; // Обычное выделение активного поля
                        }

                        Console.Write(values[i] == ""
                            ? "not stated"
                            : values[i]); // Показываем "not stated", если поле пустое
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(values[i] == "" ? "not stated" : values[i]); // Неактивное поле без цвета
                    }

                    Console.WriteLine();
                }

                Console.WriteLine("Нажмите ENTER для сохранения и выхода");

                // Обрабатываем ввод пользователя для навигации и редактирования
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter) // Сохранение изменений и выход
                {
                    // Проверяем валидность ISBN, если он указан
                    if (values[4] != "" && values[4] != "not stated" && !IsValidIsbn(values[4]))
                    {
                        Console.WriteLine("Неверный ISBN. Исправьте перед сохранением. Нажмите любую клавишу...");
                        Console.ReadKey(true);
                        continue; // Возвращаемся к вводу для исправления
                    }

                    // Проверяем, принадлежит ли жанр списку допустимых значений
                    if (values[2] != "" && values[2] != "not stated" && !_validGenre.Contains(values[2]))
                    {
                        Console.WriteLine("Неверный Genre. Используйте один жанр из списка. Нажмите любую клавишу...");
                        foreach (string i in _validGenre)
                        {
                            Console.Write(i + (i == _validGenre[_validGenre.Length - 1] ? "" : ", "));
                        }

                        Console.ReadKey(true);
                        continue; // Возвращаемся к вводу для исправления
                    }

                    // Обновляем свойства книги, используя "__NOT_STATED__" для пустых строк
                    bookToEdit.Title = values[0] == "" ? "__NOT_STATED__" : values[0];
                    bookToEdit.Author = values[1] == "" ? "__NOT_STATED__" : values[1];
                    bookToEdit.Genre = values[2] == "" ? "__NOT_STATED__" : values[2];
                    bookToEdit.Year = int.TryParse(values[3], out int year) ? year : -1;
                    bookToEdit.Isbn = values[4] == "" ? "__NOT_STATED__" : values[4];
                    bookToEdit.Rating = int.TryParse(values[5], out int rating) ? rating : -1;
                    bookToEdit.CoverUrl = bookToEdit.CoverUrl; // Сохраняем существующий путь к обложке, если он есть

                    Debug.Assert(Library != null, nameof(Library) + " != null");
                    Library.SaveBooks(); // Сохраняем изменения в файл
                    Console.WriteLine("Книга отредактирована и сохранена.");
                    Console.ReadKey(true);
                    break;
                }
                else if (key.Key == ConsoleKey.UpArrow) // Навигация вверх по полям
                {
                    currentField = (currentField - 1 + fields.Length) % fields.Length;
                }
                else if (key.Key == ConsoleKey.DownArrow) // Навигация вниз по полям
                {
                    currentField = (currentField + 1) % fields.Length;
                }
                else if (key.Key == ConsoleKey.Backspace && values[currentField] is { Length: > 0 }) // Удаление символа
                {
                    // Предполагаем, что values[currentField] не null в этом контексте, так как проверка Length > 0 гарантирует существование значения
                    values[currentField] = values[currentField]?.Substring(0, values[currentField]!.Length - 1);
                    if (currentField == 4)
                    {
                        isIsbnInvalid = false; // Сбрасываем флаг некорректности ISBN при изменении
                    }
                }
                else if (currentField >= 3 && char.IsDigit(key.KeyChar)) // Разрешены только цифры для Year и Rating
                {
                    if (values[currentField] == "")
                    {
                        values[currentField] = ""; // Убеждаемся, что не добавляем символы к "not stated"
                    }

                    values[currentField] += key.KeyChar;
                }
                else if
                    (currentField < 3 ||
                     currentField == 4) // Разрешены буквы, цифры, пробелы и дефис для Title, Author, Genre, ISBN
                {
                    if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar) || key.KeyChar == '-')
                    {
                        if (values[currentField] == "")
                        {
                            values[currentField] = ""; // Убеждаемся, что не добавляем символы к "not stated"
                        }

                        values[currentField] += key.KeyChar;
                        if (currentField == 4)
                        {
                            isIsbnInvalid = false; // Сбрасываем флаг некорректности ISBN при изменении
                        }
                    }
                }
            }

            Console.WriteLine(new string('\n', 50));
            Console.Clear();
        }
        
    

    }
}