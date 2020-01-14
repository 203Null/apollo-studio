using Avalonia.Threading;

using RtMidi.Core.Devices.Infos;

using Apollo.Core;
using Apollo.Enums;
using Apollo.Helpers;
using Apollo.Structures;
using Apollo.Windows;

namespace Apollo.Elements {
    public class AbletonLaunchpad: Launchpad {
        public int Version = 0;

        Launchpad _target = null;
        public Launchpad Target {
            get => _target;
            set {
                if (_target != value) {
                    _target?.AbletonLaunchpads.Remove(this);

                    _target = value;

                    _target?.AbletonLaunchpads.Add(this);
                }
            }
        }

        public override PatternWindow PatternWindow {
            get => Target?.PatternWindow;
            set {
                if (Target != null) Target.PatternWindow = value;
            }
        }

        public override RotationType Rotation {
            get => Target.Rotation;
            set {}
        }

        public override void Send(Signal n) => Target?.Send(n);

        public override void Clear(bool manual = false) {
            if (!Available || (manual && PatternWindow != null)) return;
            
            Target?.Clear(manual);

            Signal n = new Signal(this, this, 0, new Color(0));

            for (int i = 0; i < 100; i++) {
                n.Index = (byte)i;
                Window?.SignalRender(n);
            }

            if (Version >= 1)
                AbletonConnector.SendClear(this);
        }

        public override void Render(Signal n) => Target?.Render(n);

        public AbletonLaunchpad(string name) {
            Type = LaunchpadType.Pro;
            Name = name;

            Target = MIDI.NoOutput;
        }

        public override void Connect(IMidiInputDeviceInfo input = null, IMidiOutputDeviceInfo output = null) {
            Available = true;

            Program.Log($"MIDI Created {Name}");
        }
        
        public override void Disconnect(bool actuallyClose = true) {
            Program.Log($"MIDI Disconnected {Name}");

            Dispatcher.UIThread.InvokeAsync(() => Window?.Close());

            Available = false;
        }
    }
}