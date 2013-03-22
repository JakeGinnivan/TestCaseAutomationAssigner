using System.Windows.Input;
using Microsoft.Win32;
using TestCaseAutomationAssigner.Infrastructure;

namespace TestCaseAutomationAssigner
{
    public class MainWindowViewModel : NotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            BrowseCommand = new DelegateCommand(Browse);
        }

        private void Browse()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                TestAssembly = openFileDialog.FileName;
            }
        }

        public ICommand BrowseCommand { get; private set; }

        public string TestAssembly { get; private set; }
    }
}