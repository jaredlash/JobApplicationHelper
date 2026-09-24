using System.IO;

namespace JobApplicationHelper.WindowService;

public class FolderLauncher : IFolderLauncher
{
    public void OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = folderPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        else
        {
            throw new Exception($"Folder does not exist: {folderPath}");
        }
    }
}
