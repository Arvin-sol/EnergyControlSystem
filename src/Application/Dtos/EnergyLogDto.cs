

namespace Application.Dtos;

public class EnergyLogCuDTO
{
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    public decimal Consumption { get; set; }
}