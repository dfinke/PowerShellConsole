using System.Linq;
using System.Management.Automation;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace PowerShellConsole.Utilities
{
    public class TextEditorUtilities
    {
        static CompletionWindow completionWindow;

        public static void InvokeCompletionWindow(TextEditor textEditor)
        {
            completionWindow = new CompletionWindow(textEditor.TextArea);

            completionWindow.Closed += delegate
            {
                completionWindow = null;
            };

            var text = textEditor.Text;
            var offset = textEditor.TextArea.Caret.Offset;

            var completedInput = CommandCompletion.CompleteInput(text, offset, null, PSConsolePowerShell.PowerShellInstance);
            // var r = CommandCompletion.MapStringInputToParsedInput(text, offset);
            
            "InvokeCompletedInput".ExecuteScriptEntryPoint(completedInput.CompletionMatches);

            if (completedInput.CompletionMatches.Count > 0)
            {
                completedInput.CompletionMatches.ToList()
                    .ForEach(record =>
                    {
                        completionWindow.CompletionList.CompletionData.Add(
                            new CompletionData
                            {
                                CompletionText = record.CompletionText,
                                ToolTip = record.ToolTip,
                                Resultype = record.ResultType,
                                ReplacementLength = completedInput.ReplacementLength
                            });
                    });

                completionWindow.Show();
            }
        }
    }
}
