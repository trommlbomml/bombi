using FlunkyBall;

var builder = WebApplication.CreateBuilder(args);

var app = builder.AddGameServer().Build();

app.UseHttpsRedirection();

app.Run();