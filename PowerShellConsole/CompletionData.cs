using System;
using System.Management.Automation;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace PowerShellConsole
{
    public class CompletionData : ICompletionData
    {
        public string ToolTip { get; set; }
        public string CompletionText { get; set; }
        public CompletionResultType Resultype { get; set; }
        public int ReplacementLength { get; set; }

        public ImageSource Image
        {
            get
            {
                switch (Resultype)
                {
                    case CompletionResultType.Command:
                        break;
                    case CompletionResultType.History:
                        break;
                    case CompletionResultType.Method:
                        break;
                    case CompletionResultType.Namespace:
                        break;
                    case CompletionResultType.ParameterName:
                        break;
                    case CompletionResultType.ParameterValue:
                        break;
                    case CompletionResultType.Property:
                        break;
                    case CompletionResultType.ProviderContainer:
                        break;
                    case CompletionResultType.ProviderItem:
                        break;
                    case CompletionResultType.Text:
                        break;
                    case CompletionResultType.Type:
                        break;
                    case CompletionResultType.Variable:
                        break;
                    default:
                        break;
                }

                return null;
            }
        }

        public string Text
        {
            get
            {
                return CompletionText;
            }
        }

        // Use this property if you want to show a fancy UIElement in the drop down list.
        public object Content
        {
            get { return this.Text; }
        }

        public object Description
        {
            get { return this.ToolTip; }
        }

        public double Priority { get { return 0; } }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var length = textArea.Caret.Offset;
            var offset = completionSegment.Offset - ReplacementLength;

            length = offset == 0 ? 
                length : 
                length - completionSegment.Offset + ReplacementLength;

            textArea.Document.Replace(offset, length, this.Text);
        }
    }
}
