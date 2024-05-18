using Microsoft.EntityFrameworkCore;

namespace ControleDeTarefas.Context
{
    public class TaskContext : DbContext
    {
        public TaskContext(DbContextOptions<TaskContext> options) 
            : base(options) => Database.EnsureCreated();

        public DbSet<Task> Task { get; set; }
    }
}
