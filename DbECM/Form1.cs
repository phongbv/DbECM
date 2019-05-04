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
            txtLOSID.Text = string.Empty;
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

        private void Form1_Load(object sender, EventArgs e)
        {
            txtLOSID.ReadOnly = true;
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            decimal losId = 0;

            if (string.IsNullOrEmpty(txtLOSID.Text) && !cbAutoGenLOSID.Checked)
            {
                MessageBox.Show(this, "Los Id is required.");

            }
            else if (string.IsNullOrEmpty(txtFilePath.Text))
            {
                MessageBox.Show("File Path is required.");
            }
            else if (!File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("File does not exist.");
            }
            else if (!decimal.TryParse(txtLOSID.Text, out losId) && !cbAutoGenLOSID.Checked)
            {
                MessageBox.Show("Los Id must be a number.");
            }
            else
            {
                try
                {
                    string fileName = System.IO.Path.GetFileName(txtFilePath.Text);
                    string mimeType = MimeTypes.MimeTypeMap.GetMimeType(System.IO.Path.GetExtension(txtFilePath.Text));
                    System.IO.FileInfo f = new System.IO.FileInfo(txtFilePath.Text);
                    if (cbAutoGenLOSID.Checked)
                    {
                        losId = ora.CreateFileInfo(fileName, Path.GetExtension(txtFilePath.Text), f.Length / 1024);
                    }
                    var ecmId = ora.Upload(losId, fileName, string.Empty, File.ReadAllBytes(txtFilePath.Text), mimeType);
                    ora.UpdateFileInfo(losId, ecmId);
                    MessageBox.Show("Update successfull.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void cbAutoGenLOSID_CheckedChanged(object sender, EventArgs e)
        {
            if (cbAutoGenLOSID.Checked)
            {
                txtLOSID.Text = string.Empty;
            }
            txtLOSID.ReadOnly = cbAutoGenLOSID.Checked;

        }
    }
}
