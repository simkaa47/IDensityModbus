namespace Idensity.Modbus.Models.Settings;
/// <summary>
/// Натсройки компенсациии температуры.
/// </summary>
public class GetTemperature
{
    /// <summary>
    /// Источник получения температуры.
    /// </summary>
    public GetTemperatureSrc Src { get; internal set; }
    public GetTemperatureCoeffs[] GetTemperatureCoeffs { get; } = 
        Enumerable.Range(0, 2).Select(i => new GetTemperatureCoeffs()).ToArray()  ;

}
