﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DictionaryBase;
using System.IO;

namespace DictionaryEditor
{
    public partial class Form1 : Form
    {
        Dictionary dictionary;
        string saveLocation;
        bool saved = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            this.TryLoad();
        }

        private void UpdateFormTitle()
        {
            this.Text = "Dictionary Editor" + (this.dictionary != null ? " - " + this.dictionary.Name + " - " + this.saveLocation + (this.saved ? "" : " *") : "");
        }

        private void TryLoad()
        {
            if (!this.ResolveUnsavedData())
            {
                return;
            }
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "Load Dictionary";
            dialog.Filter = "Dictionary files|*.dic|All Files|*.*";
            dialog.DefaultExt = ".dic";
            dialog.ShowDialog();
            if (!dialog.FileName.Equals(""))
            {
                try
                {
                    this.dictionary = new Dictionary(dialog.FileName);
                    this.saveLocation = dialog.FileName;
                    this.saved = true;
                    this.UpdateFormTitle();
                    this.wordListBox.Items.Clear();
                    for (int i = 0; i < this.dictionary.GetNumberOfEntries(); i++)
                    {
                        Tuple<string, string> currentEntry = this.dictionary.GetEntry(i);
                        this.wordListBox.Items.Add(currentEntry.Item1 + " || " + currentEntry.Item2);
                    }
                    if (this.wordListBox.Items.Count > 0)
                    {
                        this.wordListBox.SelectedIndex = 0;
                        this.wordListBox.SelectedItem = this.wordListBox.Items[0];
                    }
                    else
                    {
                        this.wordListBox.SelectedIndex = -1;
                        this.wordListBox.SelectedItem = null;
                    }
                    this.saved = true;
                    this.UpdateFormTitle();
                    this.EnableButtons();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load this file as a dictionary." + e.InnerException.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            dialog.Dispose();
        }

        private void Save()
        {
            this.dictionary.Save(this.saveLocation);
            this.saved = true;
            this.UpdateFormTitle();
        }

        private void SaveAs()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Save Dictionary";
            dialog.Filter = "Dictionary files|*.dic|All Files|*.*";
            dialog.FileName = "UnnamedDictionary.dic";
            dialog.DefaultExt = ".dic";
            dialog.AddExtension = true;
            dialog.ShowDialog();
            if (!dialog.FileName.Equals(""))
            {
                this.dictionary.Save(dialog.FileName);
                this.saveLocation = dialog.FileName;
                this.saved = true;
                this.UpdateFormTitle();
            }
            dialog.Dispose();
        }

        private void EnableButtons()
        {
            this.informationButton.Enabled = true;
            this.saveAsButton.Enabled = true;
            this.saveButton.Enabled = true;
            this.deleteButton.Enabled = true;
            this.editButton.Enabled = true;
        }

        private void wordListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateSelectedIndex();
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = this.wordListBox.SelectedIndex;
            if (selectedIndex == -1)
            {
                return;
            }
            string questionString = this.questionEditTextBox.Text;
            if (questionString.Equals(""))
            {
                MessageBox.Show("You must enter the question.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            string answerString = this.answerEditTextBox.Text;
            if (answerString.Equals(""))
            {
                MessageBox.Show("You must enter the answer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            this.dictionary.SetEntry(selectedIndex, new Tuple<string, string>(questionString, answerString));
            this.saved = false;
            this.UpdateFormTitle();
            this.wordListBox.Items[selectedIndex] = questionString + " || " + answerString;
        }

        private void UpdateSelectedIndex()
        {
            if (this.wordListBox.SelectedIndex != -1)
            {
                Tuple<string, string> selectedEntry = this.dictionary.GetEntry(this.wordListBox.SelectedIndex);
                this.questionEditTextBox.Text = selectedEntry.Item1;
                this.answerEditTextBox.Text = selectedEntry.Item2;
            }
            else
            {
                this.questionEditTextBox.Text = "";
                this.answerEditTextBox.Text = "";
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = this.wordListBox.SelectedIndex;
            if (selectedIndex != -1)
            {
                this.dictionary.RemoveEntry(selectedIndex);
                this.wordListBox.Items.RemoveAt(selectedIndex);
                this.UpdateSelectedIndex();
                this.saved = false;
                this.UpdateFormTitle();
            }
        }

        private void saveAsButton_Click(object sender, EventArgs e)
        {
            this.SaveAs();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        /// <returns>Returns whether the ongoing action should continue.</returns>
        private bool ResolveUnsavedData()
        {
            if (!this.saved)
            {
                DialogResult result = MessageBox.Show("You have some unsaved progress. Would you like to save it?", "Unsaved Progress", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    this.Save();
                }
                else if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.ResolveUnsavedData())
            {
                e.Cancel = true;
            }
        }

        public string DictionaryName
        {
            get
            {
                return this.dictionary.Name;
            }
            set
            {
                this.dictionary.Name = value;
                this.saved = false;
                this.UpdateFormTitle();
            }
        }

        public string DictionaryIdentifier
        {
            get
            {
                return this.dictionary.Identifier;
            }
            set
            {
                this.dictionary.Identifier = value;
                this.saved = false;
                this.UpdateFormTitle();
            }
        }

        public string DictionaryAuthor
        {
            get
            {
                return this.dictionary.Author;
            }
        }

        public string DictionaryCreationTime
        {
            get
            {
                return this.dictionary.CreatedOn.ToString();
            }
        }

        private void informationButton_Click(object sender, EventArgs e)
        {
            GeneralInformationForm infoForm = new GeneralInformationForm(this);
            infoForm.ShowDialog(this);
        }
    }
}
