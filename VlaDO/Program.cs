using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VlaDO;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Repositories.Rooms;
using VlaDO.Services;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────── infrastructure
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ───── database  (SQLite; строка в appsettings.json)
builder.Services.AddDbContext<DocumentFlowContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default"))); // "Data Source=VlaDO.db"

// ─────────────────────────────────────────────── data layer
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ─────────────────────────────────────────────── business layer
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IShareService, ShareService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

// ─────────────────────────────────────────────── JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// ─────────────────────────────────────────────── static files (ресурсы фронта)
builder.Services.AddDirectoryBrowser();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

// ───────── init database (EnsureCreated / seed)
DatabaseInitializer.EnsureDatabaseCreated(app.Services);

// ───────── middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();      // wwwroot / Resources
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
