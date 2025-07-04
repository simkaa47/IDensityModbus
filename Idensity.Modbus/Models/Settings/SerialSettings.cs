﻿namespace Idensity.Modbus.Models.Settings;

public class SerialSettings
{
    /// <summary>
    /// Скорость передачи данных по последовательному порту.
    /// </summary>
    public uint Baudrate { get; set; }
    /// <summary>
    /// Режим работы последовательного порта.
    /// </summary>
    public SerialPortMode Mode { get; set; }
}
