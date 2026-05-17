using MassTransit;
using HighwayMonitor.Contracts;

var builder = WebApplication.CreateBuilder(args);

var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "highway_user";
var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "Highway@123456";

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });
    });
});

var app = builder.Build();

app.MapPost("/api/inspecoes", async (InspecaoPayload payload, ISendEndpointProvider sender) =>
{
    if (string.IsNullOrWhiteSpace(payload.Rodovia) || string.IsNullOrWhiteSpace(payload.FiscalId))
        return Results.BadRequest("Rodovia e FiscalId são obrigatórios.");

    var evento = new InspecaoRecebidaEvent(
        Id: Guid.NewGuid(),
        Rodovia: payload.Rodovia,
        KmInicial: payload.KmInicial,
        KmFinal: payload.KmFinal,
        FiscalId: payload.FiscalId,
        Checklist: payload.Checklist,
        RecebidoEm: DateTime.UtcNow
    );

    var endpoint = await sender.GetSendEndpoint(new Uri("queue:inspecoes-queue"));
    await endpoint.Send(evento);

    return Results.Accepted("/api/inspecoes", new { evento.Id, Mensagem = "Inspeção recebida e em processamento." });
});

app.MapPost("/api/acidentes", async (AcidentePayload payload, ISendEndpointProvider sender) =>
{
    if (string.IsNullOrWhiteSpace(payload.Rodovia) || string.IsNullOrWhiteSpace(payload.Km))
        return Results.BadRequest("Rodovia e Km são obrigatórios.");

    var evento = new AcidenteRecebidoEvent(
        Id: Guid.NewGuid(),
        Rodovia: payload.Rodovia,
        Km: payload.Km,
        TipoAcidente: payload.TipoAcidente,
        Vitimas: payload.Vitimas,
        RecebidoEm: DateTime.UtcNow
    );

    var endpoint = await sender.GetSendEndpoint(new Uri("queue:acidentes-queue"));
    await endpoint.Send(evento);

    return Results.Accepted("/api/acidentes", new { evento.Id, Mensagem = "Acidente recebido e em processamento." });
});

app.Run();

record InspecaoPayload(
    string Rodovia,
    string KmInicial,
    string KmFinal,
    string FiscalId,
    List<ItemChecklistDto> Checklist
);

record AcidentePayload(
    string Rodovia,
    string Km,
    string TipoAcidente,
    List<VitimaDto> Vitimas
);