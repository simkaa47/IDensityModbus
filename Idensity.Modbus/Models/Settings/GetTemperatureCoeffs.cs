namespace Idensity.Modbus.Models.Settings;

public class GetTemperatureCoeffs
{
    /// <summary>
    /// Коэффициент A
    /// </summary>
    public float A { get; internal set; } = 0f;
    /// <summary>
    /// Коэффициент B
    /// </summary>
    public float B { get; internal set; } = 0f;
}
