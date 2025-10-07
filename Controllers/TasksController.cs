using ApiJwtEfSQL.Data;
using ApiJwtEfSQL.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = ApiJwtEfSQL.Models.Task;

namespace ApiJwtEfSQL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Require login
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("CreateTask")]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto dto)
        {
            var userId = Convert.ToInt32(User.FindFirst("uid")?.Value); // from JWT (string)

            if (userId == null)
                return Unauthorized();

            var task = new Task
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Priority = dto.Priority,
                UserId = userId,
                Completed = false
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [HttpGet("GetTask/{id}")]
        public async Task<ActionResult<Task?>> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return task;
        }

        // GET: api/tasks
        [HttpGet("GetAllTasks")]
        public async Task<IActionResult> GetAllTasks()
        {
            var userId = Convert.ToInt32(User.FindFirst("uid")?.Value); // from JWT (string)

            if (userId == null)
                return Unauthorized();

            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();
            return Ok(tasks);
        }

        // PUT: api/tasks/{id}
        [HttpPut("UpdateTask/{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskUpdateDto dto)
        {
            var userId = Convert.ToInt32(User.FindFirst("uid")?.Value); // from JWT (string)

            if (userId == null)
                return Unauthorized();

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null) return NotFound();

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;
            task.Priority = dto.Priority;
            task.Completed = dto.Completed;

            await _context.SaveChangesAsync();
            return Ok(task);
        }

        // DELETE: api/tasks/{id}
        [HttpDelete("DeleteTask/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = Convert.ToInt32(User.FindFirst("uid")?.Value); // from JWT (string)

            if (userId == null)
                return Unauthorized();

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null) return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
