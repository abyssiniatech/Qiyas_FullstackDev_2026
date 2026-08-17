// namespace Tms.Api.Dtos;

// public class StudentResponse
// {
//     public int Id { get; set; }


//     public string Name { get; set; } = string.Empty;


//     public string Email { get; set; } = string.Empty;
// }


// namespace Tms.Api.Dtos;

// public class StudentResponse
// {
//     public int Id { get; set; }

//     public string FirstName { get; set; } = string.Empty;

//     public string LastName { get; set; } = string.Empty;

//     public string Email { get; set; } = string.Empty;

//     public DateOnly DateOfBirth { get; set; }
// }




public class StudentResponseDto
{
    public required object Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public double GPA { get; set; }
    public bool IsActive { get; set; }
}
