namespace Idensity.Modbus.Models.Settings.AdcSettings;

public class AdcBoardSettings
{
    /// <summary>
    /// Режим работы платы АЦП устройства.
    /// </summary>
    public AdcBoardMode Mode { get; set; }
    /// <summary>
    /// Уровень синхронизации
    /// </summary>
    public ushort SyncLevel { get; set; }
    /// <summary>
    /// Таймер выдачи данных платы АЦП, мс
    /// </summary>
    public ushort TimerSendData { get; set; } = 200;
    /// <summary>
    /// Коэффициент платы предусиления
    /// </summary>
    public byte Gain { get; set; }
    /// <summary>
    /// Порт UDP для передачи данных платы АЦП
    /// </summary>
    public ushort UdpPort { get;  set; }
    /// <summary>
    /// Адрес для передачи данных платы АЦП по UDP, 4 байта
    /// </summary>
    public byte[] UpdAddress { get; } = new byte[4];
    /// <summary>
    /// Уставка платы HV
    /// </summary>
    public ushort HvSv { get;  set; }
    /// <summary>
    /// Координата пика спектра
    /// </summary>
    public ushort PeakSpectrumSv { get; set; }
    /// <summary>
    /// Разрешение на выжачу данных АЦП
    /// </summary>
    public bool AdcDataSendEnabled { get; set; }

}
