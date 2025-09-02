using System.ComponentModel.DataAnnotations;

namespace TeamBuilderService.Api.Teams;

// No Id/TeamId here (server assigns those). pokemonName can be null per your spec.
public class TeamMemberCreateDto
{
    public string? PokemonName { get; set; }

    // 1..6 per slot rules
    [Range(1, 6)]
    public int Slot { get; set; }
}
