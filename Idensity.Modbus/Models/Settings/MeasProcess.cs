namespace Idensity.Modbus.Models.Settings;

public class MeasProcess
{
    /// <summary>
    /// Время измерения в секундах.
    /// </summary>
    public float MeasDuration { get; internal set; }
    /// <summary>
    /// Кол-во точек измерения для усреднения
    /// </summary>
    public int MeasDeep { get; internal set; }
    /// <summary>
    /// Диаметр трубы в мм
    /// </summary>
    public float PipeDiameter { get;internal set; }
    /// <summary>
    /// Активность процесса измерения.
    /// </summary>
    public bool Activity { get; internal set; }
    /// <summary>
    /// Тип расчета измерения.
    /// </summary>
    public CalculationType CalculationType { get; internal set; } = CalculationType.Polynom;
    /// <summary>
    /// Тип измерения
    /// </summary>
    public int MeasType { get; internal set; }
    /// <summary>
    /// Настройки стандартизаций
    /// </summary>
    public List<StandSettings> StandSettings { get; } = Enumerable
        .Range(0, 3).Select(i => new StandSettings()).ToList();
    /// <summary>
    /// Данные единичных измерений.
    /// </summary>
    public List<SingleMeasResult> SingleMeasResults { get; } = Enumerable
        .Range(0, 10).Select(i => new SingleMeasResult()).ToList();
    /// <summary>
    /// Калибровочная кривая для данного процесса измерения.
    /// </summary>
    public CalibrCurve CalibrCurve { get; } = new CalibrCurve();
    /// <summary>
    /// Плотность жидкого
    /// </summary>
    public float DensityLiquid { get; internal set; }
    /// <summary>
    /// Плотность твердого
    /// </summary>
    public float DensitySolid { get; internal set; }
    /// <summary>
    /// Настройки компенсации температуры.  
    /// </summary>
    public List<TempCompensation> TempCompensations { get; } = Enumerable
        .Range(0, 3).Select(i => new TempCompensation()).ToList();
    /// <summary>
    /// Настройки компенсации пара
    /// </summary>
    public SteamCompensation SteamCompensation { get; } = new SteamCompensation();
    /// <summary>
    /// Настройки быстрых изменений.
    /// </summary>
    public FastChange FastChange { get; } = new FastChange();
    /// <summary>
    /// Коэффиценты ослабления (вроде для уровнемера)
    /// </summary>
    public List<float> AttCoeffs { get; } 
        = Enumerable.Range(0, 2).Select(i=>0f).ToList();
    /// <summary>
    /// Коэффициенты объема (вроде для уровнемера)
    /// </summary>  
    public List<float> VolumeCoeffs { get; }
        = Enumerable.Range(0, 4).Select(i => 0f).ToList();





}
