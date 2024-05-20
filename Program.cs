using ControleDeTarefas.Context;
using ControleDeTarefas.Dtos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Connection");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>
    (options => options.UseMySQL(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseCors("AllowLocalhost3000");

app.MapPost("AddTask", async (TaskDto taskDto, Context context) =>
{
    if (taskDto.File is not null && taskDto.File.Length > 0)
    {
        var filePath = Path.Combine("/arquivos", taskDto.File.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await taskDto.File.CopyToAsync(stream);
        }
    }

    var task = new ControleDeTarefas.Model.Task
    {
        Title = taskDto.Title,
        FileName = taskDto.File?.FileName ?? "",
        Sla = taskDto.Sla
    };

    context.Task.Add(task);
    await context.SaveChangesAsync();

    return Results.Created("Criado com sucesso", task);
});

app.MapDelete("DeleteTask/{id}", async (int id, Context context) =>
{
    var taskToDelete = await context.Task.FirstOrDefaultAsync(task => task.Id == id);

    if (taskToDelete is null)
    {
        return Results.NotFound();
    }

    context.Task.Remove(taskToDelete);
    await context.SaveChangesAsync();

    return Results.Ok(taskToDelete);
});

app.MapGet("GetTasks", async (Context context) =>
{
    var tasks = await context.Task.ToListAsync();
    var taskDto = new List<TaskDto>();

    foreach (var task in tasks)
    {
        taskDto.Add(new TaskDto { 
            Id = task.Id,
            Title = task.Title,
            Sla = task.Sla,
            FileName = task.FileName,
            IsPastDue = task.Sla < DateTime.Now });
    }

    return taskDto;
});



app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();