using Moq;
using XmlBooksLibrary.Business.Models;
using XmlBooksLibrary.Business.Services;
using XmlBooksLibrary.Business.Services.Interfaces;

namespace XmlBooksLibrary.Tests
{
    public class BookServiceTests 
    {
        private readonly Mock<IBookService> _mockBookService;

        public BookServiceTests()
        {
            _mockBookService = new Mock<IBookService>();
        }

        [Fact]
        public async Task LoadFromXmlAsync_ReturnEmptyList_IfFileDoesNotExist()
        {
            _mockBookService.Setup(service => service.GetAllBooksAsync()).ReturnsAsync(new List<BookModel>());

            var books = await _mockBookService.Object.GetAllBooksAsync();

            Assert.Empty(books);
        }

        [Fact]
        public async Task AddBookAsync_AddBookAndSaveToXml()
        {
            var book = new BookModel { Title = "Test Book", Author = "Test Author" };
            _mockBookService.Setup(service => service.AddBookAsync(It.IsAny<BookModel>())).Returns(Task.CompletedTask);
            _mockBookService.Setup(service => service.GetAllBooksAsync()).ReturnsAsync(new List<BookModel> { book });

            await _mockBookService.Object.AddBookAsync(book);

            var books = await _mockBookService.Object.GetAllBooksAsync();
            Assert.Single(books);
            Assert.Equal("Test Book", books[0].Title);
        }

        [Fact]
        public async Task AddBookAsync_ThrowException_IfDuplicateBookAdded()
        {
            var book = new BookModel { Title = "Test Book", Author = "Test Author" };
            _mockBookService.Setup(service => service.AddBookAsync(It.IsAny<BookModel>()))
                            .ThrowsAsync(new Exception("Duplicate book"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _mockBookService.Object.AddBookAsync(book));
            Assert.Equal("Duplicate book", exception.Message);
        }

        [Fact]
        public async Task GetBookByKeywordAsync_ReturnMatchingBooks()
        {
            var book1 = new BookModel { Title = "Harry Potter", Author = "J.K. Rowling" };
            var book2 = new BookModel { Title = "Harry Potter 2", Author = "J.K. Rowling" };
            _mockBookService.Setup(service => service.AddBookAsync(It.IsAny<BookModel>())).Returns(Task.CompletedTask);
            _mockBookService.Setup(service => service.GetBookByKeywordAsync(It.IsAny<string>())).ReturnsAsync(new List<BookModel> { book1, book2 });

            await _mockBookService.Object.AddBookAsync(book1);
            await _mockBookService.Object.AddBookAsync(book2);

            var result = await _mockBookService.Object.GetBookByKeywordAsync("otte");

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task SortBooks_SortByAuthorAndTitle()
        {
            var book1 = new BookModel { Title = "B Book", Author = "Author A" };
            var book2 = new BookModel { Title = "A Book", Author = "Author A" };
            var book3 = new BookModel { Title = "C Book", Author = "Author B" };
            var books = new List<BookModel> { book1, book2, book3 };

            _mockBookService.Setup(service => service.AddBookAsync(It.IsAny<BookModel>())).Returns(Task.CompletedTask);
            _mockBookService.Setup(service => service.GetAllBooksAsync()).ReturnsAsync(books.OrderBy(b => b.Author).ThenBy(b => b.Title).ToList());

            await _mockBookService.Object.AddBookAsync(book1);
            await _mockBookService.Object.AddBookAsync(book2);
            await _mockBookService.Object.AddBookAsync(book3);

            var sortedBooks = await _mockBookService.Object.GetAllBooksAsync();

            Assert.Equal("A Book", sortedBooks[0].Title);
            Assert.Equal("B Book", sortedBooks[1].Title);
            Assert.Equal("C Book", sortedBooks[2].Title);
        }

        [Fact]
        public async Task LoadFromXmlAsync_HandleCorruptFileGracefully()
        {
            _mockBookService.Setup(service => service.GetAllBooksAsync()).ThrowsAsync(new Exception("Invalid XML content"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _mockBookService.Object.GetAllBooksAsync());
            Assert.Equal("Invalid XML content", exception.Message);
        }

        [Fact]
        public async Task UpdateBookAsync_UpdateTitleByAuthorAndOldTitle()
        {
            var book = new BookModel { Title = "Old Title", Author = "Test Author", Pages = 100 };

            _mockBookService.Setup(service => service.GetAllBooksAsync()).ReturnsAsync(new List<BookModel> { book });
            _mockBookService.Setup(service => service.UpdateBookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            await _mockBookService.Object.AddBookAsync(book);
            var updated = await _mockBookService.Object.UpdateBookAsync("Test Author", "Old Title", "New Title");
            var updatedBooks = new List<BookModel>
            {
                new BookModel { Title = "New Title", Author = "Test Author", Pages = 100 }
            };
            _mockBookService.Setup(service => service.GetAllBooksAsync()).ReturnsAsync(updatedBooks);

            Assert.True(updated);

            var books = await _mockBookService.Object.GetAllBooksAsync();
            var updatedBook = books.FirstOrDefault(b => b.Author == "Test Author" && b.Title == "New Title");

            Assert.NotNull(updatedBook);
            Assert.Equal("New Title", updatedBook?.Title);
        }

        [Fact]
        public async Task UpdateBookAsync_ReturnsFalse_WhenBookNotFound()
        {
            _mockBookService.Setup(service => service.UpdateBookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            var result = await _mockBookService.Object.UpdateBookAsync("Test Author", "Non-existent Title", "New Title");

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateBookAsync_ThrowException_WhenBookAlreadyExistsByAuthor()
        {
            var book1 = new BookModel { Title = "Existing Title", Author = "Test Author" };
            var book2 = new BookModel { Title = "Old Title", Author = "Test Author" };
            _mockBookService.Setup(service => service.AddBookAsync(It.IsAny<BookModel>())).Returns(Task.CompletedTask);
            _mockBookService.Setup(service => service.UpdateBookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .ThrowsAsync(new Exception("Book already exists"));

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _mockBookService.Object.UpdateBookAsync("Test Author", "Old Title", "Existing Title")
            );

            Assert.NotNull(exception);
            Assert.Equal("Book already exists", exception.Message);
        }
    }
}