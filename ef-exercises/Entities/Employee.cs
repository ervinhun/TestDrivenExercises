using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace tests.Entities;

public class Employee
{
    [NotNull]
    public int Id { get; set; }
    [MinLength(1)]
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public double Salary { get; set; }
    public DateTime HireDate { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}