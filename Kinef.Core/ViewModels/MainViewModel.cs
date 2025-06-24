using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Idensity.Modbus.Models.Settings;
using Idensity.Modbus.Models.Settings.Analogs;
using Kinef.Core.Services;
using Microsoft.Extensions.Logging;

namespace Kinef.Core.ViewModels;

public partial class MainViewModel(
    IdensityService deviceService,
    ILogger<MainViewModel> logger) : ObservableObject
{
    public IdensityService DeviceService { get; } = deviceService;

    [RelayCommand]
    private async Task SetSYncLevel()
    {
        await DeviceService.WriteSyncLevelAsync(1166);
    }


    [RelayCommand]
    private async Task SendTimer()
    {
        await DeviceService.WriteTimerAdcAsync(1234);
    }

    [RelayCommand]
    public async Task SetGainAsync()
    {
        await DeviceService.SetGainAsync(13);
    }

    [RelayCommand]
    public async Task SetUdpIpAsync()
    {
        byte[] bytes = [192, 168, 1, 172];
        await DeviceService.SetUdpIpAsync(bytes);
    }

    [RelayCommand]
    public async Task SetUdpPortAsync()
    {
        await DeviceService.SetUdpPortAsync(7777);
    }

    [RelayCommand]
    public async Task SetHvAsync()
    {
        await DeviceService.SetHvAsync(907);
    }

    [RelayCommand]
    public async Task SetCounter()
    {
        var counter = new CounterSettings()
        {
            Start = 1134,
            Width = 333
        };
        await DeviceService.SetCounter(counter);
    }

    [RelayCommand]
    public async Task SetEthernetSettings()
    {
        TcpSettings ethernetSettings = new TcpSettings()
        {
            Address = [192, 168, 1, 183],
            Mask = [255, 255, 255, 0],
            Gateway = [192, 168, 1, 1],
            MacAddress = [12, 13, 14, 15, 16, 17]
        };
        await DeviceService.SetEthernetSettings(ethernetSettings);
    }

    [RelayCommand]
    public async Task SetSerialSettings()
    {
        var serialSettings = new SerialSettings()
        {
            Baudrate = 115200,
            Mode = SerialPortMode.Rs485
        };
        await DeviceService.SetSerialSettings(serialSettings);
    }

    [RelayCommand]
    public async Task SwitchAnalogInput1()
    {
        await DeviceService.SwitchAnalogInput(0);
    }
    
    [RelayCommand]
    public async Task SwitchAnalogInput2()
    {
        await DeviceService.SwitchAnalogInput(1);
    }
    
    [RelayCommand]
    public async Task SwitchAnalogoutput1()
    {
        await DeviceService.SwitchAnalogOutput(0);
    }
    
    [RelayCommand]
    public async Task SwitchAnalogoutput2()
    {
        await DeviceService.SwitchAnalogOutput(1);
    }
    [RelayCommand]
    public async Task SetAnalogSettings()
    {
        var settings = new AnalogOutputSettings()
        {
            Mode = AnalogOutputMode.MeasuredValue,
            AnalogOutMeasType = AnalogOutMeasType.AverageValue,
            MeasProcessNum = 3,
            TestValue = 12.4f,
            MinCurrent = 5,
            MaxCurrent = 12,
            MinValue = 111,
            MaxValue = 222
        };
        await DeviceService.SetAnalogSettings(settings);
    }
    [RelayCommand]
    public async Task WriteTestValue()
    {
        await DeviceService.WriteTestValue();
    }

    [RelayCommand]
    public async Task SwitchCycleMeasOn()
    {
        await DeviceService.SwitchCycleMeas(true);
    }
    
    [RelayCommand]
    public async Task SwitchCycleMeasOff()
    {
        await DeviceService.SwitchCycleMeas(false);
    }
    [RelayCommand]
    public async Task WriteMeasTime()
    {
        await DeviceService.WriteMeasTime();
    }
    [RelayCommand]
    public async Task WriteMeasDeep()
    {
        await DeviceService.WriteMeasDeep();
    }
    [RelayCommand]
    public async Task WritePipeDiameter()
    {
        await DeviceService.WritePipeDiameter();
    }
    [RelayCommand]
    public async Task WriteCalcType()
    {
        await DeviceService.WriteCalcType();
    }
    [RelayCommand]
    public async Task WriteMeasType()
    {
        await DeviceService.WriteMeasType();
    }

    [RelayCommand]
    public async Task WriteDensityLiq()
    {
        await DeviceService.WriteDensityLiq();
    }
    
    [RelayCommand]
    public async Task WriteDensitySol()
    {
        await DeviceService.WriteDensitySol();
    }
    [RelayCommand]
    public async Task WriteFastChanges()
    {
        await DeviceService.WriteFastChanges();
    }
    [RelayCommand]
    public async Task WriteSingleMeasDuration()
    {
        await DeviceService.WriteSingleMeasDuration();
    }
    [RelayCommand]
    public async Task WriteStandartisation()
    {
        await DeviceService.WriteStandartisation();
    }
    [RelayCommand]
    public async Task MakeStandartisation()
    {
        await DeviceService.MakeStandartisation();
    }

    [RelayCommand]
    public async Task SetRtcAsync()
    {
        await DeviceService.SetRtcAsync();
    }
}