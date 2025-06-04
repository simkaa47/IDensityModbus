using Kinef.Core.Services;

namespace Kinef.Core.ViewModels;

public class MainViewModel(IdensityService deviceService)
{
    public IdensityService DeviceService { get; } = deviceService;
}