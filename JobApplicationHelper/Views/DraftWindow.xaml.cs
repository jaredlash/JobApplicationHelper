using JobApplicationHelper.ViewModels;
using System.Windows;

namespace JobApplicationHelper.Views
{
    /// <summary>
    /// Interaction logic for DraftWindow.xaml
    /// </summary>
    public partial class DraftWindow : Window
    {
        public DraftWindow(DraftWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
