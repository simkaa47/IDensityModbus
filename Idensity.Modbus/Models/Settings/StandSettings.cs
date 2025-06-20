namespace Idensity.Modbus.Models.Settings;

public class StandSettings
{
    /// <summary>
    /// Длительность стандартизации, с
    /// </summary>
    public ushort StandDuration { get; set; }
    /// <summary>
    /// Дата последней стандартизации
    /// </summary>
    public DateOnly LastStandDate { get; set; }
    /// <summary>
    /// Результат стандартизации
    /// </summary>
    public float Result { get; set; }
    /// <summary>
    /// Результат стандартизации с учетом полураспада
    /// </summary>
    public float HalfLifeResult { get; set; }

}
