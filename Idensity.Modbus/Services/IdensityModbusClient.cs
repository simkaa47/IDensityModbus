using Idensity.Modbus.Models.Indication;
using Idensity.Modbus.Models.Modbus;
using System.IO.Ports;
using System.Net;
using FluentModbus;
using Idensity.Modbus.Extensions;
using Idensity.Modbus.Models.Settings;
using Idensity.Modbus.Models.Settings.AdcSettings;
using Idensity.Modbus.Models.Settings.Analogs;
using System.ComponentModel.DataAnnotations;

namespace Idensity.Modbus.Services;

public class IdensityModbusClient
{
    private const byte RegistersMaxSizeForRead = 50;
    private readonly ushort[] _inputBuffer = new ushort[2000];
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
            if (!Connected)
                await ConnectAsync();
            await _client.WriteMultipleRegistersAsync(unitId, offset, buffer);
            if (_client is ModbusTcpClient)
                await DisconnectAsync();
        }
        catch (Exception ex)
        {
            await DisconnectAsync();
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task CommonReadAsync(ushort offset, ushort count, byte unitId, RegisterType registerType)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!Connected)
                await ConnectAsync();
            int steps = count % RegistersMaxSizeForRead == 0
                ? count / RegistersMaxSizeForRead
                : count / RegistersMaxSizeForRead + 1;
            int start = offset;
            for (int i = 0; i < steps; i++)
            {
                var tmpCnt = Math.Min(RegistersMaxSizeForRead, count- (i*RegistersMaxSizeForRead));
                ushort[] buffer;
                if (registerType == RegisterType.Holding)
                {
                    var memory = await _client.ReadHoldingRegistersAsync<ushort>(unitId, start, tmpCnt)
                        .ConfigureAwait(false);
                    buffer = memory.ToArray();
                }
                else
                {
                    var memory = await _client.ReadInputRegistersAsync<ushort>(unitId, start, tmpCnt)
                        .ConfigureAwait(false);
                    buffer = memory.ToArray();
                }                

                if (buffer.Length != tmpCnt)
                    throw new Exception("Buffer length doesn't match");
                buffer.CopyTo(_inputBuffer, start);
                start += tmpCnt;
            }
        }
        catch (Exception ex)
        {
            await DisconnectAsync();
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }


    private async Task ConnectAsync()
    {
        await Task.Run(() =>
        {
            if (ModbusType == ModbusType.Rtu)
            {
                _rtuClient.BaudRate = Baudrate;
                _rtuClient.Parity = Parity;
                _rtuClient.Connect(PortName ?? "Unknown port", ModbusEndianness.BigEndian);
                _client = _rtuClient;
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
                else if (_client is ModbusRtuClient rtuClient)
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
        return GetDeviceSettingsAsync(unitId);
    }

    private void SetEthenetSettings(string ip, int portNum)
    {
        ModbusType = ModbusType.Tcp;
        _ipAddress = ip;
        _port = portNum;
    }

    public async Task<DeviceIndication> GetIndicationDataAsync(byte unitId = 1)
    {
        await CommonReadAsync(0, 60, unitId, RegisterType.Input);
        _inputBuffer.SetMeasResults(_deviceIndication);
        _inputBuffer.SetCommunicationStates(_deviceIndication);
        _deviceIndication.Rtc = _inputBuffer.SetRtc(_deviceIndication.Rtc);
        _inputBuffer.SetAnalogOutputsIndication(_deviceIndication);
        _inputBuffer.SetAnalogInputsIndication(_deviceIndication);
        _inputBuffer.SetTemBoardTelemetry(_deviceIndication);
        _inputBuffer.SetHvBoardTelemetry(_deviceIndication);

        return _deviceIndication;
    }

    public async Task<DeviceSettings> GetDeviceSettingsAsync(byte unitId = 1)
    {
        await CommonReadAsync(0, 125, unitId, RegisterType.Holding);
        _inputBuffer.SetAdcBoardSettings(_deviceSettings.AdcBoardSettings);
        _inputBuffer.SetCounterSettings(_deviceSettings);
        _inputBuffer.SetModbusAddr(_deviceSettings);
        _inputBuffer.SetEthernetSettings(_deviceSettings);
        _inputBuffer.SetSerialSettings(_deviceSettings.SerialSettings);
        _inputBuffer.SetAnalogInputSettings(_deviceSettings);
        _inputBuffer.SetAnalogOutputSettings(_deviceSettings);
        _deviceSettings.DeviceType = (DeviceType)_inputBuffer[102];
        _deviceSettings.LevelLength = _inputBuffer.GetFloat(112);
        _inputBuffer.SetTemperatureCompensationSettings(_deviceSettings.GetTemperature);
        for (byte i = 0; i < MeasProcessExtensions.MeasProcCnt; i++)
        {
            await CommonReadAsync(
                (ushort)(MeasProcessExtensions.StartMeasProcRegisterOffset +
                         MeasProcessExtensions.MeasProcRegisterCnt * i),
                150, unitId, RegisterType.Holding);
            _inputBuffer.SetMeasProcess(i, _deviceSettings.MeasProcesses[i]);
        }

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
        SetEthenetSettings(ip, portNum);
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

    public Task WriteAdcBoardModeAsync(AdcBoardMode mode, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteAdcBoardModeAsync(mode, unitId);
    }

    /// <summary>
    /// Записать настройки платы АЦП в устройство
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public Task WriteAdcBoardModeAsync(AdcBoardMode mode, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = AdcBoardSettingsExtensions.GetModeRegisters(mode, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteAdcBoardSyncLevelAsync(ushort syncLevel, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteAdcBoardSyncLevelAsync(syncLevel, unitId);
    }

    /// <summary>
    /// Записать уровень синхронизации платы АЦП в устройство
    /// </summary>
    /// <param name="syncLevel"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public Task WriteAdcBoardSyncLevelAsync(ushort syncLevel, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = AdcBoardSettingsExtensions.GetSyncLevelRegisters(syncLevel, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteAdcBoardTimerSendDataAsync(ushort timerValue, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteAdcBoardTimerSendDataAsync(timerValue, unitId);
    }

    /// <summary>
    /// Записать таймер выдачи данных платы АЦП в устройство
    /// </summary>
    /// <param name="timerValue"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public Task WriteAdcBoardTimerSendDataAsync(ushort timerValue, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = AdcBoardSettingsExtensions.GetTimerSendRegisters(timerValue, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteAdcBoardGainAsync(byte gain, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteAdcBoardGainAsync(gain, unitId);
    }

    /// <summary>
    /// Записать к-т предусиления платы АЦП в устройство
    /// </summary>
    /// <param name="gain"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public Task WriteAdcBoardGainAsync(byte gain, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = AdcBoardSettingsExtensions.GetGainRegisters(gain, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteAdcBoardUpdAddressAsync(byte[] addr, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteAdcBoardUpdAddressAsync(addr, unitId);
    }

    /// <summary>
    /// Записать ip адрес получателя спектра
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public Task WriteAdcBoardUpdAddressAsync(byte[] addr, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = AdcBoardSettingsExtensions.GetUpdAddressRegisters(addr, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteAdcBoardUpdPortAsync(ushort port, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteAdcBoardUpdPortAsync(port, unitId);
    }

    /// <summary>
    /// Записать номер порта получателя спектра
    /// </summary>
    /// <param name="port"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public Task WriteAdcBoardUpdPortAsync(ushort port, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = AdcBoardSettingsExtensions.GetUpdPortRegisters(port, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteAdcBoardHvAsync(ushort hv, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteAdcBoardHvAsync(hv, unitId);
    }

    /// <summary>
    /// Установить уставку высокого напряжения
    /// </summary>
    /// <param name="hv">Уставка напряжения, вольт</param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public Task WriteAdcBoardHvAsync(ushort hv, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = AdcBoardSettingsExtensions.GetHvRegisters(hv, ref startIndex);
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

    public Task SetAnalogOutputActivityAsync(byte outputNumber, bool value, string ip, byte unitId = 1,
        int portNum = 502)
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

    public Task SendAnalogTestValueAsync(byte outNum, float testValue, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return SendAnalogTestValueAsync(outNum, testValue, unitId);
    }


    /// <summary>
    /// Отправить тестовое значение аналогового выхода
    /// </summary>
    /// <param name="outNum">Номер выхода (0,1)</param>
    /// <param name="testValue">Ток, в mA</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task SendAnalogTestValueAsync(byte outNum, float testValue, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = AnalogModulesExtensions.SendTestValue(outNum, testValue, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteDeviceTypeAsync(DeviceType type, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteDeviceTypeAsync(type, unitId);
    }


    /// <summary>
    /// Изменить тип прибора
    /// </summary>
    /// <param name="type"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public Task WriteDeviceTypeAsync(DeviceType type, byte unitId = 1)
    {
        return CommonWriteAsync([(ushort)type], 102, 1, unitId);
    }

    public Task WriteTempCompensationSettingsAsync(GetTemperature settings, string ip, byte unitId = 1,
        int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteTempCompensationSettingsAsync(settings, unitId);
    }


    /// <summary>
    /// Записать настройки компенсации температуры
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public Task WriteTempCompensationSettingsAsync(GetTemperature settings, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = settings.GetRegisters(ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteLevelLengthAsync(float levelLength, string ip, byte unitId = 1,
        int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteLevelLengthAsync(levelLength, unitId);
    }


    /// <summary>
    /// Записать длину уровнемера, мм
    /// </summary>
    /// <param name="levelLength"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public Task WriteLevelLengthAsync(float levelLength, byte unitId = 1)
    {
        return CommonWriteAsync([..levelLength.GetRegisters()], 112, 2, unitId);
    }

    public Task SwitchCycleMeasures(bool value, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return SwitchCycleMeasures(value, unitId);
    }


    /// <summary>
    /// Включить-выключить циклические измерения
    /// </summary>
    /// <param name="value">false  - выключить, true - включить</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task SwitchCycleMeasures(bool value, byte unitId = 1)
    {
        return CommonWriteAsync([(ushort)(value ? 1 : 0)], 114, 1, unitId);
    }

    public Task SetMeasProcDuration(float duration, int measProcIndex, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return SetMeasProcDuration(duration, measProcIndex, unitId);
    }


    /// <summary>
    /// Записать длительность одного измерения, с
    /// </summary>
    /// <param name="duration">длительность одного измерения, с</param>
    /// <param name="measProcIndex">Индекс изм процесса, 0-7</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task SetMeasProcDuration(float duration, int measProcIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetDurationRegisters(duration, measProcIndex, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteMeasProcDeep(byte deep, int measProcIndex, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteMeasProcDeep(deep, measProcIndex, unitId);
    }


    /// <summary>
    /// Записать кол-во точек усреднения
    /// </summary>
    /// <param name="deep">Кол-во точек усреднения, 0-99</param>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteMeasProcDeep(byte deep, int measProcIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetDeepRegisters(deep, measProcIndex, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }


    public Task WriteMeasProcPipeDiameter(ushort diameter, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteMeasProcPipeDiameter(diameter, measProcIndex, unitId);
    }

    /// <summary>
    /// Записать диаметр трубы, мм
    /// </summary>
    /// <param name="diameter">диаметр трубы, мм</param>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteMeasProcPipeDiameter(ushort diameter, int measProcIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetPipeDiameterRegisters(diameter, measProcIndex, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteMeasProcActivity(bool activity, int measProcIndex, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteMeasProcActivity(activity, measProcIndex, unitId);
    }


    /// <summary>
    /// Установить активность изм процесса
    /// </summary>
    /// <param name="activity">Активность</param>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteMeasProcActivity(bool activity, int measProcIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetActivityRegisters(activity, measProcIndex, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteMeasProcCalcType(CalculationType type, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteMeasProcCalcType(type, measProcIndex, unitId);
    }

    /// <summary>
    /// Записать тип расчета измерительного процесса
    /// </summary>
    /// <param name="type">Тип расчета измерительного процесса</param>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteMeasProcCalcType(CalculationType type, int measProcIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetCalculationTypeRegisters(type, measProcIndex, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteMeasProcMeasType(ushort measType, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteMeasProcMeasType(measType, measProcIndex, unitId);
    }

    /// <summary>
    /// Записать тип измерения измерительного процесса
    /// </summary>
    /// <param name="measType">Тип измерения</param>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteMeasProcMeasType(ushort measType, int measProcIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetMeasTypeRegisters(measType, measProcIndex, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteMeasProcDensityLiquid(float density, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteMeasProcDensityLiquid(density, measProcIndex, unitId);
    }

    /// <summary>
    /// Записать плотность жидкого в изм процесс
    /// </summary>
    /// <param name="density">Плотность жидкого</param>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteMeasProcDensityLiquid(float density, int measProcIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetDensityLiquidRegisters(density, measProcIndex, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteMeasProcDensitySolid(float density, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteMeasProcDensitySolid(density, measProcIndex, unitId);
    }

    /// <summary>
    /// Записать плотность твердого в изм процесс
    /// </summary>
    /// <param name="density">Плотность твердого</param>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteMeasProcDensitySolid(float density, int measProcIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetDensitySolidRegisters(density, measProcIndex, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteMeasProcFastChange(FastChange fastChange, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteMeasProcFastChange(fastChange, measProcIndex, unitId);
    }

    /// <summary>
    /// Записать настройки быстрых измерений в изм процесс
    /// </summary>
    /// <param name="fastChange">настройки быстрых измерений</param>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteMeasProcFastChange(FastChange fastChange, int measProcIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetFastChangeRegisters(fastChange, measProcIndex, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteMeasProcSingleMeasDuration(ushort duration, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteMeasProcSingleMeasDuration(duration, measProcIndex, unitId);
    }


    /// <summary>
    /// Записать длительность единичного измерения, с
    /// </summary>
    /// <param name="duration">Длительность единичного измерения, с</param>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteMeasProcSingleMeasDuration(ushort duration, int measProcIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetSingleMeasureDurationRegisters(duration, measProcIndex, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteMeasProcStandartisationData(StandSettings settings, int measProcIndex, int standIndex,
        string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteMeasProcStandartisationData(settings, measProcIndex, standIndex, unitId);
    }

    
    /// <summary>
    /// Записать данные стандартизации в измерительный процесс
    /// </summary>
    /// <param name="settings">данные стандартизации</param>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="standIndex">Индекс стандартизации</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteMeasProcStandartisationData(StandSettings settings, int measProcIndex, int standIndex,
        byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer =
            MeasProcessExtensions.GetStandartisationRegisters(settings, measProcIndex, standIndex, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteMeasProcCalibrCurve(CalibrCurve curve, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteMeasProcCalibrCurve(curve, measProcIndex, unitId);
    }
    
    /// <summary>
    /// Записать данные калибровочной кривой
    /// </summary>
    /// <param name="curve">данные калибровочной кривой</param>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteMeasProcCalibrCurve(CalibrCurve curve, int measProcIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetCalibrCurveRegisters(curve, measProcIndex, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task WriteMeasProcSingleMeasResult(SingleMeasResult result, int measProcIndex, int singleMeasIndex,
        string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return WriteMeasProcSingleMeasResult(result, measProcIndex, singleMeasIndex, unitId);
    }

    /// <summary>
    /// Записать данные единичного измерения
    /// </summary>
    /// <param name="result">Данные единичного измерения</param>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="singleMeasIndex">Индекс ед измерения</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task WriteMeasProcSingleMeasResult(SingleMeasResult result, int measProcIndex, int singleMeasIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetSingleMeasureDataRegisters(result, measProcIndex, singleMeasIndex,  ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task MakeStandartisationAsync(int measProcIndex, int standIndex, string ip, byte unitId = 1,
        int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return MakeStandartisationAsync(measProcIndex, standIndex, unitId);
    }


    /// <summary>
    /// Команда провести стандартизацию
    /// </summary>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="standIndex">Индекс стандартизации</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task MakeStandartisationAsync(int measProcIndex, int standIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetMakeStandartisationRegisters( measProcIndex, standIndex,  ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task MakeSingleMeasureAsync(int measProcIndex, int singleMeasureIndex, string ip, byte unitId = 1,
        int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return MakeSingleMeasureAsync(measProcIndex, singleMeasureIndex, unitId);
    }

    /// <summary>
    /// Провести единичное измерение
    /// </summary>
    /// <param name="measProcIndex">Индекс изм проуесса</param>
    /// <param name="singleMeasureIndex">Индекс ед измерения</param>
    /// <param name="unitId">Адрес в сети Modbus</param>
    /// <returns></returns>
    public Task MakeSingleMeasureAsync(int measProcIndex, int singleMeasureIndex, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = MeasProcessExtensions.GetMakeSingleMeasureRegisters( measProcIndex, singleMeasureIndex,  ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }

    public Task SetRtcAsync(DateTime time, string ip, byte unitId = 1, int portNum = 502)
    {
        SetEthenetSettings(ip, portNum);
        return SetRtcAsync(time, unitId);
    }

    
    /// <summary>
    /// Установить RTC прибора
    /// </summary>
    /// <param name="time"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public Task SetRtcAsync(DateTime time, byte unitId = 1)
    {
        ushort startIndex = 0;
        var buffer = RtcExtensions.GetRegisters(time, ref startIndex);
        return CommonWriteAsync(buffer, startIndex, (ushort)buffer.Length, unitId);
    }
    
    
    
    
}