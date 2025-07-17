using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemExamWeb.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Dni { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    //[Column("Role")]
    //public UserRole UserRole { get; set; }

    //// .Propiedad calculada para compatibilidad con JSON - NO mapeada a BD
    //[NotMapped]
    //[JsonIgnore]
    //public string Role => UserRole.GetDescription();

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
    
    [JsonIgnore]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
