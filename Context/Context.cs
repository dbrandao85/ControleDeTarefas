using Microsoft.EntityFrameworkCore;

namespace ControleDeTarefas.Context
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) 
            : base(options) => Database.EnsureCreated();

        public DbSet<Model.Task> Task { get; set; }
    }
}
