using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

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

        public async Task<int> GetNextItemIdAsync()
        {
            var items = await LoadDataAsync();
            return items.Any() ? items.Max(i => i.ItemId) + 1 : 1;
        }
        public async Task<bool> CheckItemAsync(int itemId)
        {
            try
            {
                var items = await LoadDataAsync();
                var itemToCheck = items.FirstOrDefault(i => i.ItemId == itemId);

                if (itemToCheck != null)
                {
                    itemToCheck.IsChecked = !itemToCheck.IsChecked;
                    await SaveDataAsync(items);
                    return true;
                }
                else
                {
                    throw new Exception("Item not found");
                }
            }
            catch (Exception error)
            {
                throw new Exception($"Error while trying to check item: {error.Message}");
            }
        }
        public async Task<bool> MuteItemAsync(int itemId)
        {
            try
            {
                var items = await LoadDataAsync();
                var itemToMute = items.FirstOrDefault(i => i.ItemId == itemId);

                if (itemToMute != null)
                {
                    itemToMute.IsMuted = !itemToMute.IsMuted;
                    await SaveDataAsync(items);
                    return true;
                }
                else
                {
                    throw new Exception("Item not found");
                }
            }
            catch (Exception error)
            {
                throw new Exception($"Error while trying to mute item: {error.Message}");
            }
        }
        public async Task<bool> DeleteItemAsync(int itemId)
        {
            try
            {
                var items = await LoadDataAsync();
                var itemToDelete = items.FirstOrDefault(i => i.ItemId == itemId);

                if (itemToDelete != null)
                {
                    items.Remove(itemToDelete);
                    await SaveDataAsync(items);
                    return true;
                }
                else
                {
                    throw new Exception("Item not found");
                }
            }
            catch (Exception error)
            {
                throw new Exception($"Error while trying to delete item: {error.Message}");
            }
        }
    }

    public class CalendarItem
    {
        public int ItemId { get; set; }
        public string Note { get; set; }
        public string Title { get; set; }
        public DateTime Time { get; set; }
        public bool IsChecked { get; set; }
        public bool IsMuted { get; set; }
    }
}
