using UserManagementApi.Middleware;
using UserManagementApi.Repositories;
using UserManagementApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar servicios a la inyección de dependencias (IoC Container)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "User Management API",
        Version = "v1",
        Description = "REST API en .NET 8 para gestión de usuarios con SQL Server, Dapper y arquitectura limpia por capas."
    });
});

// 2. Registrar ciclo de vida de Repositorios y Servicios (Scoped)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// 3. Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
        c.RoutePrefix = string.Empty; // Carga Swagger directamente en la raíz (http://localhost:PORT/)
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();