using Codeer.Friendly.Windows;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Codeer.Friendly;
using System.Windows;
using Codeer.Friendly.Dynamic;
using RM.Friendly.WPFStandardControls;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace Test
{
    [TestClass]
    public class FriendlyTest
    {
        WindowsAppFriend _app;

        [TestInitialize]
        public void TestInitialize()
        {
            var pathExe = Path.GetFullPath("../../../WpfApplication/bin/Release/WpfApplication.exe");
            _app = new WindowsAppFriend(Process.Start(pathExe));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var id = _app.ProcessId;
            Process.GetProcessById(id).Kill();
        }

        [TestMethod]
        public void コントロール特定()
        {
            AppVar main = _app.Type<Application>().Current.MainWindow;
            var userControl = main.LogicalTree().ByType("WpfApplication.DemoSimpleControl").Single();

            var logicalTree = userControl.LogicalTree();

            var textBoxX = new WPFTextBox(userControl.Dynamic()._textBox);
            var textBoxMail = new WPFTextBox(logicalTree.ByBinding("Mail").Single());
            var comboBoxxLanguage = new WPFComboBox(logicalTree.ByBinding("Language").Single());
            var buttons = userControl.LogicalTree().ByType<Button>();
            var buttonOpen = new WPFButtonBase(buttons.ByCommand(ApplicationCommands.Open).Single());
            var button1 = new WPFButtonBase(buttons.ByCommandParameter("1").Single());
            var buttonA = new WPFButtonBase(buttons.ByCommandParameter((AppVar)_app.Type().WpfApplication.EnumPram.A).Single());
            var buttonCancel = new WPFButtonBase(buttons.ByIsCancel().Single());
        }


        [TestMethod]
        public void コントロール特定2()
        {
            AppVar main = _app.Type<Application>().Current.MainWindow;

            var tab = new WPFTabControl(main.LogicalTree().ByType<TabControl>().Single());
            tab.EmulateChangeSelectedIndex(1);

            var userControl = main.LogicalTree().ByType("WpfApplication.DemoItemsControl").Single();
            var listBox = new WPFListBox(userControl.LogicalTree().ByType<ListBox>().Single());

            var textBoxAge = new WPFTextBox(listBox.GetItem(2).VisualTree().ByBinding("Age").Single());
            textBoxAge.EmulateChangeText("50");
        }

        [TestMethod]
        public void コントロール特定3()
        {
            WindowsAppExpander.LoadAssembly(_app, GetType().Assembly);
            WPFStandardControls_3_5.Injection(_app);

            AppVar main = _app.Type<Application>().Current.MainWindow;
            var userControl = main.LogicalTree().ByType("WpfApplication.DemoSimpleControl").Single();
            var elements = _app.Type(GetType()).GetDemoSimpleControlElements(userControl);

            var textBlokX = new WPFTextBlock(elements.TextBlockXName);
            var textBlokMail = new WPFTextBlock(elements.TextBlockMail);
            var textBlokxLanguage = new WPFTextBlock(elements.TextBlockLanguage);

            var textBoxX = new WPFTextBox(elements.TextBoxXName);
            var textBoxMail = new WPFTextBox(elements.TextBoxMail);
            var comboBoxxLanguage = new WPFComboBox(elements.ComboBoxLanguage);
            var buttonOpen = new WPFButtonBase(elements.ButtonOpen);
            var button1 = new WPFButtonBase(elements.Button1);
            var buttonA = new WPFButtonBase(elements.ButtonA);
            var buttonCancel = new WPFButtonBase(elements.ButtonCancel);
        }

        class DemoSimpleControlElements
        {
            public TextBlock TextBlockXName { get; set; }
            public TextBlock TextBlockMail { get; set; }
            public TextBlock TextBlockLanguage { get; set; }
            public TextBox TextBoxXName { get; set; }
            public TextBox TextBoxMail { get; set; }
            public ComboBox ComboBoxLanguage { get; set; }
            public Button ButtonOpen { get; set; }
            public Button Button1 { get; set; }
            public Button ButtonA { get; set; }
            public Button ButtonCancel { get; set; }
        }

        static DemoSimpleControlElements GetDemoSimpleControlElements(UserControl user)
        {
            var logical = user.LogicalTree();
            var textBlocks = logical.ByType<TextBlock>();
            var textBoxes = logical.ByType<TextBox>();
            var buttons = logical.ByType<Button>();
            return new DemoSimpleControlElements
            {
                TextBlockXName = textBlocks.Where(e => e.Text == "x:Name : _textBox").Single(),
                TextBlockMail = textBlocks.Where(e => e.Text == "binding : Mail").Single(),
                TextBlockLanguage = textBlocks.Where(e => e.Text == "binding : Language").Single(),
                TextBoxXName = logical.ByType<TextBox>().Where(e => e.Name == "_textBox").Single(),
                TextBoxMail = logical.ByType<TextBox>().ByBinding("Mail").Single(),
                ComboBoxLanguage = logical.ByType<ComboBox>().Single(),
                ButtonOpen = buttons.Where(e => e.Command == ApplicationCommands.Open).Single(),
                Button1 = buttons.Where(e => e.CommandParameter != null).Where(e => e.CommandParameter.ToString() == "1").Single(),
                ButtonA = buttons.Where(e => e.CommandParameter != null).Where(e => e.CommandParameter.ToString() == "A").Single(),
                ButtonCancel = buttons.Where(e => e.IsCancel).Single()
            };
        }
    }
}
