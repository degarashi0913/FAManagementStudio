using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Linq;
using System.Windows.Media;

namespace FAManagementStudio.Controls.Common;

public class CompletionData(string text) : ICompletionData
{
    public ImageSource? Image => null;

    public string Text { get; private set; } = text;

    public object Content => Text;


    public object Description => "Description for " + Text;

    public double Priority => 0.0;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(CurrentWordSegment(completionSegment, textArea.Document.Text), this.Text);
    }

    private static TextSegment CurrentWordSegment(ISegment seg, string text)
    {
        var marks = new[] { '\r', '\n', ' ', '.' };
        var str = seg.Offset - 1;

        if (text.Length < 1 || marks.Contains(text[str])) return new TextSegment { StartOffset = seg.Offset, EndOffset = seg.EndOffset };

        while (0 < str)
        {
            var c = text[str - 1];
            if (marks.Contains(c)) break;
            str--;
        }

        var end = seg.EndOffset;
        while (end < text.Length)
        {
            var c = text[end];
            if (marks.Contains(c)) break;
            end++;
        }
        return new TextSegment { StartOffset = str, EndOffset = end };
    }
}
