using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Calender.Services
{
    public class DataService
    {
        private const string FILE_NAME = "calendar_data.json";
        private string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FILE_NAME);

        public async Task SaveDataAsync(List<CalendarItem> items)
        {
            var json = JsonSerializer.Serialize(items);
            await File.WriteAllTextAsync(FilePath, json);
        }

        public async Task<List<CalendarItem>> LoadDataAsync()
        {
            if (!File.Exists(FilePath))
            {
                return new List<CalendarItem>();
            }

            var json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<List<CalendarItem>>(json) ?? new List<CalendarItem>();
        }

        public int GetNextItemId(List<CalendarItem> items)
        {
            return items.Any() ? items.Max(i => i.ItemId) + 1 : 1;
        }
    }

    public class CalendarItem
    {
        public int ItemId { get; set; }
        public string Note { get; set; }
        public string Title { get; set; }
        public DateTime Time { get; set; }
        public string NotificationHour { get; set; }
        public bool IsChecked { get; set; }
        public bool IsMuted { get; set; }
    }
}
