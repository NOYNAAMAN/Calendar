using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Calender.Services;
using Calender.UserControls;

namespace Calender
{
    public partial class MainWindow : Window
    {
        private readonly DataService _dataService;
        private List<CalendarItem> _calendarItems;

        public MainWindow()
        {
            InitializeComponent();
            _dataService = new DataService();
            LoadData();
        }

        private async void LoadData()
        {
            _calendarItems = await _dataService.LoadDataAsync();
            DisplayCalendarItems();
        }

        private void DisplayCalendarItems()
        {
            var calendarGrid = (StackPanel)FindName("calendarItemsPanel");

            calendarGrid.Children.Clear();

            foreach (var item in _calendarItems)
            {
                var calendarItemControl = new Item
                {
                    ItemId = item.ItemId,
                    Title = item.Title,
                    Time = $"{item.Time:HH:mm} - {item.Time.AddMinutes(30):HH:mm}",
                    Color = new SolidColorBrush((item.IsChecked ? Colors.LightPink : Colors.White)),
                    Icon = item.IsChecked ? FontAwesome.WPF.FontAwesomeIcon.CheckCircle : FontAwesome.WPF.FontAwesomeIcon.CircleThin,
                    IconBell = item.IsMuted ? FontAwesome.WPF.FontAwesomeIcon.BellSlash : FontAwesome.WPF.FontAwesomeIcon.Bell
                };

                calendarItemControl.DeleteItem += CalendarItemControl_DeleteItem;
                calendarGrid.Children.Add(calendarItemControl);
            }
        }
        private async void CalendarItemControl_DeleteItem(object sender, int itemId)
        {
            if (await _dataService.DeleteItemAsync(itemId))
            {
                _calendarItems.RemoveAll(item => item.ItemId == itemId);
                DisplayCalendarItems();
            }
            else
            {
                MessageBox.Show("Error deleting the item.");
            }
        }
        private async void SaveCalendarItem(CalendarItem newItem)
        {
            _calendarItems.Add(newItem);
            await _dataService.SaveDataAsync(_calendarItems);
            DisplayCalendarItems();
        }

        private void AddNewItem(object sender, RoutedEventArgs e)
        {
            var newItem = new CalendarItem
            {
                ItemId = _dataService.GetNextItemIdAsync().Result,
                Title = txtNote.Text,
                Time = DateTime.Parse(txtTime.Text),
                IsChecked = false,
                IsMuted = false
            };

            SaveCalendarItem(newItem);
        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void lblNote_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtNote.Focus();
        }

        private void lblTime_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtTime.Focus();
        }

        private void txtNote_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNote.Text) && txtNote.Text.Length > 0)
                lblNote.Visibility = Visibility.Collapsed;
            else
                lblNote.Visibility = Visibility.Visible;
        }

        private void txtTime_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtTime.Text) && txtTime.Text.Length > 0)
                lblTime.Visibility = Visibility.Collapsed;
            else
                lblTime.Visibility = Visibility.Visible;
        }

    }
}