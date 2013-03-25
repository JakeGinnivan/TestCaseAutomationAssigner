using System.Windows.Input;

namespace TestCaseAutomationAssigner
{
    public partial class TestSelector
    {
        public TestSelector()
        {
            InitializeComponent();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectCommand = ((TestSelectorViewModel) DataContext).SelectCommand;
            if (selectCommand.CanExecute(null))
                selectCommand.Execute(null);
        }
    }
}
