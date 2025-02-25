using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Collections;

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

    // Примеры методов для B-side (добавь позже)
    public void AddBook(Book book) => books.Add(book);
    public void RemoveBook(Book book) => books.Remove(book);
    // Другие методы (редактирование, рекомендации) пиши сам
}