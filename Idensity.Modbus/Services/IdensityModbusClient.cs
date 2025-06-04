using EasyModbus;
using Idensity.Modbus.Models.Indication;
using Idensity.Modbus.Models.Modbus;
using System.IO.Ports;
using Idensity.Modbus.Extensions;
using Idensity.Modbus.Models.Settings;

namespace Idensity.Modbus.Services;

public class IdensityModbusClient(
    ModbusType modbusType = ModbusType.Rtu,
    string? portName = null,
    int baudrate = 115200,
    Parity parity = Parity.None)
{
    private ModbusClient _client = new ModbusClient();
    public bool Connected { get; private set; }
    public ModbusType ModbusType { get; set; } = modbusType;
    public string? PortName { get; set; } = portName;
    public int Baudrate { get; set; } = baudrate;
    public Parity Parity { get; set; } = parity;
    private readonly DeviceIndication _deviceIndication = new DeviceIndication();
    private readonly DeviceSettings _deviceSettings = new DeviceSettings();
    private readonly object _lock = new object();


    private void Connect()
    {
        if (ModbusType == ModbusType.Rtu)
        {
            _client.Baudrate = Baudrate;
            _client.Parity = Parity;
            _client.SerialPort = PortName ?? "Unknown port";
        }
        else
        {
            _client.SerialPort = null;
        }

        _client.Connect();
        Connected = _client.Connected;
    }

    private async Task DisconnectAsync()
    {
        if (_client.Connected)
        {
            await Task.Run(() => { _client.Disconnect(); }).ConfigureAwait(false);
        }
    }

    public async Task<DeviceIndication> GetIndicationDataAsync(string ip, byte unitId = 1, int portNum = 502)
    {
        await SetEthenetSettings(ip, portNum);
        var indication = await GetIndicationDataAsync(unitId);
        await DisconnectAsync();
        return indication;
    }

    public async Task<DeviceSettings> GetDeviceSettingsAsync(string ip, byte unitId = 1, int portNum = 502)
    {
        await SetEthenetSettings(ip, portNum);
        var settings = await GetDeviceSettingsAsync(unitId);
        await DisconnectAsync();
        return settings;
    }

    private async Task SetEthenetSettings(string ip, int portNum)
    {
        if (_client.Connected)
            await DisconnectAsync();
        ModbusType = ModbusType.Tcp;
        _client.IPAddress = ip;
        _client.Port = portNum;
    }

    public async Task<DeviceIndication> GetIndicationDataAsync(byte unitId = 1)
    {
        await Task.Run(() =>
            {
                _client.UnitIdentifier = unitId;
                if (!_client.Connected)
                    Connect();
                var buffer = _client.ReadInputRegisters(0, 60);
                if (buffer == null || buffer.Length < 60)
                    throw new Exception("Failed to read data from device. Buffer is null or has insufficient length.");
                GetMeasResults(buffer);
                GetCommunicationStates(buffer);
                GetRtc(buffer);
                GetAnalogOutputs(buffer);
                GetAnalogInputs(buffer);
                GetTemBoardTelemetry(buffer);
                GetHvBoardTelemetry(buffer);
            })
            .ConfigureAwait(false);

        return _deviceIndication;
    }

    public async Task<DeviceSettings> GetDeviceSettingsAsync(byte unitId = 1)
    {
        await Task.Run(() =>
            {
                _client.UnitIdentifier = unitId;
                if (!_client.Connected)
                    Connect();
            })
            .ConfigureAwait(false);
        return _deviceSettings;
    }

    public async Task ClearSpectrumAsync(string ip, byte unitId = 1, int portNum = 502)
    {
        await SetEthenetSettings(ip, portNum);
        await ClearSpectrumAsync(unitId);
        await DisconnectAsync();
    }

    /// <summary>
    /// Очистить спектр
    /// </summary>
    /// <param name="unitId">Адрес в сети Modbus</param>
    public async Task ClearSpectrumAsync(byte unitId = 1)
    {
        await Task.Run(() =>
            {
                _client.UnitIdentifier = unitId;
                if (!_client.Connected)
                    Connect();
                _client.WriteSingleRegister(7, 1);
            })
            .ConfigureAwait(false);
    }

    public async Task SwitchAdcBoardAsync(bool value, string ip, byte unitId = 1, int portNum = 502)
    {
        await SetEthenetSettings(ip,portNum);
        await SwitchAdcBoardAsync(value);
        await DisconnectAsync();
    }
    
    /// <summary>
    /// Команда "Запуск-останов платы АЦП"
    /// </summary>
    /// <param name="value">0 - Остановить, 1 -запустить</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    public async Task SwitchAdcBoardAsync(bool value, byte unitId = 1)
    {
        await Task.Run(() =>
            {
                _client.UnitIdentifier = unitId;
                if (!_client.Connected)
                    Connect();
                _client.WriteSingleRegister(8, value ? 1 : 0);
            })
            .ConfigureAwait(false);
    }

    public async Task StartStopAdcDataAsync(bool value, string ip, byte unitId = 1, int portNum = 502)
    {
        await SetEthenetSettings(ip, portNum);
        await StartStopAdcDataAsync(value);
        await DisconnectAsync();
    }
    
    /// <summary>
    /// Команда "Запуск/останов выдачи данных АЦП "
    /// </summary>
    /// <param name="value">0 - Остановить, 1 -запустить</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    public async Task StartStopAdcDataAsync(bool value, byte unitId = 1)
    {
        await Task.Run(() =>
            {
                _client.UnitIdentifier = unitId;
                if (!_client.Connected)
                    Connect();
                _client.WriteSingleRegister(9, value ? 1 : 0);
            })
            .ConfigureAwait(false);
    }


    private void GetMeasResults(int[] buffer)
    {
        _deviceIndication.IsMeasuringState = buffer[0] != 0;
        for (int i = 0; i < 2; i++)
        {
            _deviceIndication.MeasResults[i].ProcessNumber = buffer[1 + i * 8];
            _deviceIndication.MeasResults[i].CounterValue = buffer.GetFloat(2 + i * 8);
            _deviceIndication.MeasResults[i].CurrentValue = buffer.GetFloat(4 + i * 8);
            _deviceIndication.MeasResults[i].AverageValue = buffer.GetFloat(6 + i * 8);
            _deviceIndication.MeasResults[i].IsActive = buffer[8 + i * 8] != 0;
        }
    }

    private void GetCommunicationStates(int[] buffer)
    {
        ushort commStates = (ushort)buffer[17];
        _deviceIndication.AdcBoardConnectState = (commStates & 0x0001) == 0;
        _deviceIndication.TempBoardTelemetry.BoardConnectingState = (commStates & 0x0002) == 0;
        _deviceIndication.HvBoardTelemetry.BoardConnectingState = (commStates & 0x0004) == 0;
    }

    private void GetRtc(int[] buffer)
    {
        int year = buffer[18];
        int month = buffer[19];
        int day = buffer[20];
        int hour = buffer[21];
        int minute = buffer[22];
        int second = buffer[23];
        _deviceIndication.Rtc = new DateTime(year, month, day, hour, minute, second);
    }

    private void GetAnalogOutputs(int[] buffer)
    {
        int offset = 6;
        for (int i = 0; i < 2; i++)
        {
            _deviceIndication.AnalogOutputIndications[i].PwrState = buffer[32 + i * offset] != 0;
            _deviceIndication.AnalogOutputIndications[i].CommState = buffer[33 + i * offset] != 0;
            _deviceIndication.AnalogOutputIndications[i].AdcValue = (ushort)buffer.GetFloat(34 + i * offset);
            _deviceIndication.AnalogOutputIndications[i].DacValue = (ushort)buffer.GetFloat(36 + i * offset);
        }
    }

    private void GetAnalogInputs(int[] buffer)
    {
        int offset = 6;
        for (int i = 0; i < 2; i++)
        {
            _deviceIndication.AnalogInputIndications[i].PwrState = buffer[44 + i * offset] != 0;
            _deviceIndication.AnalogInputIndications[i].CommState = buffer[45 + i * offset] != 0;
            _deviceIndication.AnalogInputIndications[i].AdcValue = (ushort)buffer.GetFloat(46 + i * offset);
        }
    }

    private void GetTemBoardTelemetry(int[] buffer)
    {
        _deviceIndication.TempBoardTelemetry.Temperature = buffer.GetFloat(24) / 10;
    }

    private void GetHvBoardTelemetry(int[] buffer)
    {
        _deviceIndication.HvBoardTelemetry.InputVoltage = buffer.GetFloat(28);
        _deviceIndication.HvBoardTelemetry.OutputVoltage = buffer.GetFloat(30);
    }
}