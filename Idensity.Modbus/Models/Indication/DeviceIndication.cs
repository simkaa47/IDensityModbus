using Idensity.Modbus.Models.Indication.Analogs;

namespace Idensity.Modbus.Models.Indication;

public class DeviceIndication
{
    /// <summary>
    /// Статус измерения
    /// </summary>
    public bool IsMeasuringState { get; internal set; }

    /// <summary>
    /// Телеметрия платы питания
    /// </summary>
    public TempBoardTelemetry TempBoardTelemetry { get; set; } = new();

    /// <summary>
    /// Телеметрия платы HV
    /// </summary>
    public HvBoardTelemetry HvBoardTelemetry { get; set; } = new();
    /// <summary>
    ///Данные RTC прибора
    /// </summary>
    public DateTime Rtc { get; internal set; }
    /// <summary>
    /// Статус связи с платой АЦП
    /// </summary>
    public bool AdcBoardConnectState { get; internal set; }
    public List<AnalogInputIndication> AnalogInputIndications { get; } = Enumerable.Range(0,2)
        .Select(x => new AnalogInputIndication()).ToList();
    public List<AnalogOutputIndication> AnalogOutputIndications { get; } = Enumerable.Range(0,2)
        .Select(x=>new AnalogOutputIndication()).ToList();
    public List<MeasResult> MeasResults { get; } = Enumerable.Range(0,2)
        .Select(x=>new MeasResult()).ToList();


}