using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem
{
    public interface IInventoryItem
    {
        int Id { get; }
        string Name { get; }
        int Quantity { get; set; }
    }

    // ---------- Product Types ----------
    public class ElectronicItem : IInventoryItem
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int Quantity { get; set; }
        public string Brand { get; private set; }
        public int WarrantyMonths { get; private set; }

        public ElectronicItem(int id, string name, int quantity, string brand, int warrantyMonths)
        {
            Id = id; Name = name; Quantity = quantity; Brand = brand; WarrantyMonths = warrantyMonths;
        }

        public override string ToString()
        {
            return $"[Electronic] Id={Id}, Name={Name}, Qty={Quantity}, Brand={Brand}, Warranty={WarrantyMonths}m";
        }
    }

    public class GroceryItem : IInventoryItem
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; private set; }

        public GroceryItem(int id, string name, int quantity, DateTime expiryDate)
        {
            Id = id; Name = name; Quantity = quantity; ExpiryDate = expiryDate;
        }

        public override string ToString()
        {
            return $"[Grocery]   Id={Id}, Name={Name}, Qty={Quantity}, Expiry={ExpiryDate:yyyy-MM-dd}";
        }
    }

    // ---------- Custom Exceptions ----------
    public class DuplicateItemException : Exception
    {
        public DuplicateItemException(string message) : base(message) { }
    }

    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string message) : base(message) { }
    }

    public class InvalidQuantityException : Exception
    {
        public InvalidQuantityException(string message) : base(message) { }
    }

    // ---------- Generic Inventory Repository ----------
    public class InventoryRepository<T> where T : IInventoryItem
    {
        private readonly Dictionary<int, T> _items = new Dictionary<int, T>();

        public void AddItem(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (_items.ContainsKey(item.Id))
                throw new DuplicateItemException($"Item with Id {item.Id} already exists.");
            _items[item.Id] = item;
        }

        public T GetItemById(int id)
        {
            T value;
            if (!_items.TryGetValue(id, out value))
                throw new ItemNotFoundException($"Item with Id {id} was not found.");
            return value;
        }

        public void RemoveItem(int id)
        {
            if (!_items.ContainsKey(id))
                throw new ItemNotFoundException($"Cannot remove: item with Id {id} does not exist.");
            _items.Remove(id);
        }

        public List<T> GetAllItems()
        {
            return new List<T>(_items.Values);
        }

        public void UpdateQuantity(int id, int newQuantity)
        {
            if (newQuantity < 0)
                throw new InvalidQuantityException("Quantity cannot be negative.");

            var item = GetItemById(id); // will throw if not found
            item.Quantity = newQuantity;
        }
    }

    // ---------- Warehouse Manager ----------
    public class WareHouseManager
    {
        private readonly InventoryRepository<ElectronicItem> _electronics = new InventoryRepository<ElectronicItem>();
        private readonly InventoryRepository<GroceryItem> _groceries = new InventoryRepository<GroceryItem>();

        public void SeedData()
        {
            // Add 2–3 of each; wrap in try-catch to log cleanly
            try
            {
                _electronics.AddItem(new ElectronicItem(1, "Laptop", 10, "Dell", 24));
                _electronics.AddItem(new ElectronicItem(2, "Smartphone", 25, "Samsung", 12));
                _electronics.AddItem(new ElectronicItem(3, "Headphones", 40, "Sony", 18));

                _groceries.AddItem(new GroceryItem(101, "Rice (5kg)", 30, DateTime.Today.AddMonths(12)));
                _groceries.AddItem(new GroceryItem(102, "Milk (1L)", 50, DateTime.Today.AddDays(10)));
                _groceries.AddItem(new GroceryItem(103, "Eggs (Tray)", 20, DateTime.Today.AddDays(14)));
            }
            catch (Exception ex)
            {
                Console.WriteLine("SeedData error: " + ex.Message);
            }
        }

        public void PrintAllItems<T>(InventoryRepository<T> repo) where T : IInventoryItem
        {
            foreach (var item in repo.GetAllItems())
            {
                Console.WriteLine(item);
            }
        }

        public void IncreaseStock<T>(InventoryRepository<T> repo, int id, int quantity) where T : IInventoryItem
        {
            try
            {
                var current = repo.GetItemById(id);
                var newQty = current.Quantity + quantity;
                repo.UpdateQuantity(id, newQty);
                Console.WriteLine($"Increased stock for Id={id} by {quantity}. New Qty={newQty}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IncreaseStock error (Id={id}): {ex.Message}");
            }
        }

        public void RemoveItemById<T>(InventoryRepository<T> repo, int id) where T : IInventoryItem
        {
            try
            {
                repo.RemoveItem(id);
                Console.WriteLine($"Removed item Id={id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RemoveItem error (Id={id}): {ex.Message}");
            }
        }

        // Helpers to access repos for the demo in Main
        public InventoryRepository<ElectronicItem> ElectronicsRepo { get { return _electronics; } }
        public InventoryRepository<GroceryItem> GroceriesRepo { get { return _groceries; } }
    }
    internal class Program
    {
        private static void Divider(string title)
        {
            Console.WriteLine("\n==== " + title + " ====");
        }
        static void Main(string[] args)
        {
            var manager = new WareHouseManager();

            // i-ii. Seed
            manager.SeedData();

            // iii. Print all grocery items
            Divider("GROCERIES");
            manager.PrintAllItems(manager.GroceriesRepo);

            // iv. Print all electronic items
            Divider("ELECTRONICS");
            manager.PrintAllItems(manager.ElectronicsRepo);

            // v. Try error scenarios
            Divider("ERROR SCENARIOS");

            // Add a duplicate item (should throw DuplicateItemException)
            try
            {
                manager.ElectronicsRepo.AddItem(new ElectronicItem(1, "Gaming Laptop", 5, "ASUS", 24));
            }
            catch (DuplicateItemException ex)
            {
                Console.WriteLine("Duplicate add error: " + ex.Message);
            }

            // Remove a non-existent item (should throw ItemNotFoundException)
            manager.RemoveItemById(manager.GroceriesRepo, 999);

            // Update with invalid quantity (negative) (should throw InvalidQuantityException)
            try
            {
                manager.ElectronicsRepo.UpdateQuantity(2, -5);
            }
            catch (InvalidQuantityException ex)
            {
                Console.WriteLine("Invalid quantity error: " + ex.Message);
            }

            // Show a successful increase for comparison
            Divider("STOCK UPDATE (SUCCESS)");
            manager.IncreaseStock(manager.GroceriesRepo, 101, 10);

            Console.WriteLine("\nDone.");
        }
    }
    
}
