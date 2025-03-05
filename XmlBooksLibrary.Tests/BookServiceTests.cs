using XmlBooksLibrary.Business.Models;
using XmlBooksLibrary.Business.Services;
using XmlBooksLibrary.Business.Services.Interfaces;

namespace XmlBooksLibrary.Tests
{
    public class BookServiceTests : IAsyncDisposable
    {
        private readonly BookService _service;
        private readonly string _testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_books.xml");

        public BookServiceTests()
        {
            CleanupTestFile();

            _service = new BookService(_testFilePath);
        }

        public async ValueTask DisposeAsync()
        {
            CleanupTestFile();
        }

        private void CleanupTestFile()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }

        [Fact]
        public async Task LoadFromXmlAsync_ReturnEmptyList_IfFileDoesNotExist()
        {
            var books = await _service.GetAllBooksAsync();
            Assert.Empty(books);
        }

        [Fact]
        public async Task AddBookAsync_AddBookAndSaveToXml()
        {
            var book = new BookModel { Title = "Test Book", Author = "Test Author" };
            await _service.AddBookAsync(book);

            var books = await _service.GetAllBooksAsync();
            Assert.Single(books);
            Assert.Equal("Test Book", books[0].Title);
        }

        [Fact]
        public async Task AddBookAsync_ThrowException_IfDuplicateBookAdded()
        {
            var book = new BookModel { Title = "Test Book", Author = "Test Author" };
            await _service.AddBookAsync(book);

            await Assert.ThrowsAsync<Exception>(() => _service.AddBookAsync(book));
        }

        [Fact]
        public async Task GetBookByKeywordAsync_ReturnMatchingBooks()
        {
            var book1 = new BookModel { Title = "Harry Potter", Author = "J.K. Rowling" };
            var book2 = new BookModel { Title = "Harry Potter 2", Author = "J.K. Rowling" };
            await _service.AddBookAsync(book1);
            await _service.AddBookAsync(book2);

            var result = await _service.GetBookByKeywordAsync("otte");
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task SortBooks_SortByAuthorAndTitle()
        {
            var book1 = new BookModel { Title = "B Book", Author = "Author A" };
            var book2 = new BookModel { Title = "A Book", Author = "Author A" };
            var book3 = new BookModel { Title = "C Book", Author = "Author B" };

            await _service.AddBookAsync(book1);
            await _service.AddBookAsync(book2);
            await _service.AddBookAsync(book3);

            var books = await _service.GetAllBooksAsync();
            Assert.Equal("A Book", books[0].Title);
            Assert.Equal("B Book", books[1].Title);
            Assert.Equal("C Book", books[2].Title);
        }

        [Fact]
        public async Task LoadFromXmlAsync_HandleCorruptFileGracefully()
        {
            await File.WriteAllTextAsync(_testFilePath, "invalid xml content");

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                var corruptedService = new BookService(_testFilePath);
                await corruptedService.GetAllBooksAsync();
            });
        }

        [Fact]
        public async Task UpdateBookAsync_UpdateTitleByAuthorAndOldTitle()
        {
            var book = new BookModel { Title = "Old Title", Author = "Test Author" };
            await _service.AddBookAsync(book);

            bool updated = await _service.UpdateBookAsync("Test Author", "Old Title", "New Title");

            Assert.True(updated);

            var books = await _service.GetAllBooksAsync();
            var updatedBook = books.FirstOrDefault(b => b.Author == "Test Author" && b.Title == "New Title");

            Assert.NotNull(updatedBook);
            Assert.Equal("New Title", updatedBook?.Title);
        }

        [Fact]
        public async Task UpdateBookAsync_ReturnsFalse_WhenBookNotFound()
        {
            var result = await _service.UpdateBookAsync("Test Author", "Non-existent Title", "New Title");
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateBookAsync_ThrowException_WhenBookAlreadyExistsByAuthor()
        {
            var book1 = new BookModel { Title = "Existing Title", Author = "Test Author" };
            var book2 = new BookModel { Title = "Old Title", Author = "Test Author" };
            await _service.AddBookAsync(book1); 
            await _service.AddBookAsync(book2); 

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _service.UpdateBookAsync("Test Author", "Old Title", "Existing Title") 
            );

            Assert.NotNull(exception);
        }
    }
}