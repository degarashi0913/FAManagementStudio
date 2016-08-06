using FAManagementStudio.Controls.Common;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
            SearchPanel.Install(this);
        }

        private CompletionWindow _completionWindow;
        private FirebirdRecommender _fbRecommender = new FirebirdRecommender();

        private string[] _marks = new string[] { ";" };
        private async void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (_marks.Contains(e.Text)) return;
            await ShowCompletionWindow();
        }

        private async Task ShowCompletionWindow()
        {
            _completionWindow = new CompletionWindow(TextArea);
            var data = _completionWindow.CompletionList.CompletionData;
            var list = await _fbRecommender.GetCompletionData(Text, TextArea.Caret.Offset - 1);
            foreach (var item in list)
            {
                data.Add(item);
            }
            _completionWindow.Show();
            _completionWindow.Closed += (obj, e) => _completionWindow = null;
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
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

        public bool IntelisenseEnabled
        {
            get { return (bool)GetValue(IntelisenseEnabledProperty); }
            set { SetValue(IntelisenseEnabledProperty, value); }
        }

        public static DependencyProperty IntelisenseEnabledProperty = DependencyProperty.Register(nameof(IntelisenseEnabled), typeof(bool), typeof(BindableEditor), new PropertyMetadata(false, OnEnablePropertyChanged));

        private static void OnEnablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as BindableEditor;
            if (obj == null) return;
            if (obj.IntelisenseEnabled)
            {
                obj.TextArea.TextEntering += obj.TextArea_TextEntering;
                obj.TextArea.TextEntered += obj.TextArea_TextEntered;
            }
            else
            {
                obj.TextArea.TextEntering -= obj.TextArea_TextEntering;
                obj.TextArea.TextEntered -= obj.TextArea_TextEntered;
            }
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
        protected override async void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (IntelisenseEnabled && e.Key == Key.Space && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;
                await ShowCompletionWindow();
            }
        }
    }
}
