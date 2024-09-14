using System;
using System.Collections.Generic;
using System.Linq;
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
            DateTime today = DateTime.Today;
            MyCalendar.SelectedDate = today;
            UpdateDateDisplay(today);
        }

        private async void LoadData()
        {
            _calendarItems = await _dataService.LoadDataAsync();
            DisplayCalendarItems(MyCalendar.SelectedDate ?? DateTime.Today);
        }

        private void DisplayCalendarItems(DateTime selectedDate)
        {
            calendarItemsPanel.Children.Clear();

            var itemsForDate = _calendarItems
                .Where(item => item.Time.Date == selectedDate.Date)
                .ToList();

            foreach (var item in itemsForDate)
            {
                var calendarItemControl = new Item
                {
                    ItemId = item.ItemId,
                    Title = item.Title,
                    Time = item.NotificationHour,
                    Color = new SolidColorBrush(item.IsChecked ? Colors.LightPink : Colors.White),
                    Icon = item.IsChecked ? FontAwesome.WPF.FontAwesomeIcon.CheckCircle : FontAwesome.WPF.FontAwesomeIcon.CircleThin,
                    IconBell = item.IsMuted ? FontAwesome.WPF.FontAwesomeIcon.BellSlash : FontAwesome.WPF.FontAwesomeIcon.Bell
                };

                // Attach event handlers
                calendarItemControl.DeleteItem += CalendarItemControl_DeleteItem;
                calendarItemControl.CheckItem += CalendarItemControl_CheckItem;
                calendarItemControl.MuteItem += CalendarItemControl_MuteItem;

                calendarItemsPanel.Children.Add(calendarItemControl);
            }
        }

        private async void CalendarItemControl_DeleteItem(object sender, int itemId)
        {
            var item = _calendarItems.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null)
            {
                _calendarItems.Remove(item);
                await _dataService.SaveDataAsync(_calendarItems);
                DisplayCalendarItems(MyCalendar.SelectedDate ?? DateTime.Today);
            }
            else
            {
                MessageBox.Show("Item not found.");
            }
        }

        private async void CalendarItemControl_CheckItem(object sender, int itemId)
        {
            var item = _calendarItems.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null)
            {
                item.IsChecked = !item.IsChecked;
                await _dataService.SaveDataAsync(_calendarItems);
                DisplayCalendarItems(MyCalendar.SelectedDate ?? DateTime.Today);
            }
            else
            {
                MessageBox.Show("Item not found.");
            }
        }

        private async void CalendarItemControl_MuteItem(object sender, int itemId)
        {
            var item = _calendarItems.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null)
            {
                item.IsMuted = !item.IsMuted;
                await _dataService.SaveDataAsync(_calendarItems);
                DisplayCalendarItems(MyCalendar.SelectedDate ?? DateTime.Today);
            }
            else
            {
                MessageBox.Show("Item not found.");
            }
        }

        private async void AddNewItem(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtNote.Text))
            {
                MessageBox.Show("Please enter a note.");
                return;
            }

            if (!TimeSpan.TryParseExact(txtTime.Text, "hh\\:mm", null, out TimeSpan notificationTime))
            {
                MessageBox.Show("Please enter a valid time in HH:mm format.");
                return;
            }

            DateTime selectedDate = MyCalendar.SelectedDate ?? DateTime.Today;

            var newItem = new CalendarItem
            {
                ItemId = _dataService.GetNextItemId(_calendarItems),
                Title = txtNote.Text,
                Time = selectedDate,
                NotificationHour = txtTime.Text,
                IsChecked = false,
                IsMuted = false
            };

            _calendarItems.Add(newItem);
            await _dataService.SaveDataAsync(_calendarItems);
            DisplayCalendarItems(selectedDate);

            txtNote.Text = string.Empty;
            txtTime.Text = string.Empty;
            lblNote.Visibility = Visibility.Visible;
            lblTime.Visibility = Visibility.Visible;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
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

        private void txtNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblNote.Visibility = string.IsNullOrEmpty(txtNote.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblTime.Visibility = string.IsNullOrEmpty(txtTime.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyCalendar.SelectedDate.HasValue)
            {
                DateTime selectedDate = MyCalendar.SelectedDate.Value;
                UpdateDateDisplay(selectedDate);
            }
        }

        private void UpdateDateDisplay(DateTime date)
        {
            DayTextBlock.Text = date.Day.ToString();
            MonthTextBlock.Text = date.ToString("MMMM");
            DayOfWeekTextBlock.Text = date.ToString("dddd");
            DisplayCalendarItems(date);
        }
    }
}
