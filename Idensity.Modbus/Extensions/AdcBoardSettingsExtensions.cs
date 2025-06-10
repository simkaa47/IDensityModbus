using Idensity.Modbus.Models.Settings.AdcSettings;

namespace Idensity.Modbus.Extensions;

/// <summary>
/// Получение массива регистров из настроек
/// </summary>
internal static class AdcBoardSettingsExtensions
{
    internal static void SetAdcBoardSettings(this ushort[] buffer, AdcBoardSettings settings)
    {
        settings.Mode = (AdcBoardMode)buffer[1];
        settings.SyncLevel = buffer[3];
        settings.TimerSendData = buffer[5];
        settings.Gain = (byte)buffer[6];
        settings.AdcDataSendEnabled = buffer[20]!=0;
    }
    
    
    internal static ushort[] GetRegisters(this AdcBoardSettings settings, ref ushort startIndex)
    {
        startIndex = 3;
        return
        [
            (ushort)settings.Mode,
            settings.SyncLevel,
            settings.TimerSendData,
            settings.Gain,
            settings.UpdAddress[0],
            settings.UpdAddress[1],
            settings.UpdAddress[2],
            settings.UpdAddress[3],
            settings.UdpPort,
            settings.HvSv
        ];
    }
    
}