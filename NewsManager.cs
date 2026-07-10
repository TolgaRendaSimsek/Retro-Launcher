using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RetroLauncher
{
    public class NewsItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string Category { get; set; } = "General";
        public string Author { get; set; } = "RetroLauncher Team";
        public string PublishDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public class NewsManager
    {
        private static readonly string NewsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "news.json");
        private static readonly object FileLock = new object();
        private List<NewsItem> _newsList = new();

        private static NewsManager? _instance;
        public static NewsManager Instance => _instance ??= new NewsManager();

        public NewsManager()
        {
            LoadNews();
        }

        public List<NewsItem> NewsList => _newsList;

        public void LoadNews()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(NewsPath))
                    {
                        string json = File.ReadAllText(NewsPath);
                        _newsList = JsonSerializer.Deserialize<List<NewsItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<NewsItem>();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading news: {ex.Message}");
                }

                // Default mock news items if news.json does not exist
                _newsList = new List<NewsItem>
                {
                    new NewsItem
                    {
                        Title = "RetroLauncher v1.0 Released!",
                        Content = "We are excited to announce the first official release of RetroLauncher, your ultimate companion for PlayStation and retro game emulation management.",
                        Category = "Announcements",
                        Author = "Admin",
                        PublishDate = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    new NewsItem
                    {
                        Title = "Achievement System Integration Guide",
                        Content = "Track your retro progression natively! You can now unlock achievements locally and showcase your rarest achievements directly on your Steam-style profile.",
                        Category = "Guides",
                        Author = "Editor",
                        PublishDate = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd HH:mm:ss")
                    }
                };
                SaveNews();
            }
        }

        public void SaveNews()
        {
            lock (FileLock)
            {
                try
                {
                    string json = JsonSerializer.Serialize(_newsList, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(NewsPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving news: {ex.Message}");
                }
            }
        }

        public void AddNewsItem(NewsItem item)
        {
            if (item == null) return;
            if (string.IsNullOrEmpty(item.Id)) item.Id = Guid.NewGuid().ToString();
            
            lock (FileLock)
            {
                _newsList.Add(item);
                SaveNews();
            }
        }

        public bool EditNewsItem(string id, NewsItem updatedItem)
        {
            if (updatedItem == null) return false;
            
            lock (FileLock)
            {
                var existing = _newsList.FirstOrDefault(n => n.Id == id);
                if (existing != null)
                {
                    existing.Title = updatedItem.Title;
                    existing.Content = updatedItem.Content;
                    existing.Category = updatedItem.Category;
                    existing.Author = updatedItem.Author;
                    existing.PublishDate = updatedItem.PublishDate;
                    
                    SaveNews();
                    return true;
                }
            }
            return false;
        }

        public bool DeleteNewsItem(string id)
        {
            lock (FileLock)
            {
                var item = _newsList.FirstOrDefault(n => n.Id == id);
                if (item != null)
                {
                    _newsList.Remove(item);
                    SaveNews();
                    return true;
                }
            }
            return false;
        }

        public List<NewsItem> GetLatestNews(int count)
        {
            lock (FileLock)
            {
                return _newsList
                    .OrderByDescending(n => n.PublishDate)
                    .Take(count)
                    .ToList();
            }
        }

        public List<NewsItem> FilterNewsByCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) return _newsList;

            lock (FileLock)
            {
                return _newsList
                    .Where(n => string.Equals(n.Category, category, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(n => n.PublishDate)
                    .ToList();
            }
        }
    }
}
