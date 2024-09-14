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
        private CalendarItem _itemBeingEdited = null;
        private bool _isTimeTextChanging = false;

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

            int totalTasks = itemsForDate.Count;
            TasksCounterTextBlock.Text = $"{totalTasks} task{(totalTasks != 1 ? "s" : "")}";


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
                calendarItemControl.EditItem += CalendarItemControl_EditItem;

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

        private void CalendarItemControl_EditItem(object sender, int itemId)
        {
            _itemBeingEdited = _calendarItems.FirstOrDefault(i => i.ItemId == itemId);

            if (_itemBeingEdited != null)
            {
                // Populate the input fields with the item's data
                txtNote.Text = _itemBeingEdited.Title;
                txtTime.Text = _itemBeingEdited.NotificationHour;

                // Change the 'Add' button icon to 'Save'
                btnAddIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Save;

                // Optionally, focus on the note input field
                txtNote.Focus();
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

            if (_itemBeingEdited != null)
            {
                // Update the existing item
                _itemBeingEdited.Title = txtNote.Text;
                _itemBeingEdited.NotificationHour = txtTime.Text;
                _itemBeingEdited.Time = selectedDate; // Update the date in case it has changed

                await _dataService.SaveDataAsync(_calendarItems);
                DisplayCalendarItems(selectedDate);

                // Reset the form fields
                txtNote.Text = string.Empty;
                txtTime.Text = string.Empty;
                lblNote.Visibility = Visibility.Visible;
                lblTime.Visibility = Visibility.Visible;

                // Reset the 'Add' button icon to 'Plus'
                btnAddIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.PlusCircle;

                // Clear the editing item
                _itemBeingEdited = null;
            }
            else
            {
                // Add new item
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

                // Reset the form fields
                txtNote.Text = string.Empty;
                txtTime.Text = string.Empty;
                lblNote.Visibility = Visibility.Visible;
                lblTime.Visibility = Visibility.Visible;
            }
        }
        private void txtTime_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
                return;
            }

            // Get the current text including the new character
            string currentText = txtTime.Text.Remove(txtTime.SelectionStart, txtTime.SelectionLength);
            currentText = currentText.Insert(txtTime.SelectionStart, e.Text);

            // Remove any existing colons
            string input = currentText.Replace(":", "");

            // Limit to 4 digits
            if (input.Length > 4)
            {
                e.Handled = true;
                return;
            }

            e.Handled = false;
        }

        private void txtTime_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Handle backspace to correctly remove characters
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                int selectionStart = txtTime.SelectionStart;

                if (selectionStart > 0)
                {
                    // Remove character before cursor
                    string text = txtTime.Text.Remove(selectionStart - 1, 1);
                    txtTime.Text = text;
                    txtTime.SelectionStart = selectionStart - 1;
                }

                e.Handled = true;
            }
        }

        private void txtTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isTimeTextChanging)
                return;

            _isTimeTextChanging = true;

            lblTime.Visibility = string.IsNullOrEmpty(txtTime.Text) ? Visibility.Visible : Visibility.Collapsed;

            string text = txtTime.Text.Replace(":", "");
            int selectionStart = txtTime.SelectionStart;

            // Auto-insert colon after two digits
            if (text.Length == 4)
            {
                text = text.Insert(2, ":");
            }

            txtTime.Text = text;
            txtTime.SelectionStart = selectionStart;

            // Validate the input
            if (TimeSpan.TryParseExact(txtTime.Text, "hh\\:mm", null, out TimeSpan time))
            {
                // Valid time format
                txtTime.Foreground = Brushes.Black;
            }
            else
            {
                // Invalid time format
                txtTime.Foreground = Brushes.Red;
            }

            _isTimeTextChanging = false;
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

        private void PreviousDay_Click(object sender, RoutedEventArgs e)
        {
            if (MyCalendar.SelectedDate.HasValue)
            {
                DateTime previousDate = MyCalendar.SelectedDate.Value.AddDays(-1);
                MyCalendar.SelectedDate = previousDate;
                UpdateDateDisplay(previousDate);
            }
        }

        private void NextDay_Click(object sender, RoutedEventArgs e)
        {
            if (MyCalendar.SelectedDate.HasValue)
            {
                DateTime nextDate = MyCalendar.SelectedDate.Value.AddDays(1);
                MyCalendar.SelectedDate = nextDate;
                UpdateDateDisplay(nextDate);
            }
        }
    }
}
