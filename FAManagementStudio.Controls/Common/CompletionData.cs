using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FAManagementStudio.Controls.Common
{
    public class CompletionData : ICompletionData
    {
        public CompletionData(string text)
        {
            this.Text = text;
        }

        public ImageSource Image
        {
            get { return null; }
        }

        public string Text { get; private set; }

        public object Content
        {
            get { return this.Text; }
        }

        public object Description
        {
            get { return "Description for " + this.Text; }
        }

        public double Priority
        {
            get
            {
                return 0.0;
            }
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(CurrentWordSegment(completionSegment, textArea.Document.Text), this.Text);
        }

        private TextSegment CurrentWordSegment(ISegment seg, string text)
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
}
