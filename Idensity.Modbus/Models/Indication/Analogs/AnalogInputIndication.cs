namespace Idensity.Modbus.Models.Indication.Analogs;

public class AnalogInputIndication:AnalogIndication
{
    public float Current => this.AdcValue * 0.001f;
}