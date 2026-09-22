using Microsoft.EntityFrameworkCore;
using PlantControlWeb.Data;
using PlantControlWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Services
builder.Services.AddControllers();

builder.Services.AddScoped<EnvironmentEvaluationService>();

// PostgreSQL
builder.Services.AddDbContext<PlantControlDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PlantControl")
    ));

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseDefaultFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
