using System;
using System.Collections.Generic;
using SystemExamWeb.Responses;

namespace SystemExamWeb.Models
{
    public class DashboardViewModel
    {
        // Lista de categor�as para mostrar en el dashboard
        public List<Category> Categories { get; set; } = new();

        //// Puedes agregar m�s propiedades seg�n lo que muestre tu dashboard, por ejemplo:
        //public string UserName { get; set; } = "";
        //public int CompletedExams { get; set; }
        //public int TotalExams { get; set; }
        //public DateTime? LastActivity { get; set; }
        //public Guid Id { get; set; }
        // Otros datos relevantes para la vista
        // public List<ExamInfo> Exams { get; set; } = new();
        // public double Progress => TotalExams == 0 ? 0 : (double)CompletedExams / TotalExams;
    }
}