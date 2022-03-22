using ICSharpCode.AvalonEdit.Highlighting;

using System.ComponentModel;
using System.IO;
using System.Windows;

namespace NHibernateQueryViewer
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        public MainViewModel? ViewModel => DataContext as MainViewModel;

        public MainView()
        {
            InitializeComponent();
            Loaded += MainView_Loaded;
        }

        private void MainView_Loaded(object sender, RoutedEventArgs args)
        {
            if (ViewModel == null) return;

            ViewModel.PropertyChanged += LoadQuery;
        }

        private void LoadQuery(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(ViewModel.SelectedQuery) && args.PropertyName != nameof(ViewModel.ViewOption)) return;
            if (ViewModel?.SelectedQuery?.DisplayQuery == null) return;

            var stream = GenerateStreamFrom(ViewModel.SelectedQuery.DisplayQuery);
            textEditor.Load(stream);
            var syntax = HighlightingManager.Instance.GetDefinition(ViewModel.SelectedQuery.Language);
            textEditor.SyntaxHighlighting = syntax;
        }

        public Stream GenerateStreamFrom(string input)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(input);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
