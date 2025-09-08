using System.ComponentModel.DataAnnotations;

namespace TeamBuilderService.Api.Teams;

public class TeamCreateDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Require at least 1 and at most 6 members
    [MinLength(1), MaxLength(6)]
    public List<TeamMemberCreateDto> Members { get; set; } = new();
}
