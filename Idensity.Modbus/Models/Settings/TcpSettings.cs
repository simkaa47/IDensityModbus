namespace Idensity.Modbus.Models.Settings;

public class TcpSettings
{
    /// <summary>
    /// Ip адрес устройства в сети Ethernet.
    /// </summary>
    public byte[] Address { get; } = new byte[4];
    /// <summary>
    /// Маска подсети устройства в сети Ethernet.
    /// </summary>
    public byte[] Mask { get; } = new byte[4];
    /// <summary>
    /// Адрес шлюза устройства в сети Ethernet.
    /// </summary>
    public byte[] Gateway { get; } = new byte[4];
    /// <summary>
    /// Адрес устройства в сети Ethernet.
    /// </summary>
    public byte[] MacAddress { get; } = new byte[6];

}
