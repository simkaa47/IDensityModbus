namespace Idensity.Modbus.Models.Indication.Analogs;

public class AnalogIndication
{
    /// <summary>
    /// Статус связи с аналоговым модулем
    /// </summary>
    public bool CommState { get; internal set; }
    /// <summary>
    /// Питание аналогового модуля
    /// </summary>
    public bool PwrState { get; internal set; }
    /// <summary>
    /// Значение АЦП, каунты
    /// </summary>
    public ushort AdcValue { get; internal set; }
}