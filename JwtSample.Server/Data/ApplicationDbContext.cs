using Microsoft.EntityFrameworkCore;
using JwtSample.Server.Models;

namespace JwtSample.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Educator> Educators { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<Recording> Recordings { get; set; }
        public DbSet<ChildRecording> ChildRecording { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Educator>().ToTable("Educator").HasIndex(f => f.FacebookId).IsUnique(true);
            builder.Entity<Educator>().HasMany(e => e.Children);
            //We cannot have these two fields as required because the user is created at Login, where we only have the facebookId
            //builder.Entity<Educator>().Property(e => e.FirstName).IsRequired(true);
            //builder.Entity<Educator>().Property(e => e.LastName).IsRequired(true);

            builder.Entity<Educator>().Property(e => e.Token).IsRequired(true).HasMaxLength(8096);
            builder.Entity<Educator>().HasMany(b => b.Children);

            builder.Entity<Child>().ToTable("Child");
            builder.Entity<Child>().Property(c => c.Name).IsRequired(true);
            builder.Entity<Child>().Property(c => c.Birthday).IsRequired(true);
            //builder.Entity<Child>().HasMany(b => b.Recordings);

            builder.Entity<Recording>().ToTable("Recording");
            builder.Entity<Recording>().Property(r => r.RecordingId).ValueGeneratedOnAdd();///?
            //builder.Entity<Recording>().HasMany(b => b.Children);

            builder.Entity<ChildRecording>().ToTable("ChildRecording");
            builder.Entity<ChildRecording>().HasKey(t => new { t.ChildId, t.RecordingId });
            //builder.Entity<ChildRecording>().HasOne(cr => cr.Child);
            //builder.Entity<ChildRecording>().HasOne(cr => cr.Recording);

            //builder.Entity<ChildRecordingInOuts>().ToTable("ChildRecordingInOuts");
        }

    }
}
