using System.Text.Json.Serialization;


    public class Book
    {
        [JsonPropertyName("Title")]
        public string Title { get; set; }

        [JsonPropertyName("Author")]
        public string Author { get; set; }

        [JsonPropertyName("Genre")]
        public string Genre { get; set; }

        [JsonPropertyName("Year")]
        public int Year { get; set; }

        [JsonPropertyName("ISBN")]
        public string ISBN { get; set; }

        [JsonPropertyName("Rating")]
        public int Rating { get; set; } // Для B-side

        [JsonPropertyName("CoverUrl")] // Новое поле для ссылки на обложку
        public string CoverUrl { get; set; } = ""; // Пустая строка по умолчанию, если ссылки нет

        public Book(string title = "__NOT_STATED__", string author = "__NOT_STATED__", 
                    string genre = "__NOT_STATED__", int year = -1, string isbn = "__NOT_STATED__", 
                    int rating = -1, string coverUrl = "")
        {
            // Проверка, чтобы название не было "__NOT_STATED__" (если пользователь явно ввёл "not stated", оставляем как есть)
            Title = title == "__NOT_STATED__" && title != "not stated" ? "not stated" : title;
            Author = author == "__NOT_STATED__" ? "not stated" : author;
            Genre = genre == "__NOT_STATED__" ? "not stated" : genre;
            Year = year == -1 ? -1 : year; // Оставляем -1 как индикатор отсутствия
            ISBN = isbn == "__NOT_STATED__" ? "not stated" : isbn;
            Rating = rating == -1 ? -1 : rating; // Оставляем -1 как индикатор отсутствия
            CoverUrl = coverUrl; // Сохраняем ссылку на обложку или пустую строку
        }

        // Для вывода в консоль, заменяем "__NOT_STATED__" на "not stated" только для отображения
        public override string ToString()
        {
            string displayTitle = Title == "__NOT_STATED__" ? "not stated" : Title;
            string displayAuthor = Author == "__NOT_STATED__" ? "not stated" : Author;
            string displayGenre = Genre == "__NOT_STATED__" ? "not stated" 
                : (Genre.Length > 50 ? Genre.Substring(0, 50) + "..." : Genre); // Ограничение длины жанра
            string displayISBN = ISBN == "__NOT_STATED__" ? "not stated" : ISBN;

            return $"Название: {displayTitle}\n" +
                   $"Автор: {displayAuthor}\n" +
                   $"Жанр: {displayGenre}\n" +
                   $"Год издания: {Year}\n" +
                   $"ISBN: {displayISBN}\n" +
                   $"Оценка: {Rating}\n" +
                   $"Ссылка на обложку: {CoverUrl}\n";
        }
    }
