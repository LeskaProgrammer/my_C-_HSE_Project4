// Book.cs

using System.Text.Json.Serialization;

namespace Alexey_Simonov_project_4
{
    // Класс, представляющий книгу в библиотеке
    public class Book
    {
        // Свойство для хранения названия книги, сериализуется в JSON как "Title"
        [JsonPropertyName("Title")]
        public string? Title { get; set; } // Может быть null, если не указано

        // Свойство для хранения автора книги, сериализуется в JSON как "Author"
        [JsonPropertyName("Author")]
        public string? Author { get; set; } // Может быть null, если автор неизвестен

        // Свойство для хранения жанра книги, сериализуется в JSON как "Genre"
        [JsonPropertyName("Genre")]
        public string? Genre { get; set; } // Может быть null, если жанр не определён

        // Свойство для хранения года издания книги, сериализуется в JSON как "Year"
        [JsonPropertyName("Year")]
        public int Year { get; set; } // Целочисленное значение, -1 означает отсутствие года

        // Свойство для хранения ISBN книги, сериализуется в JSON как "ISBN"
        [JsonPropertyName("ISBN")]
        public string? Isbn { get; set; } // Может быть null, если ISBN не указан

        // Свойство для хранения рейтинга книги (добавлено для варианта B-side), сериализуется в JSON как "Rating"
        [JsonPropertyName("Rating")]
        public int Rating { get; set; } // Целочисленное значение, -1 означает отсутствие рейтинга

        // Свойство для хранения пути к локальному файлу обложки, сериализуется в JSON как "CoverUrl"
        [JsonPropertyName("CoverUrl")] // Путь к локальному файлу обложки на диске
        public string CoverUrl { get; set; } // Пустая строка по умолчанию, если файла нет

        /// <summary>
        /// Конструктор класса Book с параметрами по умолчанию.
        /// </summary>
        /// <param name="title">Название книги, по умолчанию "__NOT_STATED__".</param>
        /// <param name="author">Автор книги, по умолчанию "__NOT_STATED__".</param>
        /// <param name="genre">Жанр книги, по умолчанию "__NOT_STATED__".</param>
        /// <param name="year">Год издания книги, по умолчанию -1.</param>
        /// <param name="isbn">ISBN книги, по умолчанию "__NOT_STATED__".</param>
        /// <param name="rating">Рейтинг книги, по умолчанию -1.</param>
        /// <param name="coverUrl">Путь к файлу обложки, по умолчанию пустая строка.</param>
        public Book(string? title = "__NOT_STATED__", string? author = "__NOT_STATED__", 
            string? genre = "__NOT_STATED__", int year = -1, string? isbn = "__NOT_STATED__", 
            int rating = -1, string coverUrl = "")
        {
            // Проверка, чтобы название не было "__NOT_STATED__" (если пользователь явно ввёл "not stated", оставляем как есть)
            Title = title == "__NOT_STATED__" && title != "not stated" ? "not stated" : title; // Присваиваем "not stated" вместо "__NOT_STATED__" для отображения
            Author = author == "__NOT_STATED__" ? "not stated" : author; // Заменяем "__NOT_STATED__" на "not stated" для автора
            Genre = genre == "__NOT_STATED__" ? "not stated" : genre; // Заменяем "__NOT_STATED__" на "not stated" для жанра
            Year = year == -1 ? -1 : year; // Оставляем -1 как индикатор отсутствия года, иначе присваиваем указанное значение
            Isbn = isbn == "__NOT_STATED__" ? "not stated" : isbn; // Заменяем "__NOT_STATED__" на "not stated" для ISBN
            Rating = rating == -1 ? -1 : rating; // Оставляем -1 как индикатор отсутствия рейтинга, иначе присваиваем указанное значение
            CoverUrl = coverUrl; // Сохраняем путь к локальному файлу обложки или пустую строку, без изменений
        }

        /// <summary>
        /// Переопределяет метод ToString для удобного отображения информации о книге в консоли.
        /// </summary>
        /// <returns>Строковое представление книги с её свойствами.</returns>
        // Для вывода в консоль, заменяем "__NOT_STATED__" на "not stated" только для отображения
        public override string ToString()
        {
            // Подготавливаем отображаемые значения, заменяя "__NOT_STATED__" на "not stated"
            string? displayTitle = Title == "__NOT_STATED__" ? "not stated" : Title; // Отображаем "not stated", если заголовок не указан
            string? displayAuthor = Author == "__NOT_STATED__" ? "not stated" : Author; // Отображаем "not stated", если автор не указан
            string? displayGenre = Genre == "__NOT_STATED__" ? "not stated" 
                : Genre != null && Genre.Length > 50 ? Genre.Substring(0, 50) + "..." : Genre; // Ограничение длины жанра до 50 символов с добавлением "..." при превышении
            string? displayIsbn = Isbn == "__NOT_STATED__" ? "not stated" : Isbn; // Отображаем "not stated", если ISBN не указан

            // Формируем строковое представление книги с переносами строк для читаемости
            return $"Название: {displayTitle}\n" +
                   $"Автор: {displayAuthor}\n" +
                   $"Жанр: {displayGenre}\n" +
                   $"Год издания: {Year}\n" +
                   $"ISBN: {displayIsbn}\n" +
                   $"Оценка: {Rating}\n" +
                   $"Путь к обложке: {CoverUrl}\n";
        }
    }
}