using ApiConventions.CommunityToolKit.Extends;
using ApiConventions.CommunityToolKit.Log4Net;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGenSetUp(new OpenApiContact
{
    Name = "wuhailong",
    Email = "13126506430@163.com"
});

builder.Services.AddFilterSetUp();
builder.Services.AddLog4Net();
builder.Services.AddAuthenticationSetUp(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerSetUp();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseRedocSetUp();

app.Run();
