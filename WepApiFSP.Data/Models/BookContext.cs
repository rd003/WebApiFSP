
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace WepApiFSP.Data.Models;

public class BookContext: DbContext
{
    public string DbPath { get; set; }
    public BookContext()
    {
        var folder = Environment.SpecialFolder.ApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "book.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");

    public DbSet<Book> Books { get; set; }
}
