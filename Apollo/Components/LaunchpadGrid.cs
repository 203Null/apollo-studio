﻿using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Input;

namespace Apollo.Components {
    public class LaunchpadGrid: UserControl {
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
        
        UniformGrid Grid;
        Shape ModeLight;

        public delegate void PadClickedEventHandler(int index);
        public event PadClickedEventHandler PadClicked;

        public static int GridToSignal(int index) => (index == -1)? 99 : ((9 - (index / 10)) * 10 + index % 10);
        public static int SignalToGrid(int index) => (index == 99)? -1 : ((9 - (index / 10)) * 10 + index % 10);

        public void SetColor(int index, SolidColorBrush color) {
            if (index == 0 || index == 9 || index == 90 || index == 99) return;

            if (index == -1) ModeLight.Fill = color;
            else ((Shape)Grid.Children[index]).Fill = color;
        }

        public LaunchpadGrid() {
            InitializeComponent();

            Grid = this.Get<UniformGrid>("LaunchpadGrid");
            ModeLight = this.Get<Rectangle>("ModeLight");
        }

        private void Clicked(object sender, PointerReleasedEventArgs e) => PadClicked?.Invoke(Grid.Children.IndexOf((IControl)sender));
    }
}
