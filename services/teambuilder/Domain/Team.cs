namespace TeamBuilderService.Domain;
public class Team
{
  public Guid Id { get; set; } = Guid.NewGuid();      // server-assigned
  public string Name { get; set; } = default!;
  public string? Notes { get; set; }
  public List<TeamMember> Members { get; set; } = new();
}