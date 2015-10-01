using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneDriveApiBrowser
{
    public partial class FormInputDialog : Form
    {
        public FormInputDialog(string title, string prompt)
        {
            InitializeComponent();
            this.Text = title;
            this.InputPrompt = prompt;
        }

        public string InputText
        {
            get { return textBoxInput.Text; }
            set { textBoxInput.Text = value; }
        }

        public string InputPrompt
        {
            get { return labelInputPrompt.Text; }
            set { labelInputPrompt.Text = value; }
        }
    }
}
