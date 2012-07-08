﻿using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace PowerShellConsole.Commands
{
    public class F5Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private TextEditor textEditor;
        private PowerShell powerShell;

        public F5Command(TextEditor textEditor, PowerShell powerShell)
        {
            if (textEditor == null)
            {
                throw new ArgumentNullException("textEditor in F5Command ctor");
            }

            if (powerShell == null)
            {
                throw new ArgumentNullException("powerShell in F5Command ctor");
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
            powerShell
                .AddScript(textEditor.Text)
                .AddCommand("Out-String");

            foreach (var item in powerShell.Invoke())
            {
                Debug.WriteLine(item);
            }
        }
    }
}
