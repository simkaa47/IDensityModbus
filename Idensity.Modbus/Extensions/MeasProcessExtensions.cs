using Idensity.Modbus.Models.Settings;

namespace Idensity.Modbus.Extensions;

internal static class MeasProcessExtensions
{
    internal const ushort StartMeasProcRegisterOffset = 200;
    internal const ushort MeasProcRegisterCnt = 180;
    internal const ushort MeasProcCnt = 8;

    internal const ushort StandRegisterOffset = 24;
    internal const ushort StandRegisterCnt = 12;
    internal const ushort StandCnt = 3;

    internal const ushort SingleMeasuresRegisterOffset = 76;
    internal const ushort SingleMeasuresRegisterCnt = 8;
    internal const ushort SingleMeasuresCnt = 10;


    internal static void SetMeasProcess(this ushort[] buffer, byte index, MeasProcess process)
    {
        int offset = StartMeasProcRegisterOffset + index * MeasProcRegisterCnt;
        if (index >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("index", $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        process.MeasDuration = ((float)buffer[offset]) / 10;
        process.MeasDeep = buffer[offset + 1];
        process.PipeDiameter = ((float)buffer[offset + 2]) / 10;
        process.Activity = buffer[offset + 3] != 0;
        process.CalculationType = (CalculationType)buffer[offset + 4];
        process.MeasType = buffer[offset + 5];
        process.DensityLiquid = buffer.GetFloat(offset + 6);
        process.DensitySolid = buffer.GetFloat(offset + 8);
        process.FastChange.IsActive = buffer[offset + 10] != 0;
        process.FastChange.Threshold = buffer[offset + 11];
        process.SingleMeasTime = (ushort)(buffer[offset + 12] / 10);


        //Стандартизации
        for (int i = 0; i < StandCnt; i++)
        {
            process.StandSettings[i].StandDuration =
                (ushort)(buffer[offset + StandRegisterOffset + i * StandRegisterCnt] / 10);
            var year = buffer[offset + StandRegisterOffset + i * StandRegisterCnt + 1] + 2000;
            int month = buffer[offset + StandRegisterOffset + i * StandRegisterCnt + 2];
            month = month is > 0 and <= 12 ? month : 1;
            int day = buffer[offset + StandRegisterOffset + i * StandRegisterCnt + 3];
            day = day is > 0 and <= 31 ? day : 1;
            process.StandSettings[i].LastStandDate = new DateOnly(year, month, day);
            process.StandSettings[i].Result = buffer.GetFloat(offset + StandRegisterOffset + i * StandRegisterCnt + 4);
            process.StandSettings[i].HalfLifeResult =
                buffer.GetFloat(offset + StandRegisterOffset + i * StandRegisterCnt + 6);
        }

        // Кривая
        process.CalibrCurve.Type = (CalibrationType)buffer[offset + 60];
        for (int i = 0; i < 5; i++)
        {
            process.CalibrCurve.Coefficients[i] = buffer.GetFloat(offset + 60 + i);
        }

        // Данные ед измерений
        for (int i = 0; i < SingleMeasuresCnt; i++)
        {
            var year = buffer[offset + SingleMeasuresRegisterOffset + i * SingleMeasuresRegisterCnt] + 2000;
            int month = buffer[offset + SingleMeasuresRegisterOffset + i * SingleMeasuresRegisterCnt + 1];
            month = month is > 0 and <= 12 ? month : 1;
            int day = buffer[offset + SingleMeasuresRegisterOffset + i * SingleMeasuresRegisterCnt + 2];
            day = day is > 0 and <= 31 ? day : 1;
            process.SingleMeasResults[i].Date = new DateOnly(year, month, day);
            process.SingleMeasResults[i].Weak =
                buffer.GetFloat(offset + SingleMeasuresRegisterOffset + i * SingleMeasuresRegisterCnt + 4);
            process.SingleMeasResults[i].PhysValue =
                buffer.GetFloat(offset + SingleMeasuresRegisterOffset + i * SingleMeasuresRegisterCnt + 6);
        }
    }


    internal static ushort[] GetDurationRegisters(float duration, int measProcIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        return
        [
            (ushort)(duration * 10)
        ];
    }

    internal static ushort[] GetDeepRegisters(byte deep, int measProcIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex + 1);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        if (deep >= 100)
            throw new ArgumentOutOfRangeException("deep", $"Кол-во точек усреднения не должно быть больше 100");
        return
        [
            deep
        ];
    }


    internal static ushort[] GetPipeDiameterRegisters(ushort diameter, int measProcIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex + 2);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        return
        [
            (ushort)(diameter * 10)
        ];
    }

    internal static ushort[] GetActivityRegisters(bool activity, int measProcIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex + 3);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        return
            [(ushort)(activity ? 1 : 0)];
    }

    internal static ushort[] GetCalculationTypeRegisters(CalculationType type, int measProcIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex + 4);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        return
            [(ushort)type];
    }
    
    internal static ushort[] GetMeasTypeRegisters(ushort measType, int measProcIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex + 5);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        return
            [measType];
    }
    
    internal static ushort[] GetDensityLiquidRegisters(float density, int measProcIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex + 6);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        return
            [..density.GetRegisters()];
    }
    
    internal static ushort[] GetDensitySolidRegisters(float density, int measProcIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex + 8);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        return
            [..density.GetRegisters()];
    }
    
    
    internal static ushort[] GetFastChangeRegisters(FastChange fastChange, int measProcIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex + 10);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        return
            [
                (ushort)(fastChange.IsActive ? 1 : 0),
                fastChange.Threshold
            ];
    }
    
    internal static ushort[] GetSingleMeasureDurationRegisters(ushort duration, int measProcIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex + 12);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        return
        [
            (ushort)(duration*10)
        ];
    }

    internal static ushort[] GetStandartisationRegisters(StandSettings settings, int measProcIndex, int standIndex,
        ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex + StandRegisterOffset + StandRegisterCnt*standIndex);
        
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        if (standIndex >= StandCnt)
            throw new ArgumentOutOfRangeException("standIndex",
                $"Индекс стандартизации должен быть меньше {StandCnt}");
        return
        [
            (ushort)(settings.StandDuration*10),
            (ushort)(settings.LastStandDate.Year-2000),
            (ushort)(settings.LastStandDate.Month),
            (ushort)(settings.LastStandDate.Day),
            ..settings.Result.GetRegisters(),
            ..settings.HalfLifeResult.GetRegisters(),
        ];
    }
    
    internal static ushort[] GetMakeStandartisationRegisters(int measProcIndex, int standIndex,
        ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex + StandRegisterOffset + StandRegisterCnt*standIndex+8);
        
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        if (standIndex >= StandCnt)
            throw new ArgumentOutOfRangeException("standIndex",
                $"Индекс стандартизации должен быть меньше {StandCnt}");
        return
        [
            1
        ];
    }
    
    

    internal static ushort[] GetCalibrCurveRegisters(CalibrCurve curve, int measProcIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + MeasProcRegisterCnt * measProcIndex + 60);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        return
        [
            (ushort)curve.Type,
            0,
            ..curve.Coefficients.SelectMany(c => c.GetRegisters()).ToArray()
        ];
    }


    internal static ushort[] GetSingleMeasureDataRegisters(SingleMeasResult result, int measProcIndex,
        int singleMeasIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + 
                              MeasProcRegisterCnt * measProcIndex + SingleMeasuresRegisterOffset + 
                              SingleMeasuresRegisterCnt*singleMeasIndex);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        if (singleMeasIndex >= SingleMeasuresCnt)
            throw new ArgumentOutOfRangeException("singleMeasIndex",
                $"Индекс ед измерения должен быть меньше {SingleMeasuresRegisterCnt}");

        return
        [
            (ushort)result.Date.Year,
            (ushort)result.Date.Month,
            (ushort)result.Date.Day,
            0,
            ..result.Weak.GetRegisters(),
            ..result.Weak.GetRegisters(),
        ];

    }
    
    internal static ushort[] GetMakeSingleMeasureRegisters(int measProcIndex,
        int singleMeasIndex, ref ushort startIndex)
    {
        startIndex = (ushort)(StartMeasProcRegisterOffset + 
                              MeasProcRegisterCnt * measProcIndex + SingleMeasuresRegisterOffset + 
                              SingleMeasuresRegisterCnt*singleMeasIndex + 3);
        if (measProcIndex >= MeasProcCnt)
            throw new ArgumentOutOfRangeException("measProcIndex",
                $"Индекс изм прцесса должен быть меньше {MeasProcCnt}");
        if (singleMeasIndex >= SingleMeasuresCnt)
            throw new ArgumentOutOfRangeException("singleMeasIndex",
                $"Индекс ед измерения должен быть меньше {SingleMeasuresRegisterCnt}");

        return
        [
            1
        ];

    }
    
}