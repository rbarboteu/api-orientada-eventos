using MassTransit;
using HighwayMonitor.Contracts;
using HighwayMonitor.Worker.Repositories;

namespace HighwayMonitor.Worker.Consumers;

public class InspecaoConsumer : IConsumer<InspecaoRecebidaEvent>
{
    private readonly ILogger<InspecaoConsumer> _logger;
    private readonly InspecaoRepository _repository;

    public InspecaoConsumer(ILogger<InspecaoConsumer> logger, InspecaoRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<InspecaoRecebidaEvent> context)
    {
        var evento = context.Message;

        _logger.LogInformation(
            "Inspeção recebida | Id: {Id} | Rodovia: {Rodovia} | Fiscal: {FiscalId}",
            evento.Id, evento.Rodovia, evento.FiscalId);

        var checklistOrdenado = evento.Checklist.OrderBy(x => x.Ordem).ToList();
        var ordemEsperada = 1;

        foreach (var item in checklistOrdenado)
        {
            if (item.Ordem != ordemEsperada)
            {
                _logger.LogError(
                    "Checklist inválido! Esperava ordem {Esperado}, mas veio {Recebido}",
                    ordemEsperada, item.Ordem);

                throw new InvalidOperationException(
                    $"Ordem do checklist inválida. Esperado: {ordemEsperada}, Recebido: {item.Ordem}");
            }

            _logger.LogInformation(
                "  Passo {Ordem}: {Descricao} -> {Status}",
                item.Ordem, item.Descricao, item.Conforme ? "CONFORME" : "NÃO CONFORME");

            ordemEsperada++;
        }

        await _repository.SalvarAsync(evento);

        _logger.LogInformation("Inspeção {Id} salva no banco com sucesso.", evento.Id);
    }
}