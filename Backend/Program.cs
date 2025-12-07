using Backend.Data;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Text;
using Backend.Models;
using Microsoft.AspNetCore.Identity;


// Explicitly load .env from current directory
DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

// Load DB connection string
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(rawConnectionString))
    throw new InvalidOperationException("Database connection string 'DefaultConnection' is missing.");

var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
if (string.IsNullOrWhiteSpace(dbPassword))
{
    Console.WriteLine("ERROR: DB_PASSWORD not loaded from .env!");
    throw new InvalidOperationException("DB_PASSWORD not loaded from .env");
}

var connectionString = rawConnectionString.Replace("${DB_PASSWORD}", dbPassword);
Console.WriteLine($"DB_PASSWORD: {dbPassword}");
Console.WriteLine($"ConnectionString: {connectionString}");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

//..............
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();


// Add HttpClient
builder.Services.AddHttpClient();

// Add JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev_secret_change_me";
var secretBytes = Encoding.UTF8.GetBytes(jwtSecret);
if (secretBytes.Length < 32)
{
    secretBytes = System.Security.Cryptography.SHA256.HashData(secretBytes);
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretBytes),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// Register repositories
builder.Services.AddScoped<DoctorRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IDoctorScheduleRepository, DoctorScheduleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();


// Register services
builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IEmailService, EmailServiceFixed>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();


// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Bind to Azure's PORT environment variable or default to 5001 for local development
var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Disabled for development

// Use CORS
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-create/update database and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        
        // Seed data if empty
        if (!db.Doctors.Any())
        {
            var doctors = new[]
            {
                new Doctor { FullName = "Dr. John Smith", Specialization = "Cardiology", NIC = "123456789V", Qualification = "MBBS, MD", Email = "john@hospital.com", ContactNo = "0771234567", Details = "Experienced cardiologist" },
                new Doctor { FullName = "Dr. Sarah Johnson", Specialization = "Dermatology", NIC = "987654321V", Qualification = "MBBS, MD", Email = "sarah@hospital.com", ContactNo = "0779876543", Details = "Skin specialist" },
                new Doctor { FullName = "Dr. Mike Wilson", Specialization = "Orthopedics", NIC = "456789123V", Qualification = "MBBS, MS", Email = "mike@hospital.com", ContactNo = "0775555555", Details = "Bone and joint specialist" }
            };
            db.Doctors.AddRange(doctors);
            db.SaveChanges();
            
            // Add schedules for each doctor
            var schedules = new[]
            {
                new DoctorSchedule { DoctorId = 1, ScheduleDate = DateTime.Today.AddDays(1), StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(12), TotalSlots = 10, AvailableSlots = 10 },
                new DoctorSchedule { DoctorId = 1, ScheduleDate = DateTime.Today.AddDays(2), StartTime = TimeSpan.FromHours(14), EndTime = TimeSpan.FromHours(17), TotalSlots = 8, AvailableSlots = 8 },
                new DoctorSchedule { DoctorId = 2, ScheduleDate = DateTime.Today.AddDays(1), StartTime = TimeSpan.FromHours(10), EndTime = TimeSpan.FromHours(13), TotalSlots = 12, AvailableSlots = 12 },
                new DoctorSchedule { DoctorId = 3, ScheduleDate = DateTime.Today.AddDays(3), StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(11), TotalSlots = 6, AvailableSlots = 6 }
            };
            db.DoctorSchedules.AddRange(schedules);
            db.SaveChanges();
            Console.WriteLine("Sample data seeded successfully");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database setup failed: {ex.Message}");
    }
}

Console.WriteLine($"App running on port {port}");
app.Run();

// Make Program class accessible for testing
public partial class Program { }