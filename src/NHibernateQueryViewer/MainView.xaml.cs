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

        _ = ViewModel.Initialize();

        ViewModel.SelectedQueryUpdated += LoadQuery;
        ViewModel.FocusFilter += ViewModel_FocusFilter;
        Loaded -= MainView_Loaded;
    }

    private void ViewModel_FocusFilter(object? sender, EventArgs args)
    {
        Filter.Focus();
        Filter.SelectAll();
    }

    private void LoadQuery(object? sender, EventArgs args)
    {
        if (ViewModel?.SelectedQuery?.Enhanced == null)
        {
            textEditor.Text = null;
            return;
        }

        using var stream = GenerateStreamFrom(ViewModel.SelectedQuery.Enhanced);
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
