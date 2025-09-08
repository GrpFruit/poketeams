using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamBuilderService.Domain;
using TeamBuilderService.Infrastructure;
using TeamBuilderService.Api.Teams;
using System.Linq;

namespace TeamBuilderService.Controllers;

[ApiController]
[Route("teams")]
public class TeamsController : ControllerBase
{
    private readonly AppDbContext _db;
  private static readonly string[] errors = new[] { "Member slots must be unique." };

  public TeamsController(AppDbContext db) => _db = db;

    // GET /teams
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Team>>> Get() =>
        await _db.Teams.Include(t => t.Members).ToListAsync();

    // GET /teams/{id}  (guid constraint rejects bad ids at routing time)
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Team>> GetById(Guid id)
    {
        var team = await _db.Teams.Include(t => t.Members).FirstOrDefaultAsync(t => t.Id == id);
        return team is null ? NotFound() : team;
    }

    // POST /teams
    // Uses DTO without IDs; [ApiController] will auto 400 on DataAnnotation failures
    [HttpPost]
    public async Task<ActionResult<Team>> Create([FromBody] TeamCreateDto dto)
    {
        // Extra rule: member slots must be unique
        var slotCount = dto.Members.Select(m => m.Slot).ToList();
        if (slotCount.Distinct().Count() != slotCount.Count)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["members.slot"] = errors
            }));
        }

        // Map DTO -> entity (server assigns Ids; TeamId is set by EF via relationship)
        var team = new Team
        {
            Name = dto.Name,
            Notes = dto.Notes,
            Members = dto.Members.Select(m => new TeamMember
            {
                PokemonName = m.PokemonName, // nullable allowed per spec
                Slot = m.Slot
            }).ToList()
        };

        // Final guard: at least 1 member (also enforced by DTO MinLength, but keep as defense-in-depth)
        if (team.Members.Count is < 1 or > 6)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["members"] = ["Team must have between 1 and 6 members."]
            }));

        _db.Teams.Add(team);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = team.Id }, team);
    }

    // DELETE /teams/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var team = await _db.Teams.FindAsync(id);
        if (team is null) return NotFound();
        _db.Teams.Remove(team);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
