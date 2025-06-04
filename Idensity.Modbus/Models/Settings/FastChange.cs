namespace Idensity.Modbus.Models.Settings;

public class FastChange
{
    /// <summary>
    /// Активность быстрой смены.
    /// </summary>
    public bool IsActive { get; internal set; }
    /// <summary>
    /// Пороговое значение для быстрой смены.
    /// </summary>
    public ushort Threshold { get; internal set; }
}
