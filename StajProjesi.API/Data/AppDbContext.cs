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

        // =====================================================
        // MEVCUT TABLOLAR
        // =====================================================

        public DbSet<Product> Products { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<PointFeature> Points { get; set; }

        public DbSet<LineFeature> Lines { get; set; }

        public DbSet<PolygonFeature> Polygons { get; set; }


        // =====================================================
        // ADMIN / YETKİ TABLOLARI
        // =====================================================

        public DbSet<Role> Roles { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<UserPermission> UserPermissions { get; set; }

        public DbSet<GeographicPermission> GeographicPermissions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =====================================================
            // POINT
            // =====================================================

            modelBuilder.Entity<PointFeature>()
                .ToTable("tbl_point");

            modelBuilder.Entity<PointFeature>()
                .Property(x => x.Geometry)
                .HasColumnType("geometry(Point,4326)");

            modelBuilder.Entity<PointFeature>()
                .Property(x => x.InsertedUserId)
                .HasColumnName("inserted_user_id");

            modelBuilder.Entity<PointFeature>()
                .Property(x => x.InsertedDate)
                .HasColumnName("inserted_date");

            modelBuilder.Entity<PointFeature>()
                .Property(x => x.ModifiedDate)
                .HasColumnName("modified_date");

            modelBuilder.Entity<PointFeature>()
                .Property(x => x.IsDeleted)
                .HasColumnName("is_deleted");

            modelBuilder.Entity<PointFeature>()
                .Property(x => x.IsActive)
                .HasColumnName("is_active");


            // =====================================================
            // LINE
            // =====================================================

            modelBuilder.Entity<LineFeature>()
                .ToTable("tbl_line");

            modelBuilder.Entity<LineFeature>()
                .Property(x => x.Geometry)
                .HasColumnType("geometry(LineString,4326)");

            modelBuilder.Entity<LineFeature>()
                .Property(x => x.InsertedUserId)
                .HasColumnName("inserted_user_id");

            modelBuilder.Entity<LineFeature>()
                .Property(x => x.InsertedDate)
                .HasColumnName("inserted_date");

            modelBuilder.Entity<LineFeature>()
                .Property(x => x.ModifiedDate)
                .HasColumnName("modified_date");

            modelBuilder.Entity<LineFeature>()
                .Property(x => x.IsDeleted)
                .HasColumnName("is_deleted");

            modelBuilder.Entity<LineFeature>()
                .Property(x => x.IsActive)
                .HasColumnName("is_active");


            // =====================================================
            // POLYGON
            // =====================================================

            modelBuilder.Entity<PolygonFeature>()
                .ToTable("tbl_polygon");

            modelBuilder.Entity<PolygonFeature>()
                .Property(x => x.Geometry)
                .HasColumnType("geometry(Polygon,4326)");

            modelBuilder.Entity<PolygonFeature>()
                .Property(x => x.InsertedUserId)
                .HasColumnName("inserted_user_id");

            modelBuilder.Entity<PolygonFeature>()
                .Property(x => x.InsertedDate)
                .HasColumnName("inserted_date");

            modelBuilder.Entity<PolygonFeature>()
                .Property(x => x.ModifiedDate)
                .HasColumnName("modified_date");

            modelBuilder.Entity<PolygonFeature>()
                .Property(x => x.IsDeleted)
                .HasColumnName("is_deleted");

            modelBuilder.Entity<PolygonFeature>()
                .Property(x => x.IsActive)
                .HasColumnName("is_active");


            // =====================================================
            // USER - ROLE
            // =====================================================

            modelBuilder.Entity<UserRole>()
                .HasKey(x => new { x.UserId, x.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);


            // =====================================================
            // ROLE - PERMISSION
            // =====================================================

            modelBuilder.Entity<RolePermission>()
                .HasKey(x => new { x.RoleId, x.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(x => x.Role)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);


            // =====================================================
            // USER - PERMISSION
            // =====================================================

            modelBuilder.Entity<UserPermission>()
                .HasKey(x => new { x.UserId, x.PermissionId });

            modelBuilder.Entity<UserPermission>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserPermissions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPermission>()
                .HasOne(x => x.Permission)
                .WithMany(x => x.UserPermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);


            // =====================================================
            // ROLE
            // =====================================================

            modelBuilder.Entity<Role>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Role>()
                .HasIndex(x => x.Name)
                .IsUnique();


            // =====================================================
// PERMISSION
// =====================================================

modelBuilder.Entity<Permission>()
    .Property(x => x.Name)
    .IsRequired()
    .HasMaxLength(150);

modelBuilder.Entity<Permission>()
    .Property(x => x.Description)
    .HasMaxLength(500);

modelBuilder.Entity<Permission>()
    .HasIndex(x => x.Name)
    .IsUnique();


// =====================================================
// COĞRAFİ YETKİ
// =====================================================

modelBuilder.Entity<GeographicPermission>()
    .ToTable("tbl_geographic_permission");

modelBuilder.Entity<GeographicPermission>()
    .Property(x => x.Geometry)
    .HasColumnType("geometry(Polygon,4326)");

modelBuilder.Entity<GeographicPermission>()
    .Property(x => x.UserId)
    .HasColumnName("user_id");

modelBuilder.Entity<GeographicPermission>()
    .Property(x => x.RoleId)
    .HasColumnName("role_id");

modelBuilder.Entity<GeographicPermission>()
    .Property(x => x.InsertedUserId)
    .HasColumnName("inserted_user_id");

modelBuilder.Entity<GeographicPermission>()
    .Property(x => x.InsertedDate)
    .HasColumnName("inserted_date");

modelBuilder.Entity<GeographicPermission>()
    .Property(x => x.ModifiedDate)
    .HasColumnName("modified_date");

modelBuilder.Entity<GeographicPermission>()
    .Property(x => x.IsDeleted)
    .HasColumnName("is_deleted");

modelBuilder.Entity<GeographicPermission>()
    .Property(x => x.IsActive)
    .HasColumnName("is_active");


// =====================================================
// COĞRAFİ YETKİ - USER
// =====================================================

modelBuilder.Entity<GeographicPermission>()
    .HasOne<User>()
    .WithMany()
    .HasForeignKey(x => x.UserId)
    .OnDelete(DeleteBehavior.Cascade);


// =====================================================
// COĞRAFİ YETKİ - ROLE
// =====================================================

modelBuilder.Entity<GeographicPermission>()
    .HasOne<Role>()
    .WithMany()
    .HasForeignKey(x => x.RoleId)
    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}