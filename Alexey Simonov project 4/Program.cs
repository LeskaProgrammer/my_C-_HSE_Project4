
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;


class Program
{
    private static LibraryManager library;
   
    static async Task Main(string[] args)
    {
        string path = null;
        while (true)
        {
            Console.WriteLine("Введите путь к файлу библиотеки:");
            path = "C:\\Users\\user\\Desktop\\проект 4.txt";

            if (TryLoadOrCreateFile(path))
            {
                library = new LibraryManager(path);
                break;
            }
            Console.WriteLine("Ошибка: Невозможно открыть или создать файл. Попробуйте снова.");
        }

        
        
        int selectedIndex = 0; // Индекс выбранного пункта меню (0–9)



while (true)
{
    Console.Clear();
    Console.WriteLine($"Книг в библиотеке: {library.Count}");
    Console.WriteLine("\nМеню:");

    // Выводим пункты меню с выделением выбранного
    string[] menuItems = new[]
    {
        "1. Просмотреть книги",
        "2. Добавить книгу вручную",
        "3. Добавить книгу по ISBN (OpenLibrary)",
        "4. Редактировать книгу",
        "5. Удалить книгу",
        "6. Показать рекомендации",
        "7. Импорт из CSV",
        "8. Экспорт в JSON/CSV",
        "9. Изменить путь к файлу (JSON или TXT)",
    };

    for (int i = 0; i < menuItems.Length; i++)
    {
        if (i == selectedIndex)
        {
            // Выделяем выбранный пункт: добавляем отступы и белый цвет
            if (i > 0) Console.WriteLine(); // Отступ сверху, кроме верхнего пункта
            Console.ForegroundColor = ConsoleColor.White; // Светлый цвет для выделения
            Console.WriteLine($"  =>  {menuItems[i]}  <=  "); // Увеличиваем визуальный размер стрелками
            Console.WriteLine(); // Отступ снизу, кроме нижнего пункта
            Console.ResetColor(); // Сбрасываем цвет
        }
        else
        {
            // Не выбранные пункты отображаем в тусклом цвете
            Console.ForegroundColor = ConsoleColor.DarkGray; // Тусклый цвет для остальных
            Console.WriteLine($"     {menuItems[i]}");
            Console.ResetColor(); // Сбрасываем цвет
        }
    }

    Console.Write("\nИспользуйте стрелки или 1-9 для навигации, Enter для выбора, Esc для выхода: ");
    
    var key = Console.ReadKey(true); // Читаем клавишу без отображения
    switch (key.Key)
    {
        case ConsoleKey.UpArrow:
            selectedIndex = Math.Max(0, selectedIndex - 1); // Перемещаемся вверх, не выходя за пределы 0
            break;
        case ConsoleKey.DownArrow:
            selectedIndex = Math.Min(menuItems.Length - 1, selectedIndex + 1); // Перемещаемся вниз, не выходя за пределы 9
            break;
        case ConsoleKey.D1: // Клавиша '1'
        case ConsoleKey.NumPad1:
            selectedIndex = 0; // Выделяем пункт 1
            break;
        case ConsoleKey.D2: // Клавиша '2'
        case ConsoleKey.NumPad2:
            selectedIndex = 1; // Выделяем пункт 2
            break;
        case ConsoleKey.D3: // Клавиша '3'
        case ConsoleKey.NumPad3:
            selectedIndex = 2; // Выделяем пункт 3
            break;
        case ConsoleKey.D4: // Клавиша '4'
        case ConsoleKey.NumPad4:
            selectedIndex = 3; // Выделяем пункт 4
            break;
        case ConsoleKey.D5: // Клавиша '5'
        case ConsoleKey.NumPad5:
            selectedIndex = 4; // Выделяем пункт 5
            break;
        case ConsoleKey.D6: // Клавиша '6'
        case ConsoleKey.NumPad6:
            selectedIndex = 5; // Выделяем пункт 6
            break;
        case ConsoleKey.D7: // Клавиша '7'
        case ConsoleKey.NumPad7:
            selectedIndex = 6; // Выделяем пункт 7
            break;
        case ConsoleKey.D8: // Клавиша '8'
        case ConsoleKey.NumPad8:
            selectedIndex = 7; // Выделяем пункт 8
            break;
        case ConsoleKey.D9: // Клавиша '9'
        case ConsoleKey.NumPad9:
            selectedIndex = 8; // Выделяем пункт 9
            break;
        case ConsoleKey.Enter:
            // Обрабатываем выбранный пункт
            switch (selectedIndex + 1) // Сдвигаем индекс на 1, так как меню начинается с 1
            {
                case 1:
                    Console.Clear();
                    library.DisplayBooks(DisplayBooksWithCoversInPanels, "\n=== Список книг в библиотеке ===");
                    Console.Clear();
                    break;
                case 2:
                    AddBook();
                    break;
                case 3:
                    string? isbn = "";
                    while (!IsValidISBN(isbn))
                    {
                        Console.Clear();
                        Console.WriteLine("Введите пустую строку для выхода");
                        Console.Write("Введите корректный IBSN: ");
                        isbn = Console.ReadLine();
                        if (isbn == "") break;
                    }
                    
                    if (IsValidISBN(isbn)) 
                    {
                        await library.AddBookFromOpenLibrary(isbn);
                    }
                    Console.WriteLine("Нажмите любую кнопку для выхода");
                    Console.ReadKey();
                    break;
                case 4:
                    EditBook();
                    break;
                case 5:
                    DeleteBook();
                    break;
                case 6:
                    ShowRecs();
                    break;
                case 7:
                    ImportFromCsv();
                    break;
                case 8:
                    Console.Clear();
                    int exportPointer = 0;
                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine("Выберите, куда экспортировать:");
                        Console.WriteLine("В JSON" + (exportPointer == 0 ? "  =>" : ""));
                        Console.WriteLine("В CSV" + (exportPointer == 1 ? "  =>" : ""));
                        var exportKey = Console.ReadKey(true);
                        if (exportKey.Key == ConsoleKey.DownArrow)
                        {
                            exportPointer = (exportPointer + 1) % 2;
                        }
                        if (exportKey.Key == ConsoleKey.UpArrow)
                        {
                            exportPointer = (exportPointer - 1 + 2) % 2;
                        }
                        if (exportKey.Key == ConsoleKey.Enter)
                        {
                            break;
                        }
                    }
                    
                    if (exportPointer == 0) ExportToJson();
                    if (exportPointer == 1) ExportToCsv();
                    break;
                case 9:
                    ChangeFilePath();
                    break;
            }
            break;
        case ConsoleKey.Escape:
            library.SaveBooks();
            Console.WriteLine("Изменения сохранены. До свидания!");
            return; // Выход по Esc
    }
}
        
        
        
     
    }
    
    
    
    static string[] validGenre =
    {
        "Фантастика",
        "Детектив",
        "Роман",
        "История",
        "Научная литература",
        "Приключения",
        "Фэнтези",
        "Боевик",
        "Триллер",
        "Комедия",
        "Драма",
        "Хоррор"
    };

    
    
    
    
    
    
    
    
private static void DisplayBooksWithCoversInPanels(List<Book> books, string text = "\n=== Список книг в библиотеке с обложками ===")
{
    if (books == null || !books.Any())
    {
        AnsiConsole.MarkupLine("[red]Список книг пуст.[/]");
        return;
    }

    int selectedIndex = 0;
    var sortableBooks = new List<Book>(books);
    int sortColumn = -1;
    bool sortAscending = true;
    string filterText = "";

    while (true)
    {
        var filteredBooks = FilterBooks(sortableBooks, filterText).ToList();
        
        
        
        if (sortColumn != -1)
        {
            filteredBooks = SortBooks(filteredBooks, sortColumn, sortAscending).ToList();
            selectedIndex = Math.Min(selectedIndex, Math.Max(0, filteredBooks.Count - 1));
        }
        
        
        
        

        Console.WriteLine(new string('\n', 50)); Console.Clear();  
        var table = new Table()
            .Border(TableBorder.Rounded)
           
            .Alignment(Justify.Center)
            .Title($"[bold blue]{text}[/]")
            .Caption("[grey]Стрелки для навигации, Enter для обложки, Q для календаря, F для фильтра (по любому полю, Esc для отмены), Esc для выхода, 1-7 для сортировки[/]");

        table.AddColumns(
            CreateSortableColumn("Название", 25, 0, sortColumn == 0, sortAscending).Alignment(Justify.Center),
            CreateSortableColumn("Автор", 15, 1, sortColumn == 1, sortAscending).Alignment(Justify.Center),
            CreateSortableColumn("Жанр", 25, 2, sortColumn == 2, sortAscending).Alignment(Justify.Center),
            CreateSortableColumn("Год", 6, 3, sortColumn == 3, sortAscending).Alignment(Justify.Center),
            CreateSortableColumn("ISBN", 15, 4, sortColumn == 4, sortAscending).Alignment(Justify.Center),
            CreateSortableColumn("Оценка", 7, 5, sortColumn == 5, sortAscending).Alignment(Justify.Center),
            CreateSortableColumn("Обложка", 20, 6, sortColumn == 6, sortAscending).Alignment(Justify.Center)
        );

        for (int i = 0; i < filteredBooks.Count; i++)
        {
            if (i > 0) table.AddEmptyRow();
            var book = filteredBooks[i];
            string displayTitle = book.Title == "__NOT_STATED__" ? "not stated" : Truncate(book.Title, 25);
            string displayAuthor = book.Author == "__NOT_STATED__" ? "not stated" : Truncate(book.Author, 15);
            string displayGenre = book.Genre == "__NOT_STATED__" ? "not stated" : Truncate(book.Genre, 25);
            string displayYear = book.Year == -1 ? "not stated" : book.Year.ToString();
            string displayISBN = book.ISBN == "__NOT_STATED__" ? "not stated" : Truncate(book.ISBN, 15);
            string displayRating = book.Rating == -1 ? "not stated" : book.Rating.ToString();
            string displayCover = !string.IsNullOrEmpty(book.CoverUrl) && File.Exists(book.CoverUrl)
                ? $"[link={book.CoverUrl}]{Path.GetFileName(book.CoverUrl)}[/]"
                : (!string.IsNullOrEmpty(book.CoverUrl) ? "[yellow]Не найдена[/]" : "Без обложки");

            string titleMarkup = i == selectedIndex ? $"[green]-> {displayTitle}[/]" : displayTitle;
            table.AddRow(
                new Markup(titleMarkup),
                new Markup(displayAuthor),
                new Markup(displayGenre),
                new Markup(displayYear),
                new Markup(displayISBN),
                new Markup(displayRating),
                new Markup(displayCover)
            );
        }
        
        
        
        
        

        AnsiConsole.Write(table);

        var keyInfo = Console.ReadKey(true);
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
                filterText = ApplyFilter(books);
                selectedIndex = 0;
                break;
            default:
                if (keyInfo.KeyChar >= '1' && keyInfo.KeyChar <= '7')
                {
                    int column = keyInfo.KeyChar - '1';
                    if (sortColumn == column) sortAscending = !sortAscending;
                    else { sortColumn = column; sortAscending = true; }
                }
                break;
        }
    }
}


private static TableColumn CreateSortableColumn(string header, int width, int columnIndex, bool isSorted, bool ascending)
{
    string headerText = $"[bold blue]{header}[/]";
    if (isSorted)
    {
        headerText += ascending ? "[red] Dw[/]" : "[red] Up[/]"; // Индикация направления сортировки (стрелка вверх/вниз)
    }

  
    return new TableColumn(new Markup(headerText)).Width(width);
}



private static string Truncate(string value, int maxLength) =>
    value.Length > maxLength ? value.Substring(0, maxLength - 3) + "..." : value;

private static string ApplyFilter(List<Book> books)
{
    Console.WriteLine(new string('\n', 50)); Console.Clear();  
    AnsiConsole.MarkupLine("[bold blue]Введите текст для фильтрации (по любому полю, Esc для отмены):[/]");
    AnsiConsole.MarkupLine("[grey]Фильтрация чувствительна к регистру. Нажмите Enter для применения.[/]");
    string filterText = "";
    while (true)
    {
        var keyInfo = Console.ReadKey(true);
        if (keyInfo.Key == ConsoleKey.Escape) return "";
        if (keyInfo.Key == ConsoleKey.Enter) return filterText;
        if (keyInfo.Key == ConsoleKey.Backspace && filterText.Length > 0)
        {
            filterText = filterText.Substring(0, filterText.Length - 1);
            Console.WriteLine(new string('\n', 50)); Console.Clear();  
            AnsiConsole.MarkupLine("[bold blue]Введите текст для фильтрации (по любому полю, Esc для отмены):[/]");
            AnsiConsole.MarkupLine("[grey]{0}[/]", filterText);
        }
        else if (char.IsLetterOrDigit(keyInfo.KeyChar) || char.IsWhiteSpace(keyInfo.KeyChar))
        {
            filterText += keyInfo.KeyChar;
            Console.WriteLine(new string('\n', 50)); Console.Clear();  
            AnsiConsole.MarkupLine("[bold blue]Введите текст для фильтрации (по любому полю, Esc для отмены):[/]");
            AnsiConsole.MarkupLine("[grey]{0}[/]", filterText);
        }
    }
}

private static IEnumerable<Book> FilterBooks(List<Book> books, string filterText)
{
    if (string.IsNullOrEmpty(filterText)) return books;

    // Удаляем тире из строки поиска и из ISBN для сравнения
    string normalizedFilterText = filterText.Replace("-", "");
    return books.Where(b =>
        (b.Title != "__NOT_STATED__" && b.Title.Contains(filterText, StringComparison.OrdinalIgnoreCase)) ||
        (b.Author != "__NOT_STATED__" && b.Author.Contains(filterText, StringComparison.OrdinalIgnoreCase)) ||
        (b.Genre != "__NOT_STATED__" && b.Genre.Contains(filterText, StringComparison.OrdinalIgnoreCase)) ||
        (b.ISBN != "__NOT_STATED__" && b.ISBN.Replace("-", "").Contains(normalizedFilterText, StringComparison.OrdinalIgnoreCase)) ||
        (b.Year != -1 && b.Year.ToString().Contains(filterText)) ||
        (b.Rating != -1 && b.Rating.ToString().Contains(filterText)) ||
        (!string.IsNullOrEmpty(b.CoverUrl) && Path.GetFileName(b.CoverUrl).Contains(filterText, StringComparison.OrdinalIgnoreCase))
    );
}

private static List<Book> SortBooks(List<Book> books, int column, bool ascending)
{
    switch (column)
    {
        case 0: return ascending ? books.OrderBy(b => b.Title == "__NOT_STATED__" ? "" : b.Title).ToList() 
                                : books.OrderByDescending(b => b.Title == "__NOT_STATED__" ? "" : b.Title).ToList();
        case 1: return ascending ? books.OrderBy(b => b.Author == "__NOT_STATED__" ? "" : b.Author).ToList() 
                                : books.OrderByDescending(b => b.Author == "__NOT_STATED__" ? "" : b.Author).ToList();
        case 2: return ascending ? books.OrderBy(b => b.Genre == "__NOT_STATED__" ? "" : b.Genre).ToList() 
                                : books.OrderByDescending(b => b.Genre == "__NOT_STATED__" ? "" : b.Genre).ToList();
        case 3: return ascending ? books.OrderBy(b => b.Year == -1 ? int.MinValue : b.Year).ToList() 
                                : books.OrderByDescending(b => b.Year == -1 ? int.MinValue : b.Year).ToList();
        case 4: return ascending ? books.OrderBy(b => b.ISBN == "__NOT_STATED__" ? "" : b.ISBN).ToList() 
                                : books.OrderByDescending(b => b.ISBN == "__NOT_STATED__" ? "" : b.ISBN).ToList();
        case 5: return ascending ? books.OrderBy(b => b.Rating == -1 ? int.MinValue : b.Rating).ToList() 
                                : books.OrderByDescending(b => b.Rating == -1 ? int.MinValue : b.Rating).ToList();
        case 6: return ascending ? books.OrderBy(b => string.IsNullOrEmpty(b.CoverUrl) || !File.Exists(b.CoverUrl) ? 1 : 0).ToList() 
                                : books.OrderByDescending(b => string.IsNullOrEmpty(b.CoverUrl) || !File.Exists(b.CoverUrl) ? 1 : 0).ToList();
        default: return books;
    }
}

private static void DisplayCoverForSelectedBook(List<Book> books, int selectedIndex)
{
    if (selectedIndex < 0 || selectedIndex >= books.Count) return;
    var book = books[selectedIndex];
    Console.WriteLine(new string('\n', 50)); Console.Clear();  
    if (!string.IsNullOrEmpty(book.CoverUrl) && File.Exists(book.CoverUrl))
    {
        AnsiConsole.MarkupLine($"[bold blue]Обложка для \"{book.Title}\":[/]");
        var image = new CanvasImage(book.CoverUrl).MaxWidth(20);
        AnsiConsole.Write(image);
    }
    else
    {
        return;
    }
    AnsiConsole.MarkupLine("[grey]Нажмите Esc для возврата к списку книг...[/]");
    while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }
}










private static void DisplayCalendar(List<Book> books, string text = "=== Календарь книг ===")
{
    if (books == null || !books.Any())
    {
        AnsiConsole.MarkupLine("[red]Список книг пуст.[/]");
        return;
    }

    var years = books.Where(b => b.Year != -1).Select(b => b.Year).Distinct().OrderByDescending(y => y).ToList();
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
        Console.WriteLine(new string('\n', 50)); Console.Clear();  
    
        var table = new Table()
            .Border(TableBorder.Rounded)
         
            .Alignment(Justify.Left)
            .Title($"[bold blue]{text}[/]") // Исправляем синтаксис Title
            .Caption("[grey]Стрелки для навигации, Enter для книг, Esc для выхода[/]")
            .AddColumn(new TableColumn("Год").Width(20).Alignment(Justify.Left));

        var yearCounts = years.ToDictionary(y => y, y => books.Count(b => b.Year == y));
        int maxCount = yearCounts.Values.Max();

        for (int i = 0; i < years.Count; i++)
        {
            int year = years[i];
            int count = yearCounts[year];
            int intensity = 128 + (127 * count / maxCount); // От 128 до 255
            string color = $"#{intensity:X2}{intensity:X2}{intensity:X2}"; // Формат #RRGGBB
            string yearMarkup = i == selectedIndex 
                ? $"[green]-> [{color}]{year} ({count} книг)[/][/] "
                : $"[{color}]{year} ({count} книг)[/]";
            table.AddRow(new Markup(yearMarkup));
        }

        AnsiConsole.Write(table);

        var keyInfo = Console.ReadKey(true);
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


private static void DisplayBooksForYear(List<Book> books, int year)
{
    Console.WriteLine(new string('\n', 50)); Console.Clear();  
    var booksInYear = books.Where(b => b.Year == year).ToList();
    if (!booksInYear.Any())
    {
        AnsiConsole.MarkupLine("[red]Книги за этот год не найдены.[/]");
        AnsiConsole.MarkupLine("[grey]Нажмите Esc для возврата к календарю...[/]");
        while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }
        return;
    }

    // Создаём красивую таблицу для отображения книг
    var table = new Table()
        .Border(TableBorder.Rounded) // Красивые закруглённые границы
      
        .Alignment(Justify.Center) // Центрируем таблицу
        .Title($"[bold blue]Книги за {year} год[/]") // Исправляем синтаксис Title
        .Caption("[grey]Нажмите Esc для возврата к календарю[/]");

    // Добавляем столбцы с фиксированными ширинами и выравниванием
    table.AddColumns(
        new TableColumn("Название").Width(25).Alignment(Justify.Center),
        new TableColumn("Автор").Width(15).Alignment(Justify.Center),
        new TableColumn("Жанр").Width(25).Alignment(Justify.Center),
        new TableColumn("Год").Width(6).Alignment(Justify.Center),
        new TableColumn("ISBN").Width(15).Alignment(Justify.Center),
        new TableColumn("Оценка").Width(7).Alignment(Justify.Center),
        new TableColumn("Обложка").Width(20).Alignment(Justify.Center)
    );

    // Добавляем строки с данными книг
    for (int i = 0; i < booksInYear.Count; i++)
    {
        if (i > 0)
        {
            table.AddEmptyRow(); // Добавляем пустую строку как разделитель
        }

        var book = booksInYear[i];
        string displayTitle = book.Title == "__NOT_STATED__" ? "not stated" 
            : (book.Title.Length > 25 ? book.Title.Substring(0, 22) + "..." : book.Title);
        string displayAuthor = book.Author == "__NOT_STATED__" ? "not stated" 
            : (book.Author.Length > 15 ? book.Author.Substring(0, 12) + "..." : book.Author);
        string displayGenre = book.Genre == "__NOT_STATED__" ? "not stated" 
            : (book.Genre.Length > 25 ? book.Genre.Substring(0, 22) + "..." : book.Genre);
        string displayYear = book.Year == -1 ? "not stated" : book.Year.ToString();
        string displayISBN = book.ISBN == "__NOT_STATED__" ? "not stated" 
            : (book.ISBN.Length > 15 ? book.ISBN.Substring(0, 12) + "..." : book.ISBN);
        string displayRating = book.Rating == -1 ? "not stated" : book.Rating.ToString();
        string displayCover = !string.IsNullOrEmpty(book.CoverUrl) && File.Exists(book.CoverUrl)
            ? $"[link={book.CoverUrl}]{Path.GetFileName(book.CoverUrl)}[/]"
            : (!string.IsNullOrEmpty(book.CoverUrl) ? "[yellow]Не найдена[/]" : "Без обложки");

        table.AddRow(
            new Markup(displayTitle),
            new Markup(displayAuthor),
            new Markup(displayGenre),
            new Markup(displayYear),
            new Markup(displayISBN),
            new Markup(displayRating),
            new Markup(displayCover)
        );
    }

    // Выводим таблицу
    AnsiConsole.Write(table);
  
    while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }
}


    

private static void DisplayBooksInConsole(List<Book> books, string text = "\n=== Список книг в библиотеке ===")
{
    if (books == null || !books.Any())
    {
        Console.WriteLine("Список книг пуст.");
        return;
    }

    // Заголовок таблицы
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(text);
    Console.ResetColor();

    // Фиксированные ширины для столбцов (с учётом ограничений и выравнивания)
    int titleWidth = 25;  // Ограничение до 30 символов для Название
    int authorWidth = 20; // Ограничение до 20 символов для Автор
    int genreWidth = 30;  // Ограничение до 30 символов для Жанр
    int yearWidth = 4;    // Фиксированная ширина для Год (4 символа)
    int isbnWidth = 15;   // Ограничение до 20 символов для ISBN
    int ratingWidth = 6;  // Фиксированная ширина для Оценка (7 символов)

    // Вывод заголовков таблицы
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(
        $"| {"Название".PadRight(titleWidth)} | {"Автор".PadRight(authorWidth)} | " +
        $"{"Жанр".PadRight(genreWidth)} | {"Год".PadRight(yearWidth)} | " +
        $"{"ISBN".PadRight(isbnWidth)} | {"Оценка".PadRight(ratingWidth)} |");
    Console.ResetColor();

    // Разделитель
    Console.WriteLine(new string('-', titleWidth + authorWidth + genreWidth + yearWidth + isbnWidth + ratingWidth + 7));

    // Вывод каждой книги
    foreach (var book in books)
    {
        string displayTitle = book.Title == "__NOT_STATED__" ? "not stated" 
            : (book.Title.Length > titleWidth ? book.Title.Substring(0, titleWidth - 3) + "..." : book.Title.PadRight(titleWidth));
        string displayAuthor = book.Author == "__NOT_STATED__" ? "not stated" 
            : (book.Author.Length > authorWidth ? book.Author.Substring(0, authorWidth - 3) + "..." : book.Author.PadRight(authorWidth));
        string displayGenre = book.Genre == "__NOT_STATED__" ? "not stated" 
            : (book.Genre.Length > genreWidth ? book.Genre.Substring(0, genreWidth - 3) + "..." : book.Genre.PadRight(genreWidth));
        string displayYear = book.Year == -1 ? "not stated".PadRight(yearWidth).Substring(0, yearWidth - 3) + "..." : book.Year.ToString().PadRight(yearWidth).Substring(0, yearWidth);
        
        string displayISBN = book.ISBN == "__NOT_STATED__" ? "not stated" 
            : (book.ISBN.Length > isbnWidth ? book.ISBN.Substring(0, isbnWidth - 3) + "..." : book.ISBN.PadRight(isbnWidth));
        string displayRating = book.Rating == -1 ? "not stated".PadRight(ratingWidth).Substring(0, ratingWidth - 3) + "..." : book.Rating.ToString().PadRight(ratingWidth).Substring(0, ratingWidth);

        Console.WriteLine(
            $"| {displayTitle} | {displayAuthor} | " +
            $"{displayGenre} | {displayYear} | " +
            $"{displayISBN} | {displayRating} |");
    }

    // Завершающий разделитель
    Console.WriteLine(new string('-', titleWidth + authorWidth + genreWidth + yearWidth + isbnWidth + ratingWidth + 7));
  
}
    
    

private static void ShowRecs()
    {
        // Фильтруем книги с рейтингом >= 7 (или другое значение, по заданию)
        List<Book> recommendedBooks = new List<Book>();
        foreach (var book in library)
        {
            if (book.Rating >= 7 && book.Rating != -1) // Игнорируем книги с рейтингом -1 (not stated)
            {
                recommendedBooks.Add(book);
            }
        }

        // Если есть рекомендуемые книги, выводим их
        if (recommendedBooks.Count > 0)
        {
            Console.WriteLine(new string('\n', 50)); Console.Clear();  
            library.DisplayPartBooks(recommendedBooks, DisplayBooksInConsole, "Рекомендуемые книги (рейтинг 7 и выше):");
            Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
            Console.ReadKey(true);
        }
        else
        {
            Console.WriteLine(new string('\n', 50)); Console.Clear();  
            Console.WriteLine("Нет рекомендаций: книги с рейтингом 7 и выше не найдены.");
            Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
            Console.ReadKey(true);
        }
    }

    private static void DeleteBook()
    {
        // Инициализируем список для найденных книг
        List<Book> matchingBooks = new List<Book>();
        string searchQuery = "";

        while (true)
        {
            // Очищаем консоль для перерисовки
            Console.WriteLine(new string('\n', 50)); Console.Clear();  

            // Выводим форму поиска
            Console.Write($"Поиск книги для удаления, введите часть любого поля (пробел это отобразить всех): {searchQuery}");
            Console.WriteLine();
            Console.WriteLine("Нажмите ENTER для выбора, ESC для выхода");

            // Показываем найденные книги через DisplayPartBooks
            if (!string.IsNullOrEmpty(searchQuery))
            {
                matchingBooks.Clear();
                foreach (var book in library)
                {
                    if (book.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                        book.Author.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                        book.Genre.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                        book.ISBN.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                        (book.Year != -1 && book.Year.ToString().Contains(searchQuery)) ||
                        (book.Rating != -1 && book.Rating.ToString().Contains(searchQuery)))
                    {
                        matchingBooks.Add(book);
                    }
                }

                if (matchingBooks.Count > 0)
                {
                    library.DisplayPartBooks(matchingBooks, DisplayBooksInConsole, "\nНайденные книги:");
                }
                else
                {
                    Console.WriteLine("\nКниги не найдены.");
                }
            }

            // Считываем нажатие клавиши
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter && !string.IsNullOrEmpty(searchQuery)) // Выбор книги
            {
                if (matchingBooks.Count == 1)
                {
                    // Если найдена одна книга, сразу удаляем
                    DeleteSelectedBook(matchingBooks[0]);
                    break;
                }
                else if (matchingBooks.Count > 1)
                {
                    // Если найдено несколько книг, показываем выбор
                    Book selectedBook = SelectBookFromList(matchingBooks);
                    if (selectedBook != null)
                    {
                        DeleteSelectedBook(selectedBook);
                        break;
                    }
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
                Console.ReadKey(true);
                break;
            }
            else if (key.Key == ConsoleKey.Backspace && searchQuery.Length > 0) // Удаление символа
            {
                searchQuery = searchQuery.Substring(0, searchQuery.Length - 1);
            }
            else if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar) || key.KeyChar == '-') // Добавление символа
            {
                searchQuery += key.KeyChar;
            }
        }
    }

    // Вспомогательная функция для удаления выбранной книги
    private static void DeleteSelectedBook(Book bookToDelete)
    {
        library.RemoveBook(bookToDelete);
        library.SaveBooks(); // Сохраняем изменения в файл
        Console.WriteLine(new string('\n', 50)); Console.Clear();  
        Console.WriteLine("Книга удалена. Нажмите любую клавишу для возврата в меню...");
        Console.ReadKey(true);
    }

    private static void ChangeFilePath()
    {
        while (true)
        {
            Console.WriteLine("Введите новый путь к файлу:");
            string newPath = Console.ReadLine();

            if (TryLoadOrCreateFile(newPath))
            {
                library.SaveBooks();
                library = new LibraryManager(newPath);
                Console.WriteLine("Путь обновлён.");
                break;
            }
            Console.WriteLine("Ошибка: Невозможно открыть или создать файл. Попробуйте снова.");
        }
    }

  


    
    
private static void ImportFromCsv()
{
    // Сохраняем текущий файл books.json как резервную копию
    if (File.Exists(library?.FilePath))
    {
        string backupPath = $"{library.FilePath}.bak";
        try
        {
            File.Copy(library.FilePath, backupPath, true);
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
        // Читаем файл с кодировкой UTF-8 (с BOM для совместимости с Excel)
        string[] lines = File.ReadAllLines(csvPath, Encoding.UTF8);
        if (lines.Length == 0)
        {
            Console.WriteLine("CSV-файл пуст. Нажмите любую клавишу...");
            Console.ReadKey(true);
            return;
        }

        // Проверяем наличие заголовка (опционально, предполагаем ; как разделитель)
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

            // Разбиваем строку по точке с запятой
            string[] parts = line.Split(';');

            // Пропускаем, если полей больше 6
            if (parts.Length > 6)
            {
                Console.WriteLine($"Строка {i + 1} пропущена: слишком много полей (больше 6).");
                continue;
            }

            // Пропускаем, если полей меньше 6 и есть ошибки в данных
            if (parts.Length < 6)
            {
                bool hasInvalidData = false;
                for (int j = 3; j < parts.Length && j < 6; j++) // Проверяем Year, ISBN, Rating
                {
                    if (j == 3 && !int.TryParse(parts[j].Trim(), out _)) // Year
                    {
                        hasInvalidData = true;
                        break;
                    }
                    if (j == 5 && !int.TryParse(parts[j].Trim(), out _)) // Rating
                    {
                        hasInvalidData = true;
                        break;
                    }
                }
                if (hasInvalidData)
                {
                    Console.WriteLine($"Строка {i + 1} пропущена: некорректные данные (например, буквы в Year или Rating).");
                    continue;
                }
            }

            // Отладочное сообщение для проверки парсинга
            Console.WriteLine($"Строка {i + 1} распарсена: {string.Join(", ", parts)}");

            // Заполняем поля, используя первые доступные значения, остальное — дефолтные
            string title = parts.Length > 0 ? (string.IsNullOrEmpty(parts[0].Trim()) ? "__NOT_STATED__" : parts[0].Trim()) : "__NOT_STATED__";
            string author = parts.Length > 1 ? (string.IsNullOrEmpty(parts[1].Trim()) ? "__NOT_STATED__" : parts[1].Trim()) : "__NOT_STATED__";
            string genre = parts.Length > 2 ? (string.IsNullOrEmpty(parts[2].Trim()) ? "__NOT_STATED__" : parts[2].Trim()) : "__NOT_STATED__";
            int year = parts.Length > 3 && int.TryParse(parts[3].Trim(), out int y) ? y : -1;
            string isbn = parts.Length > 4 ? (string.IsNullOrEmpty(parts[4].Trim()) ? "__NOT_STATED__" : parts[4].Trim()) : "__NOT_STATED__";
            int rating = parts.Length > 5 && int.TryParse(parts[5].Trim(), out int r) ? r : -1;

            var newBook = new Book(title, author, genre, year, isbn, rating);

            // Проверяем, есть ли книга с таким ISBN в библиотеке (не добавляем дубликаты, игнорируя тире)
            if (!library.Any(b => b.ISBN.Replace("-", "").Trim() == newBook.ISBN.Replace("-", "").Trim() && b.ISBN != "__NOT_STATED__"))
            {
                importedBooks.Add(newBook);
            }
            else
            {
                Console.WriteLine($"Строка {i + 1} пропущена: книга с ISBN {newBook.ISBN} уже существует в библиотеке.");
            }
        }

        // Добавляем импортированные книги в библиотеку
        if (library != null && importedBooks.Any())
        {
            foreach (var book in importedBooks)
            {
                library.AddBook(book);
            }
            library.SaveBooks();
            Console.WriteLine($"Импортировано {importedBooks.Count} новых книг из CSV. Нажмите любую клавишу...");
        }
        else if (!importedBooks.Any())
        {
            Console.WriteLine("Нет новых книг для импорта, существующие книги пропущены. Нажмите любую клавишу...");
        }
        Console.ReadKey(true);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка импорта из CSV: {ex.Message}. Нажмите любую клавишу...");
        Console.ReadKey(true);
    }
}

// Оставляем текущий метод без изменений, так как он больше не используется
private static string EnsureCsvExtension(string path)
{
    if (!Path.HasExtension(path) || Path.GetExtension(path).ToLower() != ".csv")
    {
        return Path.ChangeExtension(path, ".csv");
    }
    return path;
}
    
    
    private static string[] SplitCsvLine(string line, char separator = '\t')
    {
        List<string> parts = new List<string>();
        bool inQuotes = false;
        string currentPart = "";

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"' && (i == 0 || line[i - 1] != '\\'))
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (line[i] == separator && !inQuotes)
            {
                parts.Add(currentPart);
                currentPart = "";
            }
            else
            {
                currentPart += line[i];
            }
        }
        parts.Add(currentPart); // Добавляем последнюю часть

        return parts.Select(p => p.Trim('"').Replace("\"\"", "\"") ?? "").ToArray(); // Гарантируем, что элементы не null
    }


    private static void ExportToJson()
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
            if (library != null)
            {
                List<Book> booksToExport = library.ToList(); // Конвертируем IEnumerable в List для сериализации
                string json = JsonSerializer.Serialize(booksToExport, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonPath, json, Encoding.UTF8); // Используем UTF-8 для совместимости
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

    private static void ExportToCsv()
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
            if (library != null)
            {
                using (var writer = new StreamWriter(csvPath, false, new UTF8Encoding(true))) // UTF-8 с BOM для Excel
                {
                    writer.WriteLine("Title;Author;Genre;Year;ISBN;Rating"); // Заголовок с табуляцией
                    foreach (var book in library)
                    {
                        string title = book.Title == "__NOT_STATED__" ? "" : EscapeCsv(book.Title);
                        string author = book.Author == "__NOT_STATED__" ? "" : EscapeCsv(book.Author);
                        string genre = book.Genre == "__NOT_STATED__" ? "" : EscapeCsv(book.Genre);
                        string year = book.Year == -1 ? "" : book.Year.ToString();
                        string isbn = book.ISBN == "__NOT_STATED__" ? "" : EscapeCsv(book.ISBN);
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

    // Вспомогательные методы остаются без изменений (TryLoadOrCreateFile, EnsureJsonExtension, EnsureCsvExtension, SplitCsvLine, EscapeCsv, IsValidISBN, AddBook, EditBook, SelectBookFromList, EditSelectedBook)
    // Они уже корректно реализованы в твоём коде, поэтому я их не дублирую здесь.

    private static bool TryLoadOrCreateFile(string path)
    {
        if (string.IsNullOrEmpty(path)) return false; // Дополнительная проверка на null/пустую строку
        try
        {
            if (File.Exists(path))
            {
                return true;
            }
            else
            {
                string directory = Path.GetDirectoryName(path);
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

    private static string EnsureJsonExtension(string path)
    {
        if (!Path.HasExtension(path) || Path.GetExtension(path).ToLower() != ".json")
        {
            return Path.ChangeExtension(path, ".json");
        }
        return path;
    }

 
    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains("\t") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    private static bool IsValidISBN(string isbn)
    {
        isbn = isbn.Replace("-", "").Replace(" ", ""); // Убираем дефисы и пробелы

        if (string.IsNullOrEmpty(isbn)) return false;

        if (isbn.Length == 10) // ISBN-10
        {
            // Проверяем, что первые 9 символов — цифры, последний — цифра или 'X'
            if (!isbn.Substring(0, 9).All(char.IsDigit)) return false;
            if (!char.IsDigit(isbn[9]) && isbn[9] != 'X') return false;

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += (10 - i) * (isbn[i] - '0');
            }
            int checkDigit = isbn[9] == 'X' ? 10 : (isbn[9] - '0');
            return (sum + checkDigit) % 11 == 0;
        }
        else if (isbn.Length == 13) // ISBN-13
        {
            // Проверяем, что все 13 символов — цифры
            if (!isbn.All(char.IsDigit)) return false;

            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = isbn[i] - '0';
                sum += (i % 2 == 0 ? 1 : 3) * digit;
            }
            int checkDigit = isbn[12] - '0';
            return (10 - (sum % 10)) % 10 == checkDigit;
        }

        return false; // Неверная длина
    }

    private static void AddBook()
    {
        Book newBook = new Book(); // Создаём новую книгу с значениями по умолчанию
        string[] fields = { "Title", "Author", "Genre", "Year", "ISBN", "Rating" }; // Поля для ввода
        string[] values = { newBook.Title, newBook.Author, newBook.Genre, newBook.Year.ToString(), newBook.ISBN, newBook.Rating.ToString() };
        int currentField = 0; // Текущее поле ввода
        bool isISBNInvalid = false; // Флаг для некорректного ISBN

        // Инициализируем значения: если "not stated" или "__NOT_STATED__", заменяем на пустую строку
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == "__NOT_STATED__" || values[i] == "not stated")
            {
                values[i] = "";
            }
            else if (i >= 3) // Для Year и Rating (числовые поля)
            {
                values[i] = values[i] == "-1" ? "" : values[i];
            }
        }

        while (true)
        {
            // Очищаем консоль для перерисовки
            Console.WriteLine(new string('\n', 50)); Console.Clear();  

            // Выводим все поля
            for (int i = 0; i < fields.Length; i++)
            {
                Console.Write($"Введите {fields[i]} книги: ");
                if (i == currentField)
                {
                    // Активное поле — зелёное, если ISBN корректен или не проверяется
                    if (i == 4 && !string.IsNullOrEmpty(values[i]) && values[i] != "not stated" && isISBNInvalid)
                    {
                        Console.ForegroundColor = ConsoleColor.Red; // Красное, если ISBN неверный
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green; // Зелёное для остальных случаев
                    }
                    Console.Write(values[i] == "" ? "not stated" : values[i]); // Показываем "not stated", если поле пустое
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(values[i] == "" ? "not stated" : values[i]); // Неактивное поле — стандартный цвет
                }
                Console.WriteLine();
            }
            Console.WriteLine("Нажмите ENTER для выхода");

            // Считываем нажатие клавиши
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter) // Выход
            {
                // Проверяем, остались ли все поля "not stated" или пустыми
                bool allNotStated = true;
                for (int i = 0; i < values.Length; i++)
                {
                    if (i < 3 || i == 4) // Строки (Title, Author, Genre, ISBN)
                    {
                        if (values[i] != "" && values[i] != "not stated")
                        {
                            allNotStated = false;
                            break;
                        }
                    }
                    else // Числовые поля (Year, Rating)
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
                    Console.WriteLine("Книга не добавлена: все поля пустые или по умолчанию. Нажмите любую клавишу...");
                    Console.ReadKey(true);
                    break;
                }

                // Проверяем ISBN, если он не "not stated"
                if (values[4] != "" && values[4] != "not stated" && !IsValidISBN(values[4]))
                {
                    Console.WriteLine("Неверный ISBN. Исправьте перед сохранением. Нажмите любую клавишу...");
                    Console.ReadKey(true);
                    continue; // Возвращаемся к вводу
                }
                
                // Проверяем Genre, если он не "not stated"
                if (values[2] != "" && values[2] != "not stated" && !validGenre.Contains(values[2]))
                {
                    Console.WriteLine("Неверный Genre. Используйте один жанр из списка. Нажмите любую клавишу...");
                    foreach (var i in validGenre)
                    {
                        Console.Write(i + (i == validGenre[validGenre.Length - 1] ? "" : ", "));
                    }
                    Console.ReadKey(true);
                    continue; // Возвращаемся к вводу
                }

                // Преобразуем значения в нужные типы для Book, возвращаем "not stated" для пустых строк
                newBook.Title = values[0] == "" ? "__NOT_STATED__" : values[0];
                newBook.Author = values[1] == "" ? "__NOT_STATED__" : values[1];
                newBook.Genre = values[2] == "" ? "__NOT_STATED__" : values[2];
                newBook.Year = int.TryParse(values[3], out int year) ? year : -1;
                newBook.ISBN = values[4] == "" ? "__NOT_STATED__" : values[4];
                newBook.Rating = int.TryParse(values[5], out int rating) ? rating : -1;
                newBook.CoverUrl = ""; // Обложка по умолчанию пустая (заполняется через ISBN, если доступна)

                library.AddBook(newBook);
                Console.WriteLine("Книга добавлена. Нажмите любую клавишу...");
                Console.ReadKey(true);
                break;
            }
            else if (key.Key == ConsoleKey.UpArrow) // Переключение вверх
            {
                currentField = (currentField - 1 + fields.Length) % fields.Length;
            }
            else if (key.Key == ConsoleKey.DownArrow) // Переключение вниз
            {
                currentField = (currentField + 1) % fields.Length;
            }
            else if (key.Key == ConsoleKey.Backspace && values[currentField].Length > 0) // Удаление символа
            {
                values[currentField] = values[currentField].Substring(0, values[currentField].Length - 1);
                if (currentField == 4) isISBNInvalid = false; // Сбрасываем флаг, если ISBN изменён
            }
            else if (currentField >= 3 && char.IsDigit(key.KeyChar)) // Только цифры для Year и Rating
            {
                if (values[currentField] == "") values[currentField] = ""; // Убеждаемся, что не добавляем к "not stated"
                values[currentField] += key.KeyChar;
            }
            else if (currentField < 3 || currentField == 4) // Буквы, цифры, пробелы, дефис для Title, Author, Genre, ISBN
            {
                if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar) || key.KeyChar == '-')
                {
                    if (values[currentField] == "") values[currentField] = ""; // Убеждаемся, что не добавляем к "not stated"
                    values[currentField] += key.KeyChar;
                    if (currentField == 4) isISBNInvalid = false; // Сбрасываем флаг, если ISBN изменён
                }
            }
        }

        Console.WriteLine(new string('\n', 50)); Console.Clear();  
    }

    private static void EditBook()
    {
        // Инициализируем список для найденных книг
        List<Book> matchingBooks = new List<Book>();
        string searchQuery = "";

        while (true)
        {
            // Очищаем консоль для перерисовки
            Console.WriteLine(new string('\n', 50)); Console.Clear();  

            // Выводим форму поиска
            Console.Write($"Поиск книги для удаления, введите часть любого поля (пробел это отобразить всех): {searchQuery}");
            Console.WriteLine();
            Console.WriteLine("Нажмите ENTER для выбора, ESC для выхода");


            // Показываем найденные книги через DisplayPartBooks
            if (!string.IsNullOrEmpty(searchQuery))
            {
                matchingBooks.Clear();
                foreach (var book in library)
                {
                    if (book.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                        book.Author.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                        book.Genre.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                        book.ISBN.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                        (book.Year != -1 && book.Year.ToString().Contains(searchQuery)) ||
                        (book.Rating != -1 && book.Rating.ToString().Contains(searchQuery)))
                    {
                        matchingBooks.Add(book);
                    }
                }

                if (matchingBooks.Count > 0)
                {
                    library.DisplayPartBooks(matchingBooks, DisplayBooksInConsole, "\nНайденные книги:");
                }
                else
                {
                    Console.WriteLine("\nКниги не найдены.");
                }
            }

            // Считываем нажатие клавиши
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter && !string.IsNullOrEmpty(searchQuery)) // Выбор книги
            {
                if (matchingBooks.Count == 1)
                {
                    // Если найдена одна книга, сразу начинаем редактировать
                    EditSelectedBook(matchingBooks[0]);
                    break;
                }
                else if (matchingBooks.Count > 1)
                {
                    // Если найдено несколько книг, показываем выбор
                    Book selectedBook = SelectBookFromList(matchingBooks);
                    if (selectedBook != null)
                    {
                        EditSelectedBook(selectedBook);
                        break;
                    }
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
                Console.ReadKey(true);
                break;
            }
            else if (key.Key == ConsoleKey.Backspace && searchQuery.Length > 0) // Удаление символа
            {
                searchQuery = searchQuery.Substring(0, searchQuery.Length - 1);
            }
            else if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar) || key.KeyChar == '-') // Добавление символа
            {
                searchQuery += key.KeyChar;
            }
        }
    }

    private static Book SelectBookFromList(List<Book> books)
    {
        int selectedIndex = 0;

        while (true)
        {
            Console.WriteLine(new string('\n', 50)); Console.Clear();  
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

            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter) // Подтверждение выбора
            {
                return books[selectedIndex];
            }
            else if (key.Key == ConsoleKey.Escape) // Отмена
            {
                Console.WriteLine("Выбор отменён. Нажмите любую клавишу...");
                Console.ReadKey(true);
                return null;
            }
            else if (key.Key == ConsoleKey.UpArrow) // Переключение вверх
            {
                selectedIndex = (selectedIndex - 1 + books.Count) % books.Count;
            }
            else if (key.Key == ConsoleKey.DownArrow) // Переключение вниз
            {
                selectedIndex = (selectedIndex + 1) % books.Count;
            }
        }
    }

    private static void EditSelectedBook(Book bookToEdit)
    {
        // Подготавливаем поля для редактирования
        string[] fields = { "Title", "Author", "Genre", "Year", "ISBN", "Rating" }; // Поля для ввода
        string[] values = {
            bookToEdit.Title == "__NOT_STATED__" ? "" : bookToEdit.Title,
            bookToEdit.Author == "__NOT_STATED__" ? "" : bookToEdit.Author,
            bookToEdit.Genre == "__NOT_STATED__" ? "" : bookToEdit.Genre,
            bookToEdit.Year.ToString(),
            bookToEdit.ISBN == "__NOT_STATED__" ? "" : bookToEdit.ISBN,
            bookToEdit.Rating.ToString()
        };
        int currentField = 0; // Текущее поле ввода
        bool isISBNInvalid = false; // Флаг для некорректного ISBN

        // Инициализируем значения: если "-1", заменяем на пустую строку для числовых полей
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == "__NOT_STATED__" || values[i] == "not stated")
            {
                values[i] = "";
            }
            else if (i >= 3) // Для Year и Rating
            {
                values[i] = values[i] == "-1" ? "" : values[i];
            }
        }

        while (true)
        {
            // Очищаем консоль для перерисовки
            Console.WriteLine(new string('\n', 50)); Console.Clear();  

            // Выводим все поля
            for (int i = 0; i < fields.Length; i++)
            {
                Console.Write($"Текущее {fields[i]}: ");
                if (i == currentField)
                {
                    // Активное поле — зелёное, если ISBN корректен или не проверяется
                    if (i == 4 && !string.IsNullOrEmpty(values[i]) && values[i] != "not stated" && isISBNInvalid)
                    {
                        Console.ForegroundColor = ConsoleColor.Red; // Красное, если ISBN неверный
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green; // Зелёное для остальных случаев
                    }
                    Console.Write(values[i] == "" ? "not stated" : values[i]); // Показываем "not stated", если поле пустое
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(values[i] == "" ? "not stated" : values[i]); // Неактивное поле — стандартный цвет
                }
                Console.WriteLine();
            }
            Console.WriteLine("Нажмите ENTER для сохранения и выхода");

            // Считываем нажатие клавиши
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter) // Сохранение и выход
            {
                // Проверяем ISBN, если он не "not stated"
                if (values[4] != "" && values[4] != "not stated" && !IsValidISBN(values[4]))
                {
                    Console.WriteLine("Неверный ISBN. Исправьте перед сохранением. Нажмите любую клавишу...");
                    Console.ReadKey(true);
                    continue; // Возвращаемся к вводу
                }
                // Проверяем Genre, если он не "not stated"
                if (values[2] != "" && values[2] != "not stated" && !validGenre.Contains(values[2]))
                {
                    Console.WriteLine("Неверный Genre. Используйте один жанр из списка. Нажмите любую клавишу...");
                    foreach (var i in validGenre)
                    {
                        Console.Write(i + (i == validGenre[validGenre.Length - 1] ? "" : ", "));
                    }
                    Console.ReadKey(true);
                    continue; // Возвращаемся к вводу
                }
                
                
                

                // Преобразуем значения в нужные типы для Book, возвращаем "not stated" для пустых строк
                bookToEdit.Title = values[0] == "" ? "__NOT_STATED__" : values[0];
                bookToEdit.Author = values[1] == "" ? "__NOT_STATED__" : values[1];
                bookToEdit.Genre = values[2] == "" ? "__NOT_STATED__" : values[2];
                bookToEdit.Year = int.TryParse(values[3], out int year) ? year : -1;
                bookToEdit.ISBN = values[4] == "" ? "__NOT_STATED__" : values[4];
                bookToEdit.Rating = int.TryParse(values[5], out int rating) ? rating : -1;
                bookToEdit.CoverUrl = bookToEdit.CoverUrl; // Сохраняем существующую обложку (или пустую, если нет)

                library.SaveBooks(); // Сохраняем изменения в файл
                Console.WriteLine("Книга отредактирована и сохранена.");
                Console.ReadKey(true);
                break;
            }
            else if (key.Key == ConsoleKey.UpArrow) // Переключение вверх
            {
                currentField = (currentField - 1 + fields.Length) % fields.Length;
            }
            else if (key.Key == ConsoleKey.DownArrow) // Переключение вниз
            {
                currentField = (currentField + 1) % fields.Length;
            }
            else if (key.Key == ConsoleKey.Backspace && values[currentField].Length > 0) // Удаление символа
            {
                values[currentField] = values[currentField].Substring(0, values[currentField].Length - 1);
                if (currentField == 4) isISBNInvalid = false; // Сбрасываем флаг, если ISBN изменён
            }
            else if (currentField >= 3 && char.IsDigit(key.KeyChar)) // Только цифры для Year и Rating
            {
                if (values[currentField] == "") values[currentField] = ""; // Убеждаемся, что не добавляем к "not stated"
                values[currentField] += key.KeyChar;
            }
            else if (currentField < 3 || currentField == 4) // Буквы, цифры, пробелы, дефис для Title, Author, Genre, ISBN
            {
                if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar) || key.KeyChar == '-')
                {
                    if (values[currentField] == "") values[currentField] = ""; // Убеждаемся, что не добавляем к "not stated"
                    values[currentField] += key.KeyChar;
                    if (currentField == 4) isISBNInvalid = false; // Сбрасываем флаг, если ISBN изменён
                }
            }
        }

        Console.WriteLine(new string('\n', 50)); Console.Clear();  
    }
}