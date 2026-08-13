using Microsoft.EntityFrameworkCore;
using StajProjesi.API.Models;

namespace StajProjesi.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PointFeature> Points { get; set; }
        public DbSet<LineFeature> Lines { get; set; }
        public DbSet<PolygonFeature> Polygons { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<PointFeature>()
        .ToTable("tbl_point");

    modelBuilder.Entity<PointFeature>()
        .Property(x => x.Geometry)
        .HasColumnType("geometry(Point,4326)");

    modelBuilder.Entity<LineFeature>()
        .ToTable("tbl_line");

    modelBuilder.Entity<LineFeature>()
        .Property(x => x.Geometry)
        .HasColumnType("geometry(LineString,4326)");

    modelBuilder.Entity<PolygonFeature>()
        .ToTable("tbl_polygon");

    modelBuilder.Entity<PolygonFeature>()
        .Property(x => x.Geometry)
        .HasColumnType("geometry(Polygon,4326)");
}
    }
}