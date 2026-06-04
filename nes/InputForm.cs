using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Nespad {
    public class InputForm : Form {
        [DllImport("user32.dll")] static extern int GetKeyNameTextW(int lparam, System.Text.StringBuilder buf, int size);
        [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")] static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private static readonly string[] BTN_IDS = {
            "dpad-up", 
            "dpad-down", 
            "dpad-left", 
            "dpad-right",
            "btn-select", 
            "btn-start", 
            "btn-b", 
            "btn-a"
        };
        private static readonly string[] BTN_LABELS = {
            "Up", 
            "Down", 
            "Left", 
            "Right",
            "Select", 
            "Start", 
            "B", 
            "A"
        };

        private readonly MainForm mainform;
        private readonly Dictionary<uint, string> workingmap;
        private readonly Dictionary<uint, string> displaymap;
        private readonly ListBox btnlist;
        private readonly Label keylabel;
        private readonly Button setbtn;
        private readonly Button clearbtn;
        private readonly Button defaultbtn;
        private readonly Button okbtn;
        private readonly Button cancelbtn;
        private readonly CheckBox dbgcheck;
        private LowLevelKeyboardProc hookproc;
        private IntPtr hookid = IntPtr.Zero;
        private bool listening = false;

        public InputForm(MainForm parent) {
            mainform = parent;
            workingmap = new Dictionary<uint, string>(parent.keymap);
            displaymap = new Dictionary<uint, string>(parent.displaymap);
            this.Text = "Input Options";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(360, 320);
            this.Font = new Font("Segoe UI", 9f);
            var leftpanel = new Panel {
                Left = 8, 
                Top = 8,
                Width = 120, 
                Height = 230,
                BorderStyle = BorderStyle.FixedSingle
            };
            btnlist = new ListBox {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                IntegralHeight = false
            };
            btnlist.Items.AddRange(BTN_LABELS);
            btnlist.SelectedIndex = 0;
            btnlist.SelectedIndexChanged += (s, e) => RefreshKey();
            leftpanel.Controls.Add(btnlist);
            var keylbl = new Label {
                Text = "Current key",
                Left = 140, Top = 8,
                Width = 210, Height = 16,
                ForeColor = SystemColors.GrayText
            };
            keylabel = new Label {
                Left = 140, Top = 28,
                Width = 210, Height = 26,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0),
                Font = new Font("Consolas", 9.5f)
            };
            var hint = new Label {
                Text = "Press Set, then press any key to assign it.",
                Left = 140, 
                Top = 62,
                Width = 210, 
                Height = 32,
                ForeColor = SystemColors.GrayText
            };
            setbtn     = new Button {
                Left = 140,
                Top = 100,
                Width = 64,
                Height = 24,
                Text = "Set"
            };
            clearbtn   = new Button {
                Left = 210,
                Top = 100, 
                Width = 64, 
                Height = 24, 
                Text = "Clear"
            };
            defaultbtn = new Button {
                Left = 280, 
                Top = 100, 
                Width = 70, 
                Height = 24, 
                Text = "Default"
            };
            dbgcheck = new CheckBox {
                Left = 140, 
                Top = 136,
                Width = 210, 
                Height = 20,
                Text = "Show input overlay",
                Checked = parent.dbgvisible
            };
            dbgcheck.CheckedChanged += (s, e) => {
                parent.dbgvisible = dbgcheck.Checked;
                string val = dbgcheck.Checked ? "true" : "false";
                parent.webView.CoreWebView2?.ExecuteScriptAsync($"setDbg({val})");
            };
            okbtn = new Button {
                Left = 210, 
                Top = 284, Width = 70, 
                Height = 26,
                Text = "OK", 
                DialogResult = DialogResult.OK
            };
            cancelbtn = new Button {
                Left = 284, 
                Top = 284, 
                Width = 70, 
                Height = 26,
                Text = "Cancel", 
                DialogResult = DialogResult.Cancel
            };
            setbtn.Click += OnSet;
            clearbtn.Click += OnClear;
            defaultbtn.Click += OnDefault;
            okbtn.Click += OnOk;
            this.CancelButton = cancelbtn;
            this.Controls.AddRange(new Control[] {
                leftpanel, 
                keylbl, 
                keylabel, 
                hint,
                setbtn,
                clearbtn, 
                defaultbtn,
                dbgcheck,
                okbtn, 
                cancelbtn
            });
            hookproc = HookProc;
            using (var proc = System.Diagnostics.Process.GetCurrentProcess())
            using (var mod = proc.MainModule)
                hookid = SetWindowsHookEx(WH_KEYBOARD_LL, hookproc, GetModuleHandle(mod.ModuleName), 0);
            this.FormClosed += (s, e) => UnhookWindowsHookEx(hookid);
            RefreshKey();
        }

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && listening) {
                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN) {
                    var kb = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                    uint vk = kb.vkCode;
                    string name;
                    switch ((Keys)vk) {
                        case Keys.Return: name = "Enter"; break;
                        case Keys.Back: name = "Backspace"; break;
                        default: name = ((Keys)vk).ToString(); break;
                    }
                    if (vk == 0x1B)
                        this.BeginInvoke((MethodInvoker)(() => StopListening()));
                    else
                        this.BeginInvoke((MethodInvoker)(() => Assign(vk, name)));
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(hookid, nCode, wParam, lParam);
        }

        private void Assign(uint vk, string name) {
            string id = SelectedId();
            var toremove = new List<uint>();
            foreach (var kv in workingmap)
                if (kv.Key == vk || kv.Value == id) toremove.Add(kv.Key);
            foreach (var k in toremove) { workingmap.Remove(k); displaymap.Remove(k); }
            workingmap[vk] = id;
            displaymap[vk] = name;
            StopListening();
        }

        private string SelectedId() => BTN_IDS[btnlist.SelectedIndex];

        private void RefreshKey() {
            if (listening) StopListening();
            string id = SelectedId();
            uint? found = null;
            foreach (var kv in workingmap)
                if (kv.Value == id) {found = kv.Key; break;}
            if (!found.HasValue) {keylabel.Text = "(none)"; return; }
            string name;
            keylabel.Text = displaymap.TryGetValue(found.Value, out name) ? name : $"VK {found.Value:X2}";
        }

        private void SetListeningControls(bool en) {
            btnlist.Enabled = en;
            clearbtn.Enabled = en;
            defaultbtn.Enabled = en;
            okbtn.Enabled = en;
            cancelbtn.Enabled = en;
        }

        private void OnSet(object s, EventArgs e) {
            if (listening) { StopListening(); return; }
            listening = true;
            SetListeningControls(false);
            setbtn.Text = "Waiting...";
            keylabel.Text = "Press a key...";
            keylabel.ForeColor = SystemColors.GrayText;
        }

        private void StopListening() {
            listening = false;
            SetListeningControls(true);
            setbtn.Text = "Set";
            keylabel.ForeColor = SystemColors.ControlText;
            RefreshKey();
        }

        private void OnClear(object s, EventArgs e) {
            string id = SelectedId();
            var toremove = new List<uint>();
            foreach (var kv in workingmap)
                if (kv.Value == id) toremove.Add(kv.Key);
            foreach (var k in toremove) { workingmap.Remove(k); displaymap.Remove(k); }
            keylabel.Text = "(none)";
        }

        private void OnDefault(object s, EventArgs e) {
            workingmap.Clear();
            displaymap.Clear();
            foreach (var kv in DefaultMap()) {
                workingmap[kv.Key] = kv.Value;
                displaymap[kv.Key] = DefaultName(kv.Key);
            }
            RefreshKey();
        }

        private void OnOk(object s, EventArgs e) {
            mainform.keymap.Clear();
            foreach (var kv in workingmap) mainform.keymap[kv.Key] = kv.Value;
            mainform.displaymap.Clear();
            foreach (var kv in displaymap) mainform.displaymap[kv.Key] = kv.Value;
        }

        internal static Dictionary<uint, string> DefaultMap() => new Dictionary<uint, string> {
            {0x57, "dpad-up"}, {0x53, "dpad-down"},
            {0x41, "dpad-left"}, {0x44, "dpad-right"},
            {0x08, "btn-select"}, {0x0D, "btn-start"},
            {0x25, "btn-b"}, {0x28, "btn-a"},
        };

        private static string DefaultName(uint vk) {
            switch (vk) {
                case 0x57: return "W";
                case 0x53: return "S";
                case 0x41: return "A";
                case 0x44: return "D";
                case 0x08: return "Backspace";
                case 0x0D: return "Enter";
                case 0x25: return "Left";
                case 0x28: return "Down";
                default:   return $"VK {vk:X2}";
            }
        }
    }
}
