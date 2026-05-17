using Dapper;
using Microsoft.Data.SqlClient;
using HighwayMonitor.Contracts;

namespace HighwayMonitor.Worker.Repositories;

public class AcidenteRepository
{
    private readonly string _connectionString;

    public AcidenteRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SalvarAsync(AcidenteRecebidoEvent evento, List<VitimaProcessada> vitimas)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(@"
            INSERT INTO Acidentes (Id, Rodovia, Km, TipoAcidente, RecebidoEm, ProcessadoEm)
            VALUES (@Id, @Rodovia, @Km, @TipoAcidente, @RecebidoEm, @ProcessadoEm)",
            new
            {
                evento.Id,
                evento.Rodovia,
                evento.Km,
                evento.TipoAcidente,
                evento.RecebidoEm,
                ProcessadoEm = DateTime.UtcNow
            });

        foreach (var vitima in vitimas)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO Vitimas (Id, AcidenteId, NomeSanitizado, Documento, TipoDocumento)
                VALUES (@Id, @AcidenteId, @NomeSanitizado, @Documento, @TipoDocumento)",
                new
                {
                    Id = Guid.NewGuid(),
                    AcidenteId = evento.Id,
                    vitima.NomeSanitizado,
                    vitima.Documento,
                    vitima.TipoDocumento
                });
        }
    }
}

public record VitimaProcessada(
    string NomeSanitizado,
    string Documento,
    string TipoDocumento
);