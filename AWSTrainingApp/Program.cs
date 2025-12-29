using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// APIs

app.MapGet("/api/user/data", async () =>
    {
        string? connectionString = builder.Configuration.GetConnectionString("Default");
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        const string sql = @"SELECT id, username FROM public.users;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var rows = new List<Dictionary<string, object?>>();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>(reader.FieldCount);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                row[name] = value;
            }
            rows.Add(row);
        }

        return Results.Ok(rows);
    }
).WithName("GetUsers")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status500InternalServerError);

app.Run();
