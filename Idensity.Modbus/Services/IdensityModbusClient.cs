using Idensity.Modbus.Models.Indication;
using Idensity.Modbus.Models.Modbus;
using System.IO.Ports;
using System.Net;
using FluentModbus;
using Idensity.Modbus.Extensions;
using Idensity.Modbus.Models.Settings;
using Idensity.Modbus.Models.Settings.AdcSettings;
using Idensity.Modbus.Models.Settings.Analogs;

namespace Idensity.Modbus.Services;

public class IdensityModbusClient
{
    private readonly ModbusRtuClient _rtuClient = new ModbusRtuClient();
    private readonly ModbusTcpClient _tcpClient = new ModbusTcpClient();
    private ModbusClient _client;
    public bool Connected => ModbusType == ModbusType.Rtu ? _rtuClient.IsConnected : _tcpClient.IsConnected;
    public ModbusType ModbusType { get; set; }
    public string? PortName { get; set; }
    private string _ipAddress = "127.0.0.1";
    private int _port = 0;
    public int Baudrate { get; set; }
    public Parity Parity { get; set; }
    private readonly DeviceIndication _deviceIndication = new DeviceIndication();
    private readonly DeviceSettings _deviceSettings = new DeviceSettings();
    private readonly object _lock = new object();

    public IdensityModbusClient(ModbusType modbusType = ModbusType.Rtu,
        string? portName = null,
        int baudrate = 115200,
        Parity parity = Parity.None)
    {
        ModbusType = modbusType;
        PortName = portName;
        Baudrate = baudrate;
        Parity = parity;
        _client = modbusType != ModbusType.Rtu ? _tcpClient : _rtuClient;
    }


    private async Task  ConnectAsync()
    {
        await Task.Run(() =>
        {
            if (ModbusType == ModbusType.Rtu)
            {
                _rtuClient.BaudRate = Baudrate;
                _rtuClient.Parity = Parity;
                _rtuClient.Connect(PortName ?? "Unknown port");
                _client  = _rtuClient;
            }
            else
            {
                _tcpClient.Disconnect();
                _tcpClient.Connect(new IPEndPoint(IPAddress.Parse(_ipAddress), _port));
                _client = _tcpClient;
            }
        }).ConfigureAwait(false);
    }

    private async Task DisconnectAsync()
    {
        if (Connected)
        {
            await Task.Run(() =>
            {
                if (_client is ModbusTcpClient tcpClient)
                {
                    tcpClient.Disconnect();
                }
                else if(_client is ModbusRtuClient rtuClient)
                {
                    _rtuClient.Close();
                }
                
            }).ConfigureAwait(false);
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
        ModbusType = ModbusType.Tcp;
        _ipAddress = ip;
        _port = portNum;
        if (_tcpClient.IsConnected)
            await DisconnectAsync();
        
    }

    public async Task<DeviceIndication> GetIndicationDataAsync(byte unitId = 1)
    {
        if (!Connected)
            await ConnectAsync();
        var memory =  await _client.ReadInputRegistersAsync<ushort>(unitId, 0, 60)
            .ConfigureAwait(false);
        var buffer=memory.ToArray();
        if (buffer == null || buffer.Length < 60)
            throw new Exception("Failed to read data from device. Buffer is null or has insufficient length.");
        GetMeasResults(buffer);
        GetCommunicationStates(buffer);
        GetRtc(buffer);
        GetAnalogOutputs(buffer);
        GetAnalogInputs(buffer);
        GetTemBoardTelemetry(buffer);
        GetHvBoardTelemetry(buffer);

        return _deviceIndication;
    }

    public async Task<DeviceSettings> GetDeviceSettingsAsync(byte unitId = 1)
    {
        if (!Connected)
            await ConnectAsync();
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
        if (!Connected)
            await ConnectAsync();
        await _client.WriteSingleRegisterAsync(unitId, 7, 1)
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
        if (!Connected)
            await ConnectAsync();
        await _client.WriteSingleRegisterAsync(unitId, 8, value ? (short)1 : (short)0)
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

        if (!Connected)
            await ConnectAsync();
        await _client.WriteSingleRegisterAsync(unitId, 9, value ? (short)1 : (short)0)
            .ConfigureAwait(false);
    }


    private void GetMeasResults(ushort[] buffer)
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

    private void GetCommunicationStates(ushort[] buffer)
    {
        ushort commStates = (ushort)buffer[17];
        _deviceIndication.AdcBoardConnectState = (commStates & 0x0001) == 0;
        _deviceIndication.TempBoardTelemetry.BoardConnectingState = (commStates & 0x0002) == 0;
        _deviceIndication.HvBoardTelemetry.BoardConnectingState = (commStates & 0x0004) == 0;
    }

    private void GetRtc(ushort[] buffer)
    {
        int year = buffer[18]+2000;
        int month = buffer[19];
        if (month < 1 || month > 12)
            return;
        int day = buffer[20];
        if (day < 1 || day > DateTime.DaysInMonth(year, month))
            return;
        int hour = buffer[21];
        if (hour < 0 || hour > 23)
            return;
        int minute = buffer[22];
        if (minute < 0 || minute > 59)
            return;
        int second = buffer[23];
        if (second < 0 || second > 59)
            return;
        _deviceIndication.Rtc = new DateTime(year, month, day, hour, minute, second);
    }

    private void GetAnalogOutputs(ushort[] buffer)
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

    private void GetAnalogInputs(ushort[] buffer)
    {
        int offset = 6;
        for (int i = 0; i < 2; i++)
        {
            _deviceIndication.AnalogInputIndications[i].PwrState = buffer[44 + i * offset] != 0;
            _deviceIndication.AnalogInputIndications[i].CommState = buffer[45 + i * offset] != 0;
            _deviceIndication.AnalogInputIndications[i].AdcValue = (ushort)buffer.GetFloat(46 + i * offset);
        }
    }

    private void GetTemBoardTelemetry(ushort[] buffer)
    {
        _deviceIndication.TempBoardTelemetry.Temperature = buffer.GetFloat(24) / 10;
    }

    private void GetHvBoardTelemetry(ushort[] buffer)
    {
        _deviceIndication.HvBoardTelemetry.InputVoltage = buffer.GetFloat(28);
        _deviceIndication.HvBoardTelemetry.OutputVoltage = buffer.GetFloat(30);
    }

    public async Task WriteMeasProcessAsync(MeasProcess process, int processNum, 
        string ip, byte unitId = 1, int portNum = 502)
    {
        await Task.Delay(100);        
    }

    /// <summary>
    /// Записать измерительный процесс в устройство
    /// </summary>
    /// <param name="process">Данные</param>
    /// <param name="processNum">Номер изм процесса</param>
    /// <param name="unitId">Номер в сети modbus</param>
    /// <returns></returns>
    public async Task WriteMeasProcessAsync(MeasProcess process, int processNum,
        byte unitId = 1)
    {
        await Task.Delay(100);
    }

    /// <summary>
    /// Записать настройки счетчика в устройство
    /// </summary>
    /// <param name="counterSettings">Данные диапазона счетчика</param>
    /// <param name="counterNum">Номер диапазона</param>
    /// <param name="ip">Ip</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <param name="portNum">Номер порта (default 502)</param>
    /// <returns></returns>
    public async Task WriteCounterAsync(CounterSettings counterSettings, int counterNum,
       string ip, byte unitId = 1, int portNum = 502)
    {
        await Task.Delay(100);
    }

    /// <summary>
    /// Записать настройки счетчика в устройство
    /// </summary>
    /// <param name="counterSettings">Данные диапазона счетчика</param>
    /// <param name="counterNum">Номер диапазона</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public async Task WriteCounterAsync(CounterSettings counterSettings, int counterNum,
       byte unitId = 1)
    {
        await Task.Delay(100);
    }

    public async Task WriteModbusNumberAsync(byte modbusNum, string ip, byte unitId = 1, int portNum = 502)
    {
        await Task.Delay(100);
    }

    /// <summary>
    /// Поменять адрес в сети Modbus
    /// </summary>
    /// <param name="modbusNum">Новый адрес</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public async Task WriteModbusNumberAsync(byte modbusNum, byte unitId = 1)
    {
        await Task.Delay(100);
    }

    public async Task WriteEthernetSettingsAsync(TcpSettings settings, string ip, byte unitId = 1, int portNum = 502)
    {
        await Task.Delay(100);
    }

    /// <summary>
    /// Записать настройки Ethernet соединения в устройство
    /// </summary>
    /// <param name="settings">Настройки</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public async Task WriteEthernetSettingsAsync(TcpSettings settings, byte unitId = 1)
    {
        await Task.Delay(100);
    }

    public async Task WriteSerialSettingsAsync(SerialSettings settings, string ip, byte unitId = 1, int portNum = 502)
    {
        await Task.Delay(100);
    }

    /// <summary>
    /// Записать настройки последовательного соединения в устройство
    /// </summary>
    /// <param name="settings">Настройки</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public async Task WriteSerialSettingsAsync(SerialSettings settings, byte unitId = 1)
    {
        await Task.Delay(100);
    }

    public async Task WriteAdcBoardSettingsAsync(AdcBoardSettings settings, string ip, byte unitId = 1, int portNum = 502)
    {
        await Task.Delay(100);
    }

    /// <summary>
    /// Записать настройки платы АЦП в устройство
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public async Task WriteAdcBoardSettingsAsync(AdcBoardSettings settings, byte unitId = 1)
    {
        await Task.Delay(100);
    }

    public async Task SetAnalogInputActivityAsync(byte inputNumber, string ip, byte unitId = 1, int portNum = 502)
    {
        await Task.Delay(100);
    }

    /// <summary>
    /// Установить активность аналогового входа устройства
    /// </summary>
    /// <param name="inputNumber">Номер AI</param>
    /// <param name="value">Значение активности</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public async Task SetAnalogInputActivityAsync(byte inputNumber, bool value, byte unitId = 1)
    {
        await Task.Delay(100);
    }

    public async Task CmdSwitchAnalogInputAsync(byte inputNumber, bool value, string ip, byte unitId = 1, int portNum = 502)
    {        
        await Task.Delay(100);
    }

    /// <summary>
    /// Команда управления питанием аналогового выхода
    /// </summary>
    /// <param name="inputNumber"></param>
    /// <param name="value"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public async Task CmdSwitchAnalogInputAsync(byte inputNumber, bool value, byte unitId = 1)
    {
        await Task.Delay(100);
    }

    public async Task WriteAnalogOutputSettingsAsync(AnalogOutputSettings settings, byte outputNumber,  
        string ip, byte unitId = 1, int portNum = 502)
    {
        await Task.Delay(100);
    }


    /// <summary>
    /// Записать настройки аналогового выхода в устройство
    /// </summary>
    /// <param name="settings">Данные аналогового выхода</param>
    /// <param name="outputNumber">Номер выхода (0,1)</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public async Task WriteAnalogOutputSettingsAsync(AnalogOutputSettings settings, byte outputNumber, 
        byte unitId = 1)
    {
        await Task.Delay(100);
    }

    public async Task CmdSwitchAnalogOutputAsync(byte outputNumber, bool value, string ip, byte unitId = 1, int portNum = 502)
    {
        await Task.Delay(100);
    }

    /// <summary>
    /// Команда управления питанием аналогового выхода
    /// </summary>
    /// <param name="outputNumber">Номер выхода</param>
    /// <param name="value"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public async Task CmdSwitchAnalogOutputAsync(byte outputNumber, bool value, byte unitId = 1)
    {
        await Task.Delay(100);
    }





}