



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(); // Importante
builder.Services.AddEndpointsApiExplorer(); // Para Swagger
builder.Services.AddSwaggerGen(); // Swagger

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger
    app.UseSwaggerUI(); // Swagger UI
}

//app.UseHttpsRedirection(); // Podés comentarlo si quitaste HTTPS
app.UseAuthorization();
app.MapControllers();

app.Run();
