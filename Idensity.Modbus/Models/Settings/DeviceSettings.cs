using Idensity.Modbus.Models.Settings.AdcSettings;
using Idensity.Modbus.Models.Settings.Analogs;

namespace Idensity.Modbus.Models.Settings;

public class DeviceSettings
{
    /// <summary>
    /// Данные измерительных процессов
    /// </summary>
    public List<MeasProcess> MeasProcesses { get;} = Enumerable
        .Range(0,8).Select(i=> new MeasProcess()).ToList();
    /// <summary>
    /// Настройки счетчиков
    /// </summary>
    public List<CounterSettings> CounterSettings { get; } = Enumerable
        .Range(0, 3).Select(i => new CounterSettings()).ToList();
    /// <summary>
    /// Адрес устройства в сети Modbus.
    /// </summary>
    public byte ModbusId { get; set; }
    /// <summary>
    /// Настройки Ethernet соединения устройства.
    /// </summary>
    public TcpSettings EthernetSettings { get; } = new TcpSettings();
    /// <summary>
    /// Контрольная сумма ПО
    /// </summary>
    public uint CheckSum { get; set; }
    /// <summary>
    /// Длина уровнемера,мм
    /// </summary>
    public float LevelLength { get; set; }
    /// <summary>
    /// Натсройки компенсации температуры
    /// </summary>
    public GetTemperature GetTemperature { get; } = new GetTemperature();
    /// <summary>
    /// Настройки фильтра калмана
    /// </summary>
    public KalmanSettings[] KalmanSettings { get; } = 
        Enumerable.Range(0,2).Select(i=>new KalmanSettings()).ToArray();
    /// <summary>
    /// Номер проекта
    /// </summary>
    public string ProjectNumber { get; set; } = string.Empty;
    /// <summary>
    /// Версия встроенного ПО
    /// </summary>
    public string FwVersion { get; set; } = string.Empty;
    /// <summary>
    /// Настройки последовательного соединения устройства
    /// </summary>
    public SerialSettings SerialSettings { get; } = new SerialSettings();
    /// <summary>
    /// Настройки платы АЦП устройства
    /// </summary>
    public AdcBoardSettings AdcBoardSettings { get; } = new AdcBoardSettings();
    /// <summary>
    /// Настройки активностей аналоговых входов устройства.
    /// </summary>
    public bool[] AnalogInputActivities { get; } = new bool[2];
    /// <summary>
    /// Настйроки аналоговых выходов
    /// </summary>
    public AnalogOutputSettings[] AnalogOutputSettings { get; } = 
        Enumerable.Range(0,2).Select(i=> new AnalogOutputSettings()).ToArray();

}