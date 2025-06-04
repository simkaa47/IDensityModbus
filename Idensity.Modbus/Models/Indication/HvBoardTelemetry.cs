namespace Idensity.Modbus.Models.Indication;

public class HvBoardTelemetry
{
    /// <summary>
    /// Значение напряжения на входе, вольт
    /// </summary>
    public float InputVoltage { get; internal set; }
    /// <summary>
    /// Значение напряжения на выходе, вольт
    /// </summary>
    public float OutputVoltage { get; internal set; }
    public bool BoardConnectingState { get; internal set; }


}