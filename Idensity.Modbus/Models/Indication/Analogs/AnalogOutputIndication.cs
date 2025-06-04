namespace Idensity.Modbus.Models.Indication.Analogs;

public class AnalogOutputIndication:AnalogIndication
{
    /// <summary>
    /// Ток аналогового выхода, mA
    /// </summary>
    public float Current => 0.00735f * DacValue;
    public ushort DacValue { get; internal set; }
}