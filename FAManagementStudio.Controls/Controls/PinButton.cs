using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FAManagementStudio.Controls
{
    public class PinButton : Button
    {
        static PinButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PinButton), new FrameworkPropertyMetadata(typeof(PinButton)));
        }

        private static string PinImage = @"pack://application:,,,/FAManagementStudio.Controls;component/Images/PinItem.png";
        private static string PinedImage = @"pack://application:,,,/FAManagementStudio.Controls;component/Images/PinnedItem.png";

        public static readonly DependencyProperty PinedProperty = DependencyProperty.Register(nameof(Pined), typeof(bool), typeof(PinButton), new PropertyMetadata(false, PinedPropertyChanged));

        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(PinButton), new PropertyMetadata(PinImage));

        public static readonly DependencyProperty ReleasePinCommandProperty = DependencyProperty.Register(nameof(ReleasePinCommand), typeof(ICommand), typeof(PinButton), new PropertyMetadata(null));

        public static readonly DependencyProperty PinedCommandProperty = DependencyProperty.Register(nameof(PinedCommand), typeof(ICommand), typeof(PinButton), new PropertyMetadata(null));

        public string ImagePath
        {
            get { return (string)GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }

        private static void PinedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ICommand command;
            var aObj = obj as PinButton;
            if (aObj.Pined)
            {
                command = aObj.PinedCommand;
                aObj.ImagePath = PinedImage;
            }
            else
            {
                command = aObj.ReleasePinCommand;
                aObj.ImagePath = PinImage;
            }
            command.Execute(aObj.DataContext);
        }

        protected override void OnClick()
        {
            ICommand command;
            if (this.Pined)
            {
                command = ReleasePinCommand;
            }
            else
            {
                command = PinedCommand;
            }
            if (!command.CanExecute(this.DataContext)) return;
            Pined = !Pined;
            base.OnClick();
        }

        private bool ExecuteCommand(ICommand command, object param)
        {
            if (command == null) return false;
            if (!command.CanExecute(param)) return false;
            command.Execute(param);
            return true;
        }

        public bool Pined
        {
            get { return (bool)GetValue(PinedProperty); }
            set { SetValue(PinedProperty, value); }
        }

        public ICommand ReleasePinCommand
        {
            get { return (ICommand)GetValue(ReleasePinCommandProperty); }
            set { SetValue(ReleasePinCommandProperty, value); }
        }

        public ICommand PinedCommand
        {
            get { return (ICommand)GetValue(PinedCommandProperty); }
            set { SetValue(PinedCommandProperty, value); }
        }
    }
}
