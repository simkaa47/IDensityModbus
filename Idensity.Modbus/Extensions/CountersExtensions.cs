using Idensity.Modbus.Models.Settings;

namespace Idensity.Modbus.Extensions;

internal static class CountersExtensions
{
    const int StartRegNum = 24;
    const int DiapSize = 8;
    internal static void SetCounterSettings(this ushort[] buffer, DeviceSettings settings)
    {
        
        for (var i = 0; i < 3; i++)
        {
            settings.CounterSettings[i].Start = buffer[StartRegNum + i * DiapSize];
            settings.CounterSettings[i].Width = buffer[StartRegNum + i * DiapSize + 1];
            settings.CounterSettings[i].Mode = (CounterMode)buffer[StartRegNum + i * DiapSize + 2];
        }
    }

    internal static ushort[] GetRegisters(this CounterSettings settings,
        int counterNum, ref ushort startIndex)
    {
        if(counterNum >= 3 || counterNum < 0)
            throw new ArgumentOutOfRangeException("Counter number must be between 0 and 3");
        startIndex = (ushort)(StartRegNum + DiapSize*counterNum);
        return
        [
            settings.Start,
            settings.Width,
            (ushort)settings.Mode,
        ];

    }
}