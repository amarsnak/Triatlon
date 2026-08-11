// Program.cs
using Triatlon.Data;

var builder = WebApplication.CreateBuilder(args);

// Dodaj storitve
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registriraj DbHelper kot singleton
builder.Services.AddSingleton<DbHelper>();

// CORS - dovoli vse (za testiranje)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Swagger UI (samo v razvoju)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
