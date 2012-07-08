using System;
using System.Management.Automation;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using PowerShellConsole.Utilities;

namespace PowerShellConsole.Commands
{
    public class ControlSpacebarCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private TextEditor textEditor;

        public ControlSpacebarCommand(TextEditor textEditor)
        {
            if (textEditor == null)
            {
                throw new ArgumentNullException("textEditor in ControlSpaceBarCommand ctor");
            }

            this.textEditor = textEditor;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {            
            TextEditorUtilities.InvokeCompletionWindow(textEditor);
        }
    }
}
