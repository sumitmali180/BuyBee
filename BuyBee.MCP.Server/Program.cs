using EMart.MCP.Server.Repositories;
using EMart.MCP.Server.Repositories.Implementations;
using EMart.MCP.Server.Repositories.Interfaces;
using EMart.MCP.Server.Services.Implementations;
using EMart.MCP.Server.Services.Interfaces;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// DEPENDENCY INJECTION
// ============================================================

builder.Services.AddScoped<
    ICategoryRepository,
    CategoryRepository>();

builder.Services.AddScoped<
    ICategoryService,
    CategoryService>();

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository>();

builder.Services.AddScoped<
    IProductService,
    ProductService>();


// ============================================================
// CONTROLLERS
// ============================================================

builder.Services.AddControllers();


// ============================================================
// SWAGGER
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


// ============================================================
// MCP SERVER
// ============================================================

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithToolsFromAssembly();


var app = builder.Build();


// ============================================================
// SWAGGER
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// ============================================================
// HTTPS
// ============================================================

app.UseHttpsRedirection();


// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();


// ============================================================
// EMART TEST API
// ============================================================

app.MapGet(
    "/api/emart/welcome",
    () =>
    {
        return Results.Ok(new
        {
            success = true,
            message = "Welcome to EMart MCP Server"
        });
    })
    .WithName("GetWelcome")
    .WithTags("EMart");


// ============================================================
// MCP ENDPOINT
// ============================================================

app.MapMcp("/mcp");


// ============================================================
// RUN
// ============================================================

app.Run();