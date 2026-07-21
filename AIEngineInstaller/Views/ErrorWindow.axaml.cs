namespace AIEngineInstaller.Views
{
    using Avalonia.Controls;
    using Avalonia.Interactivity;
    public partial class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            InitializeComponent();
        }

        private void OnOkClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
