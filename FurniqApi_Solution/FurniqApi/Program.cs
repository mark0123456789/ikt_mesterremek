using FurniqApi.Models;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<FurniqContext>(options =>
    options.UseMySql(connectionString, ServerVersion.Create(10, 4, 32, Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MariaDb)));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Frontendhez (pl. külön futó webes felület) engedjük a kéréseket
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();   // Scalar felület: /scalar/v1

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
