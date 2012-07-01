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
        private PowerShell powerShell;

        public ControlSpacebarCommand(TextEditor textEditor, PowerShell powerShell)
        {
            if (textEditor == null)
            {
                throw new ArgumentNullException("textEditor in ControlSpaceBarCommand ctor");
            }

            if (powerShell == null)
            {
                throw new ArgumentNullException("powerShell in ControlSpaceBarCommand ctor");
            }

            this.textEditor = textEditor;
            this.powerShell = powerShell;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            TextEditorUtilities.InvokeCompletionWindow(textEditor, powerShell);
        }
    }
}
