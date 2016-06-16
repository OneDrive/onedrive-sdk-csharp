// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace OneDriveApiBrowser
{
    using System.Windows.Forms;

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
