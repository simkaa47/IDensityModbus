namespace Idensity.Modbus.Models.Settings;

public class KalmanSettings
{
    /// <summary>
    /// Скорость изменения
    /// </summary>
    public float Speed { get; internal set; }
    /// <summary>
    /// К-т сглаживания  
    /// </summary>
    public float Smooth { get; internal set; }
}
