using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FAManagementStudio.Controls;

public class PinButton : Button
{
    static PinButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PinButton), new FrameworkPropertyMetadata(typeof(PinButton)));
    }

    private static readonly string PinImage = @"pack://application:,,,/FAManagementStudio.Controls;component/Images/PinItem.png";
    private static readonly string PinedImage = @"pack://application:,,,/FAManagementStudio.Controls;component/Images/PinnedItem.png";

    public static readonly DependencyProperty PinedProperty = DependencyProperty.Register(nameof(Pined), typeof(bool), typeof(PinButton), new PropertyMetadata(false, PinedPropertyChanged));

    public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(PinButton), new PropertyMetadata(PinImage));

    public static readonly DependencyProperty ReleasePinCommandProperty = DependencyProperty.Register(nameof(ReleasePinCommand), typeof(ICommand), typeof(PinButton), new PropertyMetadata(null));

    public static readonly DependencyProperty PinedCommandProperty = DependencyProperty.Register(nameof(PinedCommand), typeof(ICommand), typeof(PinButton), new PropertyMetadata(null));

    public string ImagePath
    {
        get => (string)GetValue(ImagePathProperty);
        set { SetValue(ImagePathProperty, value); }
    }

    private static void PinedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is PinButton button)
        {
            ICommand command;
            if (button.Pined)
            {
                command = button.PinedCommand;
                button.ImagePath = PinedImage;
            }
            else
            {
                command = button.ReleasePinCommand;
                button.ImagePath = PinImage;
            }
            command.Execute(button.DataContext);
        }
    }

    protected override void OnClick()
    {
        ICommand command;
        if (Pined)
        {
            command = ReleasePinCommand;
        }
        else
        {
            command = PinedCommand;
        }
        if (!command.CanExecute(DataContext)) return;
        Pined = !Pined;
        base.OnClick();
    }

    public bool Pined
    {
        get => (bool)GetValue(PinedProperty);
        set { SetValue(PinedProperty, value); }
    }

    public ICommand ReleasePinCommand
    {
        get => (ICommand)GetValue(ReleasePinCommandProperty);
        set { SetValue(ReleasePinCommandProperty, value); }
    }

    public ICommand PinedCommand
    {
        get => (ICommand)GetValue(PinedCommandProperty);
        set { SetValue(PinedCommandProperty, value); }
    }
}
