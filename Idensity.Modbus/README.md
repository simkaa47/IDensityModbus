# IDensity.Modbus

[![NuGet Version](https://img.shields.io/nuget/v/IDensity.Modbus?style=flat-square)](https://www.nuget.org/packages/IDensity.Modbus/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

.NET библиотека, используемая для коммуникации с плотномером IDensity через протокол Modbus. Каждый метод имеет свою перегрузку для
TCP (Ethernet интерфейс) и RTU (интерфейс RS485)

## Установка пакета

Вы можете установить пакет `IDensity.Modbus` используя NuGet Package Manager.

**Используя .NET CLI:**

```bash
dotnet add package IDensity.Modbus
```

**Или добавить ссылку на пакет в .csproj file:**

```bash
<PackageReference Include="IDensity.Modbus" Version="0.0.4.2" />
```

## Создание клиента
**Modbus RTU**
```csharp
private IdensityModbusClient _client = new IdensityModbusClient(ModbusType.Rtu, "COM4");
```
**Modbus TCP**
```csharp
private IdensityModbusClient _client = new IdensityModbusClient(ModbusType.Tcp);
```

## Отлавливание исключений
Библиотека не отлавливает исключения, необходимо помещать вызываемые методы в try-catch

**Пример**
```csharp
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
```
## Чтение данных индикации
**Modbus RTU**
```csharp
var indication = await _client.GetIndicationDataAsync(1);
```
1 - адрес в сети RS485

**Modbus TCP**
```csharp
var indication = await _client.GetIndicationDataAsync("192.168.1.192");
```
"192.168.1.192" - IP адрес клиента

## Чтение данных настроек
**Modbus RTU**
```csharp
var settings = await _client.GetDeviceSettingsAsync(1);
```
**Modbus TCP**
```csharp
var settings = await _client.GetDeviceSettingsAsync("192.168.1.192");
```
### Кол-во измерительных процессов
```csharp
MeasProcessExtensions.MeasProcCnt = 4;
```
## Запись в плотномер

### Очистка спектра
**Modbus RTU**
```csharp
/// <summary>
/// Очистить спектр
/// </summary>
/// <param name="unitId">Адрес в сети Modbus</param>
public Task ClearSpectrumAsync(byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task ClearSpectrumAsync(string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Включить плату АЦП
**Modbus RTU**
```csharp
/// <summary>
/// Команда "Запуск-останов платы АЦП"
/// </summary>
/// <param name="value">0 - Остановить, 1 -запустить</param>
/// <param name="unitId">Адрес в сети Modbus</param>
public Task SwitchAdcBoardAsync(bool value, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task SwitchAdcBoardAsync(bool value, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Запуск/останов выдачи данных АЦП
**Modbus RTU**
```csharp
/// <summary>
/// Команда "Запуск/останов выдачи данных АЦП "
/// </summary>
/// <param name="value">0 - Остановить, 1 -запустить</param>
/// <param name="unitId">Адрес в сети Modbus</param>
public Task StartStopAdcDataAsync(bool value, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task StartStopAdcDataAsync(bool value, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Вкл-выкл HV
**Modbus RTU**
```csharp
 /// <summary>
 /// Команду управления HV
 /// </summary>
 /// <param name="value">0 - Выкл, 1 - Вкл</param>
 /// <param name="unitId">Адрес в сети Modbus</param> 
 public Task SwitcHvAsync(bool value, byte unitId = 1)
 {
    ...
 }
```
**Modbus TCP**
```csharp
public Task SwitcHvAsync(bool value, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Записать настройки счетчика в устройство
**Modbus RTU**
```csharp
 /// <summary> 
 /// Записать настройки счетчика в устройство
 /// </summary>
 /// <param name="counterSettings">Данные диапазона счетчика</param>
 /// <param name="counterNum">Номер диапазона</param>
 /// <param name="unitId">Адрес в сети Modbus</param>
 /// <returns></returns>
 public Task WriteCounterAsync(CounterSettings counterSettings, int counterNum,
     byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteCounterAsync(CounterSettings counterSettings, int counterNum,
    string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
**Структура CounterSettings**
```csharp
public class CounterSettings
{
    /// <summary>
    /// Режим работы
    /// </summary>
    public CounterMode Mode { get; set; }
    /// <summary>
    /// Старт диапазона счетчика.
    /// </summary>
    public ushort Start { get; set; }
    /// <summary>
    /// Ширина диапазона
    /// </summary>
    public ushort Width { get; set; }

}
```
### Изменить адрес в сети Modbus
**Modbus RTU**
```csharp
 /// <summary>
 /// Поменять адрес в сети Modbus
 /// </summary>
 /// <param name="modbusNum">Новый адрес</param>
 /// <param name="unitId">Адрес в сети Modbus</param>
 /// <returns></returns>
 public Task WriteModbusNumberAsync(byte modbusNum, byte unitId = 1)
 {
    ...
 }
```
**Modbus TCP**
```csharp
public Task WriteModbusNumberAsync(byte modbusNum, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Записать настройки Ethernet соединения в устройство
**Modbus RTU**
```csharp
 /// <summary>
 /// Записать настройки Ethernet соединения в устройство
 /// </summary>
 /// <param name="settings">Настройки</param>
 /// <param name="unitId">Адрес в сети Modbus</param>
 /// <returns></returns>
 public Task WriteEthernetSettingsAsync(TcpSettings settings, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteEthernetSettingsAsync(TcpSettings settings, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
**Структура TcpSettings**
```csharp
public class TcpSettings
{
    /// <summary>
    /// Ip адрес устройства в сети Ethernet.
    /// </summary>
    public byte[] Address { get; set; } = new byte[4];
    /// <summary>
    /// Маска подсети устройства в сети Ethernet.
    /// </summary>
    public byte[] Mask { get; set; } = new byte[4];
    /// <summary>
    /// Адрес шлюза устройства в сети Ethernet.
    /// </summary>
    public byte[] Gateway { get; set; } = new byte[4];
    /// <summary>
    /// Адрес устройства в сети Ethernet.
    /// </summary>
    public byte[] MacAddress { get; set; } = new byte[6];

}
```
### Записать настройки последовательного соединения в устройство
**Modbus RTU**
```csharp
 /// <summary>
 /// Записать настройки последовательного соединения в устройство
 /// </summary>
 /// <param name="settings">Настройки</param>
 /// <param name="unitId">Адрес в сети Modbus</param>
 /// <returns></returns> 
 public Task WriteSerialSettingsAsync(SerialSettings settings, byte unitId = 1)
 {
    ...
 }
```
**Modbus TCP**
```csharp
public Task WriteSerialSettingsAsync(SerialSettings settings, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
**Структура SerialSettings**
```csharp
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
```
### Изменить режим работы платы АЦП
**Modbus RTU**
```csharp
 /// <summary>
 /// Изменить режим работы платы АЦП
 /// </summary>
 /// <param name="mode"></param>
 /// <param name="unitId"></param>
 /// <returns></returns>
 public Task WriteAdcBoardModeAsync(AdcBoardMode mode, byte unitId = 1)
 {
    ...
 }
```
**Modbus TCP**
```csharp
public Task WriteAdcBoardModeAsync(AdcBoardMode mode, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
**Варианты AdcBoardMode**
```csharp
AdcBoardMode
{
    Oscilloscope,
    Spectrum
}
```

### Записать уровень синхронизации платы АЦП в устройство
**Modbus RTU**
```csharp
/// <summary>
/// Записать уровень синхронизации платы АЦП в устройство
/// </summary>
/// <param name="syncLevel"></param>
/// <param name="unitId"></param>
/// <returns></returns>
public Task WriteAdcBoardSyncLevelAsync(ushort syncLevel, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteAdcBoardSyncLevelAsync(ushort syncLevel, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Записать таймер выдачи данных платы АЦП в устройство
**Modbus RTU**
```csharp
/// <summary>
/// Записать таймер выдачи данных платы АЦП в устройство
/// </summary>
/// <param name="timerValue"></param>
/// <param name="unitId"></param>
/// <returns></returns>
public Task WriteAdcBoardTimerSendDataAsync(ushort timerValue, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteAdcBoardTimerSendDataAsync(ushort timerValue, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Записать к-т предусиления платы АЦП в устройство
**Modbus RTU**
```csharp
/// <summary>
/// Записать к-т предусиления платы АЦП в устройство
/// </summary>
/// <param name="gain"></param>
/// <param name="unitId"></param>
/// <returns></returns>
public Task WriteAdcBoardGainAsync(byte gain, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteAdcBoardGainAsync(byte gain, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Записать ip адрес получателя спектра
**Modbus RTU**
```csharp
/// <summary>
/// Записать ip адрес получателя спектра
/// </summary>
/// <param name="addr"></param>
/// <param name="unitId"></param>
/// <returns></returns>
public Task WriteAdcBoardUpdAddressAsync(byte[] addr, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteAdcBoardUpdAddressAsync(byte[] addr, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Записать номер порта получателя спектра
**Modbus RTU**
```csharp
/// <summary>
/// Записать номер порта получателя спектра
/// </summary>
/// <param name="port"></param>
/// <param name="unitId"></param>
/// <returns></returns>
public Task WriteAdcBoardUpdPortAsync(ushort port, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteAdcBoardUpdPortAsync(ushort port, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Установить уставку высокого напряжения
**Modbus RTU**
```csharp
/// <summary>
/// Установить уставку высокого напряжения
/// </summary>
/// <param name="hv">Уставка напряжения, вольт</param>
/// <param name="unitId"></param>
/// <returns></returns>
public Task WriteAdcBoardHvAsync(ushort hv, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteAdcBoardHvAsync(ushort hv, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Установить координату пика спектра
**Modbus RTU**
```csharp
/// <summary>
/// Установить координату пика спектра
/// </summary>
/// <param name="peak">Координата пика спектра</param>
/// <param name="unitId"></param>
/// <returns></returns>
public Task WriteAdcBoarPeakSpectrumAsync(ushort peak, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteAdcBoarPeakSpectrumAsync(ushort peak, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Установить активность аналогового входа устройства
**Modbus RTU**
```csharp
/// <summary>
/// Установить активность аналогового входа устройства
/// </summary>
/// <param name="inputNumber">Номер AI, 0..1</param>
/// <param name="value">Значение активности</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task SetAnalogInputActivityAsync(byte inputNumber, bool value, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task SetAnalogInputActivityAsync(byte inputNumber, bool value, string ip,
        byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Записать настройки аналогового выхода в устройство
**Modbus RTU**
```csharp
/// <summary>
/// Записать настройки аналогового выхода в устройство
/// </summary>
/// <param name="settings">Данные аналогового выхода</param>
/// <param name="outputNumber">Номер выхода (0,1)</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteAnalogOutputSettingsAsync(AnalogOutputSettings settings, byte outputNumber,
    byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteAnalogOutputSettingsAsync(AnalogOutputSettings settings, byte outputNumber,
    string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
**Структура AnalogOutputSettings**
```csharp
public class AnalogOutputSettings
{
    /// <summary>
    /// Активность выхода
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Режим работы аналогового выхода.
    /// </summary>
    public AnalogOutputMode Mode { get; set; }
    /// <summary>
    /// Номер изм процесса
    /// </summary>
    public byte MeasProcessNum { get; set; }
    /// <summary>
    /// Что выводить на выход
    /// </summary>
    public AnalogOutMeasType AnalogOutMeasType { get; set; }
    /// <summary>
    /// Нижний предел переменной
    /// </summary>
    public float MinValue { get; set; }
    /// <summary>
    /// Верхний предел переменной
    /// </summary>
    public float MaxValue { get; set; }
    /// <summary>
    /// Минимальное значение тока, мА
    /// </summary>
    public float MinCurrent { get; set; }

    /// <summary>
    /// Максимальное значение тока, мА
    /// </summary>
    public float MaxCurrent { get; set; }
    
    /// <summary>
    /// Тестовое значение, mA
    /// </summary>
    public float TestValue { get; set; }


}
```
### Команда управления питанием аналогового выхода
**Modbus RTU**
```csharp
/// <summary>
/// Команда управления питанием аналогового выхода
/// </summary>
/// <param name="outputNumber">Номер выхода</param>
/// <param name="value"></param>
/// <param name="unitId"></param>
/// <returns></returns>
public Task SetAnalogOutputActivityAsync(byte outputNumber, bool value, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task SetAnalogOutputActivityAsync(byte outputNumber, bool value, string ip, byte unitId = 1,
    int portNum = 502)
{
    ...
}
```
### Отправить тестовое значение аналогового выхода
**Modbus RTU**
```csharp
/// <summary>
/// Отправить тестовое значение аналогового выхода
/// </summary>
/// <param name="outNum">Номер выхода (0,1)</param>
/// <param name="testValue">Ток, в mA</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task SendAnalogTestValueAsync(byte outNum, float testValue, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task SendAnalogTestValueAsync(byte outNum, float testValue, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Отправить тестовое значение аналогового выхода
**Modbus RTU**
```csharp
public Task WriteDeviceTypeAsync(DeviceType type, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteDeviceTypeAsync(DeviceType type, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
**Варианты DeviceType**
```csharp
public enum DeviceType
{
    Density,
    Level
}
```
### Записать настройки компенсации температуры
**Modbus RTU**
```csharp
public Task WriteTempCompensationSettingsAsync(GetTemperature settings, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteTempCompensationSettingsAsync(GetTemperature settings, string ip, byte unitId = 1,
        int portNum = 502)
{
    ...
}
```
**Структура GetTemperature**
```csharp
public class GetTemperature
{
    /// <summary>
    /// Источник получения температуры.
    /// </summary>
    public GetTemperatureSrc Src { get; internal set; }
    public GetTemperatureCoeffs[] GetTemperatureCoeffs { get; } = 
        Enumerable.Range(0, 2).Select(i => new GetTemperatureCoeffs()).ToArray()  ;

}
```
**Структура GetTemperatureCoeffs**
```csharp
public class GetTemperatureCoeffs
{
    /// <summary>
    /// Коэффициент A
    /// </summary>
    public float A { get; internal set; } = 0f;
    /// <summary>
    /// Коэффициент B
    /// </summary>
    public float B { get; internal set; } = 0f;
}
```
### Записать длину уровнемера, мм
**Modbus RTU**
```csharp
public Task WriteLevelLengthAsync(float levelLength, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteLevelLengthAsync(float levelLength, string ip, byte unitId = 1,
        int portNum = 502)
{
    ...
}
```
### Включить-выключить циклические измерения
**Modbus RTU**
```csharp
/// <summary>
/// Включить-выключить циклические измерения
/// </summary>
/// <param name="value">false  - выключить, true - включить</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task SwitchCycleMeasures(bool value, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task SwitchCycleMeasures(bool value, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Записать длительность одного измерения, с
**Modbus RTU**
```csharp
/// <summary>
/// Записать длительность одного измерения, с
/// </summary>
/// <param name="duration">длительность одного измерения, с</param>
/// <param name="measProcIndex">Индекс изм процесса, 0-7</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task SetMeasProcDuration(float duration, int measProcIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task SetMeasProcDuration(float duration, int measProcIndex, byte unitId = 1)
{
    ...
}
```
### Записать кол-во точек усреднения
**Modbus RTU**
```csharp
/// <summary>
/// Записать кол-во точек усреднения
/// </summary>
/// <param name="deep">Кол-во точек усреднения, 0-99</param>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteMeasProcDeep(byte deep, int measProcIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteMeasProcDeep(byte deep, int measProcIndex, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Записать диаметр трубы, мм
**Modbus RTU**
```csharp
/// <summary>
/// Записать диаметр трубы, мм
/// </summary>
/// <param name="diameter">диаметр трубы, мм</param>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteMeasProcPipeDiameter(ushort diameter, int measProcIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteMeasProcPipeDiameter(ushort diameter, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
{
    ...
}
```
### Установить активность изм процесса
**Modbus RTU**
```csharp
/// <summary>
/// Установить активность изм процесса
/// </summary>
/// <param name="activity">Активность</param>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteMeasProcActivity(bool activity, int measProcIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteMeasProcActivity(bool activity, int measProcIndex, string ip, 
    byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Записать тип расчета измерительного процесса
**Modbus RTU**
```csharp
/// <summary>
/// Записать тип расчета измерительного процесса
/// </summary> 
/// <param name="type">Тип расчета измерительного процесса</param>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteMeasProcCalcType(CalculationType type, int measProcIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteMeasProcCalcType(CalculationType type, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
{
    ...
}
```
**CalculationType**
```csharp
public enum CalculationType
{
    Polynom = default,
    Attenuation,
    None
}
```
### Записать тип измерения измерительного процесса
**Modbus RTU**
```csharp
/// <summary>
/// Записать тип измерения измерительного процесса
/// </summary>
/// <param name="measType">Тип измерения</param>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteMeasProcMeasType(ushort measType, int measProcIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteMeasProcMeasType(ushort measType, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
{
    ...
}
```
### Записать плотность жидкого в изм процесс
**Modbus RTU**
```csharp
/// <summary>
/// Записать плотность жидкого в изм процесс
/// </summary>
/// <param name="density">Плотность жидкого</param>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteMeasProcDensityLiquid(float density, int measProcIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteMeasProcDensityLiquid(float density, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
{
    ...
}
```
### Записать плотность твердого в изм процесс
**Modbus RTU**
```csharp
/// <summary>
/// Записать плотность твердого в изм процесс
/// </summary>
/// <param name="density">Плотность твердого</param>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteMeasProcDensitySolid(float density, int measProcIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteMeasProcDensitySolid(float density, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
{
    ...
}
```
### Записать настройки быстрых измерений в изм процесс
**Modbus RTU**
```csharp
/// <summary>
/// Записать настройки быстрых измерений в изм процесс
/// </summary>
/// <param name="fastChange">настройки быстрых измерений</param>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteMeasProcFastChange(FastChange fastChange, int measProcIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteMeasProcFastChange(FastChange fastChange, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
{
    ...
}
```
**FastChange**
```csharp
public class FastChange
{
    /// <summary>
    /// Активность быстрой смены.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Пороговое значение для быстрой смены.
    /// </summary>
    public ushort Threshold { get; set; }
}
```
### Записать длительность единичного измерения, с
**Modbus RTU**
```csharp
/// <summary>
/// Записать длительность единичного измерения, с
/// </summary>
/// <param name="duration">Длительность единичного измерения, с</param>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteMeasProcSingleMeasDuration(ushort duration, int measProcIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteMeasProcSingleMeasDuration(ushort duration, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
{
    ...
}
```
### Записать данные стандартизации в измерительный процесс
**Modbus RTU**
```csharp
/// <summary>
/// Записать данные стандартизации в измерительный процесс
/// </summary>
/// <param name="settings">данные стандартизации</param>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="standIndex">Индекс стандартизации</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteMeasProcStandartisationData(StandSettings settings, int measProcIndex, int standIndex,
    byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteMeasProcStandartisationData(StandSettings settings, int measProcIndex, int standIndex,
        string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
**StandSettings**
```csharp
public class StandSettings
{
    /// <summary>
    /// Длительность стандартизации, с
    /// </summary>
    public ushort StandDuration { get; set; }
    /// <summary>
    /// Дата последней стандартизации
    /// </summary>
    public DateOnly LastStandDate { get; set; }
    /// <summary>
    /// Результат стандартизации
    /// </summary>
    public float Result { get; set; }
    /// <summary>
    /// Результат стандартизации с учетом полураспада
    /// </summary>
    public float HalfLifeResult { get; set; }

}
```
### Записать данные калибровочной кривой
**Modbus RTU**
```csharp
/// <summary>
/// Записать данные калибровочной кривой
/// </summary>
/// <param name="curve">данные калибровочной кривой</param>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteMeasProcCalibrCurve(CalibrCurve curve, int measProcIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteMeasProcCalibrCurve(CalibrCurve curve, int measProcIndex, string ip, byte unitId = 1,
        int portNum = 502)
{
    ...
}
```
**CalibrCurve**
```csharp
public class CalibrCurve
{
    public CalibrationType Type { get; set; }
    public List<float> Coefficients { get; } = Enumerable.Range(0, 6).Select(i => 0f).ToList();
}
```
**CalibrationType**
```csharp
public enum CalibrationType
{
    Density,
    Concentration1,
    Concentration2,
    None
}
```
### Записать данные единичного измерения
**Modbus RTU**
```csharp
/// <summary>
/// Записать данные единичного измерения
/// </summary>
/// <param name="result">Данные единичного измерения</param>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="singleMeasIndex">Индекс ед измерения</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task WriteMeasProcSingleMeasResult(SingleMeasResult result, int measProcIndex, int singleMeasIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task WriteMeasProcSingleMeasResult(SingleMeasResult result, int measProcIndex, int singleMeasIndex,
        string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
**SingleMeasResult**
```csharp
public class SingleMeasResult
{
    /// <summary>
    /// Дата измерения
    /// </summary>
    public DateOnly Date { get; set; }
    /// <summary>
    /// Ослабление
    /// </summary>
    public float Weak { get; set; }
    /// <summary>
    /// Значение счетчика
    /// </summary>
    public float PhysValue { get; set; }
}
```
### Команда провести стандартизацию
**Modbus RTU**
```csharp
/// <summary>
/// Команда провести стандартизацию
/// </summary>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="standIndex">Индекс стандартизации</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task MakeStandartisationAsync(int measProcIndex, int standIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task MakeStandartisationAsync(int measProcIndex, int standIndex, string ip, byte unitId = 1,
        int portNum = 502)
{
    ...
}
```
### Провести единичное измерение
**Modbus RTU**
```csharp
/// <summary>
/// Провести единичное измерение
/// </summary>
/// <param name="measProcIndex">Индекс изм проуесса</param>
/// <param name="singleMeasureIndex">Индекс ед измерения</param>
/// <param name="unitId">Адрес в сети Modbus</param>
/// <returns></returns>
public Task MakeSingleMeasureAsync(int measProcIndex, int singleMeasureIndex, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task MakeSingleMeasureAsync(int measProcIndex, int singleMeasureIndex, string ip, byte unitId = 1,
        int portNum = 502)
{
    ...
}
```
### Установить RTC прибора
**Modbus RTU**
```csharp
/// <summary>
/// Установить RTC прибора
/// </summary>
/// <param name="time"></param>
/// <param name="unitId"></param>
/// <returns></returns>
public Task SetRtcAsync(DateTime time, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task SetRtcAsync(DateTime time, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```
### Записать серийный номер прибора
**Modbus RTU**
```csharp
/// <summary>
/// Записать серийный номер прибора
/// </summary>
/// <param name="name"></param>
/// <param name="unitId"></param>
/// <returns></returns>
public Task SetDeviceNameAsync(string name, byte unitId = 1)
{
    ...
}
```
**Modbus TCP**
```csharp
public Task SetDeviceNameAsync(string name, string ip, byte unitId = 1, int portNum = 502)
{
    ...
}
```

