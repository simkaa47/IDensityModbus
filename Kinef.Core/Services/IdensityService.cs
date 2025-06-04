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
