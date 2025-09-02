namespace TeamBuilderService.Domain;
public class TeamMember
{
  public int Id { get; set; }                          // server-assigned
  public string? PokemonName { get; set; }             // nullable per spec
  public int Slot { get; set; }                        // 1..6
  public Guid TeamId { get; set; }                     // set by EF on save
}