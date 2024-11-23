using BlogAPI.Data;
using BlogAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "TestPolicy", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
    options.AddPolicy(name: "CorsPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowCredentials();
        });
});
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BlogContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("BlogContext"),
        new MySqlServerVersion(new Version(8, 0, 39))
    )
);
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
    })
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
        options.Events.OnRedirectToLogin = _ => throw new UnauthorizedAccessException();
        options.Events.OnRedirectToAccessDenied = _ => throw new UnauthorizedAccessException();
    });
builder.Services.AddAuthorization();

builder.Services.AddIdentityCore<BlogUser>()
    .AddEntityFrameworkStores<BlogContext>()
    .AddApiEndpoints();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.MapIdentityApi<BlogUser>();

app.MapPost("/logout",
        async (SignInManager<BlogUser> signInManager) => { await signInManager.SignOutAsync().ConfigureAwait(false); })
    .RequireAuthorization();

app.MapControllers();

app.Run();