using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestCaseAutomationAssigner.Infrastructure;

namespace TestCaseAutomationAssigner
{
    public class TestSelectorViewModel : NotifyPropertyChanged, IDisposable
    {
        private readonly IDisposable _searchQuery;
        private List<string> _allTestMethods;
        private readonly List<string> _testAttributes = new List<string>
            {
                "NUnit.Framework.TestAttribute",
                "Xunit.FactAttribute"
            };

        public TestSelectorViewModel(string assemblyLocation, TestSelector view)
        {
            SearchResults = new ObservableCollection<string>
                {
                    "Loading..."
                };
            LoadTests(assemblyLocation);
            var observable = Observable.FromEventPattern(this, "PropertyChanged").Where(e => ((PropertyChangedEventArgs)e.EventArgs).PropertyName == "SearchText");
            _searchQuery = observable
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Select(e => ((PropertyChangedEventArgs)e.EventArgs).PropertyName)
                .Subscribe(s=>Search());

            SelectCommand = new DelegateCommand(()=>view.DialogResult = true, ()=>!string.IsNullOrEmpty(SelectedMethod));
        }

        private void Search()
        {
            Application.Current.Dispatcher.Invoke(() =>
                {
                    SearchResults.Clear();
                    foreach (var result in _allTestMethods.Where(s => s.Contains(SearchText)))
                    {
                        SearchResults.Add(result);
                    }
                });
        }

        private async void LoadTests(string assemblyLocation)
        {
            _allTestMethods = await Task.Run(() => LoadTestMethods(assemblyLocation).ToList());
            SearchResults.Clear();
            foreach (var allTestMethod in _allTestMethods)
            {
                SearchResults.Add(allTestMethod);
            }
            LoadingFinished = true;
        }

        private IEnumerable<string> LoadTestMethods(string assemblyLocation)
        {
            var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(assemblyLocation);
            var allMethods = assembly.MainModule.Types
                    .SelectMany(t => t.Methods);

            return allMethods.Where(m => m.CustomAttributes.Any(a => _testAttributes.Contains(a.AttributeType.FullName)))
                .Select(m => m.DeclaringType.FullName + "." + m.Name)
                .OrderBy(n => n);
        }

        public string SearchText { get; set; }
        public string SelectedMethod { get; set; }

        public ObservableCollection<string> SearchResults { get; private set; }
        public bool LoadingFinished { get; private set; }

        public ICommand SelectCommand { get; private set; }

        public void Dispose()
        {
            _searchQuery.Dispose();
        }
    }
}