using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Interfaces
{
    /// <summary>
    /// Dialog provider interface.
    /// Contains simple interface for file save and open dialogs.
    /// </summary>
    public interface IDialogProvider
    {
        bool ShowSaveDialog(string dialogTitle, string initialDirectory, 
            string defaultFilename, string defaultExtension, string fileFilter, 
            out string filename);

        bool ShowPickDirectoryDialog(string dialogTitle, string initialDirectory, out string directory);

        bool ShowOkDialog(string dialogTitle, string dialogText);
    }
}
