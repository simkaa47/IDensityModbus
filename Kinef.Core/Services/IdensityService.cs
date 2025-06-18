using System.Diagnostics;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Idensity.Modbus.Models.Modbus;
using Idensity.Modbus.Services;
using Kinef.Core.Models.Main;
using Microsoft.Extensions.Logging;
using AdcBoardSettings = Idensity.Modbus.Models.Settings.AdcSettings.AdcBoardSettings;

namespace Kinef.Core.Services
{
    public partial class IdensityService:ObservableObject
    {
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
                    var indication = await _client.GetIndicationDataAsync(ip);
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    
                    Device.Temperature.Value = indication.TempBoardTelemetry.Temperature;
                    Device.Voltage.Value = indication.HvBoardTelemetry.OutputVoltage;
                    var settings = await _client.GetDeviceSettingsAsync(ip);
                    IndicationString = JsonSerializer.Serialize(settings, options);
                    Device.AdcBoardSettings.SyncLevel.Value = settings.AdcBoardSettings.SyncLevel;
                    Device.AdcBoardSettings.AdcSendEnabled.Value = settings.AdcBoardSettings.AdcDataSendEnabled;
                    Device.AdcBoardSettings.PreampGain.Value = settings.AdcBoardSettings.Gain;
                    Device.AdcBoardSettings.TimerAdc.Value = settings.AdcBoardSettings.TimerSendData;
                    var elapsed = timer.ElapsedMilliseconds;
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении данных с устройства");
                }
                
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


    }
}
