namespace Idensity.Modbus.Models.Settings;

public class SingleMeasResult
{
    /// <summary>
    /// Дата измерения
    /// </summary>
    public DateOnly Date { get; internal set; }
    /// <summary>
    /// Ослабление
    /// </summary>
    public float Weak { get; set; }
    /// <summary>
    /// Значение счетчика
    /// </summary>
    public float CounterValue { get; internal set; }
}
