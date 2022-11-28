using Mikrotik_DNS_Lookup.Models;
using System.Text.Json;
using tik4net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/GetAllDNSRecords",  () =>
{
    DNSRecords records = new DNSRecords();

    using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api)) // Use TikConnectionType.Api for mikrotikversion prior v6.45
    {
        connection.Open("ipAddress", "User", "Password");
        ITikCommand cmd = connection.CreateCommand("/ip/dns/static/print");
        var identity = cmd.ExecuteList();

        foreach (var item in identity)
        {
            records.DNSRecordList.Add(new DNS(
                item.Words.First(i => i.Key == "name").Value,
                item.Words.First(i => i.Key == "address").Value,
                item.Words.FirstOrDefault(i => i.Key == "comment").Value));
        }
        var resultJSON = JsonSerializer.Serialize<List<DNS>>(records.DNSRecordList);
        return Results.Json(resultJSON);   
    }
})

app.Run();

