using System.Collections;
using System.Linq;
using System.Management.Automation;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace PowerShellConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PowerShell ps;

        public MainWindow()
        {
            InitializeComponent();

            textEditor.Focus();
            textEditor.TextArea.TextEntered += new TextCompositionEventHandler(TextArea_TextEntered);
            ps = PowerShell.Create();
        }

        CompletionWindow completionWindow;
        void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {

            if (e.Text.IndexOfAny(@"$-.:\".ToCharArray()) != -1)
            {
                completionWindow = new CompletionWindow(textEditor.TextArea);

                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };

                var data = completionWindow.CompletionList.CompletionData;

                var h = new Hashtable();
                //h["RelativeFilePaths"] = true;

                var records = CommandCompletion.CompleteInput(textEditor.Text, textEditor.TextArea.Caret.Offset, null, ps).CompletionMatches;

                (from record in records
                 select new CompletionData
                 {
                     CompletionText = record.CompletionText,
                     ToolTip = record.ToolTip,
                     Resultype = record.ResultType
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
}
