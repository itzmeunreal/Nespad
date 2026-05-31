using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nespad {
    public class SkinForm : Form {
        private readonly MainForm mainform;
        private readonly ListBox skinlist;
        private readonly Panel preview;

        public SkinForm(MainForm parent) {
            mainform = parent;
            this.Text = "Skins";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(220, 220);
            this.Font = new Font("Segoe UI", 9f);
            var leftpanel = new Panel {
                Left = 8,
                Top = 8,
                Width = 120, 
                Height = MainForm.SKINS.Length * 22 + 2,
                BorderStyle = BorderStyle.FixedSingle
            };
            skinlist = new ListBox {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                ItemHeight = 22
            };
            foreach (var sk in MainForm.SKINS) skinlist.Items.Add(sk.name);
            skinlist.SelectedIndex = parent.activeskin;
            skinlist.SelectedIndexChanged += OnSelect;
            skinlist.DrawMode = DrawMode.OwnerDrawFixed;
            skinlist.DrawItem += DrawSkinItem;
            leftpanel.Controls.Add(skinlist);
            preview = new Panel {
                Left = 140, 
                Top = 8,
                Width = 64, 
                Height = 64,
                BorderStyle = BorderStyle.FixedSingle
            };
            var okbtn = new Button {
                Text = "OK", 
                DialogResult = 
                DialogResult.OK,
                Width = 70, 
                Height = 26,
                Left = 8, 
                Top = this.ClientSize.Height - 34
            };
            var cancelbtn = new Button {
                Text = "Cancel", 
                DialogResult = DialogResult.Cancel,
                Width = 70, 
                Height = 26,
                Left = 84, 
                Top = this.ClientSize.Height - 34
            };
            this.AcceptButton = okbtn;
            this.CancelButton = cancelbtn;
            this.Controls.AddRange(new Control[] {leftpanel, preview, okbtn, cancelbtn});
            UpdatePreview(parent.activeskin);
        }

        private void DrawSkinItem(object sender, DrawItemEventArgs e) {
            if (e.Index < 0) return;
            e.DrawBackground();
            var sk = MainForm.SKINS[e.Index];
            var dotrect = new Rectangle(e.Bounds.Left + 4, e.Bounds.Top + 4, 14, 14);
            using (var brush = new SolidBrush(ColorTranslator.FromHtml(sk.btn)))
                e.Graphics.FillEllipse(brush, dotrect);
            using (var brush = new SolidBrush(e.ForeColor))
                e.Graphics.DrawString(sk.name, e.Font, brush, e.Bounds.Left + 22, e.Bounds.Top + 3);
            e.DrawFocusRectangle();
        }

        private void OnSelect(object s, EventArgs e) {
            int i = skinlist.SelectedIndex;
            if (i < 0) return;
            UpdatePreview(i);
            Apply(i);
        }
        private void UpdatePreview(int i) {
            preview.BackColor = ColorTranslator.FromHtml(MainForm.SKINS[i].btn);
        }
        private void Apply(int i) {
            mainform.activeskin = i;
            var sk = MainForm.SKINS[i];
            mainform.webView.CoreWebView2?.ExecuteScriptAsync($"applySkin('{sk.btn}', '{sk.pressed}')");
        }
        protected override void OnFormClosing(FormClosingEventArgs e) {
            base.OnFormClosing(e);
            if (this.DialogResult != DialogResult.OK)
                Apply(mainform.activeskin);
        }
    }
}