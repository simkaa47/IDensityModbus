namespace Kinef.Core.Models.Main;

public class AdcBoardSettings
{
    public Parameter<ushort> TimerAdc { get; } = 
        new Parameter<ushort>("Таймер выдачи данных", 0, 10000);

    public Parameter<bool> AdcSendEnabled { get; } =
        new Parameter<bool>("Разрешение на выдачу данных платы АЦП", false, true);

    public Parameter<byte> PreampGain { get; } = 
        new Parameter<byte>("К-т предусиления", 6, 86);

    public Parameter<ushort> SyncLevel { get; } 
        = new Parameter<ushort>("Уровень синхронизации", 0, 4095);
    
    

}