using System;
using System.Collections.Generic;

namespace SystemExamWeb.Models;

public partial class Category
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Color { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    //public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
