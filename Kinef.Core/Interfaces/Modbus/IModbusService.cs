namespace Kinef.Core.Interfaces.Modbus;

public interface IModbusService
{
    Task<bool> WriteCounterSettings();
}