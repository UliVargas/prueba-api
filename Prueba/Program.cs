using Microsoft.EntityFrameworkCore;
using Prueba.Data;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { status = "ok", message = "API viva" }));

app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html"));
app.MapGet("/swagger/index.html", () => Results.Content(
    """
    <!doctype html>
    <html>
    <head>
      <meta charset="utf-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1" />
      <title>API Docs</title>
      <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css" />
    </head>
    <body>
      <div id="swagger-ui"></div>
      <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
      <script>
        window.ui = SwaggerUIBundle({
          url: '/openapi/v1.json',
          dom_id: '#swagger-ui'
        });
      </script>
    </body>
    </html>
    """,
    "text/html"));

app.MapControllers();

app.Run();
