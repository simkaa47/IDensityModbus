using System.Diagnostics;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Idensity.Modbus.Models.Modbus;
using Idensity.Modbus.Models.Settings;
using Idensity.Modbus.Models.Settings.Analogs;
using Idensity.Modbus.Services;
using Kinef.Core.Models.Main;
using Microsoft.Extensions.Logging;
using AdcBoardSettings = Idensity.Modbus.Models.Settings.AdcSettings.AdcBoardSettings;

namespace Kinef.Core.Services
{
    public partial class IdensityService:ObservableObject
    {
        private DeviceSettings? _deviceSettings;
        [ObservableProperty]
        public string _indicationString;
        string ip = "127.0.0.1";
        public Device Device { get; } = new Device();
        private IdensityModbusClient _client = new IdensityModbusClient(ModbusType.Rtu, "COM4");
        private readonly ILogger<IdensityService> _logger;

        public Task SwitchAdcDataSendAsync()
        {
            return _client.StartStopAdcDataAsync(!Device.AdcBoardSettings.AdcSendEnabled.Value);
        }

        public IdensityService(ILogger<IdensityService> logger)
        {
            _ = AskDevicesAsync();
            _logger = logger;
        }

        private async Task AskDevicesAsync()
        {
            while (true)
            {
                try
                {
                    var timer = new Stopwatch();
                    timer.Start();
                    var indication = await _client.GetIndicationDataAsync(1);
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    
                    Device.Temperature.Value = indication.TempBoardTelemetry.Temperature;
                    Device.Voltage.Value = indication.HvBoardTelemetry.OutputVoltage;
                    var settings = await _client.GetDeviceSettingsAsync(1);
                    _deviceSettings = settings;
                    IndicationString = JsonSerializer.Serialize(settings, options);
                    
                    var elapsed = timer.ElapsedMilliseconds;
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении данных с устройства");
                }
                
            }
        }

        public async Task WriteSyncLevelAsync(ushort value)
        {
            try
            {
                await _client.WriteAdcBoardSyncLevelAsync(value, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task WriteTimerAdcAsync(ushort value)
        {
            try
            { 
                await _client.WriteAdcBoardTimerSendDataAsync(value, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task SetGainAsync(byte value)
        {
            try
            { 
                await _client.WriteAdcBoardGainAsync(value, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task SetUdpIpAsync(byte[] addr)
        {
            try
            { 
                await _client.WriteAdcBoardUpdAddressAsync(addr, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public async Task SetUdpPortAsync(ushort port)
        {
            try
            { 
                await _client.WriteAdcBoardUpdPortAsync(port, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public async Task SetHvAsync(ushort hv)
        {
            try
            { 
                await _client.WriteAdcBoardHvAsync(hv, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public async Task SetCounter(CounterSettings counter)
        {
            try
            {
                await _client.WriteCounterAsync(counter, 2, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public async Task SetEthernetSettings(TcpSettings ethernetSettings)
        {
            try
            {
                await _client.WriteEthernetSettingsAsync(ethernetSettings, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task SetSerialSettings(SerialSettings serialSettings)
        {
            try
            {
                await _client.WriteSerialSettingsAsync(serialSettings, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task SwitchAnalogInput(byte index)
        {
            var value = _deviceSettings?.AnalogInputActivities[index] ?? false;
            try
            {
                await _client.SetAnalogInputActivityAsync(index, !value, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public async Task SwitchAnalogOutput(byte index)
        {
            var value = _deviceSettings?.AnalogOutputSettings[index].IsActive ?? false;
            try
            {
                await _client.SetAnalogOutputActivityAsync(index, !value, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public async Task SetAnalogSettings(AnalogOutputSettings settings)
        {
            try
            {
                await _client.WriteAnalogOutputSettingsAsync(settings, 1, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public async Task WriteTestValue()
        {
            try
            {
                await _client.SendAnalogTestValueAsync(0, 12.87f,1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task SwitchCycleMeas(bool value)
        {
            try
            {
                await _client.SwitchCycleMeasures(value, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


    }
}
