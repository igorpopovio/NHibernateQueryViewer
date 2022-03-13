using System.ComponentModel;
using System.IO;

namespace NHibernateQueryViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainViewModel? ViewModel => DataContext as MainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            ViewModel.PropertyChanged += LoadQueryOnSelectionChange;
        }

        private void LoadQueryOnSelectionChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ViewModel.SelectedQuery)) return;
            var stream = GenerateStreamFrom(ViewModel.SelectedQuery.WithParameters);
            textEditor.Load(stream);
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
