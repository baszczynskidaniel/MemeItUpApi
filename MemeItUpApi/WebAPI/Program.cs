using Application.Services;
using Core.Interfaces;
using MemeItUpApi;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowAnyOrigin", p => p
        .WithOrigins("null") 
        .AllowAnyHeader()
        .AllowCredentials());
});


builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IGameService, GameService>();

builder.Services.AddScoped<IMemeTemplateService, MemeTemplateService>();
builder.Services.AddScoped<ITextPositionService, TextPositionService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseRouting();




app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowAnyOrigin");
app.MapHub<GameHub>("/lobbyHub");

app.Run();
