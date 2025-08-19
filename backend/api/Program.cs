using api.Constants;
using api.Hubs;
using api.Startup;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddApiDocumentation();
builder.Services.AddGlobalExceptionHandling();
builder.Services.AddConfiguredControllers();

builder.Services.AddSqlServerDbContext(builder.Configuration);
builder.Services.AddDependencyInjectionServices();

// Auth configs
builder.Services.AddConfiguredIdentityCore();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddPolicyBasedAuthorization();

builder.Services.AddConfiguredCors();
builder.Services.AddSignalR();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.StartDevDatabase();

    app.UseSwagger();
    app.UseSwaggerUI();
} 
else if (app.Environment.EnvironmentName == "Test")
{
    app.UseSwagger();
    app.UseSwaggerUI();   
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors(CorsPolicyConstants.AllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<StoryHub>($"/{UrlConstants.StoryHub}");
app.MapControllers();

app.Run();

public partial class Program {  }