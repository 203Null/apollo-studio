using System.Collections.Generic;
using System.Linq;

using Apollo.Components;
using Apollo.Core;
using Apollo.Devices;

namespace Apollo.Structures {
    public class Frame: ISelect {
        public Color[] Screen;

        public ISelectViewer IInfo {
            get => Info;
        }

        public ISelectParent IParent {
            get => Parent;
        }

        public int? IParentIndex {
            get => ParentIndex;
        }

        public FrameDisplay Info;
        public Pattern Parent;
        public int? ParentIndex;

        public bool Mode; // true uses Length
        public Length Length;
        private int _time;

        public int Time {
            get => _time;
            set {
                if (10 <= value && value <= 30000)
                    _time = value;
            }
        }

        public string TimeString => Mode? Length.ToString() : $"{Time}ms";

        public Frame Clone() => new Frame(Mode, Length.Clone(), Time, (from i in Screen select i.Clone()).ToArray());

        public Frame(bool mode = false, Length length = null, int time = 1000, Color[] screen = null) {
            if (screen == null || screen.Length != 100) {
                screen = new Color[100];
                for (int i = 0; i < 100; i++) screen[i] = new Color(0);
            }

            Mode = mode;
            Time = time;
            Length = length?? new Length();
            Screen = screen;
        }

        public static bool Move(List<Frame> source, Frame target, bool copy = false) {
            if (source[0].Parent.Count == source.Count) return false;

            if (!copy)
                for (int i = 0; i < source.Count; i++)
                    if (source[i] == target) return false;
            
            List<Frame> moved = new List<Frame>();

            for (int i = 0; i < source.Count; i++) {
                if (!copy) {
                    source[i].Parent.Window.Contents_Remove(source[i].ParentIndex.Value);
                    source[i].Parent.Remove(source[i].ParentIndex.Value);
                }

                moved.Add(copy? source[i].Clone() : source[i]);

                target.Parent.Window.Contents_Insert(target.ParentIndex.Value + i + 1, moved.Last());
                target.Parent.Insert(target.ParentIndex.Value + i + 1, moved.Last());
            }

            target.Parent.Window.Selection.Select(moved.First());
            target.Parent.Window.Selection.Select(moved.Last(), true);

            target.Parent.Window.Frame_Select(moved.Last().ParentIndex.Value);
            
            return true;
        }

        public static bool Move(List<Frame> source, Pattern target, bool copy = false) {
            if (source[0].Parent.Count == source.Count) return false;

            if (!copy)
                if (target.Count > 0 && source[0] == target[0]) return false;
            
            List<Frame> moved = new List<Frame>();

            for (int i = 0; i < source.Count; i++) {
                if (!copy) {
                    source[i].Parent.Window.Contents_Remove(source[i].ParentIndex.Value);
                    source[i].Parent.Remove(source[i].ParentIndex.Value);
                }

                moved.Add(copy? source[i].Clone() : source[i]);

                target.Window.Contents_Insert(i, moved.Last());
                target.Insert(i, moved.Last());
            }

            target.Window.Selection.Select(moved.First());
            target.Window.Selection.Select(moved.Last(), true);

            target.Window.Frame_Select(moved.Last().ParentIndex.Value);
            
            return true;
        }
    }
}