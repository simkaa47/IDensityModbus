namespace Idensity.Modbus.Extensions
{
    internal static class ModbusExtensions
    {
        public static float GetFloat(this ushort[] arr, int index)
        {
            var bytes = arr.Skip(index)
                .Select(x => (ushort)x)
                .SelectMany(BitConverter.GetBytes)
                .ToArray();
            return BitConverter.ToSingle(bytes);
        }
    }
}
