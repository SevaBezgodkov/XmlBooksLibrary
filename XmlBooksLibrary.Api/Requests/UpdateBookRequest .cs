namespace XmlBooksLibrary.Api.Requests
{
    public class UpdateBookRequest
    {
        public string Author { get; set; } = null!;
        public string OldTitle { get; set; } = null!;
        public string NewTitle { get; set; } = null!;
    }
}
