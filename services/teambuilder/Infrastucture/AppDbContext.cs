using Microsoft.EntityFrameworkCore;
using TeamBuilderService.Domain;

namespace TeamBuilderService.Infrastructure;
public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
  public DbSet<Team> Teams => Set<Team>();
  public DbSet<TeamMember> TeamMembers => Set<TeamMember>();


  protected override void OnModelCreating(ModelBuilder b)
  {
    b.Entity<Team>(e =>
    {
      e.HasKey(t => t.Id);
      e.Property(t => t.Name).IsRequired().HasMaxLength(100);
      e.HasMany(t => t.Members)
      .WithOne()
      .HasForeignKey(m => m.TeamId)
      .OnDelete(DeleteBehavior.Cascade);
    });

    b.Entity<TeamMember>(e =>
    {
      e.HasKey(m => m.Id);
      e.Property(m => m.PokemonName).HasMaxLength(100);
      e.Property(m => m.Slot).IsRequired();
      e.HasCheckConstraint("CK_TeamMember_Slot_Range", "\"Slot\" BETWEEN 1 AND 6");
    });
  }
}