﻿namespace SystemExamWeb.Responses
{
    public class CategoryResponse
    {
        
            public Guid Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string Color { get; set; } = "";
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        
    }
}
