using Idensity.Modbus.Services;
using Kinef.Core.Models.Main;
using Microsoft.Extensions.Logging;

namespace Kinef.Core.Services
{
    public class IdensityService
    {
        public Device Device { get; } = new Device();
        private IdensityModbusClient _client = new IdensityModbusClient();
        private readonly ILogger<IdensityService> _logger;

        public Task SwitchAdcDataSendAsync()
        {
            return _client.StartStopAdcDataAsync(!Device.AdcBoardSettings.AdcSendEnabled.Value, "127.0.0.1");
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
                    var indication = await _client.GetIndicationDataAsync("127.0.0.1");
                    Device.Temperature.Value = indication.TempBoardTelemetry.Temperature;
                    Device.Voltage.Value = indication.HvBoardTelemetry.OutputVoltage;
                    var settings = await _client.GetDeviceSettingsAsync("127.0.0.1");
                    Device.AdcBoardSettings.SyncLevel.Value = settings.AdcBoardSettings.SyncLevel;
                    Device.AdcBoardSettings.AdcSendEnabled.Value = settings.AdcBoardSettings.AdcDataSendEnabled;
                    Device.AdcBoardSettings.PreampGain.Value = settings.AdcBoardSettings.Gain;
                    Device.AdcBoardSettings.TimerAdc.Value = settings.AdcBoardSettings.TimerSendData;
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении данных с устройства");
                }
                
            }
        }


    }
}
