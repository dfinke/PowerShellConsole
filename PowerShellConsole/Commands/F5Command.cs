using System;
using System.Diagnostics;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace PowerShellConsole.Commands
{
    public class F5Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private TextEditor textEditor;

        public F5Command(TextEditor textEditor)
        {
            if (textEditor == null)
            {
                throw new ArgumentNullException("textEditor in F5Command ctor");
            }

            this.textEditor = textEditor;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ApplicationExtensionPoints
                .ApplicationExtensionPointsInstance
                .InvokeCurrentScript
                .ExecuteScriptEntryPoint();

            foreach (var item in textEditor.Text.ExecutePS())
            {
                Debug.WriteLine(item);
            }
        }
    }
}
