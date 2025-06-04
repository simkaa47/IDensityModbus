namespace Idensity.Modbus.Models.Settings;

public class CounterSettings
{
    /// <summary>
    /// Режим работы
    /// </summary>
    public CounterMode Mode { get; internal set; }
    /// <summary>
    /// Старт диапазона счетчика.
    /// </summary>
    public ushort Start { get; internal set; }
    /// <summary>
    /// Ширина диапазона
    /// </summary>
    public ushort Width { get; internal set; }
   

}
