using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.InnerApi.Controllers;

//todo THIS CONTROLLER IS FOR FLP-1527 SPIKE AND SHOULD BE REMOVED; It calls the DBContext directly just to prove we can perform operations on the new data model for GSO / ShortCourses
[Route("GSOModellingSpike")]
[ApiController]
public class ShortCourseSpikeController : ControllerBase
{
    private readonly DataAccess.LearningDataContext _context;
    public ShortCourseSpikeController(DataAccess.LearningDataContext context)
    {
        _context = context;
    }

    [HttpGet("shortCourses/count")]
    public async Task<IActionResult> GetShortCourseCount()
    {
        var count = await _context.ShortCourseLearnings.CountAsync();
        return Ok(count);
    }
    
    [HttpGet("shortCourses")]
    public async Task<IActionResult> GetAllShortCourses()
    {
        var courses = await _context.ShortCourseLearnings
            .AsNoTracking()
            .ToListAsync();

        return Ok(courses);
    }

    [HttpGet("shortCourses/{key:guid}")]
    public async Task<IActionResult> GetShortCourse(Guid key)
    {
        var course = await _context.ShortCourseLearnings
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Key == key);

        if (course == null)
        {
            return NotFound();
        }

        return Ok(course);
    }

    [HttpPost("shortCourses")]
    public async Task<IActionResult> CreateShortCourse([FromBody] ShortCourseLearning course)
    {
        _context.ShortCourseLearnings.Add(course);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetShortCourse),
            new { key = course.Key },
            course);
    }

    [HttpPut("shortCourses/{key:guid}")]
    public async Task<IActionResult> UpdateShortCourse(Guid key, [FromBody] ShortCourseLearning updated)
    {
        var existing = await _context.ShortCourseLearnings
            .Include(l => l.Episodes)
                .ThenInclude(e => e.Milestones)
            .SingleOrDefaultAsync(l => l.Key == key);

        if (existing == null)
            return NotFound();

        existing.LearnerKey = updated.LearnerKey;
        existing.WithdrawalDate = updated.WithdrawalDate;
        existing.ExpectedEndDate = updated.ExpectedEndDate;
        existing.IsApproved = updated.IsApproved;
        existing.CompletionDate = updated.CompletionDate;

        if (updated.Episodes != null)
        {
            // Remove episodes that no longer exist
            var toRemoveEpisodes = existing.Episodes
                .Where(e => !updated.Episodes.Any(u => u.Key == e.Key))
                .ToList();
            _context.RemoveRange(toRemoveEpisodes);

            foreach (var updatedEpisode in updated.Episodes)
            {
                var existingEpisode = existing.Episodes
                    .SingleOrDefault(e => e.Key == updatedEpisode.Key);

                if (existingEpisode != null)
                {
                    existingEpisode.LearningKey = updatedEpisode.LearningKey;
                    existingEpisode.Ukprn = updatedEpisode.Ukprn;
                    existingEpisode.EmployerAccountId = updatedEpisode.EmployerAccountId;
                    existingEpisode.TrainingCode = updatedEpisode.TrainingCode;

                    if (updatedEpisode.Milestones != null)
                    {
                        // Remove milestones no longer present
                        var toRemoveMilestones = existingEpisode.Milestones
                            .Where(m => !updatedEpisode.Milestones.Any(u => u.Key == m.Key))
                            .ToList();
                        _context.RemoveRange(toRemoveMilestones);

                        foreach (var updatedMilestone in updatedEpisode.Milestones)
                        {
                            var existingMilestone = existingEpisode.Milestones
                                .SingleOrDefault(m => m.Key == updatedMilestone.Key);

                            if (existingMilestone != null)
                            {
                                existingMilestone.Milestone = updatedMilestone.Milestone;
                                existingMilestone.EpisodeKey = existingEpisode.Key;
                            }
                            else
                            {
                                // Add new milestone
                                updatedMilestone.EpisodeKey = existingEpisode.Key;
                                existingEpisode.Milestones.Add(updatedMilestone);
                            }
                        }
                    }
                }
                else
                {
                    if (updatedEpisode.Milestones != null)
                    {
                        foreach (var milestone in updatedEpisode.Milestones)
                            milestone.EpisodeKey = updatedEpisode.Key;
                    }
                    existing.Episodes.Add(updatedEpisode);
                }
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}