using Microsoft.AspNetCore.Mvc;
using WepApiFSP.Data.Repositories;


namespace WebApiFSP.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepo;

        public BookController(IBookRepository bookRepo)
        {
            _bookRepo = bookRepo;
        }
        [HttpGet]
        public async Task<IActionResult> GetBooks(string? term, string? sort, int page = 1, int limit = 10)
        {
            var bookResult = await _bookRepo.GetBooks(term, sort, page, limit);

            // Add pagination headers to the response
            Response.Headers.Add("X-Total-Count", bookResult.TotalCount.ToString());
            Response.Headers.Add("X-Total-Pages", bookResult.TotalPages.ToString());
            return Ok(bookResult.Books);
        }


    }
}
