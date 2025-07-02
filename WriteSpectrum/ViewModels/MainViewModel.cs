using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Idensity.Modbus.Models.Modbus;
using Idensity.Modbus.Services;

namespace WriteSpectrum.ViewModels;

public partial class MainViewModel:ObservableObject
{
    private IdensityModbusClient _client = new IdensityModbusClient(ModbusType.Tcp);
    
    
    [ObservableProperty]
    private List<LogCell> _logs = new();
    [ObservableProperty]
    private string? _errStatus;
    
    [ObservableProperty]
    private byte _id = 1;
    
    [ObservableProperty]
    private string? _path;
    
    [ObservableProperty]
    private LogCell? _selectedLog;

    [ObservableProperty]
    private byte _ip0 = 192;
    
    [ObservableProperty]
    private byte _ip1 = 168;
    
    [ObservableProperty]
    private byte _ip2 = 1;
    [ObservableProperty]
    private byte _ip3 = 180;

    [RelayCommand]
    private async Task WriteSpectrumAsync()
    {
        try
        {
            if (SelectedLog is not  null)
            {
                ErrStatus = string.Empty;
                await _client.WriteSpectrumAsync(SelectedLog.Spectrum, $"{Ip0}.{Ip1}.{Ip2}.{Ip3}", Id);
            }
        }
        catch (Exception e)
        {
            ErrStatus = e.Message;
        }
    }
    
    
    
    

    [RelayCommand]
    private async Task GetFile(TopLevel topLevel)
    {
        if (topLevel == null)
        {
            return;
        }
        var storageProvider = topLevel.StorageProvider;

        // Проверяем, поддерживает ли текущая платформа открытие файлов
        if (!storageProvider.CanOpen)
        {
            Console.WriteLine("Поставщик хранилища не может открывать файлы на этой платформе.");
            // Можете показать сообщение пользователю через какой-нибудь сервис сообщений.
            return;
        }
        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите файл",
            AllowMultiple = false, // Разрешаем выбор только одного файла
            FileTypeFilter = new[]
            {
                FilePickerFileTypes.TextPlain, // Фильтр для текстовых файлов
                new FilePickerFileType("Изображения") // Пользовательский фильтр для изображений
                {
                    Patterns = new[] { "*.txt"},
                    MimeTypes = new[] { "image/txt", }
                },
                FilePickerFileTypes.All // Разрешить любые файлы
            }
        });
        
        if (files != null && files.Count > 0)
        {
            Path = files[0].TryGetLocalPath();
            await GetFromFile(Path);
        }
    }

    private async Task GetFromFile(string? path)
    {
        if(path is null)return;
        try
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string? line;
                var logs = new List<LogCell>();
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var logCell = LogCell.GetFromString(line);
                    if (logCell != null)
                        logs.Add(logCell);
                }
                Logs = logs;
                SelectedLog = Logs.FirstOrDefault();
            }
        }
        catch (Exception e)
        {
            _errStatus = e.Message;
        }
    }
}