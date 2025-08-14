using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;  // Add this line
using Kota.Services.Items;
using Kota.Domain.DTOs;
using Kota.App.Services;

namespace Kota.App.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IItemService _itemService;
        private readonly ICurrentUserService _currentUserService;

        private ObservableCollection<ItemGridDto> _items = new();
        private string _searchText = string.Empty;
        private bool _isLoading;

        public MainViewModel(IItemService itemService, ICurrentUserService currentUserService)
        {
            _itemService = itemService;
            _currentUserService = currentUserService;

            RefreshCommand = new RelayCommand(async () => await RefreshItemsAsync());

            // Load items on startup
            _ = Task.Run(async () => await RefreshItemsAsync());
        }

        public ObservableCollection<ItemGridDto> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string CurrentUser => _currentUserService.CurrentUser?.Username ?? "Unknown";
        public bool IsAdmin => _currentUserService.IsAdmin;

        public ICommand RefreshCommand { get; }

        private async Task RefreshItemsAsync()
        {
            try
            {
                IsLoading = true;

                var items = await _itemService.GetAllItemsAsync();

                // Update on UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Items = new ObservableCollection<ItemGridDto>(items);
                });
            }
            catch (Exception ex)
            {
                // Handle error - for now just ignore
                System.Diagnostics.Debug.WriteLine($"Error loading items: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}