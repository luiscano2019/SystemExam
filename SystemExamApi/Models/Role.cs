using System;
using System.Collections.Generic;

namespace SystemExamApi.Models
{
    public partial class Role
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; } = null!;
        
        public string Description { get; set; } = null!;
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        // Relación con usuarios
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
} 