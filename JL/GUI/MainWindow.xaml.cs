﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using JL.Utilities;

namespace JL.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _currentTextIndex;

        private static PopupWindow s_firstPopupWindow;

        public static PopupWindow FirstPopupWindow
        {
            get { return s_firstPopupWindow ??= new PopupWindow(); }
        }

        public static MainWindow Instance { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
            {
                Exception ex = (Exception)eventArgs.ExceptionObject;
                Utils.Logger.Error(ex.ToString());
            };

            ClipboardManager windowClipboardManager = new(this);
            windowClipboardManager.ClipboardChanged += ClipboardChanged;

            Instance = this;

            MainWindowUtilities.InitializeMainWindow();
            MainTextBox.IsInactiveSelectionHighlightEnabled = true;
            MainWindowChrome.Freeze();

            CopyFromClipboard();
        }

        private void CopyFromClipboard()
        {
            bool gotTextFromClipboard = false;
            while (Clipboard.ContainsText() && !gotTextFromClipboard)
            {
                try
                {
                    string text = Clipboard.GetText();
                    gotTextFromClipboard = true;
                    if (MainWindowUtilities.JapaneseRegex.IsMatch(text))
                    {
                        text = text.Trim();
                        MainWindowUtilities.Backlog.Add(text);
                        MainTextBox.Text = text;
                        MainTextBox.Foreground = ConfigManager.MainWindowTextColor;
                        _currentTextIndex = MainWindowUtilities.Backlog.Count - 1;
                    }
                }
                catch (Exception e)
                {
                    Utils.Logger.Warning(e, "CopyFromClipboard failed");
                }
            }
        }

        private void ClipboardChanged(object sender, EventArgs e)
        {
            CopyFromClipboard();
        }

        public void MainTextBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (ConfigManager.LookupOnSelectOnly || Background.Opacity == 0 || MainTextboxContextMenu.IsVisible) return;
            FirstPopupWindow.TextBox_MouseMove(MainTextBox);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MainTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                string allBacklogText = string.Join("\n", MainWindowUtilities.Backlog);
                if (MainTextBox.Text != allBacklogText)
                {
                    if (MainTextBox.GetFirstVisibleLineIndex() == 0)
                    {
                        int caretIndex = allBacklogText.Length - MainTextBox.Text.Length;

                        MainTextBox.Text =
                            "Characters: " + new StringInfo(string.Join("", MainWindowUtilities.Backlog)).LengthInTextElements + " / "
                            + "Lines: " + MainWindowUtilities.Backlog.Count + "\n"
                            + allBacklogText;
                        MainTextBox.Foreground = ConfigManager.MainWindowBacklogTextColor;

                        if (caretIndex >= 0)
                            MainTextBox.CaretIndex = caretIndex;

                        MainTextBox.ScrollToEnd();
                    }
                }
            }
        }

        private void MinimizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpacitySlider.Visibility = Visibility.Collapsed;
            FontSizeSlider.Visibility = Visibility.Collapsed;
            WindowState = WindowState.Minimized;
        }

        private void MinimizeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            MinimizeButton.Foreground = new SolidColorBrush(Colors.SteelBlue);
        }

        private void MainTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            //OpacitySlider.Visibility = Visibility.Collapsed;
            //FontSizeSlider.Visibility = Visibility.Collapsed;

            if (FirstPopupWindow.MiningMode || ConfigManager.LookupOnSelectOnly) return;

            FirstPopupWindow.Hide();
            FirstPopupWindow.LastText = "";
        }

        private void MinimizeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            MinimizeButton.Foreground = new SolidColorBrush(Colors.White);
        }

        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            CloseButton.Foreground = new SolidColorBrush(Colors.SteelBlue);
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseButton.Foreground = new SolidColorBrush(Colors.White);
        }

        private void OpacityButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FontSizeSlider.Visibility = Visibility.Collapsed;

            if (Background.Opacity == 0)
                Background.Opacity = OpacitySlider.Value / 100;

            else if (OpacitySlider.Visibility == Visibility.Collapsed)
            {
                OpacitySlider.Visibility = Visibility.Visible;
                OpacitySlider.Focus();
            }

            else
                OpacitySlider.Visibility = Visibility.Collapsed;
        }

        private void FontSizeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpacitySlider.Visibility = Visibility.Collapsed;

            if (FontSizeSlider.Visibility == Visibility.Collapsed)
            {
                FontSizeSlider.Visibility = Visibility.Visible;
                FontSizeSlider.Focus();
            }

            else
                FontSizeSlider.Visibility = Visibility.Collapsed;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.SaveBeforeClosing();
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Background.Opacity = OpacitySlider.Value / 100;
        }

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainTextBox.FontSize = FontSizeSlider.Value;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Utils.KeyGestureComparer(e, ConfigManager.ShowPreferencesWindowKeyGesture))
            {
                Utils.ShowPreferencesWindow();
            }

            else if (Utils.KeyGestureComparer(e, ConfigManager.MousePassThroughModeKeyGesture))
            {
                Background.Opacity = 0;
                Keyboard.ClearFocus();
            }

            else if (Utils.KeyGestureComparer(e, ConfigManager.KanjiModeKeyGesture))
            {
                // fixes double toggling KanjiMode
                e.Handled = true;

                ConfigManager.KanjiMode = !ConfigManager.KanjiMode;
                FirstPopupWindow.LastText = "";
                MainTextBox_MouseMove(null, null);
            }

            else if (Utils.KeyGestureComparer(e, ConfigManager.ShowAddNameWindowKeyGesture))
            {
                if (Storage.Ready)
                    Utils.ShowAddNameWindow(MainTextBox.SelectedText);
            }

            else if (Utils.KeyGestureComparer(e, ConfigManager.ShowAddWordWindowKeyGesture))
            {
                if (Storage.Ready)
                    Utils.ShowAddWordWindow(MainTextBox.SelectedText);
            }

            else if (Utils.KeyGestureComparer(e, ConfigManager.ShowManageDictionariesWindowKeyGesture))
            {
                if (Storage.Ready
                    && !Storage.UpdatingJMdict
                    && !Storage.UpdatingJMnedict
                    && !Storage.UpdatingKanjidic)
                {
                    Utils.ShowManageDictionariesWindow();
                }
            }

            else if (Utils.KeyGestureComparer(e, ConfigManager.SearchWithBrowserKeyGesture))
            {
                Utils.SearchWithBrowser(MainTextBox.SelectedText);
            }

            else if (Utils.KeyGestureComparer(e, ConfigManager.InactiveLookupModeKeyGesture))
            {
                ConfigManager.InactiveLookupMode = !ConfigManager.InactiveLookupMode;
            }

            else if (Utils.KeyGestureComparer(e, ConfigManager.MotivationKeyGesture))
            {
                Utils.Motivate("Resources/Motivation");
            }

            else if (Utils.KeyGestureComparer(e, ConfigManager.ClosePopupKeyGesture))
            {
                FirstPopupWindow.MiningMode = false;
                FirstPopupWindow.TextBlockMiningModeReminder.Visibility = Visibility.Collapsed;

                FirstPopupWindow.PopUpScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                FirstPopupWindow.Hide();
            }
        }

        private void AddName(object sender, RoutedEventArgs e)
        {
            Utils.ShowAddNameWindow(MainTextBox.SelectedText);
        }

        private void AddWord(object sender, RoutedEventArgs e)
        {
            Utils.ShowAddWordWindow(MainTextBox.SelectedText);
        }

        private void ShowPreferences(object sender, RoutedEventArgs e)
        {
            Utils.ShowPreferencesWindow();
        }

        private void SearchWithBrowser(object sender, RoutedEventArgs e)
        {
            Utils.SearchWithBrowser(MainTextBox.SelectedText);
        }

        private void ShowManageDictionariesWindow(object sender, RoutedEventArgs e)
        {
            Utils.ShowManageDictionariesWindow();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            SteppedBacklog(e);
        }

        private void SteppedBacklog(KeyEventArgs e)
        {
            if (Utils.KeyGestureComparer(e, ConfigManager.SteppedBacklogBackwardsKeyGesture))
            {
                if (_currentTextIndex != 0)
                {
                    _currentTextIndex--;
                    MainTextBox.Foreground = ConfigManager.MainWindowBacklogTextColor;
                }

                MainTextBox.Text = MainWindowUtilities.Backlog[_currentTextIndex];
            }
            else if (Utils.KeyGestureComparer(e, ConfigManager.SteppedBacklogForwardsKeyGesture))
            {
                if (_currentTextIndex < MainWindowUtilities.Backlog.Count - 1)
                {
                    _currentTextIndex++;
                    MainTextBox.Foreground = ConfigManager.MainWindowBacklogTextColor;
                }

                if (_currentTextIndex == MainWindowUtilities.Backlog.Count - 1)
                {
                    MainTextBox.Foreground = ConfigManager.MainWindowTextColor;
                }

                MainTextBox.Text = MainWindowUtilities.Backlog[_currentTextIndex];
            }
        }

        private void OpacitySlider_LostMouseCapture(object sender, MouseEventArgs e)
        {
            OpacitySlider.Visibility = Visibility.Collapsed;
        }

        private void OpacitySlider_LostFocus(object sender, RoutedEventArgs e)
        {
            OpacitySlider.Visibility = Visibility.Collapsed;
        }

        private void FontSizeSlider_LostMouseCapture(object sender, MouseEventArgs e)
        {
            FontSizeSlider.Visibility = Visibility.Collapsed;
        }

        private void FontSizeSlider_LostFocus(object sender, RoutedEventArgs e)
        {
            FontSizeSlider.Visibility = Visibility.Collapsed;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConfigManager.MainWindowHeight = Height;
            ConfigManager.MainWindowWidth = Width;
        }

        private void MainTextBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ManageDictionariesButton.IsEnabled = Storage.Ready
                                                 && !Storage.UpdatingJMdict
                                                 && !Storage.UpdatingJMnedict
                                                 && !Storage.UpdatingKanjidic;

            AddNameButton.IsEnabled = Storage.Ready;
            AddWordButton.IsEnabled = Storage.Ready;
        }

        private void MainTextBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!ConfigManager.LookupOnSelectOnly
                || Background.Opacity == 0
                || ConfigManager.InactiveLookupMode) return;

            //if (ConfigManager.RequireLookupKeyPress
            //    && !Keyboard.Modifiers.HasFlag(ConfigManager.LookupKey))
            //    return;

            FirstPopupWindow.LookupOnSelect(MainTextBox);
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (PopupWindow popupWindow in Application.Current.Windows.OfType<PopupWindow>().ToList())
            {
                popupWindow.MiningMode = false;
                popupWindow.TextBlockMiningModeReminder.Visibility = Visibility.Collapsed;

                popupWindow.Hide();
            }
        }

        private void Window_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            Utils.Dpi = e.NewDpi;
            Utils.ActiveScreen = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(this).Handle);
            Utils.WorkAreaWidth = Utils.ActiveScreen.Bounds.Width / e.NewDpi.DpiScaleX;
            Utils.WorkAreaHeight = Utils.ActiveScreen.Bounds.Height / e.NewDpi.DpiScaleY;
            Utils.DpiAwareXOffset = ConfigManager.PopupXOffset / e.NewDpi.DpiScaleX;
            Utils.DpiAwareYOffset = ConfigManager.PopupYOffset / e.NewDpi.DpiScaleY;
        }

        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ConfigManager.LookupOnSelectOnly)
            {
                double verticalOffset = MainTextBox.VerticalOffset;
                MainTextBox.Select(0, 0);
                MainTextBox.ScrollToVerticalOffset(verticalOffset);
            }
        }
    }
}
