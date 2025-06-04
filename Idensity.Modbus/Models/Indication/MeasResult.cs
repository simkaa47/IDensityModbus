namespace Idensity.Modbus.Models.Indication;

public record MeasResult()
{
    /// <summary>
    /// Значение счетчика
    /// </summary>
    public float CounterValue { get; internal set; }
    /// <summary>
    /// Мгновенное значение физической величины
    /// </summary>
    public float CurrentValue { get; internal set; }
    /// <summary>
    /// Усредненное значение физической величины
    /// </summary>
    public float AverageValue { get; internal set; }
    /// <summary>
    /// Номер процесса, к которому относится измерение
    /// </summary>
    public int ProcessNumber { get; internal set; }
    /// <summary>
    /// Состояние измерения
    /// </summary>
    public bool IsActive { get; internal set; } 
}
