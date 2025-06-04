namespace Idensity.Modbus.Extensions
{
    internal static class ModbusExtensions
    {
        public static float GetFloat(this int[] arr, int index)
        {
            var bytes = arr.Skip(index)
                .Select(x => (ushort)x)
                .SelectMany(x => BitConverter.GetBytes(x))
                .ToArray();
            return BitConverter.ToSingle(bytes);
        }
    }
}
