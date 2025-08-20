using CustomAuth;
using CustomAuth.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

builder.Services.AddScoped<IAuditService, AuditServiceDummy>();


builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Jwt:Issuer"];
    options.Audience = builder.Configuration["Jwt:Audience"];

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = false
    };
}).AddScheme<CustomMultiAuthOptions, CustomMultiAuthHandler>(
                "CombinedScheme",
                options =>
                {
                    options.Issuer = builder.Configuration["Jwt:Issuer"];
                    options.Audience = builder.Configuration["Jwt:Audience"];
                    options.Tenant = builder.Configuration["Jwt:Tenant"];
                });


builder.Services.AddControllers(options =>
{
    // Global registration - applies to all controllers
    options.Filters.Add<AuditLogActionFilter>();
});

// Add authorization services
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
