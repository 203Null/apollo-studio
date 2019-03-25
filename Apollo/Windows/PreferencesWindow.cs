using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

using Newtonsoft.Json;

using Apollo.Core;
using Apollo.Elements;
using Apollo.Viewers;

namespace Apollo.Windows {
    public class PreferencesWindow: Window {
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        private CheckBox AlwaysOnTop, CenterTrackContents;

        private void UpdateTopmost(bool value) {
            Topmost = value;
        }

        public PreferencesWindow() {
            InitializeComponent();
            #if DEBUG
                this.AttachDevTools();
            #endif
            
            Icon = new WindowIcon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Apollo.Resources.WindowIcon.png"));
            
            UpdateTopmost(Preferences.AlwaysOnTop);
            Preferences.AlwaysOnTopChanged += UpdateTopmost;

            Preferences.Window = this;

            AlwaysOnTop = this.Get<CheckBox>("AlwaysOnTop");
            AlwaysOnTop.IsChecked = Preferences.AlwaysOnTop;

            CenterTrackContents = this.Get<CheckBox>("CenterTrackContents");
            CenterTrackContents.IsChecked = Preferences.CenterTrackContents;
        }

        private void Unloaded(object sender, EventArgs e) {
            Preferences.Window = null;

            Preferences.AlwaysOnTopChanged -= UpdateTopmost;
        }

        private void MoveWindow(object sender, PointerPressedEventArgs e) {
            BeginMoveDrag();
        }

        private void AlwaysOnTop_Changed(object sender, EventArgs e) {
            Preferences.AlwaysOnTop = AlwaysOnTop.IsChecked.Value;
        }

        private void CenterTrackContents_Changed(object sender, EventArgs e) {
            Preferences.CenterTrackContents = CenterTrackContents.IsChecked.Value;
        }

        public static void Create() {
            if (Preferences.Window == null) {
                Preferences.Window = new PreferencesWindow();
                Preferences.Window.Show();
            } else {
                Preferences.Window.WindowState = WindowState.Normal;
                Preferences.Window.Activate();
            }
        }
    }
}