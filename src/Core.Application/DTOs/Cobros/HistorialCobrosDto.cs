using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Application.DTOs.Cobros;

public record HistorialCobrosDto(
    IReadOnlyList<CobroListItemDto> Items,
    int TotalCount,
    int Pagina,
    int TamanoPagina,
    decimal TotalCobradoPeriodo,
    decimal ComisionesTotalesPeriodo,
    decimal SaldoPendienteGlobal
);
