using System.Windows;
using Kota.App.ViewModels;

namespace Kota.App
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void LocationManagement_Click(object sender, RoutedEventArgs e)
        {
            var locationWindow = new LocationManagementWindow();
            locationWindow.Owner = this;
            locationWindow.ShowDialog();
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            var itemDialog = new ItemDialogWindow();
            itemDialog.Owner = this;
            if (itemDialog.ShowDialog() == true)
            {
                // Refresh the main grid when item is added
                MessageBox.Show("Item added! (Grid refresh will be implemented next)", "Success");
            }
        }
    }
}