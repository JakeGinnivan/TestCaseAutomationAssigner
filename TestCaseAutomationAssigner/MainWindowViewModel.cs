using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.Win32;
using TestCaseAutomationAssigner.Infrastructure;

namespace TestCaseAutomationAssigner
{
    public class MainWindowViewModel : NotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            BrowseCommand = new DelegateCommand(Browse);
            BrowseTestsCommand = new DelegateCommand(BrowseTests);
            AssignTestCaseCommand = new AwaitableDelegateCommand(AssignTestCase, CanAssignTestCase);
        }

        private bool CanAssignTestCase()
        {
            return !string.IsNullOrEmpty(TestMethod) &&
                   !string.IsNullOrEmpty(AutomationType) &&
                   !string.IsNullOrEmpty(TfsServer) &&
                   !string.IsNullOrEmpty(TeamProject) &&
                   !string.IsNullOrEmpty(TestCaseNumber);
        }

        private async Task AssignTestCase()
        {
            try
            {
                var testCase = await Task.Run(() => GetTestCase());
                await Task.Run(() => AssociateAutomation(testCase, TestMethod, AutomationType, Path.GetFileName(TestAssembly)));

                TestMethod = null;
                TestCaseNumber = null;
                MessageBox.Show(TestMethod + " assigned to test case #" + TestCaseNumber, "Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Occured");
            }
        }

        private ITestCase GetTestCase()
        {
            var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(TfsServer));
            var testService = collection.GetService<ITestManagementService>();
            var project = testService.GetTeamProject(TeamProject);
            return project.TestCases.Find(int.Parse(TestCaseNumber));
        }

        /// <summary>
        /// Associates an automation to the test case.
        /// </summary>
        /// <param name="testCase">The test case artifact to which to associate automation</param>
        /// <param name="automationTestName">The automation test name. It should be fully
        /// qualified of format "Namespace.ClassName.TestMethodName.</param>
        /// <param name="automationTestType">The automation test type like "CodedUITest".</param>
        /// <param name="automationStorageName">The assembly name containing the above
        /// test method without path like MyTestProject.dll.</param>
        private void AssociateAutomation(ITestCase testCase, string automationTestName, string automationTestType, string automationStorageName)
        {
            // Build automation guid
            var crypto = new SHA1CryptoServiceProvider();
            var bytes = new byte[16];
            Array.Copy(crypto.ComputeHash(Encoding.Unicode.GetBytes(automationTestName)), bytes, bytes.Length);
            var automationGuid = new Guid(bytes);

            // Create the associated automation.
            testCase.Implementation = testCase.Project.CreateTmiTestImplementation(
                    automationTestName, automationTestType,
                    automationStorageName, automationGuid);

            // Save the test. If you are doing this for lots of test, you can consider
            // bulk saving too (outside of this method) for performance reason.
            testCase.Save();
        }

        private void BrowseTests()
        {
            var testSelector = new TestSelector();
            using (var testSelectorViewModel = new TestSelectorViewModel(TestAssembly, testSelector))
            {
                testSelector.DataContext = testSelectorViewModel;
                var result = testSelector.ShowDialog();

                if (result == true)
                    TestMethod = testSelectorViewModel.SelectedMethod;
            }
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
        public ICommand BrowseTestsCommand { get; private set; }
        public ICommand AssignTestCaseCommand { get; private set; }

        public string TestAssembly { get; private set; }
        public string TestMethod { get; private set; }
        public string AutomationType { get; set; }

        public string TfsServer { get; set; }
        public string TeamProject { get; set; }
        public string TestCaseNumber { get; set; }
    }
}