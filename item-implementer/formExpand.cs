using System;
using System.Windows.Forms;

namespace item_implementer
{
    public partial class formExpand : Form
    {
        // Property to hold the description text
        public string DescriptionText { get; set; }

        public formExpand(string description)
        {
            InitializeComponent();

            // Set the description text to the passed value
            DescriptionText = description;
            txtDescription.Text = description;
        }

        // On save, update the DescriptionText and close the form
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Update the description text
            DescriptionText = txtDescription.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();  // Close the form after saving
        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void minimiseBtn_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}