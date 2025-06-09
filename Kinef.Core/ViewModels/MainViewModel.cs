using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kinef.Core.Services;
using Microsoft.Extensions.Logging;

namespace Kinef.Core.ViewModels;

public partial class MainViewModel(
    IdensityService deviceService,
    ILogger<MainViewModel> logger) : ObservableObject
{
    public IdensityService DeviceService { get; } = deviceService;

    [RelayCommand]
    private async Task StartStopAdcDataAsync()
    {
        try
        {
            await DeviceService.SwitchAdcDataSendAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }

    [RelayCommand]
    private async Task SendTimer()
    {
        await DeviceService.WriteTimerAdcAsync(1234);
    }
}