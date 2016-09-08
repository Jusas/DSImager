using DSImager.Core.Interfaces;
using Microsoft.Win32;

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
    }
}
