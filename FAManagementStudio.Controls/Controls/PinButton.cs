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

        public static readonly DependencyProperty PinedProperty = DependencyProperty.Register(nameof(Pined), typeof(bool), typeof(PinButton), new PropertyMetadata(false, new PropertyChangedCallback(OnPinedChnaged)));

        private static void OnPinedChnaged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as PinButton;
            if (obj.Pined)
            {
                obj.PinedCommand?.Execute(obj.DataContext);
            }
            else
            {
                obj.PinCommand?.Execute(obj.DataContext);
            }
        }

        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(PinButton), new PropertyMetadata(PinImage));

        public static readonly DependencyProperty PinCommandProperty = DependencyProperty.Register(nameof(PinCommand), typeof(ICommand), typeof(PinButton), new PropertyMetadata(null));

        public static readonly DependencyProperty PinedCommandProperty = DependencyProperty.Register(nameof(PinedCommand), typeof(ICommand), typeof(PinButton), new PropertyMetadata(null));

        public string ImagePath
        {
            get { return (string)GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }

        protected override void OnClick()
        {
            base.OnClick();
            this.Pined = !this.Pined;
        }

        public bool Pined
        {
            get { return (bool)GetValue(PinedProperty); }
            set
            {
                SetValue(PinedProperty, value);
                ImagePath = value ? PinedImage : PinImage;
            }
        }

        public ICommand PinCommand
        {
            get { return (ICommand)GetValue(PinCommandProperty); }
            set { SetValue(PinCommandProperty, value); }
        }

        public ICommand PinedCommand
        {
            get { return (ICommand)GetValue(PinedCommandProperty); }
            set { SetValue(PinedCommandProperty, value); }
        }
    }
}
