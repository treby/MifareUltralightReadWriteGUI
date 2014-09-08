using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FeliCaNfcLibrary;

namespace MifareUltralightReadWriteGUI
{
    public partial class Form1 : Form
    {
        FeliCaNfc nfc;
        TextBox[] cells = null;
        Label[] labels = null;

        const int ARRAY_SIZE = 72;

        public Form1()
        {
            InitializeComponent();
            
            myInitialize();
            nfc = new FeliCaNfc();
            nfc.NfcReadCompleted += new NfcReadCompletedEventHandler(nfc_NfcReadCompleted);
            nfc.NfcWriteCompleted += new AsyncCompletedEventHandler(nfc_NfcWriteCompleted);

            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);
        }

        private void myInitialize()
        {
            int defX = 10;
            int defY = 20;

            cells = new TextBox[ARRAY_SIZE];
            labels = new Label[ARRAY_SIZE];

            for (int i = 0; i < ARRAY_SIZE; i++)
            {
                labels[i] = new Label();
                labels[i].Name = "l" + i.ToString();
                labels[i].AutoSize = true;
                labels[i].Size = new Size(17, 12);
                labels[i].Text = "0x";
                labels[i].Location = new Point(defX + (i % 4) * 70, defY + (i / 4) * 25);

                cells[i] = new TextBox();
                cells[i].Name = "t" + i.ToString();
                cells[i].Size = new Size(30, 19);
                cells[i].Text = "XX";
                cells[i].Location = new Point(defX + (i % 4) * 70 + 20, defY + (i / 4) * 25 - 3);
                cells[i].TextAlign = HorizontalAlignment.Center;
                cells[i].MaxLength = 2;
                if (i < 16) cells[i].ReadOnly = true;
                else cells[i].Text = "00";

                this.groupBoxData.Controls.Add(labels[i]);
                this.groupBoxData.Controls.Add(cells[i]);
            }
        }

        private void buttonRead_Click(object sender, EventArgs e)
        {
            nfc.NfcReadAsync();
            label39.Text = "読み込み待機中じゃ";
        }
        private void nfc_NfcReadCompleted(object sender, NfcReadCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                label39.Text = "例外じゃ(" + e.Error.Message + ")";
                return;
            }

            for (int i = 0; i < e.ReadBytes.Length; i++)
            {
                cells[i].Text = string.Format("{0:X2}", e.ReadBytes[i]);
            }

            label39.Text = "ぬしよ、読み込みが終わったようじゃ";
        }

        private void buttonWrite_Click(object sender, EventArgs e)
        {
            List<byte> dataList = new List<byte>();

            Int32 bit = 0;
            if (checkBox1.Checked)
            {
                bit += 0x0001;
                dataList.Add((byte)Convert.ToInt16(cells[16].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[17].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[18].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[19].Text, 16));
            }
            if (checkBox2.Checked)
            {
                bit += 0x0002;
                dataList.Add((byte)Convert.ToInt16(cells[20].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[21].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[22].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[23].Text, 16));
            }
            if (checkBox3.Checked)
            {
                bit += 0x0004;
                dataList.Add((byte)Convert.ToInt16(cells[24].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[25].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[26].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[27].Text, 16));
            }
            if (checkBox4.Checked)
            {
                bit += 0x0008;
                dataList.Add((byte)Convert.ToInt16(cells[28].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[29].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[30].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[31].Text, 16));
            }
            if (checkBox5.Checked)
            {
                bit += 0x0010;
                dataList.Add((byte)Convert.ToInt16(cells[32].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[33].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[34].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[35].Text, 16));
            }
            if (checkBox6.Checked)
            {
                bit += 0x0020;
                dataList.Add((byte)Convert.ToInt16(cells[36].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[37].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[38].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[39].Text, 16));
            }
            if (checkBox7.Checked)
            {
                bit += 0x0040;
                dataList.Add((byte)Convert.ToInt16(cells[40].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[41].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[42].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[43].Text, 16));
            }
            if (checkBox8.Checked)
            {
                bit += 0x0080;
                dataList.Add((byte)Convert.ToInt16(cells[44].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[45].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[46].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[47].Text, 16));
            }
            if (checkBox9.Checked)
            {
                bit += 0x0100;
                dataList.Add((byte)Convert.ToInt16(cells[48].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[49].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[50].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[51].Text, 16));
            }
            if (checkBox10.Checked)
            {
                bit += 0x0200;
                dataList.Add((byte)Convert.ToInt16(cells[52].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[53].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[54].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[55].Text, 16));
            }
            if (checkBox11.Checked)
            {
                bit += 0x0400;
                dataList.Add((byte)Convert.ToInt16(cells[56].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[57].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[58].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[59].Text, 16));
            }
            if (checkBox12.Checked)
            {
                bit += 0x0800;
                dataList.Add((byte)Convert.ToInt16(cells[60].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[61].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[62].Text, 16));
                dataList.Add((byte)Convert.ToInt16(cells[63].Text, 16));
            }

            if (bit == 0)
            {
                label39.Text = "書き込むpageにチェックを入れてくりゃれ？";
                return;
            }

            byte[] dataToWrite = dataList.ToArray();
            nfc.NfcWriteAsync(bit, dataToWrite);

            label39.Text = "チェックしたpageに書き込むのかや？";
        }
        private void nfc_NfcWriteCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                label39.Text = "例外じゃ(" + e.Error.Message + ")";
                return;
            }

            label39.Text = "書き込み完了じゃ。Readで確認してくりゃれ？";
        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            nfc.CancelAsync();
        }
    }
}
