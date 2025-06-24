namespace Idensity.Modbus.Extensions;

internal static class RtcExtensions
{
    internal static DateTime SetRtc(this ushort[] arr,  DateTime time)
    {
        int year = arr[18]+2000;
        int month = arr[19];
        if (month is < 1 or > 12)
            return time;
        int day = arr[20];
        if (day < 1 || day > DateTime.DaysInMonth(year, month))
            return time;
        int hour = arr[21];
        if (hour > 23)
            return time;
        int minute = arr[22];
        if (minute > 59)
            return time;
        int second = arr[23];
        if (second > 59)
            return time;
        return new DateTime(year, month, day, hour, minute, second);
    }


    internal static ushort[] GetRegisters(DateTime time, ref ushort startIndex)
    {
        startIndex = 116;
        return
        [
            (ushort)(time.Year % 2000),
            (ushort)time.Month,
            (ushort)time.Day,
            (ushort)time.Hour,
            (ushort)time.Minute,
            (ushort)time.Second,
            1
        ];
    }
}