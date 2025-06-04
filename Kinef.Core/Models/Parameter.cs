using CommunityToolkit.Mvvm.ComponentModel;

namespace Kinef.Core.Models;

public class Parameter<T>(string description, T minValue, T maxValue)
    :ObservableValidator where T:IComparable
{
    public T MinValue { get; } = minValue;
    public T MaxValue { get;  } = maxValue;
    public string Description { get; } = description;

    private T? _value;

    public T Value
    {
        get
        {
            Console.WriteLine($"Thread num {Thread.CurrentThread.ManagedThreadId} (get)");
            return _value;
        }
        set
        {
            Console.WriteLine($"Thread num {Thread.CurrentThread.ManagedThreadId} (set)");
            SetProperty(ref _value, value);
        }
    }
    
}