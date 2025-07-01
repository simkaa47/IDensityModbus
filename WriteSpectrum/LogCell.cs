using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WriteSpectrum;

public class LogCell
{
    public string? Time { get;  }
    public float CounterValue { get; }
    public float CounterBeginning{ get; }
    public float CounterWidth{ get;}
    public ushort[] Spectrum { get;}

    private LogCell(string time, float counterValue, float counterBeginning, float counterWidth, ushort[] spectrum)
    {
        Time = time;
        CounterValue = counterValue;
        CounterBeginning = counterBeginning;
        CounterWidth = counterWidth;
        Spectrum = spectrum;
    }
    


    public static LogCell? GetFromString(string str)
    {
        try
        {
            var paths = str.Split([';','\t'], StringSplitOptions.RemoveEmptyEntries);
            if (paths.Length == 4106)
            {
                float temp = 0;
                var spectrum = paths.Skip(10)
                    .Take(4096)
                    .Where(s => float.TryParse(s, out temp))
                    .Select(s=> (ushort)temp)
                    .ToArray();

                if (spectrum.Length == 4096)
                    return new LogCell(paths[0], 0, 0, 0, spectrum);
            }
            return null;
        }
        catch (Exception e)
        {
            return null;
        }
        
    }
}