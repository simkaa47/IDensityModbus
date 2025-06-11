using Idensity.Modbus.Models.Indication;
using Idensity.Modbus.Models.Settings;

namespace Idensity.Modbus.Extensions;

internal static class AnalogModulesExtensions
{
    private const ushort AnalogInputsSettingsStartRegisterNum = 70;
    internal static void SetAnalogInputSettings(this ushort[] buffer, DeviceSettings settings)
    {
        settings.AnalogInputActivities[0] = buffer[AnalogInputsSettingsStartRegisterNum] != 0;
        settings.AnalogInputActivities[1] = buffer[AnalogInputsSettingsStartRegisterNum+1] != 0;
    }

    internal static ushort[] SwitchAnalogInputsPwr(ushort inputNum, bool value, ref ushort startIndex)
    {
        startIndex = (ushort)(AnalogInputsSettingsStartRegisterNum + inputNum);
        return
        [
            (ushort)(value ? 1 : 0)
        ];
    }
    
    internal static void SetAnalogInputsIndication(this ushort[] buffer, DeviceIndication deviceIndication)
    {
        const int offset = 6;
        for (int i = 0; i < 2; i++)
        {
            deviceIndication.AnalogInputIndications[i].PwrState = buffer[44 + i * offset] != 0;
            deviceIndication.AnalogInputIndications[i].CommState = buffer[45 + i * offset] != 0;
            deviceIndication.AnalogInputIndications[i].AdcValue = (ushort)buffer.GetFloat(46 + i * offset);
        }
    }
}