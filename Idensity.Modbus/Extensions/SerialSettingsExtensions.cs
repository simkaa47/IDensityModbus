using Idensity.Modbus.Models.Settings;

namespace Idensity.Modbus.Extensions;

internal static class SerialSettingsExtensions
{
    const ushort StartRegNum = 66;
    internal static void SetSerialSettings(this ushort[] buffer, SerialSettings settings)
    {
        settings.Baudrate = buffer.GetUint(StartRegNum);
        settings.Mode = (SerialPortMode)buffer[StartRegNum+2];
    }

    internal static ushort[] GetRegisters(this SerialSettings settings, ref ushort startIndex)
    {
        startIndex = StartRegNum;
        return
        [
            ..settings.Baudrate.GetRegisters(),
            (ushort)settings.Mode,
        ];
    }
}