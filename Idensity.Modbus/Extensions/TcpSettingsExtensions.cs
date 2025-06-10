using Idensity.Modbus.Models.Settings;

namespace Idensity.Modbus.Extensions;

internal static class TcpSettingsExtensions
{
    const int StartRegNum = 48;

    internal static void SetEthernetSettings(this ushort[] buffer, DeviceSettings settings)
    {
        for (var i = 0; i < 4; i++)
        {
            settings.EthernetSettings.Address[i] = (byte)buffer[StartRegNum + i];
            settings.EthernetSettings.Mask[i] = (byte)buffer[StartRegNum +  i + 4];
            settings.EthernetSettings.Gateway[i] = (byte)buffer[StartRegNum +  i + 8];
        }

        for (var i = 0; i < 6; i++)
        {
            settings.EthernetSettings.MacAddress[i] = (byte)buffer[StartRegNum + i+12];
        }
    }

    internal static ushort[] GetRegisters(this TcpSettings settings, ref ushort startIndex)
    {
        startIndex = StartRegNum;
        byte[] bytes =
        [
            ..settings.Address,
            ..settings.Mask,
            ..settings.Gateway,
            ..settings.MacAddress
        ];

        return bytes.Select(b=>(ushort) b).ToArray();
    }
}