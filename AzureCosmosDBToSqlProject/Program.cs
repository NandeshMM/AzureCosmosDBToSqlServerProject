using AzureCosmosDBToSqlServerProject.Middleware;
using ConfigReader.Abstraction;
using ConfigReader.Abstraction.Reader;
using DataStore.Abstraction.IDTO;
using DataStore.Abstraction.IRepositories;
using DataStore.Implementation.DTO;
using DataStore.Implementation.Repositories;
using FeatureObjects.Abstraction.IManager;
using FeatureObjects.Implementation.Manager;
using NoSql.CosmosDb.Extensions.DependencyInjection;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using AzureCosmosDBToSqlServerProject.NotificationCenter;

var builder = WebApplication.CreateBuilder(args);



builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddScoped<ICosmosFetchService, CosmosFetchService>();
builder.Services.AddScoped<IBulkinsertion, BulkinsertionService>();
builder.Services.AddScoped<ICosmosDBDataFetchingRepository, CosmosDBDataFetchingRepository>();
builder.Services.AddScoped<IcolumnFetchReop, CoulmnFetchRepo>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddSignalR();

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:5175") // Your React app
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/NotificationHub");

app.Run();
