namespace Idensity.Modbus.Models.Settings;

public class CalibrCurve
{
    public CalibrationType Type { get; set; }
    public List<float> Coefficients { get; } = Enumerable.Range(0, 6).Select(i => 0f).ToList();
}
