using System;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private readonly KeyboardHook _keyboardHook;

        public MainWindow()
        {
            InitializeComponent();
            _keyboardHook = new KeyboardHook();
            _keyboardHook.KeyPressed += OnHookKeyPressed;
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            AppendLog("WPF KeyDown", e.Key.ToString());
        }
        private void OnHookKeyPressed(Key key)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                AppendLog("Low-level Hook", key.ToString());
            }));
        }

        private void AppendLog(string source, string key)
        {
            string timestamp = DateTimeOffset.Now.ToString("HH:mm:ss.fff");
            LogTextBox.AppendText($"[{timestamp}] [{source}] {key}\r\n");
            LogTextBox.ScrollToEnd();
        }

        private void HookCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsActive) StartHook();
        }

        private void HookCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            StopHook();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            StopHook();
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            if (HookCheckBox.IsChecked == true) StartHook();
        }

        private void StartHook()
        {
            if (_keyboardHook.IsInstalled) return;
            try
            {
                _keyboardHook.Start();
                StatusLabel.Text = "Hook установлен";
                AppendLog("System", "Hook установлен");
            }
            catch (Win32Exception ex)
            {
                StatusLabel.Text = "Ошибка установки hook";
                AppendLog("Error", ex.Message);
                HookCheckBox.IsChecked = false;
            }
        }

        private void StopHook()
        {
            if (!_keyboardHook.IsInstalled) return;
            try
            {
                _keyboardHook.Stop();
                StatusLabel.Text = "Hook не установлен";
                AppendLog("System", "Hook снят");
            }
            catch (Win32Exception ex)
            {
                StatusLabel.Text = "Ошибка снятия hook";
                AppendLog("Error", ex.Message);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _keyboardHook.Dispose();
            base.OnClosed(e);
        }
    }
}
/*
1.
KeyDown ловит нажатия только внутри твоей программы, когда окно активно.
А hook - это перехватчик на уровне всей системы, он видит клавиши, даже если твоя программа свернута.

2.
Чтобы передать нажатие дальше. Если его не вызвать, другие программы или сама Windows могут перестать получать сигнал от клавиатуры.

3. 
Потому что Windows ждет, пока он закончит, и только потом пропускает нажатие. 
Если внутри делать что-то долгое, клавиатура будет тормозить, и система сама отключит hook.

4. 
Чтобы сборщик мусора его не удалил. Если удалит, Windows попытается обратиться к функции, которой больше нет, и программа вылетит.

5. 
Чтобы очистить память. Если этого не сделать, hook останется висеть в системе и будет перехватывать клавиши, даже когда программа закрыта.

6.
Потому что он работает на уровне всей ОС и не зависит от того, какое окно сейчас активно. 
Обычный обработчик привязан к конкретному окну, а hook ко всей системе.
*/
