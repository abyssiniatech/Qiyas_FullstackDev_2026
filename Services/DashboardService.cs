using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.DTOs;


public class DashboardService
{

private readonly TmsDbContext _context;


public DashboardService(TmsDbContext context)
{
    _context=context;
}



public async Task<List<StudentDto>> GetStudentsPagedAsync(
int pageNumber,
CancellationToken cancellationToken)
{

int pageSize=20;


return await _context.Students

.OrderBy(s=>s.Name)

.Skip((pageNumber-1)*pageSize)

.Take(pageSize)

.Select(s=>new StudentDto
{
    Id=s.Id,
    Name=s.Name
})

.ToListAsync(cancellationToken);

}




public async Task<List<CourseEnrollmentSummaryDto>>
GetTopCoursesAsync(
CancellationToken cancellationToken)
{


return await _context.Courses

.GroupBy(c=>new
{
    c.Id,
    c.Title
})


.Select(g=>new CourseEnrollmentSummaryDto
{
    CourseTitle = g.Key.Title,
    EnrollmentCount = g.Count()
})


.OrderByDescending(x=>x.EnrollmentCount)

.Take(5)

.ToListAsync(cancellationToken);


}


}

public class CourseEnrollmentSummaryDto
{
    public string? CourseTitle { get; internal set; }
    public int EnrollmentCount { get; internal set; }
}