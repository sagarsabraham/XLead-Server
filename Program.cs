using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using XLead_Server.Configuration;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.Repositories;
using XLead_Server.Services;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; });
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseWrapperFilter>();
})
.AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; });

builder.Services.AddHttpClient("GeminiClient");

builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy => policy
            .WithOrigins("http://localhost:4200","http://172.16.4.89:9011") 
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<ApiDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDbSchemaRepository, DbSchemaRepository>();
builder.Services.AddScoped<IAiQueryGeneratorRepository, AiQueryGeneratorRepository>();
builder.Services.AddScoped<ISqlValidationService, SqlValidationService>();
builder.Services.AddScoped<IDataQueryRepository, DapperDataQueryRepository>();

builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<IUserPrivilegeRepository, UserPrivilegeRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IDealRepository, DealRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IRevenueType, RevenueTypeRepository>();
builder.Services.AddScoped<IServiceline,ServicelineRepository>();
builder.Services.AddScoped<IIndustryVertical, IndustryVerticalRepository>();
builder.Services.AddScoped<IDuRepository, DuRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IDomainRepository, DomainRepository>();
builder.Services.AddScoped<IDealStageRepository, DealStageRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();

builder.Services.AddHttpClient("GeminiClient");

builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "UploadedFiles")), // Use builder.Environment instead of env
    RequestPath = "/UploadedFiles" // This makes them accessible via /UploadedFiles URL path
});

if (app.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
    app.UseSwagger();
    app.UseSwaggerUI();
}

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "UploadedFiles");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/UploadedFiles" // The URL path to access the files
});

app.UseCors("AllowAll");
app.UseCors("AllowAngularApp");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }