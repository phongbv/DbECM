using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbECM
{
    public partial class Form1 : Form
    {
        OraAccess ora = new OraAccess();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openFileDialog1.FileName;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtFilePath.Text = string.Empty;
            txtEcmId.Text = string.Empty;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtEcmId.Text))
            {
                MessageBox.Show(this, "Ecm Id is required.");

            }
            else if (string.IsNullOrEmpty(txtFilePath.Text))
            {
                MessageBox.Show("File Path is required.");
            }
            else if (!File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("File does not exist.");
            }
            else
            {
                try
                {
                    ora.Update(txtEcmId.Text, File.ReadAllBytes(txtFilePath.Text));
                    MessageBox.Show("Update successfull.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtEcmId.Text))
            {
                MessageBox.Show(this, "Ecm Id is required.");

            }
            else
            {
                var fileInfo = ora.Download(txtEcmId.Text);
                if (fileInfo == null)
                {
                    MessageBox.Show(this, "Cannot file template file content.");
                }
                else
                {
                    saveFileDialog1.FileName = fileInfo.FileName;
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllBytes(saveFileDialog1.FileName, fileInfo.FileContent);
                    }
                }
                
            }
        }
    }
}
