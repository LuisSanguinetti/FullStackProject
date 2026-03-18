
var builder = WebApplication.CreateBuilder(args);

// CORS
const string CorsPolicy = "AllowAngular";
builder.Services.AddCors(o =>
{
    o.AddPolicy(CorsPolicy, p => p
        .WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

obligatorio.WebApi.ServiceCollectionExtensions.AddDataAccess(builder.Services, builder.Configuration);
obligatorio.WebApi.ServiceCollectionExtensions.AddBusinessServices(builder.Services);
obligatorio.WebApi.ServiceCollectionExtensions.AddApiDefaults(builder.Services);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Configure the HTTP request pipeline.

// app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
