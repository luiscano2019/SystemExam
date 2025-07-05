//using System;

//namespace SystemExamWeb.Models;

//public class DashboardModel
//{
//    public string UserName { get; set; } = "Juan Pérez";
//    public int CompletedExams { get; set; } = 5;
//    public int TotalExams { get; set; } = 12;
//    public DateTime LastActivity { get; set; } = new DateTime(2025, 6, 14);

//    public double Progress => TotalExams == 0 ? 0 : (double)CompletedExams / TotalExams;

//    public List<CategoryInfo> Categories { get; set; } = new()
//    {
//        new CategoryInfo { Name = "Matemáticas", ExamCount = 3, Icon = "📄" },
//        new CategoryInfo { Name = "Ciencias", ExamCount = 4, Icon = "🔖" },
//        new CategoryInfo { Name = "Historia", ExamCount = 5, Icon = "🏛️" }
//    };

//    public List<ExamInfo> Exams { get; set; } = new()
//    {
//        new ExamInfo { Title = "Prueba 1: Álgebra", Duration = 30, Status = "Completado", LastScore = "85", LastDate = "" },
//        new ExamInfo { Title = "Prueba 2: Geometría", Duration = 30, Status = "No iniciado", LastScore = "70", LastDate = "" },
//        new ExamInfo { Title = "Prueba 3: Cálculo", Duration = 45, Status = "En progreso", LastScore = "10 junio", LastDate = "" }
//    };
//}

//public class CategoryInfo
//{
//    public string Name { get; set; } = "";
//    public int ExamCount { get; set; }
//    public string Icon { get; set; } = "";
//}

//public class ExamInfo
//{
//    public string Title { get; set; } = "";
//    public int Duration { get; set; }
//    public string Status { get; set; } = "";
//    public string LastScore { get; set; } = "";
//    public string LastDate { get; set; } = "";
//}