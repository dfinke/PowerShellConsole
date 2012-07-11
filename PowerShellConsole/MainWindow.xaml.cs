// http://www.opensourcejavaphp.net/csharp/sharpdevelop/CodeEditor.cs.html

using System;
using System.Diagnostics;
using System.Management.Automation.Language;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using ICSharpCode.AvalonEdit.AddIn;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.SharpDevelop.Editor;
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
        AbstractFoldingStrategy foldingStrategy;
        FoldingManager foldingManager;
        TextMarkerService textMarkerService;
        ToolTip toolTip;

        public MainWindow()
        {
            InitializeComponent();

            textEditor.Focus();

            textEditor.TextArea.PreviewKeyUp += new KeyEventHandler(TextArea_PreviewKeyUp);
            textEditor.TextArea.TextEntered += new TextCompositionEventHandler(TextArea_TextEntered);

            textEditor.MouseHover += new MouseEventHandler(textEditor_MouseHover);
            textEditor.MouseHoverStopped += new MouseEventHandler(textEditor_MouseHoverStopped);

            textEditor.ShowLineNumbers = true;

            //textEditor.TextArea.TextView.LineTransformers.Add(new TransfromFunctionKeyword());

            textEditor.Load("TextFile.ps1");

            InstallFoldingStrategy();
            AddSyntaxHighlighting();
            SetupInputHandlers();
            SetupMarkerService();
            TestForSyntaxErrors();

            PSConsolePowerShell.SetVariable("tse", this);
            PSConsolePowerShell.SetVariable("textEditor", this.textEditor);

            //ApplicationExtensionPoints
            //    .ApplicationExtensionPointsInstance
            //    .InvokeInitializeConsole
            //    .ExecuteScriptEntryPoint();

            "InvokeInitializeConsole".ExecuteScriptEntryPoint(this);
        }

        private void SetupMarkerService()
        {
            textMarkerService = new TextMarkerService(this.textEditor.Document);
            textEditor.TextArea.TextView.BackgroundRenderers.Add(textMarkerService);

            var textView = textEditor.TextArea.TextView;
            textView.LineTransformers.Add(textMarkerService);
            textView.Services.AddService(typeof(ITextMarkerService), textMarkerService);
        }

        private void SetupInputHandlers()
        {
            AddF5();
            AddCtrlSpacebar();
        }

        private void AddCtrlSpacebar()
        {
            var handleCtrlSpacebar = new KeyBinding(new ControlSpacebarCommand(textEditor), Key.Space, ModifierKeys.Control);
            AddKeyBinding(handleCtrlSpacebar);
        }

        private void AddF5()
        {
            KeyBinding handleF5 = new KeyBinding(new F5Command(textEditor), new KeyGesture(Key.F5));
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
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("PowerShellConsole.PowerShell.xshd"))
            {
                if (s != null)
                {
                    using (var reader = new XmlTextReader(s))
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
                TextEditorUtilities.InvokeCompletionWindow(textEditor);
            }
        }

        void TextArea_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (
                e.KeyboardDevice.Modifiers == ModifierKeys.Control ||
                e.KeyboardDevice.Modifiers == ModifierKeys.None ||
                e.KeyboardDevice.Modifiers == ModifierKeys.Shift
                )
            {
                if (e.Key == Key.Down ||
                        e.Key == Key.Up ||
                        e.Key == Key.PageDown ||
                        e.Key == Key.PageUp ||
                        e.Key == Key.Home ||
                        e.Key == Key.End ||
                        e.Key == Key.Left ||
                        e.Key == Key.Right ||
                        e.Key == Key.RightCtrl ||
                        e.Key == Key.LeftCtrl
                        ) return;
            }

            // don't check for syntax errors 
            // if any of those keys were pressed
            var msg = string.Format("PreviewKeyUp: {0} {1}", e.KeyboardDevice.Modifiers, e.Key);
            Debug.WriteLine(msg);

            TestForSyntaxErrors();
        }

        private void TestForSyntaxErrors()
        {
            textMarkerService.RemoveAll();
            var script = textEditor.TextArea.TextView.Document.Text;

            Token[] token;
            ParseError[] errors;
            var ast = Parser.ParseInput(script, out token, out errors);

            foreach (var item in errors)
            {
                var startOffset = item.Extent.StartOffset;
                var endOffset = item.Extent.EndOffset;
                var toolTip = item.Message;
                var length = endOffset - startOffset;
                var m = textMarkerService.Create(startOffset, length);

                m.MarkerType = TextMarkerType.SquigglyUnderline;
                m.MarkerColor = Colors.Red;
                m.ToolTip = toolTip;
            }
        }

        void textEditor_MouseHover(object sender, MouseEventArgs e)
        {
            var textView = textEditor.TextArea.TextView;
            var textMarkerService = textView.Services.GetService(typeof(ITextMarkerService)) as ITextMarkerService;
            if (textMarkerService != null)
            {
                foreach (var textMarker in textMarkerService.TextMarkers)
                {
                    if (toolTip == null)
                    {
                        toolTip = new ToolTip();
                    }

                    toolTip.Content = textMarker.ToolTip;
                    toolTip.IsOpen = true;
                    e.Handled = true;
                }
            }
        }

        void textEditor_MouseHoverStopped(object sender, MouseEventArgs e)
        {
            if (toolTip != null)
            {
                toolTip.IsOpen = false;
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
