namespace HighwayMonitor.Contracts;

public record AcidenteRecebidoEvent(
    Guid Id,
    string Rodovia,
    string Km,
    string TipoAcidente,
    List<VitimaDto> Vitimas,
    DateTime RecebidoEm
);

public record VitimaDto(
    string NomeRaw,
    string DocumentoRaw,
    string TipoDocumento
);