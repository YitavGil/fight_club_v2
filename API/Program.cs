using API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<DataContext>(option => 
{
    option.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();
Console.WriteLine($"Current environment: {app.Environment.EnvironmentName}");
// Configure the HTTP request pipeline.
app.MapControllers();

app.Run();
