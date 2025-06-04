namespace Idensity.Modbus.Models.Settings;

public class SteamCompensation
{
    /// <summary>
    /// Активность компенсации пара.
    /// </summary>
    public bool IsActive { get; internal set; }
    /// <summary>
    /// Коэффициенты компенсации пара.
    /// </summary>
    public List<float> Coeffs { get; } = Enumerable.Range(0, 2).Select(i => 0f).ToList();
    /// <summary>
    /// 0  - AI0, 1 - AI1
    /// </summary>
    public int Src { get; internal set; }
}
