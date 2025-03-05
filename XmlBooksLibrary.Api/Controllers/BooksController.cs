using Microsoft.AspNetCore.Mvc;
using XmlBooksLibrary.Business.Models;
using XmlBooksLibrary.Business.Services;
using XmlBooksLibrary.Business.Services.Interfaces;

namespace XmlBooksLibrary.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _service;
        public BooksController(IBookService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {
                return Ok(await _service.GetAllBooksAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("{keyword}")]
        public async Task<IActionResult> GetBookByKeyword(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    return BadRequest("The keyword can not be empty!");

                var book = await _service.GetBookByKeywordAsync(keyword);

                if (book == null)
                    return NotFound(new { message = "Book not found." });

                return Ok(book);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddBook([FromBody] BookModel book)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(book.Author) || string.IsNullOrWhiteSpace(book.Title))
                    return BadRequest("The author and title can't be empty!");

                await _service.AddBookAsync(book);

                return CreatedAtAction(nameof(GetBookByKeyword), new { keyword = book.Title }, book);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    message = ex.Message, 
                    innerException = ex.InnerException?.Message
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBookByAuthorAndTitle([FromQuery] string author, [FromQuery] string oldTitle, [FromQuery] string newTitle)
        {
            try
            {
                var result = await _service.UpdateBookAsync(author, oldTitle, newTitle);

                if (!result)
                    return NotFound($"Book with title '{oldTitle}' by author '{author}' not found.");

                return Ok($"Book with title '{oldTitle}' by author '{author}' has been updated.");
            }
            catch (Exception ex)
            {
                return StatusCode(400, new
                {
                    message = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }
    }
}
