using LoginApp.Data;
using LoginApp.Middleware;
using LoginApp.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    if (context.HostingEnvironment.IsProduction())
    {
        serverOptions.ListenLocalhost(5033, listenOptions =>
        {
            listenOptions.UseHttps();
        });
    } else
    {
        serverOptions.ListenLocalhost(5033);
    }
});

Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("app-log.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddDbContext<AppDatabaseContext>(dbContext =>
{
    dbContext.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});

builder.Services.AddControllers();
builder.Services.ServicesRegister();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowCors",
//                      policy => policy.WithOrigins("http://localhost:3000")
//                    .AllowAnyHeader()
//                    .AllowAnyMethod()
//                    .AllowCredentials()
//    );
//});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
    };
});
builder.Services.AddAuthentication();

builder.Services.Configure<GmailOptions> (builder.Configuration.GetSection(GmailOptions.GmailOptionsKey));

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pages")),
    RequestPath = "" 
});
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new[] { "pages/index.html" }
});


app.UseDefaultFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseCors("AllowCors");
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapStaticAssets();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
