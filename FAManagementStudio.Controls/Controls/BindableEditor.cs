using FAManagementStudio.Controls.Common;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
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
            SetCommand();
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
            new PropertyMetadata(string.Empty, (obj, args) =>
             {
                 var target = (BindableEditor)obj;
                 if (target.baseText != (string)args.NewValue)
                     target.baseText = (string)args.NewValue;
             })
        );

        protected override void OnTextChanged(EventArgs e)
        {
            if (Text != baseText)
            {
                SetValue(TextProperty, baseText);
            }
            base.OnTextChanged(e);
        }

        private bool donePre = false;

        private void SetCommand()
        {
            var insert = new RelayCommand(() =>
             {
                 var document = this.Document;
                 var start = document.GetLineByOffset(this.SelectionStart);
                 var end = document.GetLineByOffset(this.SelectionStart + this.SelectionLength);
                 using (document.RunUpdate())
                 {
                     var line = start;
                     while (line != null)
                     {
                         InsertComment(document, line);
                         if (line == end) break;
                         line = line.NextLine;
                     }
                 }
             });

            var delete = new RelayCommand(() =>
            {
                var document = this.Document;
                var start = document.GetLineByOffset(this.SelectionStart);
                var end = document.GetLineByOffset(this.SelectionStart + this.SelectionLength);
                using (document.RunUpdate())
                {
                    var line = start;
                    while (line != null)
                    {
                        DeleteComment(document, line);
                        if (line == end) break;
                        line = line.NextLine;
                    }
                }
            });

            this.PreviewKeyDown += (object sender, KeyEventArgs e) =>
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    switch (e.Key)
                    {
                        case Key.K:
                            donePre = !donePre;
                            break;
                        case Key.C:
                            if (donePre)
                            {
                                insert.Execute(null);
                            }
                            donePre = false;
                            break;
                        case Key.U:
                            if (donePre)
                            {
                                delete.Execute(null);
                            }
                            donePre = false;
                            break;
                        default:
                            donePre = false;
                            break;
                    }
                }
                else
                {
                    donePre = false;
                }
            };
        }

        private const string FbCommentString = "--";

        private void InsertComment(TextDocument doc, DocumentLine line)
        {
            if (line.TotalLength == 0) return;
            doc.Insert(line.Offset, FbCommentString);
        }
        private void DeleteComment(TextDocument doc, DocumentLine line)
        {
            if (doc.TextLength < 2) return;
            if (doc.GetText(line.Offset, line.EndOffset - line.Offset).TrimStart().StartsWith(FbCommentString))
            {
                var idx = doc.Text.IndexOf(FbCommentString, line.Offset);
                doc.Remove(idx, FbCommentString.Length);
            }
        }
    }
}
