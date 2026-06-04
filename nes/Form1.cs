using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace Nespad {
    public partial class MainForm : Form {
        internal WebView2 webView;
        internal bool dbgvisible = false;
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc hookCallback;
        private IntPtr hookId = IntPtr.Zero;

        [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")] static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN    = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        internal Dictionary<uint, string> keymap = InputForm.DefaultMap();
        internal Dictionary<uint, string> displaymap = new Dictionary<uint, string> {
            {0x57, "W"}, {0x53, "S"}, {0x41, "A"}, {0x44, "D"},
            {0x08, "Backspace"}, {0x0D, "Enter"}, {0x25, "Left"}, {0x28, "Down"},
        };
        internal int activeskin = 0;

        internal static readonly (string name, string btn, string pressed)[] SKINS = {
            ("Red", "#FF0000", "#5a0000"),
            ("Blue", "#0055FF", "#002280"),
            ("Green", "#00BB44", "#005020"),
            ("Orange", "#FF7700", "#7a3800"),
            ("Purple", "#9900EE", "#440077"),
            ("Gold", "#DDAA00", "#6b5000"),
        };

        public MainForm() {
            InitializeComponent();
            Settings.LoadLast(this);
            this.Text = "Nespad";
            try { this.Icon = new System.Drawing.Icon("icon.ico"); } catch {}
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            menuStrip1.Dock = DockStyle.Top;
            menuStrip1.Visible = true;
            webView = new WebView2();
            webView.Dock = DockStyle.Fill;
            this.Controls.Add(webView);
            webView.BringToFront();
            this.Load += async (s, e) => {
                await webView.EnsureCoreWebView2Async();
                string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html");
                webView.CoreWebView2.Navigate(new Uri(filepath).AbsoluteUri);
                webView.CoreWebView2.NavigationCompleted += (ns, ne) => {
                    var sk = SKINS[activeskin];
                    webView.CoreWebView2.ExecuteScriptAsync($"applySkin('{sk.btn}', '{sk.pressed}')");
                };
            };
            reloadToolStripMenuItem.Click += (s, e) => {
                string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html");
                webView.CoreWebView2?.Navigate(new Uri(filepath).AbsoluteUri);
            };
            quitToolStripMenuItem.Click += (s, e) => Application.Exit();
            inputToolStripMenuItem.Click += (s, e) => {
                using (var dlg = new InputForm(this))
                    dlg.ShowDialog(this);
            };
            otherToolStripMenuItem.Click += (s, e) => {
                using (var dlg = new SkinForm(this))
                    dlg.ShowDialog(this);
            };
            aboutToolStripMenuItem.Click += (s, e) => {
                using (var dlg = new AboutForm())
                    dlg.ShowDialog(this);
            };
            licenseToolStripMenuItem.Click += (s, e) => {
                using (var dlg = new LicenseForm())
                    dlg.ShowDialog(this);
            };
            WireLayouts();
            hookCallback = HookProc;
            using (var curprocess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curmodule = curprocess.MainModule) {
                hookId = SetWindowsHookEx(WH_KEYBOARD_LL, hookCallback, GetModuleHandle(curmodule.ModuleName), 0);
            }
            this.FormClosing += (s, e) => {
                UnhookWindowsHookEx(hookId);
                Settings.SaveLast(this);
            };
            ResizeClient();
            this.Shown += (s, e) => ResizeClient();
        }

        private void WireLayouts() {
            for (int i = 1; i <= 10; i++) {
                int profile = i;
                var save = new System.Windows.Forms.ToolStripMenuItem($"Layout {profile}");
                save.Click += (s, e) => {
                    Settings.SaveSlot(profile, this);
                    UpdateLayoutMenu();
                };
                savelayoutToolStripMenuItem.DropDownItems.Add(save);
                var load = new System.Windows.Forms.ToolStripMenuItem($"Layout {profile}");
                load.Enabled = Settings.SlotExists(profile);
                load.Click += (s, e) => {
                    if (!Settings.LoadSlot(profile, this)) return;
                    var sk = SKINS[activeskin];
                    webView.CoreWebView2?.ExecuteScriptAsync($"applySkin('{sk.btn}', '{sk.pressed}')");
                };
                loadlayoutToolStripMenuItem.DropDownItems.Add(load);
            }
        }

        private void UpdateLayoutMenu() {
            for (int i = 0; i < 10; i++)
                ((System.Windows.Forms.ToolStripMenuItem)loadlayoutToolStripMenuItem.DropDownItems[i]).Enabled =
                    Settings.SlotExists(i + 1);
        }

        internal void ResizeClient() {
            int width = 612;
            int height = 252;
            this.ClientSize = new System.Drawing.Size(
                width,
                menuStrip1.Visible ? height + menuStrip1.Height : height
            );
        }

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0) {
                uint vk = (uint)Marshal.ReadInt32(lParam);
                if (vk == 0x1B && wParam == (IntPtr)WM_KEYDOWN) {
                    this.Invoke((MethodInvoker)(() => {
                        menuStrip1.Visible = !menuStrip1.Visible;
                        ResizeClient();
                    }));
                    return (IntPtr)1;
                }
                string id;
                if (keymap.TryGetValue(vk, out id)) {
                    bool down = wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN;
                    webView.CoreWebView2?.ExecuteScriptAsync($"receiveBtn('{id}', {(down ? "true" : "false")})");
                }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }
    }
}
