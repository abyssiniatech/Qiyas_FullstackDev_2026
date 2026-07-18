namespace TmsApi.Entities;

public class Course
{
    public int Id { get; set; }


    public string Code { get; set; } = string.Empty;


    public string Title { get; set; } = string.Empty;


    public string Description { get; set; } = string.Empty;


    public int Capacity { get; set; }


    // If Module 6 renamed Capacity -> MaxCapacity
    // keep compatibility:
    public int MaxCapacity 
    { 
        get => Capacity; 
        set => Capacity = value; 
    }


    public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();
}