using System;
using System.Windows.Forms;
using DSImager.Core.Interfaces;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace DSImager.Application.AppPlatform
{
    /// <summary>
    /// Implementation for the dialog provider interface.
    /// </summary>
    public class DialogProvider : IDialogProvider
    {
        public bool ShowSaveDialog(string dialogTitle, string initialDirectory, 
            string defaultFilename, string defaultExtension, string fileFilter, 
            out string filename)
        {
            var dialog = new SaveFileDialog();
            dialog.Title = dialogTitle;
            dialog.FileName = defaultFilename;
            dialog.DefaultExt = defaultExtension;
            dialog.Filter = fileFilter;
            dialog.InitialDirectory = initialDirectory;

            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                filename = dialog.FileName;
                return true;
            }

            filename = null;
            return false;
        }

        public bool ShowPickDirectoryDialog(string dialogTitle, string initialDirectory, out string directory)
        {
            var dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            dialog.Description = dialogTitle;
            dialog.SelectedPath = initialDirectory;
            dialog.ShowNewFolderButton = true;

            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                directory = dialog.SelectedPath;
                return true;
            }

            directory = null;
            return false;
        }

        public bool ShowOkDialog(string dialogTitle, string dialogText)
        {
            var result = MessageBox.Show(dialogText, dialogTitle, MessageBoxButtons.OK);
            return true;
        }
    }
}
