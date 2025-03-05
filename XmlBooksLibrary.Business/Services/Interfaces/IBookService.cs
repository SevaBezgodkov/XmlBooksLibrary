using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlBooksLibrary.Business.Models;

namespace XmlBooksLibrary.Business.Services.Interfaces
{
    public interface IBookService
    {
        Task<List<BookModel>> GetAllBooksAsync();
        Task<List<BookModel>> GetBookByKeywordAsync(string keyword);
        Task AddBookAsync(BookModel book);
        Task<bool> UpdateBookAsync(string author, string oldTitle, string newTitle);
    }
}
