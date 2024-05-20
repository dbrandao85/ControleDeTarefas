using ControleDeTarefas.Context;
using ControleDeTarefas.Dtos;
using Microsoft.AspNetCore.Http.Features;
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

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 20_000_000;
});

var app = builder.Build();

app.UseSwagger();
app.UseCors("AllowLocalhost3000");

app.MapPost("AddTask", async (HttpContext httpContext,Context context) =>
{
    try
    {
        TaskDto taskDto = new TaskDto
        {
            Title = httpContext.Request.Form["title"],
            Sla = DateTime.Parse(httpContext.Request.Form["sla"])
        };

        if (httpContext.Request.Form.Files.Count() > 0)
        {
            taskDto.File = httpContext.Request.Form.Files[0];
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "arquivos");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, taskDto.File.FileName);
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
    }
    catch(Exception e)
    {
        return Results.BadRequest(e);
    }

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
            FileName = task.FileName == "" ? "Sem arquivo" : task.FileName,
            IsPastDue = task.Sla < DateTime.Now,
            NeedNotify = !task.Notified && task.Sla < DateTime.Now && !task.Done,
            Done = task.Done
        });
    }

    return taskDto;
});

app.MapGet("DownloadFile/{fileName}", async (string fileName) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "arquivos", fileName);

    if (File.Exists(filePath))
    {
        var memory = new MemoryStream();
        using (var stream = new FileStream(filePath, FileMode.Open))
        {
            await stream.CopyToAsync(memory);
        }
        memory.Position = 0;
        return Results.File(memory, "application/octet-stream", fileName);
    }
    else
    {
        return Results.NotFound();
    }
});

app.MapPatch("DoneTask/{id}", async (int id, Context context) =>
{
    var taskToDone = await context.Task.FirstOrDefaultAsync(task => task.Id == id);

    if (taskToDone is null)
    {
        return Results.NotFound();
    }

    taskToDone.Done = true;

    await context.SaveChangesAsync();

    return Results.Ok(taskToDone);
});

app.MapPatch("NotifiedTask/{id}", async (int id, Context context) =>
{
    var taskToNotify = await context.Task.FirstOrDefaultAsync(task => task.Id == id);

    if (taskToNotify is null)
    {
        return Results.NotFound();
    }

    taskToNotify.Notified = true;

    await context.SaveChangesAsync();

    return Results.Ok(taskToNotify);
});

app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();