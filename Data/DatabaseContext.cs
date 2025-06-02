using Microsoft.EntityFrameworkCore;
using daSSH.Models;

namespace daSSH.Data;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options) {
    public DbSet<User> Users { get; set; }
    public DbSet<Instance> Instances { get; set; }
    public DbSet<PortForward> Forwards { get; set; }
    public DbSet<InstanceFile> Files { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Instance>()
            .HasOne(i => i.Owner)
            .WithMany(u => u.Instances)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Instance>()
            .HasMany(i => i.SharedUsers)
            .WithMany(u => u.SharedInstances)
            .UsingEntity(j => j.ToTable("SharedInstances"));

        modelBuilder.Entity<InstanceFile>()
            .HasOne(f => f.Instance)
            .WithMany(i => i.Files)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Instance>()
            .HasOne(i => i.PortForward)
            .WithOne(f => f.Instance)
            .HasForeignKey<PortForward>("InstanceID")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
