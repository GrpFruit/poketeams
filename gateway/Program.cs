using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddHttpClient(); // <- required to inject IHttpClientFactory

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

var catalog = builder.Configuration["Downstreams:Catalog"] ?? "http://catalog-service:8080";
var team    = builder.Configuration["Downstreams:TeamBuilder"] ?? "http://teambuilder-service:8080";

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// GET /api/pokemon?offset=&limit=
app.MapGet("/api/pokemon", async (HttpContext ctx, [FromServices] IHttpClientFactory http) =>
{
    var client = http.CreateClient();
    var q = ctx.Request.QueryString.Value ?? "";
    using var upstream = await client.GetAsync($"{catalog}/pokemon{q}",
        HttpCompletionOption.ResponseHeadersRead, ctx.RequestAborted);

    ctx.Response.StatusCode = (int)upstream.StatusCode;
    if (upstream.Content.Headers.ContentType is { } ct)
        ctx.Response.ContentType = ct.ToString();
    await upstream.Content.CopyToAsync(ctx.Response.Body, ctx.RequestAborted);
});

// GET /api/pokemon/{name}
app.MapGet("/api/pokemon/{name}", async (HttpContext ctx, [FromRoute] string name, [FromServices] IHttpClientFactory http) =>
{
    var client = http.CreateClient();
    using var upstream = await client.GetAsync($"{catalog}/pokemon/{name}",
        HttpCompletionOption.ResponseHeadersRead, ctx.RequestAborted);

    ctx.Response.StatusCode = (int)upstream.StatusCode;
    if (upstream.Content.Headers.ContentType is { } ct)
        ctx.Response.ContentType = ct.ToString();
    await upstream.Content.CopyToAsync(ctx.Response.Body, ctx.RequestAborted);
});

// GET /api/teams
app.MapGet("/api/teams", async (HttpContext ctx, [FromServices] IHttpClientFactory http) =>
{
    var client = http.CreateClient();
    using var upstream = await client.GetAsync($"{team}/teams",
        HttpCompletionOption.ResponseHeadersRead, ctx.RequestAborted);

    ctx.Response.StatusCode = (int)upstream.StatusCode;
    if (upstream.Content.Headers.ContentType is { } ct)
        ctx.Response.ContentType = ct.ToString();
    await upstream.Content.CopyToAsync(ctx.Response.Body, ctx.RequestAborted);
});

// POST /api/teams
app.MapPost("/api/teams", async (HttpContext ctx, [FromServices] IHttpClientFactory http) =>
{
    var client = http.CreateClient();
    using var upstream = await client.PostAsync($"{team}/teams", new StreamContent(ctx.Request.Body), ctx.RequestAborted);

    ctx.Response.StatusCode = (int)upstream.StatusCode;
    if (upstream.Content.Headers.ContentType is { } ct)
        ctx.Response.ContentType = ct.ToString();
    await upstream.Content.CopyToAsync(ctx.Response.Body, ctx.RequestAborted);
});

// DELETE /api/teams/{id}
app.MapDelete("/api/teams/{id:guid}", async (HttpContext ctx, [FromRoute] Guid id, [FromServices] IHttpClientFactory http) =>
{
    var client = http.CreateClient();
    using var upstream = await client.DeleteAsync($"{team}/teams/{id}", ctx.RequestAborted);
    ctx.Response.StatusCode = (int)upstream.StatusCode;
});

app.Run();
