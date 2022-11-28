using Mikrotik_DNS_Lookup.Models;
using System.Text;
using System.Text.Json;
using tik4net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

IConfiguration _configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, true)
    .Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/GetAllDNSRecords", () =>
{
    DNSRecords records = new DNSRecords();

    using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api)) // Use TikConnectionType.Api for mikrotikversion prior v6.45
    {
        try
        {
            connection.Open(_configuration.GetValue<string>("Appsettings:Address"), _configuration.GetValue<string>("Appsettings:User"), _configuration.GetValue<string>("Appsettings:Password"));
            ITikCommand cmd = connection.CreateCommand("/ip/dns/static/print");
            var identity = cmd.ExecuteList();

            foreach (var item in identity)
            {
                records.DNSRecordList.Add(new DNS(
                    item.Words.FirstOrDefault(i => i.Key == "name").Value,
                    item.Words.FirstOrDefault(i => i.Key == "address").Value,
                    item.Words.FirstOrDefault(i => i.Key == "comment").Value));
            }
            var resultJSON = JsonSerializer.Serialize<List<DNS>>(records.DNSRecordList);
            return Results.Json(resultJSON);
        }
        catch (Exception)
        {
            return Results.Problem("[resutl:\"Mikrotik returned error code!\"]");
        }
    }
});


app.MapPost("/AddDNSRecord", async (DNS dns) =>
{
    try
    {
        using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api)) // Use TikConnectionType.Api for mikrotikversion prior v6.45
        {
            connection.Open(
                _configuration.GetValue<string>("Appsettings:Address"),
                _configuration.GetValue<string>("Appsettings:User"),
                _configuration.GetValue<string>("Appsettings:Password")
                );
            ITikCommand cmd = connection.CreateCommand("/ip/dns/static/add",
                connection.CreateParameter("name", dns.Name),
                connection.CreateParameter("address", dns.Address),
                connection.CreateParameter("comment", dns.Comment));
            cmd.ExecuteScalar();
            return Results.Accepted();
        }
    }
    catch (Exception)
    {
        return Results.BadRequest();
    }
});

app.Run();

