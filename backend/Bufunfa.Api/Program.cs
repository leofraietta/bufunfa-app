using Microsoft.EntityFrameworkCore;
using Bufunfa.Api.Data;
using Bufunfa.Api.Services;
using Bufunfa.Api.Factories;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar formatação de data para padrão brasileiro
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new Bufunfa.Api.Converters.BrazilianDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new Bufunfa.Api.Converters.BrazilianNullableDateTimeConverter());
        
        // Configurar para evitar ciclos de referência
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        
        // Configurar cultura brasileira para formatação de números
        var cultureInfo = new System.Globalization.CultureInfo("pt-BR");
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura o DbContext para usar PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar serviços
builder.Services.AddScoped<FolhaMensalService>();
builder.Services.AddScoped<ICartaoCreditoService, CartaoCreditoService>();
builder.Services.AddScoped<IProvisionamentoService, ProvisionamentoService>();

// Registrar novos serviços para lançamentos e folhas
builder.Services.AddScoped<ILancamentoProcessorService, LancamentoProcessorService>();
builder.Services.AddScoped<IFolhaAutomaticaService, FolhaAutomaticaService>();
builder.Services.AddScoped<ILancamentoFactory, LancamentoFactory>();

// Registrar serviços para contas conjuntas e cartões compartilhados
builder.Services.AddScoped<IContaConjuntaService, ContaConjuntaService>();
builder.Services.AddScoped<ICartaoCompartilhadoService, CartaoCompartilhadoService>();

// Configura CORS para permitir requisições do frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configura a autenticação
builder.Services.AddAuthentication(options =>
{
    // JWT Bearer como esquema padrão para APIs
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    // Cookie como esquema padrão para sign-in (necessário para OAuth2)
    options.DefaultSignInScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    // Configurações mínimas para OAuth2 callback
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    googleOptions.CallbackPath = "/signin-google";
    
    // Configurações adicionais para evitar erros
    googleOptions.SaveTokens = true;
    googleOptions.Scope.Add("email");
    googleOptions.Scope.Add("profile");
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disable HTTPS redirection in development to avoid OAuth2 callback issues
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Adiciona CORS antes da autenticação
app.UseCors("AllowFrontend");

// Middleware de tratamento global de exceções
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        Console.WriteLine("Exception handler middleware executado");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Erro interno do servidor");
    });
});

// Temporariamente desabilitar autenticação para debug
// app.UseAuthentication(); 
// app.UseAuthorization();

app.MapControllers();

Console.WriteLine("Backend iniciado em http://localhost:5000");
Console.WriteLine("Swagger disponível em http://localhost:5000/swagger");
Console.WriteLine("Aguardando requisições...");

app.Run();


