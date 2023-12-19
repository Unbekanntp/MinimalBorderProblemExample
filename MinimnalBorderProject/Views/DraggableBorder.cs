using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace Projektinformationen.Utils.Panels;

public class DraggableBorder : Border
{
    private bool _isPressed;
    private bool _moved;
    private Point _relativePositionInBlock;

    private Point? _initialBorderPosition;
    private double _deltaX;
    private double _deltaY;

    private TranslateTransform? _currentTt;
    
    public ICommand? Command
    {
        get { return GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }
    
    public object CommandParameter
    {
        get { return GetValue(CommandParameterProperty); }
        set { SetValue(CommandParameterProperty, value); }
    }

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<DraggableBorder, ICommand?>(nameof(Command), inherits: true);

    public static readonly StyledProperty<object> CommandParameterProperty =
        AvaloniaProperty.Register<DraggableBorder, object>(nameof(CommandParameter), inherits: true);
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _isPressed = true;

        _initialBorderPosition ??= this.TranslatePoint(new Point(0, 0), GetParentWindow());
        

        var mousePosition = e.GetPosition(GetParentWindow());
        _deltaX = mousePosition.X - _initialBorderPosition?.X ?? 0;
        _deltaY = mousePosition.Y - _initialBorderPosition?.Y ?? 0;
        
        base.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _isPressed = false;

        if (_moved)
        {
            _moved = false;
        }
        else
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }

        _currentTt = RenderTransform as TranslateTransform;

        base.OnPointerReleased(e);
    }


    

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isPressed || Parent == null)
            return;

        _moved = true;

        var parentWindowBounds = GetParentWindow()?.Bounds;
        
        var parentSize = parentWindowBounds?.Size ?? new Size(0, 0);


        var mousePoint = e.GetPosition(GetParentWindow());

        var absolutePosX = (_currentTt == null 
            ? _initialBorderPosition.Value.X
            : _initialBorderPosition.Value.X - _currentTt.X);
        var absolutePosY = (_currentTt == null
            ? _initialBorderPosition.Value.Y
            : _initialBorderPosition.Value.Y - _currentTt.Y);

        var offsetX = absolutePosX +
            _deltaX - mousePoint.X;
        var offsetY = absolutePosY +
            _deltaY - mousePoint.Y;

        if ((_initialBorderPosition.Value.X) - offsetX < 0)
            offsetX += (_initialBorderPosition.Value.X) - offsetX;
        if ((_initialBorderPosition.Value.Y) - offsetY < 0)
            offsetY += (_initialBorderPosition.Value.Y) - offsetY;

        var parentWindowWidth = parentWindowBounds?.Width ?? 0;
        var parentWindowHeight = parentWindowBounds?.Height ?? 0;
        
        if ((_initialBorderPosition.Value.X + Width) - offsetX > parentWindowWidth)
            offsetX += (_initialBorderPosition.Value.X + Width) - parentWindowWidth - offsetX;
        if ((_initialBorderPosition.Value.Y + Height) - offsetY > parentWindowHeight)
            offsetY += (_initialBorderPosition.Value.Y + Height) - parentWindowHeight - offsetY;


        _relativePositionInBlock = new Point(
            -offsetX / parentSize.Width,
            -offsetY / parentSize.Height
        );

        var transformer = new TranslateTransform(-offsetX, -offsetY);
        
        RenderTransform = transformer;
    }


    private Visual? GetParentWindow()
    {
        return (Visual?) VisualRoot;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Parent != null)
        {
            (Parent as Control).SizeChanged += OnParentResize;
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if(Parent != null)
            (Parent as Control).SizeChanged -= OnParentResize;
    }

    private void OnParentResize(object sender, EventArgs e)
    {
        var parentSize = GetParentWindow()?.Bounds.Size ?? new Size(0, 0);
        _currentTt = new TranslateTransform(_relativePositionInBlock.X * parentSize.Width,
            _relativePositionInBlock.Y * parentSize.Height);
        RenderTransform = _currentTt;
    }
}