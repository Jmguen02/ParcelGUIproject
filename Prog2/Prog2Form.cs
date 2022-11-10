// Program 3
// CIS 200-76
// Fall 2020
// Due: 11/5/2020
// By: E3753

// File: Prog2Form.cs
// This class creates the main GUI for Program 2. It provides a
// File menu with About and Exit items, an Insert menu with Address and
// Letter items, and a Report menu with List Addresses and List Parcels
// items.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using Prog2;

namespace UPVApp
{
    public partial class Prog2Form : Form
    {
        private UserParcelView upv; // The UserParcelView
        private BinaryFormatter formatter = new BinaryFormatter(); // object for serializing records in binary format
        private FileStream output; // stream for writing to a file
        private FileStream input; // stream for reading to a file
        private StreamReader fileReader;

        // Precondition:  None
        // Postcondition: The form's GUI is prepared for display. A few test addresses are
        //                added to the list of addresses
        public Prog2Form()
        {
            InitializeComponent();

            upv = new UserParcelView();
        }

        // Precondition:  File, About menu item activated
        // Postcondition: Information about author displayed in dialog box
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string NL = Environment.NewLine; // Newline shorthand

            MessageBox.Show($"Program 2{NL}By: Joe Guenther{NL}CIS 200{NL}Fall 2020",
                "About Program 2");
        }

        // Precondition:  File, Exit menu item activated
        // Postcondition: The application is exited
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Precondition:  Insert, Address menu item activated
        // Postcondition: The Address dialog box is displayed. If data entered
        //                are OK, an Address is created and added to the list
        //                of addresses
        private void addressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddressForm addressForm = new AddressForm();    // The address dialog box form
            DialogResult result = addressForm.ShowDialog(); // Show form as dialog and store result
            int zip; // Address zip code

            if (result == DialogResult.OK) // Only add if OK
            {
                if (int.TryParse(addressForm.ZipText, out zip))
                {
                    upv.AddAddress(addressForm.AddressName, addressForm.Address1,
                        addressForm.Address2, addressForm.City, addressForm.State,
                        zip); // Use form's properties to create address
                }
                else // This should never happen if form validation works!
                {
                    MessageBox.Show("Problem with Address Validation!", "Validation Error");
                }
            }

            addressForm.Dispose(); // Best practice for dialog boxes
                                   // Alternatively, use with using clause as in Ch. 17
        }

        // Pre: Edit address button is clicked
        // Post: The edit address form is shown. If valid entries then address entry is saved.
        private void editAddress_Click(object sender, EventArgs e)
        {
            EditAddressForm editForm = new EditAddressForm(upv.AddressList); // the edit address dialog box
            DialogResult result = editForm.ShowDialog(); // shows the form as a dialog box

            if (result == DialogResult.OK) // only adds if OK
            {
                    Address address = upv.AddressAt(editForm.AddressIndex);
                    AddressForm editAddress = new AddressForm();

                    editAddress.AddressName = address.Name; // sets all of the objects equal to the other form
                    editAddress.Address1 = address.Address1;
                    editAddress.Address2 = address.Address2;
                    editAddress.City = address.City;
                    editAddress.State = address.State;
                    editAddress.ZipText = address.Zip.ToString();

                result = editAddress.ShowDialog(); // creates a new form with the objects from the selected address

                if (result == DialogResult.OK) // only adds if OK
                {
                    address.Name = editAddress.AddressName; // sets all of the objects equal to the other form
                    address.Address1 = editAddress.Address1;
                    address.Address2 = editAddress.Address2;
                    address.City = editAddress.City;
                    address.State = editAddress.State;
                    address.Zip = int.Parse(editAddress.ZipText);
                }
            }

            editForm.Dispose();
        }

        // Precondition:  Report, List Addresses menu item activated
        // Postcondition: The list of addresses is displayed in the addressResultsTxt
        //                text box
        private void listAddressesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder result = new StringBuilder(); // Holds text as report being built
                                                        // StringBuilder more efficient than String
            string NL = Environment.NewLine;            // Newline shorthand

            result.Append("Addresses:");
            result.Append(NL); // Remember, \n doesn't always work in GUIs
            result.Append(NL);

            foreach (Address a in upv.AddressList)
            {
                result.Append(a.ToString());
                result.Append(NL);
                result.Append("------------------------------");
                result.Append(NL);
            }

            reportTxt.Text = result.ToString();

            // -- OR --
            // Not using StringBuilder, just use TextBox directly

            //reportTxt.Clear();
            //reportTxt.AppendText("Addresses:");
            //reportTxt.AppendText(NL); // Remember, \n doesn't always work in GUIs
            //reportTxt.AppendText(NL);

            //foreach (Address a in upv.AddressList)
            //{
            //    reportTxt.AppendText(a.ToString());
            //    reportTxt.AppendText(NL);
            //    reportTxt.AppendText("------------------------------");
            //    reportTxt.AppendText(NL);
            //}

            // Put cursor at start of report
            reportTxt.Focus();
            reportTxt.SelectionStart = 0;
            reportTxt.SelectionLength = 0;
        }

        // Precondition:  Insert, Letter menu item activated
        // Postcondition: The Letter dialog box is displayed. If data entered
        //                are OK, a Letter is created and added to the list
        //                of parcels
        private void letterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LetterForm letterForm; // The letter dialog box form
            DialogResult result;   // The result of showing form as dialog
            decimal fixedCost;     // The letter's cost

            if (upv.AddressCount < LetterForm.MIN_ADDRESSES) // Make sure we have enough addresses
            {
                MessageBox.Show("Need " + LetterForm.MIN_ADDRESSES + " addresses to create letter!",
                    "Addresses Error");
                return; // Exit now since can't create valid letter
            }

            letterForm = new LetterForm(upv.AddressList); // Send list of addresses
            result = letterForm.ShowDialog();

            if (result == DialogResult.OK) // Only add if OK
            {
                if (decimal.TryParse(letterForm.FixedCostText, out fixedCost))
                {
                    // For this to work, LetterForm's combo boxes need to be in same
                    // order as upv's AddressList
                    upv.AddLetter(upv.AddressAt(letterForm.OriginAddressIndex),
                        upv.AddressAt(letterForm.DestinationAddressIndex),
                        fixedCost); // Letter to be inserted
                }
               else // This should never happen if form validation works!
                {
                    MessageBox.Show("Problem with Letter Validation!", "Validation Error");
                }
            }

            letterForm.Dispose(); // Best practice for dialog boxes
                                  // Alternatively, use with using clause as in Ch. 17
        }

        // Precondition:  Report, List Parcels menu item activated
        // Postcondition: The list of parcels is displayed in the parcelResultsTxt
        //                text box
        private void listParcelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // This report is generated without using a StringBuilder, just to show an
            // alternative approach more like what most students will have done
            // Method AppendText is equivalent to using .Text +=

            decimal totalCost = 0;                      // Running total of parcel shipping costs
            string NL = Environment.NewLine;            // Newline shorthand

            reportTxt.Clear(); // Clear the textbox
            reportTxt.AppendText("Parcels:");
            reportTxt.AppendText(NL); // Remember, \n doesn't always work in GUIs
            reportTxt.AppendText(NL);

            foreach (Parcel p in upv.ParcelList)
            {
                reportTxt.AppendText(p.ToString());
                reportTxt.AppendText(NL);
                reportTxt.AppendText("------------------------------");
                reportTxt.AppendText(NL);
                totalCost += p.CalcCost();
            }

            reportTxt.AppendText(NL);
            reportTxt.AppendText($"Total Cost: {totalCost:C}");

            // Put cursor at start of report
            reportTxt.Focus();
            reportTxt.SelectionStart = 0;
            reportTxt.SelectionLength = 0;
        }

        private void saveAsBtn_Click(object sender, EventArgs e)
        {
            DialogResult result; // show dialog box for user to find and open file to save to
            string fileName; // name of file to save to

            using (SaveFileDialog fileChooser = new SaveFileDialog())
            {
                fileChooser.CheckFileExists = false; // lets user create file
                result = fileChooser.ShowDialog();
                fileName = fileChooser.FileName; // get specific file name
            }

            if (result == DialogResult.OK) // add if user hits OK
            {
                if (string.IsNullOrEmpty(fileName)) // shows error if invalid file
                {
                    MessageBox.Show("Invalid File Name", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    try // save via FileStream if valid file
                    {
                        // open file with write access
                        output = new FileStream(fileName,
                            FileMode.OpenOrCreate, FileAccess.Write);
                    }
                    catch (IOException) // shows if file couldnt be opened
                    {
                        MessageBox.Show("Error opening the file", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            try // store values and serialize
            {
                formatter.Serialize(output, upv); // gets values from UPV
            }
            catch (IOException) // shows error if record couldnt serialize
            {
                MessageBox.Show("Failed to serialize", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try // closes file
            {
                output?.Close(); // close FileStream
            }
            catch (IOException) // shows error if file couldnt close
            {
                MessageBox.Show("Cannot close file", "Error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openBtn_Click(object sender, EventArgs e)
        {
            DialogResult result; // show dialog box for user to open a file
            string fileName; // name of file to open

            using (OpenFileDialog fileChooser = new OpenFileDialog())
            {
                result = fileChooser.ShowDialog(); // gets result of dialog box
                fileName = fileChooser.FileName; // gets specific file name
            }

            if (result == DialogResult.OK) // makes sure OK button is hit
            {
                if(string.IsNullOrEmpty(fileName)) // shows error if invalid file
                {
                    MessageBox.Show("Invalid File Name", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    input = new FileStream(fileName, FileMode.Open, FileAccess.Read); // opens file with read access

                    fileReader = new StreamReader(input);
                }
            }

            try // deserializes records in file for use by the application
            {
                upv = (UserParcelView)formatter.Deserialize(input); // gets deserialized objects for use
            }
            catch (IOException) // shows error if couldnt deserialize object
            {
                MessageBox.Show("Failed to deserialize", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try // closes file
            {
                fileReader?.Close(); // close FileStream
            }
            catch (IOException) // shows error if file couldnt close
            {
                MessageBox.Show("Cannot close file", "Error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}