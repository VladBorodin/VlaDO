using Microsoft.EntityFrameworkCore;
using VlaDO.Repositories;
using VlaDO;
using VlaDO.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DocumentFlowContext>(options => options.UseSqlite("Data Source=VlaDO.db"));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<AuthService>();

var app = builder.Build();

DatabaseInitializer.EnsureDatabaseCreated(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
