// See https://aka.ms/new-console-template for more information

using System.Text;
using Idensity.Modbus.Services;

ushort[] regs = [17481, 12848, 51, 0, 0];
var bytes = regs.Take(5)
    .SelectMany(BitConverter.GetBytes)
    .ToArray();

var client = new IdensityModbusClient();
await client.SetDeviceNameAsync("ID023");



Console.WriteLine(Encoding.ASCII.GetString(bytes));