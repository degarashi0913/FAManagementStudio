using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Windows;
using System.Xml;

namespace FAManagementStudio.Controls
{
    public class BindableEditor : TextEditor
    {
        public BindableEditor()
        {
            Uri fileUri = new Uri("pack://application:,,,/FAManagementStudio.Controls;component/files/sql.xshd", UriKind.Absolute);
            try
            {
                using (var reader = new XmlTextReader(Application.GetResourceStream(fileUri).Stream))
                {
                    this.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            catch //放置
            {
            }
        }
        public new string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        internal string baseText
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        public static DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(BindableEditor),
            new PropertyMetadata((obj, args) =>
            {
                var target = (BindableEditor)obj;
                if (target.baseText != (string)args.NewValue)
                    target.baseText = (string)args.NewValue;
            })
        );

        protected override void OnTextChanged(EventArgs e)
        {
            if (Text != null && Text != baseText)
            {
                SetValue(TextProperty, baseText);
            }
            base.OnTextChanged(e);
        }
    }
}
