using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;

using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

using PowerShellConsole.Commands;
using PowerShellConsole.FoldingStrategies;
using PowerShellConsole.Utilities;

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

        public MainWindow()
        {
            InitializeComponent();

            ps = PowerShell.Create();

            textEditor.Focus();
            textEditor.TextArea.TextEntered += new TextCompositionEventHandler(TextArea_TextEntered);
            textEditor.ShowLineNumbers = true;

            //textEditor.TextArea.TextView.LineTransformers.Add(new TransfromFunctionKeyword());

            textEditor.Load("TextFile.ps1");

            InstallFoldingStrategy();
            AddSyntaxHighlighting();
            SetupInputHandlers();
        }

        private void SetupInputHandlers()
        {
            AddF5();
            AddCtrlSpacebar();
        }

        private void AddCtrlSpacebar()
        {
            var handleCtrlSpacebar = new KeyBinding(new ControlSpacebarCommand(textEditor, ps), Key.Space, ModifierKeys.Control);
            AddKeyBinding(handleCtrlSpacebar);
        }

        private void AddF5()
        {
            KeyBinding handleF5 = new KeyBinding(new F5Command(textEditor, ps), new KeyGesture(Key.F5));
            AddKeyBinding(handleF5);
        }

        private void AddKeyBinding(KeyBinding targetBinding)
        {
            textEditor.TextArea.DefaultInputHandler.Editing.InputBindings.Add(targetBinding);
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

            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
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
                TextEditorUtilities.InvokeCompletionWindow(textEditor, ps);
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
