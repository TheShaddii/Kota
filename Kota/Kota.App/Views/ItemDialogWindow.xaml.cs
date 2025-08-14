using System.Windows;
using Kota.Domain.Entities;
using Kota.Domain.DTOs;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Kota.Data.Repositories;
using Kota.Services.Items;
using Kota.App.Services;
using System.Data.OleDb;

namespace Kota.App
{
    public partial class ItemDialogWindow : Window
    {
        private readonly ISiteRepository _siteRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IItemService _itemService;
        private readonly ICurrentUserService _currentUserService;

        private ObservableCollection<Site> Sites = new();
        private ObservableCollection<Building> Buildings = new();
        private int _selectedBinId = 1;

        public ItemDialogWindow()
        {
            InitializeComponent();

            // Get services
            var app = (App)Application.Current;
            _siteRepository = app.Host.Services.GetRequiredService<ISiteRepository>();
            _buildingRepository = app.Host.Services.GetRequiredService<IBuildingRepository>();
            _itemService = app.Host.Services.GetRequiredService<IItemService>();
            _currentUserService = app.Host.Services.GetRequiredService<ICurrentUserService>();

            // Debug what's actually in the database
            DebugDatabaseContents();

            LoadLocations();

            // Set default values
            ItemIdTextBox.Text = "Auto-assigned";
            QtyOnHandTextBox.Text = "0";
            StartingQtyTextBox.Text = "0";
            MinQtyTextBox.Text = "0";
        }

        private void DebugDatabaseContents()
        {
            try
            {
                var app = (App)Application.Current;
                var settingsService = app.Host.Services.GetRequiredService<Kota.App.Services.ISettingsService>();
                var connectionFactory = app.Host.Services.GetRequiredService<Kota.Data.IConnectionFactory>();

                var dbPath = settingsService.GetDatabasePathAsync().Result;
                connectionFactory.SetDatabasePath(dbPath);

                using var connection = (OleDbConnection)connectionFactory.CreateConnection();
                connection.Open();

                // Check sites table
                using var sitesCommand = new OleDbCommand("SELECT id, code, name FROM sites", connection);
                using var sitesReader = sitesCommand.ExecuteReader();

                var sitesInfo = "Sites in database:\n";
                while (sitesReader.Read())
                {
                    sitesInfo += $"ID: {sitesReader["id"]}, Code: {sitesReader["code"]}, Name: {sitesReader["name"]}\n";
                }
                sitesReader.Close();

                MessageBox.Show(sitesInfo, "Sites Debug");

                // Check buildings table
                using var buildingsCommand = new OleDbCommand("SELECT id, site_id, code, name FROM buildings", connection);
                using var buildingsReader = buildingsCommand.ExecuteReader();

                var buildingsInfo = "Buildings in database:\n";
                while (buildingsReader.Read())
                {
                    buildingsInfo += $"ID: {buildingsReader["id"]}, Site_ID: {buildingsReader["site_id"]}, Code: {buildingsReader["code"]}, Name: {buildingsReader["name"]}\n";
                }
                buildingsReader.Close();

                if (buildingsInfo == "Buildings in database:\n")
                {
                    buildingsInfo += "No buildings found!";
                }

                MessageBox.Show(buildingsInfo, "Buildings Debug");

                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database debug error: {ex.Message}", "Error");
            }
        }

        private async void LoadLocations()
        {
            try
            {
                // Load sites
                var sites = await _siteRepository.GetAllAsync();
                Sites.Clear();
                foreach (var site in sites)
                {
                    Sites.Add(site);
                }
                SiteComboBox.ItemsSource = Sites;

                if (Sites.Count == 0)
                {
                    MessageBox.Show("No sites found! Please create a site first in Location Management.", "No Sites");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading locations: {ex.Message}", "Error");
            }
        }

        private async Task LoadBuildingsForSite(int siteId)
        {
            try
            {
                var buildings = await _buildingRepository.GetBySiteIdAsync(siteId);
                Buildings.Clear();
                foreach (var building in buildings)
                {
                    Buildings.Add(building);
                }
                BuildingComboBox.ItemsSource = Buildings;

                if (Buildings.Any())
                {
                    BuildingComboBox.SelectedIndex = 0;
                    UpdateLocationDisplay();
                    MessageBox.Show($"Found {Buildings.Count} buildings for site {siteId}", "Success");
                }
                else
                {
                    BuildingComboBox.SelectedIndex = -1;
                    LocationDisplayTextBlock.Text = "No buildings found for this site";
                    MessageBox.Show($"No buildings found for site ID {siteId}. You may need to create buildings for this site.", "No Buildings");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading buildings: {ex.Message}", "Error");
            }
        }

        private async void SiteComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SiteComboBox.SelectedItem is Site selectedSite)
            {
                await LoadBuildingsForSite(selectedSite.Id);
            }
            else
            {
                Buildings.Clear();
                BuildingComboBox.ItemsSource = Buildings;
                LocationDisplayTextBlock.Text = "Select Site and Building above";
            }
        }

        private void BuildingComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateLocationDisplay();
        }

        private void UpdateLocationDisplay()
        {
            if (SiteComboBox.SelectedItem is Site site && BuildingComboBox.SelectedItem is Building building)
            {
                LocationDisplayTextBlock.Text = $"{site.Name} > {building.Name} > Default Location";
                _selectedBinId = 1;
            }
            else
            {
                LocationDisplayTextBlock.Text = "Select Site and Building above";
            }
        }

        private async void UseDefaultLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainSite = Sites.FirstOrDefault(s => s.Name.Contains("Main") || s.Code == "MAIN");
                if (mainSite != null)
                {
                    SiteComboBox.SelectedItem = mainSite;
                    // The SelectionChanged event will trigger and load buildings
                }
                else
                {
                    MessageBox.Show("Main Warehouse not found. Available sites:\n" +
                                  string.Join("\n", Sites.Select(s => $"- {s.Code}: {s.Name}")), "Info");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting default location: {ex.Message}", "Error");
            }
        }

        private async void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
                {
                    MessageBox.Show("Please enter a description.", "Validation Error");
                    return;
                }

                if (SiteComboBox.SelectedItem == null || BuildingComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a site and building.", "Validation Error");
                    return;
                }

                if (!double.TryParse(StartingQtyTextBox.Text, out double startingQty) || startingQty < 0)
                {
                    MessageBox.Show("Please enter a valid starting quantity (0 or greater).", "Validation Error");
                    return;
                }

                if (!double.TryParse(MinQtyTextBox.Text, out double minQty) || minQty < 0)
                {
                    MessageBox.Show("Please enter a valid minimum quantity (0 or greater).", "Validation Error");
                    return;
                }

                var item = new Item
                {
                    Description = DescriptionTextBox.Text.Trim(),
                    ManufacturerSku = string.IsNullOrWhiteSpace(SkuTextBox.Text) ? null : SkuTextBox.Text.Trim(),
                    QtyOnHand = startingQty,
                    MinQty = minQty,
                    BinId = _selectedBinId,
                    Notes = string.IsNullOrWhiteSpace(NotesTextBox.Text) ? null : NotesTextBox.Text.Trim()
                };

                var savedItem = await _itemService.CreateItemAsync(item, _currentUserService.CurrentUser!.Id);

                MessageBox.Show($"Item '{savedItem.Description}' created successfully!\n\nItem ID: {savedItem.Id}", "Success");

                DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving item: {ex.Message}", "Error");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}