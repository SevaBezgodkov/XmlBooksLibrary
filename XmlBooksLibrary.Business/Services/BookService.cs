using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using XmlBooksLibrary.Business.Models;
using XmlBooksLibrary.Business.Services.Interfaces;

namespace XmlBooksLibrary.Business.Services
{
    public class BookService : IBookService
    {
        private readonly string _filePath;
        private List<BookModel> _books;

        public BookService() : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "books.xml")) { }

        public BookService(string filePath)
        {
            _filePath = filePath;
            try
            {
                _books = LoadFromXmlAsync().Result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading books", ex);
            }
        }

        public async Task<List<BookModel>> GetAllBooksAsync() => _books;

        public async Task<List<BookModel>> GetBookByKeywordAsync(string keyword)
        {
            try
            {
                return await Task.FromResult(_books.Where(b => b.Title.Contains(keyword, System.StringComparison.OrdinalIgnoreCase)).ToList());
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching for book", ex);
            }
        }

        public async Task AddBookAsync(BookModel book)
        {
            try
            {
                if (IsBookByAuthorExist(book.Author, book.Title))
                    throw new Exception("Book with the same title by this author already exists.");
                
                _books.Add(book);
                SortBooks();

                await SaveToXmlAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding book", ex);
            }
        }

        public async Task<bool> UpdateBookAsync(string author, string oldTitle, string newTitle)
        {
            try
            {
                var bookToUpdate = _books.FirstOrDefault(b => b.Author.Equals(author, StringComparison.OrdinalIgnoreCase) && b.Title.Equals(oldTitle, StringComparison.OrdinalIgnoreCase));

                if (bookToUpdate == null)
                    return false;

                if (IsBookByAuthorExist(author, newTitle))
                    throw new Exception($"You can't update - '{oldTitle}' to '{newTitle}'. {author} already has this book!");

                bookToUpdate.Title = newTitle;

                SortBooks();

                await SaveToXmlAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating book", ex);
            }
        }

        private async Task<List<BookModel>> LoadFromXmlAsync()
        {
            try
            {
                if (!File.Exists(_filePath)) 
                    return new List<BookModel>();

                using var stream = new FileStream(_filePath, FileMode.Open);
                var serializer = new XmlSerializer(typeof(List<BookModel>));

                return (List<BookModel>)serializer.Deserialize(stream) ?? new List<BookModel>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading XML file", ex);
            }
        }

        private async Task SaveToXmlAsync()
        {
            try
            {
                using var stream = new FileStream(_filePath, FileMode.Create);
                var serializer = new XmlSerializer(typeof(List<BookModel>));

                serializer.Serialize(stream, _books);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving XML file", ex);
            }
        }

        private bool IsBookByAuthorExist(string author, string title) =>
             _books.Any(b => b.Author.Equals(author, StringComparison.OrdinalIgnoreCase) && b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

        private void SortBooks() =>
            _books = _books.OrderBy(b => b.Author).ThenBy(b => b.Title).ToList();
    }
}
