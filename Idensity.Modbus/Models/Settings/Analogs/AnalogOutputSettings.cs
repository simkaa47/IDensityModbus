namespace Idensity.Modbus.Models.Settings.Analogs;

public class AnalogOutputSettings
{
    /// <summary>
    /// Активность выхода
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Режим работы аналогового выхода.
    /// </summary>
    public AnalogOutputMode Mode { get; set; }
    /// <summary>
    /// Номер изм процесса
    /// </summary>
    public byte MeasProcessNum { get; set; }
    /// <summary>
    /// Что выводить на выход
    /// </summary>
    public AnalogOutMeasType AnalogOutMeasType { get; set; }
    /// <summary>
    /// Нижний предел переменной
    /// </summary>
    public float MinValue { get; set; }
    /// <summary>
    /// Верхний предел переменной
    /// </summary>
    public float MaxValue { get; set; }
    /// <summary>
    /// Минимальное значение тока, мА
    /// </summary>
    public float MinCurrent { get; set; }

    /// <summary>
    /// Максимальное значение тока, мА
    /// </summary>
    public float MaxCurrent { get; set; }
    
    /// <summary>
    /// Тестовое значение, mA
    /// </summary>
    public float TestValue { get; set; }


}
