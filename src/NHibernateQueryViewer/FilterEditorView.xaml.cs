namespace NHibernateQueryViewer;

using System.Windows;

public partial class FilterEditorView
{
    public FilterEditorView()
    {
        InitializeComponent();
        Loaded += FilterEditorView_Loaded;
    }

    public FilterEditorViewModel? ViewModel => DataContext as FilterEditorViewModel;

    private void FilterEditorView_Loaded(object sender, RoutedEventArgs args)
    {
        if (ViewModel == null)
        {
            return;
        }

        ViewModel.CloseView += (s, e) => Close();
    }
}
