using Idensity.Modbus.Extensions;

namespace Idensity.Modbus.Models.Settings;

public class MeasProcess
{
    /// <summary>
    /// Время измерения в секундах.
    /// </summary>
    public float MeasDuration { get; set; }
    /// <summary>
    /// Кол-во точек измерения для усреднения
    /// </summary>
    public int MeasDeep { get; set; }
    /// <summary>
    /// Диаметр трубы в мм
    /// </summary>
    public float PipeDiameter { get;set; }
    /// <summary>
    /// Активность процесса измерения.
    /// </summary>
    public bool Activity { get; set; }
    /// <summary>
    /// Тип расчета измерения.
    /// </summary>
    public CalculationType CalculationType { get; set; } = CalculationType.Polynom;
    /// <summary>
    /// Тип измерения
    /// </summary>
    public int MeasType { get; set; }
    /// <summary>
    /// Настройки стандартизаций
    /// </summary>
    public List<StandSettings> StandSettings { get; } = Enumerable
        .Range(0, MeasProcessExtensions.StandCnt).Select(i => new StandSettings()).ToList();
    /// <summary>
    /// Данные единичных измерений.
    /// </summary>
    public List<SingleMeasResult> SingleMeasResults { get; } = Enumerable
        .Range(0, MeasProcessExtensions.SingleMeasuresCnt).Select(i => new SingleMeasResult()).ToList();
    /// <summary>
    /// Калибровочная кривая для данного процесса измерения.
    /// </summary>
    public CalibrCurve CalibrCurve { get; } = new CalibrCurve();
    /// <summary>
    /// Плотность жидкого
    /// </summary>
    public float DensityLiquid { get; set; }
    /// <summary>
    /// Плотность твердого
    /// </summary>
    public float DensitySolid { get; set; }
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
    
    /// <summary>
    /// Время ед измерения, с
    /// </summary>
    public ushort SingleMeasTime { get; set; }

}
