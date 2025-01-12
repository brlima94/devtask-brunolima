using DevTask.DiscountService;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.UseUtcTimestamp = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(new DiscountService.DiscountServiceClient(GrpcChannel.ForAddress(builder.Configuration["DiscountServiceEndpoint"]!)));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseDeveloperExceptionPage();

//note: using Get and query strings to facilitate testing through any web browser

app.MapGet("/discount/generate",
    async (DiscountService.DiscountServiceClient client, [FromQuery] uint numberOfCodes, [FromQuery] uint length) =>
    {
        var result = await client.GenerateAsync(new GenerateRequest
        {
            Count = numberOfCodes,
            Length = length
        });
        return result;
    });

app.MapGet("/discount/use/{code}",
    async (DiscountService.DiscountServiceClient client, [FromRoute] string code) =>
    {
        var result = await client.UseCodeAsync(new UseCodeRequest
        {
            Code = code
        });
        return result;
    });

await app.RunAsync();