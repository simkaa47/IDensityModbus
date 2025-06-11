using Idensity.Modbus.Models.Settings;

namespace Idensity.Modbus.Extensions;

internal static class TemperatureCompensationExtensions
{
    internal const ushort StartRegNum = 103;

    internal static void SetTemperatureCompensationSettings(this ushort[] buffer,
        GetTemperature getTemperature)
    {
        getTemperature.Src = (GetTemperatureSrc)buffer[StartRegNum];
        for (int i = 0; i < 2; i++)
        {
            getTemperature.GetTemperatureCoeffs[i].A = buffer.GetFloat(StartRegNum + 1 + i * 4);
            getTemperature.GetTemperatureCoeffs[i].B = buffer.GetFloat(StartRegNum + 3 + i * 4);
        }
    }


    internal static ushort[] GetRegisters(this GetTemperature getTemperature, ref ushort startIndex)
    {
        startIndex = StartRegNum;
        return
        [
            (ushort)getTemperature.Src,
            ..getTemperature.GetTemperatureCoeffs[0].A.GetRegisters(),
            ..getTemperature.GetTemperatureCoeffs[0].B.GetRegisters(),
            ..getTemperature.GetTemperatureCoeffs[1].A.GetRegisters(),
            ..getTemperature.GetTemperatureCoeffs[1].B.GetRegisters()
        ];
    }
}