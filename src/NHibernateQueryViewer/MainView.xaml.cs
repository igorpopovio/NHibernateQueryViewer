namespace NHibernateQueryViewer;

using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

using ICSharpCode.AvalonEdit.Highlighting;

public partial class MainView
{
    public MainView()
    {
        InitializeComponent();
        Loaded += MainView_Loaded;
    }

    public MainViewModel? ViewModel => DataContext as MainViewModel;

    private void MainView_Loaded(object sender, RoutedEventArgs args)
    {
        if (ViewModel == null)
        {
            return;
        }

        ViewModel.PropertyChanged += LoadQuery;
        ViewModel.FocusFilter += ViewModel_FocusFilter;
        Loaded -= MainView_Loaded;
    }

    private void ViewModel_FocusFilter(object? sender, EventArgs args)
    {
        Filter.Focus();
        Filter.SelectAll();
    }

    private void LoadQuery(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(ViewModel.SelectedQuery) && args.PropertyName != nameof(ViewModel.ViewOption))
        {
            return;
        }

        if (ViewModel?.SelectedQuery?.DisplayQuery == null)
        {
            textEditor.Text = null;
            return;
        }

        using var stream = GenerateStreamFrom(ViewModel.SelectedQuery.DisplayQuery);
        textEditor.Load(stream);
        var syntax = HighlightingManager.Instance.GetDefinition(ViewModel.SelectedQuery.Language);
        textEditor.SyntaxHighlighting = syntax;
    }

    private Stream GenerateStreamFrom(string input)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.Write(input);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
