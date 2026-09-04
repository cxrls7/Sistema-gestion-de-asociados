namespace MemberManagementSystem.Models;

/// <summary>
/// Represents a TRM value obtained from the official open dataset.
/// </summary>
public class TrmRate
{
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public decimal? Valor { get; set; }
}
