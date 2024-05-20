using System.ComponentModel.DataAnnotations;

namespace ControleDeTarefas.Model
{
    public class Task 
    { 
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Sla { get; set; }
        public string FileName { get; set; }
        public bool Notified { get; set; }
        public bool Done { get; set; }
    }
}
