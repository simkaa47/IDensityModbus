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
        settings.PeakSpectrumSv = buffer[13];
    }


    internal static ushort[] GetModeRegisters(AdcBoardMode mode, ref ushort startIndex)
    {
        startIndex = 3;
        return
        [
            (ushort)mode
        ];
    }
    
    internal static ushort[] GetSyncLevelRegisters(ushort level, ref ushort startIndex)
    {
        startIndex = 4;
        return
        [
            level
        ];
    }
    
    internal static ushort[] GetTimerSendRegisters(ushort timerValue, ref ushort startIndex)
    {
        startIndex = 5;
        return
        [
            timerValue
        ];
    }
    
    internal static ushort[] GetGainRegisters(byte gain, ref ushort startIndex)
    {
        startIndex = 6;
        return
        [
            gain
        ];
    }
    
    internal static ushort[] GetUpdAddressRegisters(byte[] addr, ref ushort startIndex)
    {
        startIndex = 7;
        if (addr.Length != 4)
            throw new Exception("Адрес получателя данных спектра должен иметь 4 байта");
        return addr.Select(x => (ushort)x).ToArray();
    }
    
    internal static ushort[] GetUpdPortRegisters(ushort port, ref ushort startIndex)
    {
        startIndex = 11;
        return [port];
    }
    
    internal static ushort[] GetHvRegisters(ushort hv, ref ushort startIndex)
    {
        startIndex = 12;
        return
        [
            (ushort)(hv * 20)
        ];

    }

    internal static ushort[] GetPeakSpectrumSvRegisters(ushort peak, ref ushort startIndex)
    {
        startIndex = 13;
        return
        [
            peak
        ];

    }

}