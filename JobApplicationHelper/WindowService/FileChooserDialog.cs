using Microsoft.Win32;

namespace JobApplicationHelper.WindowService;

public class FileChooserDialog : IFileChooserDialog
{

    public string InitialFolder { get; set; } = string.Empty;

    public string ResultFile { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;



    public bool? ShowDialog()
    {
        var dialog = new OpenFileDialog()
        {
            Title = Title,
            InitialDirectory = InitialFolder,
            Multiselect = false
        };

        //var dialog2 = new FolderBrowserDialog
        //{
        //    Description = Title,
        //    UseDescriptionForTitle = true,
        //    SelectedPath = InitialFolder,

        //    ShowNewFolderButton = false
        //};


        // Process open file dialog box results 
        var result = dialog.ShowDialog();
        if (result == true)
        {
            ResultFile = dialog.FileName;

            return true;
        }

        return false;
    }

}
