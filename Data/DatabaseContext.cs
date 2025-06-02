using Microsoft.EntityFrameworkCore;
using daSSH.Models;

namespace daSSH.Data;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options) {
    public DbSet<User> Users { get; set; }
}
