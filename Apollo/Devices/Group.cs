using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using Newtonsoft.Json;

using Apollo.Core;
using Apollo.Elements;
using Apollo.Structures;

namespace Apollo.Devices {
    public class Group: Device, IChainParent {
        public static readonly new string DeviceIdentifier = "group";

        private Action<Signal> _midiexit;
        public override Action<Signal> MIDIExit {
            get => _midiexit;
            set {
                _midiexit = value;
                Reroute();
            }
        }

        private List<Chain> _chains = new List<Chain>();

        private void Reroute() {
            for (int i = 0; i < _chains.Count; i++) {
                _chains[i].Parent = this;
                _chains[i].ParentIndex = i;
                _chains[i].MIDIExit = ChainExit;
            }
        }

        public Chain this[int index] {
            get => _chains[index];
        }

        public int Count {
            get => _chains.Count;
        }

        public override Device Clone() => new Group((from i in _chains select i.Clone()).ToList(), Expanded);

        public void Insert(int index, Chain chain = null) {
            _chains.Insert(index, chain?? new Chain());
            
            Reroute();
        }

        public void Add(Chain chain) {
            _chains.Add(chain);

            Reroute();
        }

        public void Remove(int index) {
            _chains[index].Dispose();
            _chains.RemoveAt(index);

            Reroute();
        }

        public int? Expanded;

        public Group(List<Chain> init = null, int? expanded = null): base(DeviceIdentifier) {
            foreach (Chain chain in init?? new List<Chain>()) _chains.Add(chain);
            Expanded = expanded;

            Reroute();
        }

        private void ChainExit(Signal n) => MIDIExit?.Invoke(n);

        public override void MIDIEnter(Signal n) {
            if (_chains.Count == 0) ChainExit(n);

            foreach (Chain chain in _chains)
                chain.MIDIEnter(n.Clone());
        }

        public override void Dispose() {
            foreach (Chain chain in _chains) chain.Dispose();
            base.Dispose();
        }

        public static Device DecodeSpecific(string jsonString) {
            Dictionary<string, object> json = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            if (json["device"].ToString() != DeviceIdentifier) return null;

            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json["data"].ToString());
            
            List<object> chains = JsonConvert.DeserializeObject<List<object>>(data["chains"].ToString());
            List<Chain> init = new List<Chain>();

            foreach (object chain in chains)
                init.Add(Chain.Decode(chain.ToString()));
            
            return new Group(
                init,
                int.TryParse(data["expanded"].ToString(), out int i)? (int?)i : null
            );
        }

        public override string EncodeSpecific() {
            StringBuilder json = new StringBuilder();

            using (JsonWriter writer = new JsonTextWriter(new StringWriter(json))) {
                writer.WriteStartObject();

                    writer.WritePropertyName("device");
                    writer.WriteValue(DeviceIdentifier);

                    writer.WritePropertyName("data");
                    writer.WriteStartObject();

                        writer.WritePropertyName("chains");
                        writer.WriteStartArray();

                            for (int i = 0; i < _chains.Count; i++)
                                writer.WriteRawValue(_chains[i].Encode());
                        
                        writer.WriteEndArray();

                        writer.WritePropertyName("expanded");
                        writer.WriteValue(Expanded);
                
                    writer.WriteEndObject();

                writer.WriteEndObject();
            }
            
            return json.ToString();
        }
    }
}