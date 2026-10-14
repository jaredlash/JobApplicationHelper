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

        var result = dialog.ShowDialog();
        if (result == true)
        {
            ResultFile = dialog.FileName;

            return true;
        }

        return false;
    }

}
