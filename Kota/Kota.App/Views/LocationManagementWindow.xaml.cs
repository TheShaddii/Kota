using System.Windows;
using Kota.Domain.Entities;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Kota.Data.Repositories;

namespace Kota.App
{
    public partial class LocationManagementWindow : Window
    {
        private readonly ISiteRepository _siteRepository;
        private readonly IBuildingRepository _buildingRepository;
        private ObservableCollection<Site> Sites = new();
        private ObservableCollection<Building> Buildings = new();

        public LocationManagementWindow()
        {
            InitializeComponent();

            // Get services from DI container
            var app = (App)Application.Current;
            _siteRepository = app.Host.Services.GetRequiredService<ISiteRepository>();
            _buildingRepository = app.Host.Services.GetRequiredService<IBuildingRepository>();

            // Load data immediately - database is already initialized
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                // Load sites from database
                var sites = await _siteRepository.GetAllAsync();
                Sites.Clear();
                foreach (var site in sites)
                {
                    Sites.Add(site);
                }

                // Load buildings from database
                var buildings = await _buildingRepository.GetAllAsync();
                Buildings.Clear();
                foreach (var building in buildings)
                {
                    Buildings.Add(building);
                }

                // Bind to UI
                SitesGrid.ItemsSource = Sites;
                BuildingsGrid.ItemsSource = Buildings;
                BuildingSiteComboBox.ItemsSource = Sites;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}\n\nThe database may need to be reinitialized.", "Error");
            }
        }

        private void AddSite_Click(object sender, RoutedEventArgs e)
        {
            SiteCodeTextBox.Clear();
            SiteNameTextBox.Clear();
            SiteCodeTextBox.Focus();
        }

        private async void SaveSite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SiteCodeTextBox.Text) ||
                    string.IsNullOrWhiteSpace(SiteNameTextBox.Text))
                {
                    MessageBox.Show("Please enter both code and name.", "Validation Error");
                    return;
                }

                var site = new Site
                {
                    Code = SiteCodeTextBox.Text.Trim(),
                    Name = SiteNameTextBox.Text.Trim()
                };

                // Save to database
                site = await _siteRepository.CreateAsync(site);

                // Add to UI collection
                Sites.Add(site);

                MessageBox.Show($"Site '{site.Name}' saved successfully!", "Success");
                ClearSite_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving site: {ex.Message}", "Error");
            }
        }

        private void ClearSite_Click(object sender, RoutedEventArgs e)
        {
            SiteCodeTextBox.Clear();
            SiteNameTextBox.Clear();
        }

        private void AddBuilding_Click(object sender, RoutedEventArgs e)
        {
            if (BuildingSiteComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a site first.", "Validation Error");
                return;
            }

            BuildingCodeTextBox.Clear();
            BuildingNameTextBox.Clear();
            BuildingCodeTextBox.Focus();
        }

        private async void SaveBuilding_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BuildingSiteComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a site.", "Validation Error");
                    return;
                }

                if (string.IsNullOrWhiteSpace(BuildingCodeTextBox.Text) ||
                    string.IsNullOrWhiteSpace(BuildingNameTextBox.Text))
                {
                    MessageBox.Show("Please enter both code and name.", "Validation Error");
                    return;
                }

                var selectedSite = (Site)BuildingSiteComboBox.SelectedItem;
                var building = new Building
                {
                    SiteId = selectedSite.Id,
                    Code = BuildingCodeTextBox.Text.Trim(),
                    Name = BuildingNameTextBox.Text.Trim()
                };

                // Save to database
                building = await _buildingRepository.CreateAsync(building);

                // Add to UI collection
                Buildings.Add(building);

                MessageBox.Show($"Building '{building.Name}' saved to '{selectedSite.Name}'!", "Success");
                ClearBuilding_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving building: {ex.Message}", "Error");
            }
        }

        private void ClearBuilding_Click(object sender, RoutedEventArgs e)
        {
            BuildingCodeTextBox.Clear();
            BuildingNameTextBox.Clear();
        }

        private async void CreateSampleStructure_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("This will create sample location data. Continue?",
                                           "Create Sample Data", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    // Create sample site
                    var site = new Site { Code = "MAIN", Name = "Main Warehouse" };
                    site = await _siteRepository.CreateAsync(site);
                    Sites.Add(site);

                    // Create sample building
                    var building = new Building { SiteId = site.Id, Code = "BLDG-A", Name = "Building A" };
                    building = await _buildingRepository.CreateAsync(building);
                    Buildings.Add(building);

                    MessageBox.Show("Sample location structure created and saved to database!", "Sample Data Created");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating sample data: {ex.Message}", "Error");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}