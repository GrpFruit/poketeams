using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Read the colon form; fall back to env var name; default to container DNS "redis:6379"
var redisConn =
    builder.Configuration.GetValue<string>("REDIS:CONNECTIONSTRING")
    ?? Environment.GetEnvironmentVariable("REDIS__CONNECTIONSTRING")
    ?? "redis:6379";

builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);

// Ensure PokeAPI base ends with '/'
var baseUrl = builder.Configuration["POKEAPI__BASEURL"] ?? "https://pokeapi.co/api/v2/";
if (!baseUrl.EndsWith("/")) baseUrl += "/";
builder.Services.AddHttpClient("pokeapi", c => c.BaseAddress = new Uri(baseUrl));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

// GET /pokemon?offset=0&limit=20
app.MapGet("/pokemon", async (
        IHttpClientFactory http,
        IDistributedCache   cache,
        ILogger<Program>    log,
        int                 offset = 0,
        int                 limit  = 20) =>
{
    var key = $"pokemon:list:{offset}:{limit}";
    var cached = await cache.GetStringAsync(key);
    if (cached is not null) return Results.Content(cached, "application/json");

    var client   = http.CreateClient("pokeapi");
    var response = await client.GetAsync($"pokemon?offset={offset}&limit={limit}");
    var json     = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
        return Results.Content(json,
            response.Content.Headers.ContentType?.ToString() ?? "application/json",
            statusCode: (int)response.StatusCode);

    await cache.SetStringAsync(key, json, new DistributedCacheEntryOptions {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    });

    return Results.Content(json, "application/json");
});

// GET /pokemon/{name}
app.MapGet("/pokemon/{name}", async (string name, IHttpClientFactory http, IDistributedCache cache) =>
{
    var key = $"pokemon:detail:{name.ToLower()}"; // fixed typo from "datail"
    var cached = await cache.GetStringAsync(key);
    if (cached is not null) return Results.Content(cached, "application/json");

    var client = http.CreateClient("pokeapi");
    var response = await client.GetAsync($"pokemon/{name}");
    var json = await response.Content.ReadAsStringAsync();
    if (!response.IsSuccessStatusCode)
        return Results.Content(json, response.Content.Headers.ContentType?.ToString() ?? "application/json",
                               statusCode: (int)response.StatusCode);

    await cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
    { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });

    return Results.Content(json, "application/json");
});

app.Run();
