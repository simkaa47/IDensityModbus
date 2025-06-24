namespace Idensity.Modbus.Models.Settings;

public class CounterSettings
{
    /// <summary>
    /// Режим работы
    /// </summary>
    public CounterMode Mode { get; set; }
    /// <summary>
    /// Старт диапазона счетчика.
    /// </summary>
    public ushort Start { get; set; }
    /// <summary>
    /// Ширина диапазона
    /// </summary>
    public ushort Width { get; set; }

}
