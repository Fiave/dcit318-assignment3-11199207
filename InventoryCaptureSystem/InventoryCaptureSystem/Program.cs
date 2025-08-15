using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

// Marker Interface
public interface IInventoryEntity
{
    int Id { get; }
}

// Immutable Inventory Record
public record InventoryItem(int Id, string Name, int Quantity, DateTime DateAdded) : IInventoryEntity;

namespace InventoryCaptureSystem
{
    // Generic Inventory Logger
    public class InventoryLogger<T> where T : IInventoryEntity
    {
        private List<T> _log = new();
        private readonly string _filePath;

        public InventoryLogger(string filePath)
        {
            _filePath = filePath;
        }

        public void Add(T item) => _log.Add(item);

        public List<T> GetAll() => new List<T>(_log);

        public void SaveToFile()
        {
            try
            {
                using var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write);
                JsonSerializer.Serialize(stream, _log);
                Console.WriteLine("Data saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }

        public void LoadFromFile()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    Console.WriteLine("No data file found.");
                    return;
                }

                using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
                var items = JsonSerializer.Deserialize<List<T>>(stream);

                if (items != null)
                    _log = items;

                Console.WriteLine("Data loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading file: {ex.Message}");
            }
        }
    }
}

// Integration Layer
public class InventoryApp
{
    private InventoryCaptureSystem.InventoryLogger<InventoryItem> _logger;

    public InventoryApp(string filePath)
    {
        _logger = new InventoryCaptureSystem.InventoryLogger<InventoryItem>(filePath);
    }

    public void SeedSampleData()
    {
        _logger.Add(new InventoryItem(1, "Laptop", 5, DateTime.Now));
        _logger.Add(new InventoryItem(2, "Mouse", 20, DateTime.Now));
        _logger.Add(new InventoryItem(3, "Keyboard", 15, DateTime.Now));
        _logger.Add(new InventoryItem(4, "Monitor", 7, DateTime.Now));
        _logger.Add(new InventoryItem(5, "USB Cable", 30, DateTime.Now));
    }

    public void SaveData() => _logger.SaveToFile();
    public void LoadData() => _logger.LoadFromFile();

    public void PrintAllItems()
    {
        foreach (var item in _logger.GetAll())
        {
            Console.WriteLine($"ID: {item.Id}, Name: {item.Name}, Quantity: {item.Quantity}, Added: {item.DateAdded}");
        }
    }
}

internal class Program
{
    static void Main(string[] args)
    {
        string filePath = "inventory.json";
        var app = new InventoryApp(filePath);

        // Seed and Save Data
        app.SeedSampleData();
        app.SaveData();

        // Simulate new session (fresh app instance)
        app = new InventoryApp(filePath);
        app.LoadData();
        app.PrintAllItems();
    }
}
