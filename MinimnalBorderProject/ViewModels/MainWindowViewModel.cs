using System.Reactive;
using ReactiveUI;

namespace MinimnalBorderProject.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private bool _borderOpen;
    
    private double _borderMinHeight = 110;
    private double _borderMinWidth = 107;
    
    private double _borderMaxHeight = 541;
    private double _borderMaxWidth = 551;
    
    private double _borderHeight = 110;
    private double _borderWidth = 107;
    
    public ReactiveCommand<Unit, Unit> ChangeBorderSizeCommand { get; init; }

    public MainWindowViewModel()
    {
        ChangeBorderSizeCommand = ReactiveCommand.Create(() =>
        {
            _borderOpen = !_borderOpen;
            BorderHeight = _borderOpen ? _borderMaxHeight : _borderMinHeight;
            BorderWidth = _borderOpen ? _borderMaxWidth : _borderMinWidth;
        });
    }
    
    public double BorderWidth
    {
        get => _borderWidth;
        set => this.RaiseAndSetIfChanged(ref _borderWidth, value);
    }

    public double BorderHeight
    {
        get => _borderHeight;
        set => this.RaiseAndSetIfChanged(ref _borderHeight, value);
    }

}