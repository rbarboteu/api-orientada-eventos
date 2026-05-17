using MassTransit;
using HighwayMonitor.Contracts;
using HighwayMonitor.Worker.Repositories;

namespace HighwayMonitor.Worker.Consumers;

public class AcidenteConsumer : IConsumer<AcidenteRecebidoEvent>
{
    private readonly ILogger<AcidenteConsumer> _logger;
    private readonly AcidenteRepository _repository;

    public AcidenteConsumer(ILogger<AcidenteConsumer> logger, AcidenteRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<AcidenteRecebidoEvent> context)
    {
        var evento = context.Message;

        _logger.LogInformation(
            "Acidente recebido | Id: {Id} | Rodovia: {Rodovia} | Km: {Km}",
            evento.Id, evento.Rodovia, evento.Km);

        var vitimasProcessadas = evento.Vitimas.Select(v => new VitimaProcessada(
            NomeSanitizado: v.NomeRaw.Trim(),
            Documento: SanitizarDocumento(v.DocumentoRaw, v.TipoDocumento),
            TipoDocumento: v.TipoDocumento.ToUpper()
        )).ToList();

        foreach (var vitima in vitimasProcessadas)
        {
            _logger.LogInformation(
                "  Vítima: {Nome} | Documento sanitizado: {Documento}",
                vitima.NomeSanitizado, vitima.Documento);
        }

        await _repository.SalvarAsync(evento, vitimasProcessadas);

        _logger.LogInformation("Acidente {Id} salvo no banco com sucesso.", evento.Id);
    }

    private static string SanitizarDocumento(string documentoRaw, string tipoDocumento)
    {
        var apenasDigitos = new string(documentoRaw.Where(char.IsDigit).ToArray());

        return tipoDocumento.ToUpper() switch
        {
            "CPF" => FormatarCpf(apenasDigitos),
            "RG" => apenasDigitos,
            _ => apenasDigitos
        };
    }

    private static string FormatarCpf(string digitos)
    {
        if (digitos.Length != 11)
            return digitos;

        return $"{digitos[..3]}.{digitos[3..6]}.{digitos[6..9]}-{digitos[9..]}";
    }
}