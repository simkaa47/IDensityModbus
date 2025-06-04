namespace Idensity.Modbus.Models.Indication;

public class TempBoardTelemetry
{
    /// <summary>
    /// Показания датчика температуры
    /// </summary>
    public float Temperature { get; internal set; }
    /// <summary>
    /// Статус соединения с платой питания
    /// </summary>
    public bool BoardConnectingState { get; internal set; }
}