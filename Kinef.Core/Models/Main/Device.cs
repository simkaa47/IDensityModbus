namespace Kinef.Core.Models.Main;

public class Device
{
    public Parameter<float> Temperature { get; } = new Parameter<float>("Температура", -50, 150);
    public Parameter<float> Voltage { get; } = new Parameter<float>("Напряжение", 0, 100);
}