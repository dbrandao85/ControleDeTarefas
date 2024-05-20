namespace ControleDeTarefas.Dtos
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Sla { get; set; }
        public string FileName { get; set; }
        public bool IsPastDue { get; set; }
        public IFormFile File { get; set; }
    }
}
