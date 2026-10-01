using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TallerMecanicoAPI.Data;
using QuestPDF.Infrastructure;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Repositories;
using TallerMecanicoAPI.Services;

//QuesrPDF License
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// ---- Base de datos ----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- Controllers y Swagger ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TallerMecanico API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingresar: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()

    });
});

// ---- JWT ----
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<JwtTokenGenerator>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

builder.Services.AddAuthorization();

// ---- Repositories ----
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepositoryEF>();
builder.Services.AddScoped<IClienteRepository, ClienteRepositoryEF>();
builder.Services.AddScoped<IVehiculoRepository, VehiculoRepositoryEF>();
builder.Services.AddScoped<IOrdenTrabajoRepository, OrdenTrabajoRepositoryEF>();
builder.Services.AddScoped<IDiagnosticoRepository, DiagnosticoRepositoryEF>();
builder.Services.AddScoped<IReparacionRepository, ReparacionRepositoryEF>();
builder.Services.AddScoped<IRepuestoRepository, RepuestoRepositoryEF>();
builder.Services.AddScoped<IReparacionRepuestoRepository, ReparacionRepuestoRepositoryEF>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepositoryEF>();
builder.Services.AddScoped<ICatalogoRepository, CatalogoRepositoryEF>();

// ---- Services ----
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IVehiculoService, VehiculoService>();
builder.Services.AddScoped<IOrdenTrabajoService, OrdenTrabajoService>();
builder.Services.AddScoped<IDiagnosticoService, DiagnosticoService>();
builder.Services.AddScoped<IReparacionService, ReparacionService>();
builder.Services.AddScoped<IRepuestoService, RepuestoService>();
builder.Services.AddScoped<IReparacionRepuestoService, ReparacionRepuestoService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();

var app = builder.Build();

// ---- Pipeline HTTP ----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TallerMecanico API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
