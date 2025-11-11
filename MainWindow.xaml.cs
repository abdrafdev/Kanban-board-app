using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Text;
using System.Windows.Threading;
using Microsoft.Win32;

namespace KanbanBoard
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<ProcessCard> allCards = new ObservableCollection<ProcessCard>();
        private ObservableCollection<ProcessCard> filteredCards = new ObservableCollection<ProcessCard>();
        private string currentFilter = "ALL";
        private double currentZoomLevel = 1.0;

        public event PropertyChangedEventHandler? PropertyChanged;

        public double ZoomLevel
        {
            get => currentZoomLevel;
            set
            {
                if (currentZoomLevel != value)
                {
                    currentZoomLevel = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomLevel)));
                }
            }
        }

        public static Dictionary<string, string> IconLibrary = new Dictionary<string, string>
        {
            {"BARVANJE", "Assets/Icons/BARVANJE.png"},
            {"BRUŠENJE", "Assets/Icons/BRUŠENJE.png"},
            {"ELEKTROPOLIRANJE", "Assets/Icons/ELEKTROPOLIRANJE.png"},
            {"KRIVLJENJE", "Assets/Icons/KRIVLJENJE (1).png"},
            {"LASER", "Assets/Icons/LASER.png"}
        };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeData();
            ProcessCards.ItemsSource = filteredCards;
            ApplyFilter("ALL");
            LoadZoomLevel();
        }

        private void InitializeData()
        {
            // Green cards (Completed/Good status)
            var card1 = new ProcessCard
            {
                Id = "CARD-PSA001",
                Title = "PRESTAVNA SKRINJA-ASSY-001",
                Subtitle = "Pratni podatek",
                Timestamp = DateTime.Now.AddHours(-2).ToString("dd:HH:mm:ss:ff"),
                Button1Icon = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z",
                Button2Icon = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z",
                Button3Icon = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z",
                BadgeVisibility = Visibility.Collapsed
            };
            card1.AssignedTests.Add(new TestAssignment { TestName = "Welding Process", IconPath = IconLibrary["LASER"], Progress = 100 });
            card1.AssignedTests.Add(new TestAssignment { TestName = "Assembly Check", IconPath = IconLibrary["BARVANJE"], Progress = 100 });
            card1.AssignedTests.Add(new TestAssignment { TestName = "Quality Test", IconPath = IconLibrary["BRUŠENJE"], Progress = 100 });
            card1.UpdateOverallProgress();
            allCards.Add(card1);

            // Add other cards similarly, updating with icon paths
            var card2 = new ProcessCard
            {
                Id = "CARD-REZ002",
                Title = "REZKAR",
                Subtitle = "Pratni podatek",
                Timestamp = DateTime.Now.AddHours(-5).ToString("dd:HH:mm:ss:ff"),
                Button1Icon = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z",
                Button2Icon = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z",
                Button3Icon = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z",
                BadgeText = "Optalnica",
                BadgeColor = new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                BadgeVisibility = Visibility.Visible
            };
            card2.AssignedTests.Add(new TestAssignment { TestName = "Cutting Test", IconPath = IconLibrary["ELEKTROPOLIRANJE"], Progress = 50 });
            card2.AssignedTests.Add(new TestAssignment { TestName = "Quality Check", IconPath = IconLibrary["KRIVLJENJE"], Progress = 100 });
            card2.AssignedTests.Add(new TestAssignment { TestName = "Calibration", IconPath = IconLibrary["LASER"], Progress = 75 });
            card2.UpdateOverallProgress();
            allCards.Add(card2);

            // Continue for other cards, assigning appropriate icons and names
            var card3 = new ProcessCard
            {
                Id = "CARD-RBT003",
                Title = "ROBOTSKA ROK-V2",
                Subtitle = "Pratni podatek",
                Timestamp = DateTime.Now.AddHours(-12).ToString("dd:HH:mm:ss:ff"),
                Button1Icon = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z",
                Button2Icon = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z",
                Button3Icon = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z",
                BadgeVisibility = Visibility.Collapsed
            };
            card3.AssignedTests.Add(new TestAssignment { TestName = "Arm Calibration", IconPath = IconLibrary["BARVANJE"], Progress = 40 });
            card3.AssignedTests.Add(new TestAssignment { TestName = "Motion Test", IconPath = IconLibrary["BRUŠENJE"], Progress = 50 });
            card3.AssignedTests.Add(new TestAssignment { TestName = "Safety Check", IconPath = IconLibrary["ELEKTROPOLIRANJE"], Progress = 30 });
            card3.UpdateOverallProgress();
            allCards.Add(card3);

            var card4 = new ProcessCard
            {
                Id = "CARD-RBT004",
                Title = "ROBOTSKA ROK",
                Subtitle = "Pratni podatek",
                Timestamp = DateTime.Now.AddHours(-16).ToString("dd:HH:mm:ss:ff"),
                Button1Icon = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z",
                Button2Icon = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z",
                Button3Icon = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z",
                BadgeText = "Warning",
                BadgeColor = new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                BadgeVisibility = Visibility.Visible
            };
            card4.AssignedTests.Add(new TestAssignment { TestName = "Basic Arm Test", IconPath = IconLibrary["KRIVLJENJE"], Progress = 30 });
            card4.AssignedTests.Add(new TestAssignment { TestName = "Spin Check", IconPath = IconLibrary["LASER"], Progress = 50 });
            card4.UpdateOverallProgress();
            allCards.Add(card4);

            var card5 = new ProcessCard
            {
                Id = "CARD-SKV005",
                Title = "SONTTOLA KAYVOUTI",
                Subtitle = "Pratni podatek",
                Timestamp = DateTime.Now.AddHours(-8).ToString("dd:HH:mm:ss:ff"),
                Button1Icon = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z",
                Button2Icon = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z",
                Button3Icon = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z",
                BadgeVisibility = Visibility.Collapsed
            };
            card5.AssignedTests.Add(new TestAssignment { TestName = "Solar Test", IconPath = IconLibrary["BARVANJE"], Progress = 60 });
            card5.AssignedTests.Add(new TestAssignment { TestName = "Bolt Check", IconPath = IconLibrary["BRUŠENJE"], Progress = 50 });
            card5.AssignedTests.Add(new TestAssignment { TestName = "Panel Test", IconPath = IconLibrary["ELEKTROPOLIRANJE"], Progress = 40 });
            card5.UpdateOverallProgress();
            allCards.Add(card5);

            var card6 = new ProcessCard
            {
                Id = "CARD-SAS006",
                Title = "SASTAVA",
                Subtitle = "Pratni podatek",
                Timestamp = DateTime.Now.AddHours(-24).ToString("dd:HH:mm:ss:ff"),
                Button1Icon = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z",
                Button2Icon = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z",
                Button3Icon = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z",
                BadgeVisibility = Visibility.Collapsed
            };
            card6.AssignedTests.Add(new TestAssignment { TestName = "Sastava Test", IconPath = IconLibrary["KRIVLJENJE"], Progress = 20 });
            card6.UpdateOverallProgress();
            allCards.Add(card6);

            var card7 = new ProcessCard
            {
                Id = "CARD-SPF007",
                Title = "SOLARNI PANEL-FLEX",
                Subtitle = "Pratni podatek",
                Timestamp = DateTime.Now.AddHours(-30).ToString("dd:HH:mm:ss:ff"),
                Button1Icon = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z",
                Button2Icon = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z",
                Button3Icon = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z",
                BadgeVisibility = Visibility.Collapsed
            };
            card7.AssignedTests.Add(new TestAssignment { TestName = "Panel Test", IconPath = IconLibrary["LASER"], Progress = 10 });
            card7.AssignedTests.Add(new TestAssignment { TestName = "Flex Check", IconPath = IconLibrary["BARVANJE"], Progress = 40 });
            card7.UpdateOverallProgress();
            allCards.Add(card7);

            var card8 = new ProcessCard
            {
                Id = "CARD-RB2008",
                Title = "ROBOTSKA ROK-V2",
                Subtitle = "Pratni podatek",
                Timestamp = DateTime.Now.AddHours(-36).ToString("dd:HH:mm:ss:ff"),
                Button1Icon = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z",
                Button2Icon = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z",
                Button3Icon = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z",
                BadgeVisibility = Visibility.Collapsed
            };
            card8.AssignedTests.Add(new TestAssignment { TestName = "V2 Arm Test", IconPath = IconLibrary["BRUŠENJE"], Progress = 15 });
            card8.UpdateOverallProgress();
            allCards.Add(card8);
        }


        private void ToggleSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SearchFilterBar.Visibility == Visibility.Collapsed)
            {
                SearchFilterBar.Visibility = Visibility.Visible;
                // Optional: Focus the search box when it appears
                SearchBox.Focus();
            }
            else
            {
                SearchFilterBar.Visibility = Visibility.Collapsed;
            }
        }

        private void ApplyFilter(string filter)
        {
            currentFilter = filter;
            filteredCards.Clear();
            IEnumerable<ProcessCard> filtered = allCards;

            switch (filter)
            {
                case "COMPLETED":
                    filtered = allCards.Where(c => c.Status == "COMPLETED");
                    break;
                case "IN_PROGRESS":
                    filtered = allCards.Where(c => c.Status == "IN_PROGRESS");
                    break;
                case "CRITICAL":
                    filtered = allCards.Where(c => c.Status == "CRITICAL");
                    break;
                case "ALL":
                default:
                    filtered = allCards;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                string searchText = SearchBox.Text.ToLower();
                filtered = filtered.Where(c => c.Title.ToLower().Contains(searchText));
            }

            foreach (var card in filtered)
            {
                filteredCards.Add(card);
            }
        }

        private void FilterAll_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter("ALL");
        }

        private void FilterCompleted_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter("COMPLETED");
        }

        private void FilterInProgress_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter("IN_PROGRESS");
        }

        private void FilterCritical_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter("CRITICAL");
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchPlaceholder != null)
            {
                SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
            ApplyFilter(currentFilter);
        }

        private void Card_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is ProcessCard card)
            {
                // Toggle expanded state on click
                card.IsExpanded = !card.IsExpanded;
                e.Handled = true;
            }
        }

        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProcessCard card)
            {
                TouchFriendlyDialog.Show("Statistika", $"Prikazujem statistiko za: {card.Title}\nID: {card.Id}", false, this);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProcessCard card)
            {
                var editWindow = new EditCardWindow(card);
                editWindow.Owner = this;
                if (editWindow.ShowDialog() == true)
                {
                    card.OnPropertyChanged(nameof(card.Title));
                    card.OnPropertyChanged(nameof(card.Progress));
                    TouchFriendlyDialog.Show("Uspeh", "Spremembe shranjene!", false, this);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProcessCard card)
            {
                var result = TouchFriendlyDialog.Show("Potrditev brisanja", $"Ali ste prepričani, da želite izbrisati '{card.Title}'?\nID: {card.Id}", true, this);
                if (result == true)
                {
                    allCards.Remove(card);
                    ApplyFilter(currentFilter);
                    TouchFriendlyDialog.Show("Uspeh", $"Kartica izbrisana!\nID: {card.Id}", false, this);
                }
            }
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProcessCard card)
            {
                TouchFriendlyDialog.Show("Podrobnosti kartice", $"Naslov: {card.Title}\nStatus: {card.Status}\nNapredek: {card.Progress}%\nČasovni žig: {card.Timestamp}", false, this);
            }
        }

        private void AddNewProduct_Click(object sender, RoutedEventArgs e)
        {
            var newCard = new ProcessCard
            {
                // ID is auto-generated in constructor
                Title = "NOV IZDELEK",
                Subtitle = "Pratni podatek",
                Timestamp = DateTime.Now.ToString("dd:HH:mm:ss:ff"),
                Button1Icon = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z",
                Button2Icon = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z",
                Button3Icon = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z",
                BadgeVisibility = Visibility.Collapsed
            };
            newCard.UpdateOverallProgress();
            allCards.Insert(0, newCard);
            ApplyFilter(currentFilter);
            TouchFriendlyDialog.Show("Uspeh", "Nov izdelek dodan!", false, this);
        }

        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV datoteka (*.csv)|*.csv|Besedilna datoteka (*.txt)|*.txt",
                    DefaultExt = ".csv",
                    FileName = $"Kanban_Export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Naslov,Status,Napredek,Časovni žig");
                    foreach (var card in allCards)
                    {
                        sb.AppendLine($"{card.Title},{card.Status},{card.Progress}%,{card.Timestamp}");
                    }
                    File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);
                    TouchFriendlyDialog.Show("Izvoz uspešen", $"Podatki izvoženi v:\n{saveDialog.FileName}", false, this);
                }
            }
            catch (Exception ex)
            {
                TouchFriendlyDialog.Show("Napaka", $"Napaka pri izvozu: {ex.Message}", false, this);
            }
        }

        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            int total = allCards.Count;
            if (total == 0)
            {
                TouchFriendlyDialog.Show("Statistika", "Ni podatkov za prikaz statistike.", false, this);
                return;
            }

            int completed = allCards.Count(c => c.Status == "COMPLETED");
            int inProgress = allCards.Count(c => c.Status == "IN_PROGRESS");
            int critical = allCards.Count(c => c.Status == "CRITICAL");
            double avgProgress = allCards.Average(c => c.Progress);

            string stats = $"STATISTIKA KANBAN DESKE\n\n" +
                           $"Skupaj kartic: {total}\n" +
                           $"Dokončano: {completed} ({completed * 100.0 / total:F1}%)\n" +
                           $"V teku: {inProgress} ({inProgress * 100.0 / total:F1}%)\n" +
                           $"Kritično: {critical} ({critical * 100.0 / total:F1}%)\n\n" +
                           $"Povprečen napredek: {avgProgress:F1}%";

            TouchFriendlyDialog.Show("Statistika", stats, false, this);
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            var result = TouchFriendlyDialog.Show("Zapri", "Ali želite zapreti aplikacijo?", true, this);
            if (result == true)
            {
                Application.Current.Shutdown();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SearchBox.Focus();
            }
            else if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                AddNewProduct_Click(sender, e);
            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ExportData_Click(sender, e);
            }
        }

        private void AddIcon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProcessCard card)
            {
                if (card.AssignedTests.Count >= 10) return;

                var addWindow = new AddIconWindow();
                if (addWindow.ShowDialog() == true)
                {
                    card.AssignedTests.Add(new TestAssignment
                    {
                        TestName = addWindow.SelectedName,
                        IconPath = addWindow.SelectedIconPath,
                        Progress = 0
                    });
                    card.UpdateOverallProgress();
                }
            }
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ZoomSlider == null) return;

            ZoomLevel = ZoomSlider.Value;
            
            // Update tooltip
            if (ZoomToolTip != null)
            {
                ZoomToolTip.Content = $"{(int)(ZoomLevel * 100)}%";
            }

            // Apply intelligent zoom using LayoutTransform
            ApplyIntelligentZoom();
            
            // Save zoom level for persistence
            SaveZoomLevel();
        }

        private void ApplyIntelligentZoom()
        {
            if (ProcessCards == null) return;

            // Apply scaling to the ItemsControl using LayoutTransform
            // This ensures the layout engine recalculates everything properly
            ProcessCards.LayoutTransform = new ScaleTransform(ZoomLevel, ZoomLevel);
        }

        private void SaveZoomLevel()
        {
            try
            {
                Microsoft.Win32.Registry.SetValue(
                    @"HKEY_CURRENT_USER\Software\KanbanBoard",
                    "ZoomLevel",
                    ZoomLevel,
                    Microsoft.Win32.RegistryValueKind.String);
            }
            catch
            {
                // Silent fail - not critical
            }
        }

        private void LoadZoomLevel()
        {
            try
            {
                var value = Microsoft.Win32.Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\KanbanBoard",
                    "ZoomLevel",
                    "1.0");

                if (value != null && double.TryParse(value.ToString(), out double savedZoom))
                {
                    if (savedZoom >= 0.7 && savedZoom <= 1.5)
                    {
                        ZoomSlider.Value = savedZoom;
                    }
                }
            }
            catch
            {
                // Silent fail - use default
            }
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            // Suppress bounce effect at boundaries for smoother touch experience
            e.Handled = true;
        }

        private void Card_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Reset scale after press
            if (sender is FrameworkElement element)
            {
                var scaleTransform = new ScaleTransform(1.0, 1.0);
                element.RenderTransform = scaleTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        private void Card_TouchDown(object sender, TouchEventArgs e)
        {
            // Slight scale down for touch feedback (haptic-like)
            if (sender is FrameworkElement element)
            {
                var scaleTransform = new ScaleTransform(0.98, 0.98);
                element.RenderTransform = scaleTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        private void Card_TouchUp(object sender, TouchEventArgs e)
        {
            // Reset scale after touch
            if (sender is FrameworkElement element)
            {
                var scaleTransform = new ScaleTransform(1.0, 1.0);
                element.RenderTransform = scaleTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }
    }

    public class ProcessCard : INotifyPropertyChanged
    {
        private string id = string.Empty;
        private string title = string.Empty;
        private string subtitle = string.Empty;
        private double progress;
        private Brush progressColor = Brushes.Gray;
        private string timestamp = string.Empty;
        private string button1Icon = string.Empty;
        private Brush button1Color = Brushes.Gray;
        private string button2Icon = string.Empty;
        private Brush button2Color = Brushes.Gray;
        private string button3Icon = string.Empty;
        private Brush button3Color = Brushes.Gray;
        private string badgeText = string.Empty;
        private Brush badgeColor = Brushes.Transparent;
        private Visibility badgeVisibility;
        private string status = string.Empty;
        private bool isExpanded = false;
        private static Random random = new Random();

        public ProcessCard()
        {
            Id = GenerateUniqueId();
            AssignedTests = new ObservableCollection<TestAssignment>();
            AssignedTests.CollectionChanged += AssignedTests_CollectionChanged;
        }

        private static string GenerateUniqueId()
        {
            // Generate a unique ID using timestamp + random for uniqueness
            return $"CARD-{DateTime.Now:yyyyMMdd-HHmmss}-{random.Next(1000, 9999)}";
        }

        public string Id
        {
            get => id;
            set { id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Title
        {
            get => title;
            set { title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string Subtitle
        {
            get => subtitle;
            set { subtitle = value; OnPropertyChanged(nameof(Subtitle)); }
        }

        public double Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string ProgressText => $"{Progress:F0}%";

        public Brush ProgressColor
        {
            get => progressColor;
            set { progressColor = value; OnPropertyChanged(nameof(ProgressColor)); }
        }

        public string Timestamp
        {
            get => timestamp;
            set { timestamp = value; OnPropertyChanged(nameof(Timestamp)); }
        }

        public string Button1Icon
        {
            get => button1Icon;
            set { button1Icon = value; OnPropertyChanged(nameof(Button1Icon)); }
        }

        public Brush Button1Color
        {
            get => button1Color;
            set { button1Color = value; OnPropertyChanged(nameof(Button1Color)); }
        }

        public string Button2Icon
        {
            get => button2Icon;
            set { button2Icon = value; OnPropertyChanged(nameof(Button2Icon)); }
        }

        public Brush Button2Color
        {
            get => button2Color;
            set { button2Color = value; OnPropertyChanged(nameof(Button2Color)); }
        }

        public string Button3Icon
        {
            get => button3Icon;
            set { button3Icon = value; OnPropertyChanged(nameof(Button3Icon)); }
        }

        public Brush Button3Color
        {
            get => button3Color;
            set { button3Color = value; OnPropertyChanged(nameof(Button3Color)); }
        }

        public string BadgeText
        {
            get => badgeText;
            set { badgeText = value; OnPropertyChanged(nameof(BadgeText)); }
        }

        public Brush BadgeColor
        {
            get => badgeColor;
            set { badgeColor = value; OnPropertyChanged(nameof(BadgeColor)); }
        }

        public Visibility BadgeVisibility
        {
            get => badgeVisibility;
            set { badgeVisibility = value; OnPropertyChanged(nameof(BadgeVisibility)); }
        }

        public string Status
        {
            get => status;
            set { status = value; OnPropertyChanged(nameof(Status)); }
        }

        public ObservableCollection<TestAssignment> AssignedTests { get; }

        public bool CanAddMoreIcons => AssignedTests.Count < 10;

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AssignedTests_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (TestAssignment item in e.OldItems)
                {
                    item.PropertyChanged -= TestProgress_Changed;
                }
            }

            if (e.NewItems != null)
            {
                foreach (TestAssignment item in e.NewItems)
                {
                    item.PropertyChanged += TestProgress_Changed;
                }
            }

            UpdateOverallProgress();
            OnPropertyChanged(nameof(CanAddMoreIcons));
        }

        private void TestProgress_Changed(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TestAssignment.Progress))
            {
                UpdateOverallProgress();
            }
        }

        public void UpdateOverallProgress()
        {
            if (AssignedTests.Any())
            {
                Progress = AssignedTests.Average(t => t.Progress);
            }
            else
            {
                Progress = 0;
            }

            if (Progress >= 100)
            {
                ProgressColor = new SolidColorBrush(Color.FromRgb(107, 191, 89)); // green
                Status = "COMPLETED";
            }
            else if (Progress > 30)
            {
                ProgressColor = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // orange
                Status = "IN_PROGRESS";
            }
            else if (Progress > 0)
            {
                ProgressColor = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // red
                Status = "CRITICAL";
            }
            else
            {
                ProgressColor = new SolidColorBrush(Color.FromRgb(158, 158, 158)); // gray
                Status = "IN_PROGRESS";
            }

            UpdateColors();
        }

        private void UpdateColors()
        {
            Button1Color = ProgressColor;
            Button2Color = ProgressColor;
            Button3Color = ProgressColor;
            OnPropertyChanged(nameof(Button1Color));
            OnPropertyChanged(nameof(Button2Color));
            OnPropertyChanged(nameof(Button3Color));
        }

    }

    public class TestAssignment : INotifyPropertyChanged
    {
        private double progress;
        private string testName = string.Empty;
        private string iconPath = string.Empty;

        public string TestName
        {
            get => testName;
            set { testName = value; OnPropertyChanged(nameof(TestName)); }
        }

        public string IconPath
        {
            get => iconPath;
            set { iconPath = value; OnPropertyChanged(nameof(IconPath)); }
        }

        public double Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AngleToPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double progress = System.Convert.ToDouble(value);
            double angle = 360 * (progress / 100);
            double rad = Math.PI * angle / 180;
            
            // For 32x32 circles, radius is 15, center is 16,16
            double centerX = 16;
            double centerY = 16;
            double radius = 15;
            
            double x = centerX + radius * Math.Sin(rad);
            double y = centerY - radius * Math.Cos(rad);
            return new Point(x, y);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AngleToIsLargeArcConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double progress = System.Convert.ToDouble(value);
            return progress > 50;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Fixed: Now safely accepts double progress (0–100) and returns correct SolidColorBrush.
    /// </summary>
    public class ProgressToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Safety: Only accept double
            if (value is not double progress || progress < 0 || progress > 100)
                return Brushes.Gray;

            // Color logic: <30 → Red, 30–70 → Orange, >70 → Green
            if (progress < 30)
                return Brushes.Red;
            if (progress <= 70)
                return Brushes.Orange;
            return Brushes.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts progress percentage and actual width to proportional width
    /// </summary>
    public class ProgressWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[0] is not double progress || values[1] is not double actualWidth)
                return 0.0;

            if (progress < 0) progress = 0;
            if (progress > 100) progress = 100;

            // Calculate proportional width based on actual container width
            return (progress / 100.0) * actualWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts expansion state and zoom level to progress bar height
    /// </summary>
    public class ProgressHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return 4.0;

            bool isExpanded = values[0] is bool expanded && expanded;
            double zoomLevel = values[1] is double zoom ? zoom : 1.0;

            // Base height: 4px normal, 8px expanded
            double baseHeight = isExpanded ? 8.0 : 4.0;
            
            // Scale with zoom level
            return baseHeight * zoomLevel;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts progress percentage to dash array for circular progress ring
    /// StrokeDashArray in WPF uses units relative to the stroke thickness
    /// We need to normalize the circumference to work with this unit system
    /// </summary>
    public class ProgressToDashArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double progress || progress < 0 || progress > 100)
                return new DoubleCollection { 0, 100 };

            if (progress == 0)
                return new DoubleCollection { 0, 100 };

            if (progress >= 100)
                return new DoubleCollection { 100, 0 };

            // For a 52px diameter circle with 4px stroke:
            // The stroke center is at radius = (52/2) - (4/2) = 24px
            // Circumference = 2 * PI * r = 2 * PI * 24 ≈ 150.8 px
            
            // StrokeDashArray uses "dash units" where 1 unit = stroke thickness
            // So we need to convert from pixels to dash units
            double strokeThickness = 4.0;
            double radius = 24.0;
            double circumferenceInPixels = 2.0 * Math.PI * radius;
            
            // Convert to dash units (divide by stroke thickness)
            double circumferenceInDashUnits = circumferenceInPixels / strokeThickness;
            
            // Calculate filled and unfilled portions
            double filledDashUnits = (progress / 100.0) * circumferenceInDashUnits;
            double unfilledDashUnits = circumferenceInDashUnits - filledDashUnits;
            
            return new DoubleCollection { filledDashUnits, unfilledDashUnits };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converter for scaling values based on zoom level
    /// Parameter should be the base value to scale
    /// </summary>
    public class ZoomLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double zoomLevel || parameter == null)
                return parameter ?? 1.0;

            if (double.TryParse(parameter.ToString(), out double baseValue))
            {
                return baseValue * zoomLevel;
            }

            return baseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class TouchFriendlyDialog : Window
    {
        public bool DialogResultValue { get; private set; }

        public TouchFriendlyDialog(string title, string message, bool showCancel = false)
        {
            Title = title;
            Width = 700;
            Height = showCancel ? 400 : 350;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(248, 249, 250));
            WindowStyle = WindowStyle.None;
            BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            BorderThickness = new Thickness(2);

            var mainStack = new StackPanel { Margin = new Thickness(40, 40, 40, 40) };

            // Title
            mainStack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(31, 41, 55)),
                Margin = new Thickness(0, 0, 0, 30),
                TextWrapping = TextWrapping.Wrap
            });

            // Message
            mainStack.Children.Add(new TextBlock
            {
                Text = message,
                FontSize = 22,
                Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99)),
                Margin = new Thickness(0, 0, 0, 40),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 32
            });

            // Buttons panel
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            if (showCancel)
            {
                // No button
                var noBtn = CreateTouchButton("NE", new SolidColorBrush(Color.FromRgb(239, 68, 68)));
                noBtn.Click += (s, e) => { DialogResultValue = false; DialogResult = false; Close(); };
                buttonPanel.Children.Add(noBtn);

                // Yes button
                var yesBtn = CreateTouchButton("DA", new SolidColorBrush(Color.FromRgb(34, 197, 94)));
                yesBtn.Click += (s, e) => { DialogResultValue = true; DialogResult = true; Close(); };
                buttonPanel.Children.Add(yesBtn);
            }
            else
            {
                // OK button
                var okBtn = CreateTouchButton("V REDU", new SolidColorBrush(Color.FromRgb(59, 130, 246)));
                okBtn.Click += (s, e) => { DialogResultValue = true; DialogResult = true; Close(); };
                buttonPanel.Children.Add(okBtn);
            }

            mainStack.Children.Add(buttonPanel);
            Content = mainStack;
        }

        private Button CreateTouchButton(string text, Brush background)
        {
            var btn = new Button
            {
                Content = text,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(60, 25, 60, 25),
                Margin = new Thickness(15, 0, 15, 0),
                Background = background,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                MinWidth = 200,
                MinHeight = 80
            };

            // Apply rounded corners via template
            var template = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetValue(Border.BackgroundProperty, background);
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(12));
            factory.SetValue(Border.PaddingProperty, new Thickness(60, 25, 60, 25));
            var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AppendChild(contentFactory);
            template.VisualTree = factory;
            btn.Template = template;

            return btn;
        }

        public static bool? Show(string title, string message, bool showCancel = false, Window? owner = null)
        {
            var dialog = new TouchFriendlyDialog(title, message, showCancel);
            if (owner != null)
                dialog.Owner = owner;
            return dialog.ShowDialog();
        }
    }

    public class CardDetailsWindow : Window
    {
        public CardDetailsWindow(ProcessCard card)
        {
            Title = "Podrobnosti kartice";
            Width = 700;
            MinHeight = 600;
            MaxHeight = 900;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
            ResizeMode = ResizeMode.CanResize;

            var mainStack = new StackPanel { Margin = new Thickness(30, 30, 30, 30) };
            mainStack.Children.Add(new TextBlock
            {
                Text = card.Title,
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            });

            mainStack.Children.Add(CreateDetailRow("ID:", card.Id));
            mainStack.Children.Add(CreateDetailRow("Status:", card.Status));
            mainStack.Children.Add(CreateDetailRow("Napredek:", $"{card.Progress}%"));
            mainStack.Children.Add(CreateDetailRow("Časovni žig:", card.Timestamp));
            mainStack.Children.Add(CreateDetailRow("Podnaslov:", card.Subtitle));

            var progressGrid = new Grid { Margin = new Thickness(0, 20, 0, 0) };
            progressGrid.Children.Add(new Border
            {
                Height = 20,
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                CornerRadius = new CornerRadius(10, 10, 10, 10)
            });
            progressGrid.Children.Add(new Border
            {
                Height = 20,
                Background = card.ProgressColor,
                CornerRadius = new CornerRadius(10, 10, 10, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = (card.Progress / 100.0) * 540
            });
            mainStack.Children.Add(progressGrid);

            mainStack.Children.Add(new TextBlock
            {
                Text = "Tests:",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 10)
            });

            foreach (var test in card.AssignedTests)
            {
                mainStack.Children.Add(CreateDetailRow($"{test.TestName}:", $"{test.Progress}%"));
            }

            var closeBtn = new Button
            {
                Content = "Zapri",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(50, 20, 50, 20),
                Margin = new Thickness(0, 30, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Center,
                MinWidth = 180,
                MinHeight = 60
            };
            closeBtn.Click += (s, e) => Close();
            mainStack.Children.Add(closeBtn);

            Content = new ScrollViewer
            {
                Content = mainStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        private StackPanel CreateDetailRow(string label, string value)
        {
            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 10)
            };
            stack.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Width = 150
            });
            stack.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 16
            });
            return stack;
        }
    }

    public class AddIconWindow : Window
    {
        public string SelectedName { get; private set; } = string.Empty;
        public string SelectedIconPath { get; private set; } = string.Empty;

        public AddIconWindow()
        {
            Title = "Dodaj nov icon";
            Width = 500;
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));

            var stack = new StackPanel { Margin = new Thickness(30, 30, 30, 30) };
            stack.Children.Add(new TextBlock { Text = "Ime procesa:", FontSize = 16, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 8) });
            var nameBox = new TextBox { FontSize = 18, Padding = new Thickness(10, 8, 10, 8) };
            stack.Children.Add(nameBox);

            stack.Children.Add(new TextBlock { Text = "Izberi icon:", FontSize = 16, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 20, 0, 8) });
            var combo = new ComboBox { FontSize = 18, Padding = new Thickness(10, 8, 10, 8) };
            foreach (var key in MainWindow.IconLibrary.Keys)
            {
                combo.Items.Add(key);
            }
            combo.SelectedIndex = 0;
            stack.Children.Add(combo);

            var btn = new Button 
            { 
                Content = "Dodaj", 
                Margin = new Thickness(0, 30, 0, 0), 
                Padding = new Thickness(40, 18, 40, 18),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Color.FromRgb(34, 197, 94)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Center,
                MinWidth = 180,
                MinHeight = 60
            };
            btn.Click += (s, e) =>
            {
                SelectedName = nameBox.Text;
                if (string.IsNullOrWhiteSpace(SelectedName))
                {
                    TouchFriendlyDialog.Show("Napaka", "Vnesite ime procesa.", false, this);
                    return;
                }

                if (combo.SelectedItem is string selectedKey)
                {
                    SelectedIconPath = MainWindow.IconLibrary[selectedKey];
                    DialogResult = true;
                }
                else
                {
                    TouchFriendlyDialog.Show("Napaka", "Izberite icon.", false, this);
                }
            };
            stack.Children.Add(btn);

            Content = stack;
        }
    }

    public class EditCardWindow : Window
    {
        private ProcessCard card;
        private TextBox titleBox;
        private StackPanel testsPanel;

        public EditCardWindow(ProcessCard card)
        {
            this.card = card;
            Title = "Uredi kartico";
            Width = 900;
            MinHeight = 600;
            MaxHeight = 900;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
            ResizeMode = ResizeMode.CanResize;

            var mainStack = new StackPanel { Margin = new Thickness(30, 30, 30, 30) };
            mainStack.Children.Add(new TextBlock { Text = "Naslov:", FontSize = 18, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 8) });
            titleBox = new TextBox { Text = card.Title, FontSize = 18, Padding = new Thickness(10, 8, 10, 8) };
            mainStack.Children.Add(titleBox);

            var addBtn = new Button 
            { 
                Content = "Dodaj nov icon", 
                Margin = new Thickness(0, 20, 0, 0),
                Padding = new Thickness(25, 12, 25, 12),
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Background = new SolidColorBrush(Color.FromRgb(34, 197, 94)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                MinHeight = 50
            };
            addBtn.Click += AddNewIcon_Click;
            mainStack.Children.Add(addBtn);

            testsPanel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            mainStack.Children.Add(new TextBlock { Text = "Dodeljeni procesi:", FontSize = 18, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 20, 0, 10) });
            mainStack.Children.Add(testsPanel);

            RefreshRows();

            var btnStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 30, 0, 0)
            };
            var saveBtn = new Button 
            { 
                Content = "Shrani", 
                Padding = new Thickness(50, 18, 50, 18),
                Margin = new Thickness(10, 0, 10, 0),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                MinWidth = 180,
                MinHeight = 60
            };
            saveBtn.Click += Save_Click;
            btnStack.Children.Add(saveBtn);

            var cancelBtn = new Button 
            { 
                Content = "Prekliči", 
                Padding = new Thickness(50, 18, 50, 18),
                Margin = new Thickness(10, 0, 10, 0),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Color.FromRgb(156, 163, 175)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                MinWidth = 180,
                MinHeight = 60
            };
            cancelBtn.Click += (s, e) => { DialogResult = false; Close(); };
            btnStack.Children.Add(cancelBtn);

            mainStack.Children.Add(btnStack);
            Content = new ScrollViewer { Content = mainStack };
        }

        private void AddNewIcon_Click(object sender, RoutedEventArgs e)
        {
            if (card.AssignedTests.Count >= 10)
            {
                TouchFriendlyDialog.Show("Omejitev", "Maksimalno število ikon je 10.", false, this);
                return;
            }

            var addWindow = new AddIconWindow();
            if (addWindow.ShowDialog() == true)
            {
                var newTest = new TestAssignment
                {
                    TestName = addWindow.SelectedName,
                    IconPath = addWindow.SelectedIconPath,
                    Progress = 0
                };
                card.AssignedTests.Add(newTest);
                RefreshRows();
            }
        }

        private void RefreshRows()
        {
            testsPanel.Children.Clear();
            foreach (var test in card.AssignedTests)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 8), Tag = test };

                var nameTb = new TextBox { Text = test.TestName, Width = 200, FontSize = 16, Padding = new Thickness(8, 6, 8, 6) };
                row.Children.Add(nameTb);

                var combo = new ComboBox { Width = 180, FontSize = 16, Padding = new Thickness(8, 6, 8, 6), Margin = new Thickness(10, 0, 10, 0) };
                foreach (var key in MainWindow.IconLibrary.Keys)
                    combo.Items.Add(key);

                var currentKey = MainWindow.IconLibrary.FirstOrDefault(x => x.Value == test.IconPath).Key;
                combo.SelectedItem = currentKey ?? MainWindow.IconLibrary.Keys.First();
                row.Children.Add(combo);

                var slider = new Slider { Value = test.Progress, Minimum = 0, Maximum = 100, Width = 150, Height = 30 };
                row.Children.Add(slider);

                var valText = new TextBlock { Text = $"{test.Progress:F0}%", Margin = new Thickness(15, 0, 15, 0), VerticalAlignment = VerticalAlignment.Center, FontSize = 16, FontWeight = FontWeights.Bold, MinWidth = 50 };
                slider.ValueChanged += (s, ev) => valText.Text = $"{slider.Value:F0}%";
                row.Children.Add(valText);

                var delBtn = new Button { Content = "Izbriši", Margin = new Thickness(5, 0, 5, 0), Padding = new Thickness(15, 8, 15, 8), FontSize = 14, Background = new SolidColorBrush(Color.FromRgb(239, 68, 68)), Foreground = Brushes.White, BorderThickness = new Thickness(0), Cursor = Cursors.Hand };
                delBtn.Click += (s, ev) => { card.AssignedTests.Remove(test); RefreshRows(); };
                row.Children.Add(delBtn);

                var upBtn = new Button { Content = "↑", Margin = new Thickness(5, 0, 5, 0), Padding = new Thickness(15, 8, 15, 8), FontSize = 16, FontWeight = FontWeights.Bold, Background = new SolidColorBrush(Color.FromRgb(59, 130, 246)), Foreground = Brushes.White, BorderThickness = new Thickness(0), Cursor = Cursors.Hand };
                upBtn.Click += (s, ev) => MoveItem(test, -1);
                row.Children.Add(upBtn);

                var downBtn = new Button { Content = "↓", Margin = new Thickness(5, 0, 5, 0), Padding = new Thickness(15, 8, 15, 8), FontSize = 16, FontWeight = FontWeights.Bold, Background = new SolidColorBrush(Color.FromRgb(59, 130, 246)), Foreground = Brushes.White, BorderThickness = new Thickness(0), Cursor = Cursors.Hand };
                downBtn.Click += (s, ev) => MoveItem(test, 1);
                row.Children.Add(downBtn);

                testsPanel.Children.Add(row);
            }
        }

        private void MoveItem(TestAssignment test, int direction)
        {
            int index = card.AssignedTests.IndexOf(test);
            int newIndex = index + direction;
            if (newIndex >= 0 && newIndex < card.AssignedTests.Count)
            {
                card.AssignedTests.Move(index, newIndex);
                RefreshRows();
            }
        }


    

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            card.Title = titleBox.Text;

            foreach (var row in testsPanel.Children.OfType<StackPanel>())
            {
                var test = (TestAssignment)row.Tag;
                var children = row.Children;
                test.TestName = ((TextBox)children[0]).Text;

                string key = ((ComboBox)children[1]).SelectedItem as string ?? string.Empty;
                test.IconPath = MainWindow.IconLibrary.TryGetValue(key, out var path) ? path : string.Empty;

                test.Progress = ((Slider)children[2]).Value;
            }

            card.UpdateOverallProgress();
            DialogResult = true;
            Close();
        }
    }
}