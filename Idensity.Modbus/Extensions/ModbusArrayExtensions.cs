﻿using Idensity.Modbus.Models.Indication;
using Idensity.Modbus.Models.Settings;
using Idensity.Modbus.Models.Settings.AdcSettings;

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
        
        public static uint GetUint(this ushort[] arr, int index)
        {
            var bytes = arr.Skip(index)
                .Select(x => (ushort)x)
                .SelectMany(BitConverter.GetBytes)
                .ToArray();
            return BitConverter.ToUInt32(bytes);
        }

        public static ushort[] GetRegisters(this float value)
        {
            var bytes = BitConverter.GetBytes(value);
            return
            [
                BitConverter.ToUInt16(bytes,0),
                BitConverter.ToUInt16(bytes,2),
            ];
        }
        
        public static ushort[] GetRegisters(this uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            return
            [
                BitConverter.ToUInt16(bytes,0),
                BitConverter.ToUInt16(bytes,2),
            ];
        }

        

        internal static void SetMeasResults(this ushort[] arr, DeviceIndication deviceIndication)
        {
            deviceIndication.IsMeasuringState = arr[0] != 0;
            for (int i = 0; i < 2; i++)
            {
                deviceIndication.MeasResults[i].ProcessNumber = arr[1 + i * 8];
                deviceIndication.MeasResults[i].CounterValue = arr.GetFloat(2 + i * 8);
                deviceIndication.MeasResults[i].CurrentValue = arr.GetFloat(4 + i * 8);
                deviceIndication.MeasResults[i].AverageValue = arr.GetFloat(6 + i * 8);
                deviceIndication.MeasResults[i].IsActive = arr[8 + i * 8] != 0;
            }
        }

        internal static void SetCommunicationStates(this ushort[] buffer, DeviceIndication deviceIndication)
        {
            var commStates = (ushort)buffer[17];
            deviceIndication.AdcBoardConnectState = (commStates & 0x0001) == 0;
            deviceIndication.TempBoardTelemetry.BoardConnectingState = (commStates & 0x0002) == 0;
            deviceIndication.HvBoardTelemetry.BoardConnectingState = (commStates & 0x0004) == 0;
        }

        internal static void SetTemBoardTelemetry(this ushort[] buffer, DeviceIndication deviceIndication)
        {
            deviceIndication.TempBoardTelemetry.Temperature = buffer.GetFloat(24) / 10;
        }

        internal static void SetHvBoardTelemetry(this ushort[] buffer, DeviceIndication deviceIndication)
        {
            deviceIndication.HvBoardTelemetry.InputVoltage = buffer.GetFloat(28);
            deviceIndication.HvBoardTelemetry.OutputVoltage = buffer.GetFloat(30);
        }

        internal static void SetModbusAddr(this ushort[] buffer, DeviceSettings settings)
        {
            settings.ModbusId = (byte)buffer[0];
        }
    }
}
