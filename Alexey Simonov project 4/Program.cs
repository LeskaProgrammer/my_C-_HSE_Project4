using System;
using System.IO;

// Read Me
/// <summary>
///
///Используется формат хранения JSON в текстовом документе
///
///
///
///
/// 
/// </summary>



class Program
{
    private static LibraryManager library;

    static void Main(string[] args)
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

        while (true)
        {
            Console.WriteLine("\nМеню:");
            Console.WriteLine("1. Просмотреть книги");
            Console.WriteLine("2. Добавить книгу вручную");
            Console.WriteLine("3. Добавить книгу по ISBN (OpenLibrary)");
            Console.WriteLine("4. Редактировать книгу");
            Console.WriteLine("5. Удалить книгу");
            Console.WriteLine("6. Показать рекомендации");
            Console.WriteLine("7. Импорт из JSON/CSV");
            Console.WriteLine("8. Экспорт в JSON/CSV");
            Console.WriteLine("9. Изменить путь к файлу");
            Console.WriteLine("10. Выход");
            Console.Write("Выберите действие: ");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.Clear();
                    Console.WriteLine("Список книг:\n");
                    library.DisplayBooks(DisplayBooksInConsole); // Передаём метод для вывода
                    Console.WriteLine("\nНажмите для выхода");
                    Console.ReadKey();
                    Console.Clear();
                    break;
                case "2":
                    AddBook();
                    break; // Добавить книгу вручную (реализуй сам)
                case "3":
                    break; // Добавить по ISBN (OpenLibrary, B-side)
                case "4":
                    EditBook();
                    break; // Редактировать книгу (реализуй сам)
                case "5":
                    break; // Удалить книгу (реализуй сам)
                case "6":
                    break; // Показать рекомендации (B-side)
                case "7":
                    break; // Импорт (B-side)
                case "8":
                    break; // Экспорт (B-side)
                case "9":
                    ChangeFilePath();
                    break; // Изменить путь (реализован ниже)
                case "10":
                    library.SaveBooks();
                    Console.WriteLine("Изменения сохранены. До свидания!");
                    return;
                default:
                    Console.WriteLine("Неверный выбор.");
                    break;
            }
        }
    }

    // Метод для вывода списка книг в консоль (базовый формат)
    private static void DisplayBooksInConsole(List<Book> books)
    {

        
        
        foreach (var book in books)
        {
            Console.WriteLine($"Название: {book.Title}");
            Console.WriteLine($"Автор: {book.Author}");
            Console.WriteLine($"Жанр: {book.Genre}");
            Console.WriteLine($"Год издания: {(book.Year == -1 ? "not stated" : book.Year)}");
            Console.WriteLine($"ISBN: {book.ISBN}");
            Console.WriteLine($"Оценка: {(book.Rating == -1 ? "not stated" : book.Rating)}\n");
        }
        
       

    }

    private static bool TryLoadOrCreateFile(string path)
    {
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
        Console.Clear();

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

            // Преобразуем значения в нужные типы для Book, возвращаем "not stated" для пустых строк
            newBook.Title = values[0] == "" ? "__NOT_STATED__" : values[0];
            newBook.Author = values[1] == "" ? "__NOT_STATED__" : values[1];
            newBook.Genre = values[2] == "" ? "__NOT_STATED__" : values[2];
            newBook.Year = int.TryParse(values[3], out int year) ? year : -1;
            newBook.ISBN = values[4] == "" ? "__NOT_STATED__" : values[4];
            newBook.Rating = int.TryParse(values[5], out int rating) ? rating : -1;

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

    Console.Clear();
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


private static void EditBook()
{
    // Инициализируем список для найденных книг
    List<Book> matchingBooks = new List<Book>();
    string searchQuery = "";

    while (true)
    {
        // Очищаем консоль для перерисовки
        Console.Clear();

        // Выводим форму поиска
        Console.Write($"Поиск книги (введите часть любого поля, ENTER для выбора): {searchQuery}");
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
                Console.WriteLine("\nНайденные книги:");
                library.DisplayPartBooks(matchingBooks, DisplayBooksInConsole);
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

// Вспомогательная функция для выбора книги из списка (без изменений)
private static Book SelectBookFromList(List<Book> books)
{
    int selectedIndex = 0;

    while (true)
    {
        Console.Clear();
        Console.WriteLine("Выберите книгу для редактирования:");

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

// Вспомогательная функция для редактирования выбранной книги (без изменений, кроме проверки ISBN)
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
        Console.Clear();

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

            // Преобразуем значения в нужные типы для Book, возвращаем "not stated" для пустых строк
            bookToEdit.Title = values[0] == "" ? "__NOT_STATED__" : values[0];
            bookToEdit.Author = values[1] == "" ? "__NOT_STATED__" : values[1];
            bookToEdit.Genre = values[2] == "" ? "__NOT_STATED__" : values[2];
            bookToEdit.Year = int.TryParse(values[3], out int year) ? year : -1;
            bookToEdit.ISBN = values[4] == "" ? "__NOT_STATED__" : values[4];
            bookToEdit.Rating = int.TryParse(values[5], out int rating) ? rating : -1;

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

    Console.Clear();
}
  




}