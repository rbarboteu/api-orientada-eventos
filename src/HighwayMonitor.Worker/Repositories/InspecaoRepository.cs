using Dapper;
using Microsoft.Data.SqlClient;
using HighwayMonitor.Contracts;

namespace HighwayMonitor.Worker.Repositories;

public class InspecaoRepository
{
    private readonly string _connectionString;

    public InspecaoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SalvarAsync(InspecaoRecebidaEvent evento)
    {
        using var connection = new SqlConnection(_connectionString);

        // Salva a inspeção principal
        var inspecaoId = evento.Id;

        await connection.ExecuteAsync(@"
            INSERT INTO Inspecoes (Id, Rodovia, KmInicial, KmFinal, FiscalId, Status, RecebidoEm, ProcessadoEm)
            VALUES (@Id, @Rodovia, @KmInicial, @KmFinal, @FiscalId, @Status, @RecebidoEm, @ProcessadoEm)",
            new
            {
                Id = inspecaoId,
                evento.Rodovia,
                evento.KmInicial,
                evento.KmFinal,
                evento.FiscalId,
                Status = "PROCESSADO",
                evento.RecebidoEm,
                ProcessadoEm = DateTime.UtcNow
            });

        // Salva cada item do checklist preservando a ORDEM
        foreach (var item in evento.Checklist.OrderBy(x => x.Ordem))
        {
            await connection.ExecuteAsync(@"
                INSERT INTO ItensChecklist (Id, InspecaoId, Ordem, Descricao, Conforme)
                VALUES (@Id, @InspecaoId, @Ordem, @Descricao, @Conforme)",
                new
                {
                    Id = Guid.NewGuid(),
                    InspecaoId = inspecaoId,
                    item.Ordem,
                    item.Descricao,
                    item.Conforme
                });
        }
    }
}