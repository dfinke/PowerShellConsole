using System.Collections;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

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
            textEditor.ShowLineNumbers = true;
            
            textEditor.Load("TextFile.ps1");

            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("PowerShellConsole.PowerShell.xshd"))
            {
                if (s != null)
                {
                    using (XmlTextReader reader = new XmlTextReader(s))
                    {
                        textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }

                }
            }
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
}
