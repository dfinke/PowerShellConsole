using System.Collections;
using System.Linq;
using System.Management.Automation;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace PowerShellConsole.Utilities
{
    public class TextEditorUtilities
    {
        static CompletionWindow completionWindow;

        public static void InvokeCompletionWindow(TextEditor textEditor, PowerShell ps)
        {
            completionWindow = new CompletionWindow(textEditor.TextArea);

            completionWindow.Closed += delegate
            {
                completionWindow = null;
            };

            var data = completionWindow.CompletionList.CompletionData;

            var h = new Hashtable();
            //h["RelativeFilePaths"] = true;

            var completedInput = CommandCompletion.CompleteInput(textEditor.Text, textEditor.TextArea.Caret.Offset, null, ps);

            var records = completedInput.CompletionMatches;

            (from record in records
             select new CompletionData
             {
                 CompletionText = record.CompletionText,
                 ToolTip = record.ToolTip,
                 Resultype = record.ResultType,
                 ReplacementLength = completedInput.ReplacementLength
             })
             .ToList()
             .ForEach(i => data.Add(i));

            if (data.Count > 0)
            {
                completionWindow.Show();
            }
        }

    }
}
