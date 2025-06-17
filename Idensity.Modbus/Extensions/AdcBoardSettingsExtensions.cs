using Idensity.Modbus.Models.Settings.AdcSettings;

namespace Idensity.Modbus.Extensions;

/// <summary>
/// Получение массива регистров из настроек
/// </summary>
internal static class AdcBoardSettingsExtensions
{
    internal static void SetAdcBoardSettings(this ushort[] buffer, AdcBoardSettings settings)
    {
        settings.Mode = (AdcBoardMode)buffer[3];
        settings.SyncLevel = buffer[4];
        settings.TimerSendData = buffer[5];
        settings.Gain = (byte)buffer[6];
        settings.AdcDataSendEnabled = buffer[20] != 0;
        for (int i = 0; i < 4; i++)
        {
            settings.UpdAddress[i] = (byte)buffer[i+7];
        }
        settings.UdpPort = buffer[11];
        settings.HvSv = (ushort)(buffer[12]/20);
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
            (ushort)(settings.HvSv*20)
        ];
    }
    
}