using System.Reflection;
using System.Text;
using WepApiFSP.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace WepApiFSP.Data.Repositories;

public interface IBookRepository
{
    Task<PagedBookResult> GetBooks(string? term, string? sort, int page, int limit);
}
public class BookRepository : IBookRepository
{
    private readonly BookContext _bookContext;

    public BookRepository(BookContext bookContext)
    {
        _bookContext= bookContext;
    }
   public async Task<PagedBookResult> GetBooks(string? term, string? sort, int page, int limit)
    {
        IQueryable<Book> books;
        if (string.IsNullOrWhiteSpace(term))
            books = _bookContext.Books;
        else
        {
            term = term.Trim().ToLower();
            // filtering records with author or title
            books = _bookContext
                .Books
                .Where(b => b.Title.ToLower().Contains(term)
                || b.Author.ToLower().Contains(term)
                );
        }

        // sorting
        // sort=title,-year
        // (arrange order title ascending and year descending
        if (!string.IsNullOrWhiteSpace(sort))
        {
            var sortFields = sort.Split(','); // ['title','-year']
            StringBuilder orderQueryBuilder = new StringBuilder();
            // using reflection to get properties of book
            // propertyInfo= [Id,Title,Year,Author,Language] 
            PropertyInfo[] propertyInfo = typeof(Book).GetProperties();


            foreach (var field in sortFields)
            {
                // iteration 1, field=title
                // iteration 2, field=-year
                string sortOrder = "ascending";
                // iteration 1, sortField= title
                // iteration 2, sortField=-year
                var sortField = field.Trim(); 
                if (sortField.StartsWith("-"))
                {
                    sortField = sortField.TrimStart('-');
                    sortOrder = "descending";
                }
                // property = 'Title'
                // property = 'Year'
                var property = propertyInfo.FirstOrDefault(a => a.Name.Equals(sortField, StringComparison.OrdinalIgnoreCase));
                if (property == null)
                    continue;
                // orderQueryBuilder= "Title ascending,Year descending, "
                // it have trailing , and whitespace
                orderQueryBuilder.Append($"{property.Name.ToString()} {sortOrder}, ");
            }
            // remove trailing , and whitespace here
            // orderQuery = ""Title ascending,Year descending"
            string orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
            if (!string.IsNullOrWhiteSpace(orderQuery))
                // use System.Linq.Dynamic.Core namespace for this
                books = books.OrderBy(orderQuery);
            else
                books = books.OrderBy(a => a.Id);
        }

        // apply pagination
        
        // totalCount=101 ,page=1,limit=10 (10 record per page)
        var totalCount = await _bookContext.Books.CountAsync();  //101
        // 101/10 = 10.1->11 
        var totalPages = (int)Math.Ceiling(totalCount / (double)limit);

        // page=1 , skip=(1-1)*10=0, take=10
        // page=2 , skip=(2-1)*10=10, take=10
        // page=3 , skip=(3-1)*10=20, take=10
        var pagedBooks = await books.Skip((page - 1) * limit).Take(limit).ToListAsync();

        var pagedBookData = new PagedBookResult
        {
            Books = pagedBooks,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
        return pagedBookData;
    }


}