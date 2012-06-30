using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
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
        AbstractFoldingStrategy foldingStrategy;
        FoldingManager foldingManager;
        CompletionWindow completionWindow;

        public MainWindow()
        {
            InitializeComponent();

            textEditor.Focus();
            textEditor.TextArea.TextEntered += new TextCompositionEventHandler(TextArea_TextEntered);
            textEditor.ShowLineNumbers = true;

            //textEditor.TextArea.TextView.LineTransformers.Add(new TransfromFunctionKeyword());

            textEditor.Load("TextFile.ps1");

            InstallFoldingStrategy();
            AddSyntaxHighlighting();

            ps = PowerShell.Create();
        }

        private void InstallFoldingStrategy()
        {
            foldingManager = FoldingManager.Install(textEditor.TextArea);
            foldingStrategy = new BraceFoldingStrategy();
            foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);

            AddFoldingStrategyTimer();
        }

        private void AddSyntaxHighlighting()
        {
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
        }

        private void AddFoldingStrategyTimer()
        {
            var foldingUpdateTimer = new DispatcherTimer();

            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2.5);
            foldingUpdateTimer.Tick += new EventHandler(foldingUpdateTimer_Tick);
            foldingUpdateTimer.Start();
        }

        void foldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
        }

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

    //public class TransfromFunctionKeyword : DocumentColorizingTransformer
    //{
    //    protected override void ColorizeLine(DocumentLine line)
    //    {
    //        int lineStartOffset = line.Offset;
    //        string text = CurrentContext.Document.GetText(line);
    //        int start = 0;
    //        int index;
    //        while ((index = text.IndexOf("function", start)) >= 0)
    //        {
    //            base.ChangeLinePart(
    //                lineStartOffset + index, // startOffset
    //                lineStartOffset + index + 10, // endOffset
    //                (VisualLineElement element) =>
    //                {
    //                    // This lambda gets called once for every VisualLineElement
    //                    // between the specified offsets.
    //                    Typeface tf = element.TextRunProperties.Typeface;
    //                    // Replace the typeface with a modified version of
    //                    // the same typeface
    //                    element.TextRunProperties.SetTypeface(new Typeface(
    //                        tf.FontFamily,
    //                        FontStyles.Italic,
    //                        FontWeights.Bold,
    //                        tf.Stretch
    //                    ));
    //                });
    //            start = index + 1; // search for next occurrence
    //        }
    //    }
    //}
}
