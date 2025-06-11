using Idensity.Modbus.Models.Indication;
using Idensity.Modbus.Models.Settings;
using Idensity.Modbus.Models.Settings.Analogs;

namespace Idensity.Modbus.Extensions;

internal static class AnalogModulesExtensions
{
    private const ushort AnalogInputsSettingsStartRegisterNum = 70;
    private const ushort AnalogOutputsSettingsStartRegisterNum = 74;
    private const ushort AnalogOutputsSettingsRegSize = 14;
    internal static void SetAnalogInputSettings(this ushort[] buffer, DeviceSettings settings)
    {
        settings.AnalogInputActivities[0] = buffer[AnalogInputsSettingsStartRegisterNum] != 0;
        settings.AnalogInputActivities[1] = buffer[AnalogInputsSettingsStartRegisterNum+1] != 0;
    }

    internal static ushort[] GetRegisters(this AnalogOutputSettings settings, byte outputNum, ref ushort startIndex)
    {
        if (outputNum > 1)
            throw new Exception("Номер аналогового выхода должен быть между 0 и 1");
        startIndex = (ushort)(AnalogOutputsSettingsStartRegisterNum + outputNum*AnalogOutputsSettingsRegSize);
        return
        [
            (ushort)(settings.IsActive ? 1 : 0),
            (ushort)settings.Mode,
            settings.MeasProcessNum,
            (ushort)settings.AnalogOutMeasType,
            ..settings.MinValue.GetRegisters(),
            ..settings.MaxValue.GetRegisters(),
            ..((uint)settings.MinCurrent*1000).GetRegisters(),
            ..((uint)settings.MaxCurrent*1000).GetRegisters(),
            (ushort)(settings.TestValue*1000)
        ];
    }
    
    internal static void SetAnalogOutputSettings(this ushort[] buffer, DeviceSettings settings)
    {
        for (int i = 0; i < 2; i++)
        {
            settings.AnalogOutputSettings[i].IsActive = 
                buffer[AnalogOutputsSettingsStartRegisterNum + i*AnalogOutputsSettingsRegSize] != 0;
            settings.AnalogOutputSettings[i].Mode = 
                (AnalogOutputMode)buffer[AnalogOutputsSettingsStartRegisterNum + i*AnalogOutputsSettingsRegSize+1];
            settings.AnalogOutputSettings[i].MeasProcessNum = 
                (byte)buffer[AnalogOutputsSettingsStartRegisterNum + i*AnalogOutputsSettingsRegSize+2];
            settings.AnalogOutputSettings[i].AnalogOutMeasType = 
                (AnalogOutMeasType)buffer[AnalogOutputsSettingsStartRegisterNum + i*AnalogOutputsSettingsRegSize+3];
            settings.AnalogOutputSettings[i].MinValue =
                buffer.GetFloat(AnalogOutputsSettingsStartRegisterNum + i * AnalogOutputsSettingsRegSize + 4);
            settings.AnalogOutputSettings[i].MaxValue =
                buffer.GetFloat(AnalogOutputsSettingsStartRegisterNum + i * AnalogOutputsSettingsRegSize + 6);
            settings.AnalogOutputSettings[i].MinCurrent =
                (float)buffer.GetUint(AnalogOutputsSettingsStartRegisterNum + i * AnalogOutputsSettingsRegSize + 8)/1000;
            settings.AnalogOutputSettings[i].MaxCurrent =
                (float)buffer.GetUint(AnalogOutputsSettingsStartRegisterNum + i * AnalogOutputsSettingsRegSize + 10)/1000;
            settings.AnalogOutputSettings[i].TestValue =
                (float)buffer[AnalogOutputsSettingsStartRegisterNum + i * AnalogOutputsSettingsRegSize + 12]/1000;
        }
    }

    internal static ushort[] SwitchAnalogInputsPwr(ushort inputNum, bool value, ref ushort startIndex)
    {
        if (inputNum > 1)
            throw new Exception("Номер аналогового входа должен быть между 0 и 1");
        startIndex = (ushort)(AnalogInputsSettingsStartRegisterNum + inputNum);
        return
        [
            (ushort)(value ? 1 : 0)
        ];
    }
    
    internal static ushort[] SwitchAnalogOutputsPwr(ushort inputNum, bool value, ref ushort startIndex)
    {
        if (inputNum > 1)
            throw new Exception("Номер аналогового входа должен быть между 0 и 1");
        startIndex = (ushort)(AnalogOutputsSettingsStartRegisterNum + inputNum*AnalogOutputsSettingsRegSize);
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
    
    internal static void SetAnalogOutputsIndication(this ushort[] buffer, DeviceIndication deviceIndication)
    {
        const int offset = 6;
        for (int i = 0; i < 2; i++)
        {
            deviceIndication.AnalogOutputIndications[i].PwrState = buffer[32 + i * offset] != 0;
            deviceIndication.AnalogOutputIndications[i].CommState = buffer[33 + i * offset] != 0;
            deviceIndication.AnalogOutputIndications[i].AdcValue = (ushort)buffer.GetFloat(34 + i * offset);
            deviceIndication.AnalogOutputIndications[i].DacValue = (ushort)buffer.GetFloat(36 + i * offset);
        }
    }


    internal static ushort[] SendTestValue(byte outputNum, float value, ref ushort startIndex)
    {
        if (outputNum > 1)
            throw new Exception("Номер аналогового выхода должен быть между 0 и 1");
        startIndex = (ushort)(AnalogOutputsSettingsStartRegisterNum + outputNum*AnalogOutputsSettingsRegSize + 12);
        return
        [
            (ushort)(value*1000),
            1
        ];

    }
}