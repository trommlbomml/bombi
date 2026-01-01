using Bombi.Service;
using FlunkyBall;

var builder = WebApplication.CreateBuilder(args);

var app = builder.AddFlunkyBall<SampleGameServer>().Build();

app.Run();