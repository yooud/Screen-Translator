﻿using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls.Window;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Screen_Translator.Views.Windows;

public partial class SettingsWindow : FluentWindow
{
    public SettingsWindow(Window owner)
    {
        Watcher.Watch(this);
        InitializeComponent();

        Owner = owner;
        KeyUp += OnKeyUp;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => CenterOnParent();

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) 
            Hide();
    }

    public void CenterOnParent()
    {
        Left = (Owner.Left + Owner.Width / 2) - Width / 2;
        Top = (Owner.Top + Owner.Height / 2) - Height / 2;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) => Hide();
}