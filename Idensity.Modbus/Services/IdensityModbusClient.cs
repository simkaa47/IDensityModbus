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
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

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

    private async Task CommonWriteAsync(ushort[] buffer, ushort offset, ushort count, byte unitId)
    {
        await _semaphore.WaitAsync();
        try
        {
            if(!Connected)
                await ConnectAsync();
            await _client.WriteMultipleRegistersAsync(unitId, offset, buffer);
            if (_client is ModbusTcpClient)
                await DisconnectAsync();
            
        }
        catch(Exception ex)
        {
            await DisconnectAsync();
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<ushort[]> CommonReadAsync(ushort offset, ushort count, byte unitId, RegisterType registerType)
    {
        
        await _semaphore.WaitAsync();
        try
        {
            if(!Connected)
                await ConnectAsync();
            ushort[] buffer;
            if (registerType == RegisterType.Holding)
            {
                var memory = await _client.ReadHoldingRegistersAsync<ushort>(unitId, offset, count)
                    .ConfigureAwait(false);
                buffer=memory.ToArray();
            }
            else
            {
                var memory = await _client.ReadInputRegistersAsync<ushort>(unitId, offset, count)
                    .ConfigureAwait(false);
                buffer=memory.ToArray();
            }
            if(buffer.Length != count)
                throw new Exception("Buffer length doesn't match");
            return buffer;
        }
        catch(Exception ex)
        {
            await DisconnectAsync();
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }


    private async Task  ConnectAsync()
    {
        await Task.Run(() =>
        {
            if (ModbusType == ModbusType.Rtu)
            {
                _rtuClient.BaudRate = Baudrate;
                _rtuClient.Parity = Parity;
                _rtuClient.Connect(PortName ?? "Unknown port", ModbusEndianness.BigEndian);
                _client  = _rtuClient;
            }
            else
            {
                _tcpClient.Disconnect();
                _tcpClient.Connect(new IPEndPoint(IPAddress.Parse(_ipAddress), _port), ModbusEndianness.BigEndian);
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

    public Task<DeviceIndication> GetIndicationDataAsync(string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return GetIndicationDataAsync(unitId);
    }

    public Task<DeviceSettings> GetDeviceSettingsAsync(string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return  GetDeviceSettingsAsync(unitId);
    }

    private void SetEthenetSettings(string ip, int portNum)
    {
        ModbusType = ModbusType.Tcp;
        _ipAddress = ip;
        _port = portNum;
    }

    public async Task<DeviceIndication> GetIndicationDataAsync(byte unitId = 1)
    {
        var buffer = await CommonReadAsync(0, 60, unitId, RegisterType.Input);
        buffer.SetMeasResults(_deviceIndication);
        buffer.SetCommunicationStates(_deviceIndication);
        buffer.SetRtc(_deviceIndication.Rtc);
        buffer.SetAnalogOutputsIndication(_deviceIndication);
        buffer.SetAnalogInputsIndication(_deviceIndication);
        buffer.SetTemBoardTelemetry(_deviceIndication);
        buffer.SetHvBoardTelemetry(_deviceIndication);

        return _deviceIndication;
    }

    public async Task<DeviceSettings> GetDeviceSettingsAsync(byte unitId = 1)
    {
        var buffer = await CommonReadAsync(0,125,unitId, RegisterType.Holding);
        buffer.SetAdcBoardSettings(_deviceSettings.AdcBoardSettings);
        buffer.SetCounterSettings(_deviceSettings);
        buffer.SetModbusAddr(_deviceSettings);
        buffer.SetEthernetSettings(_deviceSettings);
        buffer.SetSerialSettings(_deviceSettings.SerialSettings);
        buffer.SetAnalogInputSettings(_deviceSettings);
        buffer.SetAnalogOutputSettings(_deviceSettings);
        return _deviceSettings;
    }

    public Task ClearSpectrumAsync(string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return ClearSpectrumAsync(unitId);
    }

    /// <summary>
    /// Очистить спектр
    /// </summary>
    /// <param name="unitId">Адрес в сети Modbus</param>
    public Task ClearSpectrumAsync(byte unitId = 1)
    {
        var buffer = new ushort[] { 1 };
        return CommonWriteAsync(buffer, 18, (ushort)buffer.Length, unitId);
    }

    public Task SwitchAdcBoardAsync(bool value, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip,portNum);
        return SwitchAdcBoardAsync(value);
    }
    
    /// <summary>
    /// Команда "Запуск-останов платы АЦП"
    /// </summary>
    /// <param name="value">0 - Остановить, 1 -запустить</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    public Task SwitchAdcBoardAsync(bool value, byte unitId = 1)
    {
        var buffer = new ushort[] { value ? (ushort)1 : (ushort)0 };
        return CommonWriteAsync(buffer, 19, (ushort)buffer.Length, unitId);
    }

    public Task StartStopAdcDataAsync(bool value, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
         return StartStopAdcDataAsync(value);
    }

    /// <summary>
    /// Команда "Запуск/останов выдачи данных АЦП "
    /// </summary>
    /// <param name="value">0 - Остановить, 1 -запустить</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    public Task StartStopAdcDataAsync(bool value, byte unitId = 1)
    {
        var buffer = new ushort[] { value ? (ushort)1 : (ushort)0 };
        return CommonWriteAsync(buffer, 20, (ushort)buffer.Length, unitId);
    }

    public Task SwitcHvAsync(bool value, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return SwitcHvAsync(value, unitId);
    }
    
    /// <summary>
    /// Команду управления HV
    /// </summary>
    /// <param name="value">0 - Выкл, 1 - Вкл</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    public Task SwitcHvAsync(bool value, byte unitId = 1)
    {
        var buffer = new ushort[] { value ? (ushort)1 : (ushort)0 };
        return CommonWriteAsync(buffer, 21, (ushort)buffer.Length, unitId);
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
    public Task WriteCounterAsync(CounterSettings counterSettings, int counterNum,
       string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteCounterAsync(counterSettings, counterNum, unitId);
    }

    /// <summary>
    /// Записать настройки счетчика в устройство
    /// </summary>
    /// <param name="counterSettings">Данные диапазона счетчика</param>
    /// <param name="counterNum">Номер диапазона</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteCounterAsync(CounterSettings counterSettings, int counterNum,
       byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = counterSettings.GetRegisters(counterNum, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteModbusNumberAsync(byte modbusNum, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteModbusNumberAsync(modbusNum, unitId);
    }

    /// <summary>
    /// Поменять адрес в сети Modbus
    /// </summary>
    /// <param name="modbusNum">Новый адрес</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteModbusNumberAsync(byte modbusNum, byte unitId = 1)
    {
       return CommonWriteAsync([modbusNum], 0, 1, unitId);
    }

    public Task WriteEthernetSettingsAsync(TcpSettings settings, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteEthernetSettingsAsync(settings, unitId);
    }

    /// <summary>
    /// Записать настройки Ethernet соединения в устройство
    /// </summary>
    /// <param name="settings">Настройки</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteEthernetSettingsAsync(TcpSettings settings, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = settings.GetRegisters(ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteSerialSettingsAsync(SerialSettings settings, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteSerialSettingsAsync(settings, unitId);
    }

    /// <summary>
    /// Записать настройки последовательного соединения в устройство
    /// </summary>
    /// <param name="settings">Настройки</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteSerialSettingsAsync(SerialSettings settings, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = settings.GetRegisters(ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteAdcBoardSettingsAsync(AdcBoardSettings settings, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteAdcBoardSettingsAsync(settings, unitId);
    }

    /// <summary>
    /// Записать настройки платы АЦП в устройство
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public  Task WriteAdcBoardSettingsAsync(AdcBoardSettings settings, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = settings.GetRegisters(ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task SetAnalogInputActivityAsync(byte inputNumber, bool value, string ip, 
        byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return SetAnalogInputActivityAsync(inputNumber, value, unitId);
    }

    /// <summary>
    /// Установить активность аналогового входа устройства
    /// </summary>
    /// <param name="inputNumber">Номер AI</param>
    /// <param name="value">Значение активности</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task SetAnalogInputActivityAsync(byte inputNumber, bool value, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = AnalogModulesExtensions.SwitchAnalogInputsPwr(inputNumber, value, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId); 
    }
    
    public Task WriteAnalogOutputSettingsAsync(AnalogOutputSettings settings, byte outputNumber,  
        string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteAnalogOutputSettingsAsync(settings, outputNumber, unitId);
    }


    /// <summary>
    /// Записать настройки аналогового выхода в устройство
    /// </summary>
    /// <param name="settings">Данные аналогового выхода</param>
    /// <param name="outputNumber">Номер выхода (0,1)</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteAnalogOutputSettingsAsync(AnalogOutputSettings settings, byte outputNumber, 
        byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = settings.GetRegisters(outputNumber, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId); 
    }

    public Task SetAnalogOutputActivityAsync(byte outputNumber, bool value, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return SetAnalogOutputActivityAsync(outputNumber, value, unitId);
    }

    /// <summary>
    /// Команда управления питанием аналогового выхода
    /// </summary>
    /// <param name="outputNumber">Номер выхода</param>
    /// <param name="value"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public Task SetAnalogOutputActivityAsync(byte outputNumber, bool value, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = AnalogModulesExtensions.SwitchAnalogOutputsPwr(outputNumber, value, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId); 
    }

    public Task SendAnalogTestValue(byte outNum, float testValue, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return SendAnalogTestValue(outNum, testValue, unitId);
    }


    /// <summary>
    /// Отправить тестовое значение аналогового выхода
    /// </summary>
    /// <param name="outNum">Номер выхода (0,1)</param>
    /// <param name="testValue">Ток, в mA</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task SendAnalogTestValue(byte outNum, float testValue, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = AnalogModulesExtensions.SendTestValue(outNum, testValue, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId); 
    }

}