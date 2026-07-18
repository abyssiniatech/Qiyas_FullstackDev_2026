using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly DbContext _dbContext;

        public StudentsController(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedStudents(int pageNumber = 1)
        {
            int pageSize = 20;

            var students = await _dbContext.Set<object>()
                .OrderBy(s => EF.Property<string>(s, "Name"))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(students);
        }
    }
}