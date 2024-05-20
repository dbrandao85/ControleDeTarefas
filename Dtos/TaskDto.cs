using System.ComponentModel.DataAnnotations;

namespace ControleDeTarefas.Dtos
{
    public class TaskDto
    {
        public int Id { get; set; }
        [Required(ErrorMessageResourceName ="Necessário preencher o título da tarefa")]
        public string Title { get; set; }
        [Required(ErrorMessageResourceName = "Necessário preencher o SLA da tarefa")]
        public DateTime Sla { get; set; }
        public string FileName { get; set; }
        public bool IsPastDue { get; set; }
        public IFormFile File { get; set; }
        public bool NeedNotify { get; set; }
        public bool Done { get; set; }
    }
}
