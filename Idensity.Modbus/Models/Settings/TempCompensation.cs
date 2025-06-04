namespace Idensity.Modbus.Models.Settings;

public class TempCompensation
{
    /// <summary>
    /// Активность компенсации температуры.
    /// </summary>
    public bool IsActive { get; internal set; }
    /// <summary>
    /// Коэффициенты  компенсации температуры.
    /// </summary>
    public List<float> Coeffs { get; } = Enumerable.Range(0, 2).Select(i=>0f).ToList();
}
