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
using Codeer.Friendly.Windows.Grasp;
using System;
using Codeer.Friendly.Windows.NativeStandardControls;

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

        [TestMethod, Timeout(1000 * 60)]
        public void コントロール特定()
        {
            // 型名からウィンドウを類推
            var main = WindowControl.IdentifyFromTypeFullName(_app, "WpfApplication.MainWindow");
            // 対象アプリケーションのタイプのカレントから、ウィンドウを取得。(内部型を知っている場合)
            //            var mainWindowCore = _app.Type<Application>().Current.MainWindows;
            //            main = new WindowControl(mainWindowCore);
            // ウィンドウタイトルから類推
            main = WindowControl.IdentifyFromWindowText(_app, "Friendly Handson");

            // 試しにキャプション変えてみよう。
            main.SetWindowText("そんな、こじゃれたキャプションじゃなくてやな…");

            // タブの取得
            var logicalMain = main.LogicalTree();
            AppVar tabCore = logicalMain.ByType<TabControl>().Single();
            var tab = new WPFTabControl(tabCore);
            // タブの選択を「二つ目」にする。
            for (int i = 0; i < 100; i++)
            {
                tab.EmulateChangeSelectedIndex(i % 2);
            }

            // UserControll取得。
            AppVar demoSimpleControlCore = logicalMain.ByType("WpfApplication.DemoSimpleControl").Single();
            var logicalDemoSimple = demoSimpleControlCore.LogicalTree();

            tab.EmulateChangeSelectedIndex(1);
            var textBox1 = new WPFTextBox(demoSimpleControlCore.Dynamic()._textBox);
            textBox1.EmulateChangeText("いやいや、そうじゃなくてやな…");

            var textBoxMail = new WPFTextBox(logicalDemoSimple.ByBinding("Mail").Single());
            textBoxMail.EmulateChangeText("xxx@yyy");

            Assert.AreEqual("xxx@yyy", textBoxMail.Text);

            // コンボボックスの取得
            var comboBox = new WPFComboBox(logicalDemoSimple.ByType<ComboBox>().Single());
            comboBox.EmulateChangeSelectedIndex(2);

            // ボタンの取得
            var buttons = logicalDemoSimple.ByType<Button>();
            var buttonOpen = new WPFButtonBase(buttons.ByCommand(ApplicationCommands.Open).Single());

            // "1"ボタンを取得
            var button1 = new WPFButtonBase(buttons.ByCommandParameter("1").Single());

            var enumParamType = _app.Type("WpfApplication.EnumParam");
            var buttonEnum = new WPFButtonBase(buttons.ByCommandParameter((AppVar)_app.Type().WpfApplication.EnumPram.A).Single());

            var buttonCansel = new WPFButtonBase(buttons.ByIsCancel().Single());

            // クリックしてみる(メッセージボックスが出るので、非同期で)
            var async = new Async();
            buttonOpen.EmulateClick(async);

            // 出てきたモーダルのメッセージボックスを取る

            // WPFじゃ、これはできない(なぞのWindowに邪魔される)
            //var msgBox = main.WaitForNextModal(); 

            // WPF考慮入り版
            WindowControl msgBox = null;
            while (msgBox == null)
            {
                foreach (var x in WindowControl.GetTopLevelWindows(_app))
                {
                    if (string.IsNullOrEmpty(x.TypeFullName))
                    {
                        msgBox = x;
                        break;
                    }
                }
            }
            var msgBoxWrapper = new NativeMessageBox(msgBox);
            msgBoxWrapper.EmulateButtonClick("OK");

            Console.WriteLine("テスト用");
        }

        [TestMethod]
        public void コントロール特定2()
        {
        }

        [TestMethod]
        public void コントロール特定3()
        {
            WindowsAppExpander.LoadAssembly(_app, GetType().Assembly);
            WPFStandardControls_4.Injection(_app);

            var main = WindowControl.IdentifyFromWindowText(_app, "Friendly Handson");
            var logicalMain = main.LogicalTree();
            AppVar demoSimpleControlCore = logicalMain.ByType("WpfApplication.DemoSimpleControl").Single();

            Mapping mapping = _app.Type(GetType()).GetMapping(demoSimpleControlCore);

            mapping.TextBoxMail.Text = "日本語で何かに変更する";
        }

        class Mapping
        {
            internal TextBox TextBoxMail { get; set; }
            internal Button ButtonOpen { get; set; }
        }

        static Mapping GetMapping(UserControl user)
        {
            var tree = user.LogicalTree();
            return new Mapping()
            {
                TextBoxMail = tree.OfType<TextBox>().ByBinding("Mail").Single(),
                ButtonOpen = tree.OfType<Button>().Where(e => (string)e.Content == "ApplicationCommands.Open").Single()
            };
        }
    }
}
