namespace Idensity.Modbus.Models.Settings.Analogs;

public class AnalogOutputSettings
{
    /// <summary>
    /// Активность выхода
    /// </summary>
    public bool IsActive { get; internal set; }
    /// <summary>
    /// Режим работы аналогового выхода.
    /// </summary>
    public AnalogOutputMode Mode { get; internal set; }
    /// <summary>
    /// Номер изм процесса
    /// </summary>
    public byte MeasProcessNum { get; internal set; }
    /// <summary>
    /// Что выводить на выход
    /// </summary>
    public AnalogOutMeasType AnalogOutMeasType { get; internal set; }
    /// <summary>
    /// Нижний предел переменной
    /// </summary>
    public float MinValue { get; internal set; }
    /// <summary>
    /// Верхний предел переменной
    /// </summary>
    public float MaxValue { get; internal set; }
    /// <summary>
    /// Минимальное значение тока, мА
    /// </summary>
    public float MinCurrent { get; internal set; }

    /// <summary>
    /// Максимальное значение тока, мА
    /// </summary>
    public float MaxCurrent { get; internal set; }


}
