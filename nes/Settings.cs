using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Nespad {
    [DataContract]
    public class InputSlot {
        [DataMember] public List<KeyEntry> keys = new List<KeyEntry>();
        [DataMember] public int skin = 0;

        [DataContract]
        public class KeyEntry {
            [DataMember] public uint vk;
            [DataMember] public string id;
            [DataMember] public string display;
        }
    }

    public static class Settings {
        private static string appdir = AppDomain.CurrentDomain.BaseDirectory;
        private static string lastfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "last.json");

        private static string slotfile(int i) => Path.Combine(appdir, $"slot{i}.json");

        public static void SaveLast(MainForm f) => Save(lastfile, f);
        public static bool LoadLast(MainForm f) => Load(lastfile, f);
        public static void SaveSlot(int i, MainForm f) => Save(slotfile(i), f);
        public static bool LoadSlot(int i, MainForm f) => Load(slotfile(i), f);
        public static bool SlotExists(int i) => File.Exists(slotfile(i));

        private static void Save(string path, MainForm f) {
            var slot = new InputSlot {skin = f.activeskin};
            foreach (var kv in f.keymap) {
                string display;
                f.displaymap.TryGetValue(kv.Key, out display);
                slot.keys.Add(new InputSlot.KeyEntry {vk = kv.Key, id = kv.Value, display = display ?? ""});
            }
            var ser = new DataContractJsonSerializer(typeof(InputSlot));
            using (var ms = new MemoryStream()) {
                ser.WriteObject(ms, slot);
                File.WriteAllText(path, Encoding.UTF8.GetString(ms.ToArray()));
            }
        }

        private static bool Load(string path, MainForm f) {
            if (!File.Exists(path)) return false;
            try {
                var ser = new DataContractJsonSerializer(typeof(InputSlot));
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(path)))) {
                    var slot = (InputSlot)ser.ReadObject(ms);
                    f.keymap.Clear();
                    f.displaymap.Clear();
                    foreach (var e in slot.keys) {
                        f.keymap[e.vk] = e.id;
                        f.displaymap[e.vk] = e.display;
                    }
                    f.activeskin = slot.skin;
                    return true;
                }
            } catch { return false; }
        }
    }
}
