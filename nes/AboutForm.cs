using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Nespad {
    public class AboutForm : Form {
        public AboutForm() {
            this.Text = "About Nespad";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(260, 240);
            this.Font = new Font("Segoe UI", 9f);
            try { this.Icon = new System.Drawing.Icon("icon.ico"); } catch {}
            var icon = new PictureBox {
                Width = 48,
                Height = 48,
                Left = 106,
                Top = 14,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            try { icon.Image = Image.FromFile("icon.png"); } catch {}
            var name = new Label {
                Text = "Nespad v1.0",
                Font = new Font("Segoe UI", 9f),
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 260,
                Height = 20,
                Top = 68
            };
            var copyright = new Label {
                Text = "Copyright © 2026 unreal!",
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 260,
                Height = 20,
                Top = 90
            };
            var homeplacelbl = new Label {
                Text = "Homepage",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = SystemColors.GrayText,
                Width = 260,
                Height = 18,
                Top = 118
            };
            var homelink = new LinkLabel {
                Text = "itzmeunreal.neocities.org/nespad",
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 260,
                Height = 18,
                Top = 134
            };
            homelink.LinkClicked += (s, e) =>
                Process.Start(new ProcessStartInfo("https://itzmeunreal.neocities.org/nespad") {UseShellExecute = true});
            var contactlbl = new Label {
                Text = "Contact",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = SystemColors.GrayText,
                Width = 260,
                Height = 18,
                Top = 160
            };
            var contactlink = new LinkLabel {
                Text = "info.unreal@proton.me",
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 260,
                Height = 18,
                Top = 176
            };
            contactlink.LinkClicked += (s, e) =>
                Process.Start(new ProcessStartInfo("mailto:info.unreal@proton.me") {UseShellExecute = true});
            var okbtn = new Button {
                Text = "OK",
                Width = 80,
                Height = 26,
                Left = 90,
                Top = 204,
                DialogResult = DialogResult.OK
            };
            this.AcceptButton = okbtn;
            this.Controls.AddRange(new Control[] {
                icon,
                name,
                copyright,
                homeplacelbl,
                homelink,
                contactlbl,
                contactlink,
                okbtn
            });
        }
    }
}
