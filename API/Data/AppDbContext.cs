
using API.model;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<FileEntity> Files { get; set; }
    }
}