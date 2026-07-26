using System.Threading;
using System.Threading.Tasks;
using TurnosApp.Core.Application.DTOs.Metricas;

namespace TurnosApp.Core.Application.Interfaces.Services;

public interface IMetricasAppService
{
    Task<ResumenMetricasDto> GetResumenAsync(MetricasFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<IngresosMetricasDto> GetIngresosAsync(MetricasFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<TurnosMetricasDto> GetTurnosAsync(MetricasFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<ClientesMetricasDto> GetClientesAsync(MetricasFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<ServiciosRecursosMetricasDto> GetServiciosRecursosAsync(MetricasFiltroDto filtro, CancellationToken cancellationToken = default);
}
