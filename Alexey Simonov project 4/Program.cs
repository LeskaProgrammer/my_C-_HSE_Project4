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
                    library.DisplayBooks(DisplayBooksInConsole); // Передаём метод для вывода
                    break;
                case "2":
                    AddBook();
                    break; // Добавить книгу вручную (реализуй сам)
                case "3":
                    break; // Добавить по ISBN (OpenLibrary, B-side)
                case "4":
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

        Console.WriteLine("Список книг:");
        Console.Clear();
        
        foreach (var book in books)
        {
            Console.WriteLine($"Название: {book.Title}");
            Console.WriteLine($"Автор: {book.Author}");
            Console.WriteLine($"Жанр: {book.Genre}");
            Console.WriteLine($"Год издания: {(book.Year == -1 ? "not stated" : book.Year)}");
            Console.WriteLine($"ISBN: {book.ISBN}");
            Console.WriteLine($"Оценка: {(book.Rating == -1 ? "not stated" : book.Rating)}\n");
        }
        
        Console.WriteLine("\nНажмите для выхода");
        Console.ReadKey();
        Console.Clear();

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
                // Активное поле — зелёное
                Console.ForegroundColor = ConsoleColor.Green;
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
            // Преобразуем значения в нужные типы для Book, возвращаем "not stated" для пустых строк
            newBook.Title = values[0] == "" ? "__NOT_STATED__" : values[0];
            newBook.Author = values[1] == "" ? "__NOT_STATED__" : values[1];
            newBook.Genre = values[2] == "" ? "__NOT_STATED__" : values[2];
            newBook.Year = int.TryParse(values[3], out int year) ? year : -1;
            newBook.ISBN = values[4] == "" ? "__NOT_STATED__" : values[4];
            newBook.Rating = int.TryParse(values[5], out int rating) ? rating : -1;

            library.AddBook(newBook);
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
        }
        else if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar) || key.KeyChar == '-') // Добавление символа
        {
            if (values[currentField] == "") values[currentField] = ""; // Убеждаемся, что не добавляем к "not stated"
            values[currentField] += key.KeyChar;
        }
    }


    Console.Clear();
}

  
}