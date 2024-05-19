using ControleDeTarefas.Context;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Common;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Connection");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>
    (options => options.UseMySQL(connectionString));

var app = builder.Build();

app.UseSwagger();

app.MapPost("AddTask", async (ControleDeTarefas.Model.Task task, Context context) =>
{
    context.Task.Add(task);
    await context.SaveChangesAsync();

    return Results.Created("Criado com sucesso", task);
});

app.MapPost("DeleteTask/{id}", async (int id, Context context) =>
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
    return await context.Task.ToListAsync();
});

app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();

