namespace JobApplicationHelper.WindowService;

public interface IFileChooserDialog
{
    string Title { get; set; }
    string InitialFolder { get; set; }

    bool? ShowDialog();
    string ResultFile { get; set; }
}
