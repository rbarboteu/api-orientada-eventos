namespace HighwayMonitor.Contracts;

public record InspecaoRecebidaEvent(
    Guid Id,
    string Rodovia,
    string KmInicial,
    string KmFinal,
    string FiscalId,
    List<ItemChecklistDto> Checklist,
    DateTime RecebidoEm
);

public record ItemChecklistDto(
    int Ordem,
    string Descricao,
    bool Conforme
);