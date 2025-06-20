namespace Idensity.Modbus.Models.Settings;

public class SingleMeasResult
{
    /// <summary>
    /// Дата измерения
    /// </summary>
    public DateOnly Date { get; set; }
    /// <summary>
    /// Ослабление
    /// </summary>
    public float Weak { get; set; }
    /// <summary>
    /// Значение счетчика
    /// </summary>
    public float PhysValue { get; set; }
}
