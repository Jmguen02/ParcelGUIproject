// Program 3
// CIS 200-76
// Fall 2020
// Due: 11/5/2020
// By: E3753
// Builds the form to find a record to edit and opens a second form that has the fields to edit.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Prog2
{
    public partial class EditAddressForm : Form
    {
        private List<Address> addressList; // List of address to fill the combo box
        
        //pre: None
        //post: The GUI is created and displayed
        public EditAddressForm(List<Address>addresses)
        {
            InitializeComponent();
            addressList = addresses;

            //adds addresses to the combo box
            foreach (Address names in addresses)
            {
                addressCmbo.Items.Add(names.Name);
            }
        }

        internal int AddressIndex
        {
            // pre: None
            // post: the forms address combo box item is returned
            get
            {
                return addressCmbo.SelectedIndex;
            }
            // pre: None
            // post: the forms set item is found and set to the value it is located at and is validated for an item being selected
            set
            {
                if ((value >= -1) && (value < addressList.Count))
                    addressCmbo.SelectedIndex = value;
                else
                    throw new ArgumentOutOfRangeException("AddressIndex", value,
                        "Index must be valid");
            }
        }

        // pre: user clicked the ok button
        // post: the form is closed and prepares the next field to open for editing
        private void okBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            
        }

        // pre: None
        // post: The form is closed and sends a cancel result
        private void cancelBtn_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) // Was it a left-click?
                this.DialogResult = DialogResult.Cancel;
        }
    }
}
