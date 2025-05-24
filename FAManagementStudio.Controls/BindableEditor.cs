using FAManagementStudio.Controls.Common;
using FAManagementStudio.Foundation.Common;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace FAManagementStudio.Controls;

public class BindableEditor : TextEditor
{
    public BindableEditor()
    {
        Uri fileUri = new("pack://application:,,,/FAManagementStudio.Controls;component/files/sql.xshd", UriKind.Absolute);
        try
        {
            using var reader = new XmlTextReader(Application.GetResourceStream(fileUri).Stream);
            SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
        catch //放置
        {
        }
        SetCommand();
        SearchPanel.Install(this);
    }

    private CompletionWindow? _completionWindow;

    public static readonly DependencyProperty RecommendProperty = DependencyProperty.Register(nameof(Recommender), typeof(IRecommender), typeof(BindableEditor), null);

    public IRecommender Recommender
    {
        get => (IRecommender)GetValue(RecommendProperty);
        set { SetValue(RecommendProperty, value); }
    }

    private string[] _marks = [";"];
    private async void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
    {
        if (_marks.Contains(e.Text)) return;
        await ShowCompletionWindow();
    }

    private async Task ShowCompletionWindow()
    {
        if (Recommender == null) return;
        _completionWindow = new CompletionWindow(TextArea);
        var data = _completionWindow.CompletionList.CompletionData;
        var list = await Recommender.GetCompletionData(Text, TextArea.Caret.Offset - 1);
        foreach (var item in list)
        {
            data.Add(item);
        }
        if (0 < data.Count)
        {
            _completionWindow.Show();
            _completionWindow.Closed += (obj, e) => _completionWindow = null;
        }
        else
        {
            _completionWindow = null;
        }
    }

    private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
    {
    }

    public new string Text
    {
        get => (string)GetValue(TextProperty);
        set { SetValue(TextProperty, value); }
    }

    internal string BaseText
    {
        get => base.Text;
        set { base.Text = value; }
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(BindableEditor),
        new PropertyMetadata(string.Empty, (obj, args) =>
         {
             var target = (BindableEditor)obj;
             if (target.BaseText != (string)args.NewValue)
                 target.BaseText = (string)args.NewValue;
         })
    );

    protected override void OnTextChanged(EventArgs e)
    {
        if (Text != BaseText)
        {
            SetValue(TextProperty, BaseText);
        }
        base.OnTextChanged(e);
    }

    public bool IntellisenseEnabled
    {
        get => (bool)GetValue(IntellisenseEnabledProperty);
        set { SetValue(IntellisenseEnabledProperty, value); }
    }

    public static readonly DependencyProperty IntellisenseEnabledProperty = DependencyProperty.Register(nameof(IntellisenseEnabled), typeof(bool), typeof(BindableEditor), new PropertyMetadata(false, OnEnablePropertyChanged));

    private static void OnEnablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not BindableEditor obj) return;
        if (obj.IntellisenseEnabled)
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
             var document = Document;
             var start = document.GetLineByOffset(SelectionStart);
             var end = document.GetLineByOffset(SelectionStart + SelectionLength);
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
            var document = Document;
            var start = document.GetLineByOffset(SelectionStart);
            var end = document.GetLineByOffset(SelectionStart + SelectionLength);
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

        PreviewKeyDown += (object sender, KeyEventArgs e) =>
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
        if (IntellisenseEnabled && e.Key == Key.Space && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
        {
            e.Handled = true;
            await ShowCompletionWindow();
        }
    }
}
