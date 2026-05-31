using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nespad {
    public class LicenseForm : Form {
        public LicenseForm() {
            this.Text = "License";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(480, 360);
            this.Font = new Font("Segoe UI", 9f);
            var header = new Label {
                Text = "NES Controller SVG by Fant0men",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Width = 460, 
                Height = 20,
                Left = 10, 
                Top = 10
            };
            var box = new RichTextBox {
                Left = 10, 
                Top = 36,
                Width = 460, 
                Height = 270,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Font = new Font("Segoe UI", 8.5f),
                Text =
"The NES controller SVG used in this application was created by Fant0men " +
"and is available on Wikimedia Commons.\r\n\r\n" +
"It is published under the following licenses (you may select the license of your choice):\r\n\r\n" +
"GNU Free Documentation License\r\n" +
"Permission is granted to copy, distribute and/or modify this document under the terms of the " +
"GNU Free Documentation License, Version 1.2 or any later version published by the Free Software " +
"Foundation; with no Invariant Sections, no Front-Cover Texts, and no Back-Cover Texts.\r\n\r\n" +
"Creative Commons Attribution-Share Alike 3.0 Unported\r\n" +
"This file is licensed under the Creative Commons Attribution-Share Alike 3.0 Unported license.\r\n\r\n" +
"Attribution: I, Fant0men\r\n\r\n" +
"You are free:\r\n" +
"  • To share - to copy, distribute and transmit the work\r\n" +
"  • To remix - to adapt the work\r\n\r\n" +
"Under the following conditions:\r\n" +
"  • Attribution: You must give appropriate credit, provide a link to the license, and indicate " +
"if changes were made. You may do so in any reasonable manner, but not in any way that suggests " +
"the licensor endorses you or your use.\r\n" +
"  • Share alike: If you remix, transform, or build upon the material, you must distribute your " +
"contributions under the same or compatible license as the original.\r\n\r\n" +
"This licensing tag was added to this file as part of the GFDL licensing update."
            };
            var okbtn = new Button {
                Text = "OK",
                Width = 80,
                Height = 26,
                Left = 200, 
                Top = 318,
                DialogResult = DialogResult.OK
            };
            this.AcceptButton = okbtn;
            this.Controls.AddRange(new Control[] {header, box, okbtn});
        }
    }
}
