// BookData.cs
public class BookData
{
    public string Title { get; set; } = "__NOT_STATED__";
    public string Author { get; set; } = "__NOT_STATED__";
    public string Genre { get; set; } = "__NOT_STATED__";
    public int Year { get; set; } = -1;
    public string CoverUrl { get; set; } = ""; // URL обложки (для скачивания, но не сохраняется в конечной книге)
    public int Rating { get; set; } = -1;
    public string ISBN { get; set; } = "__NOT_STATED__";
}