using System;
using System.Collections.Generic;

namespace FinanceManagementSystem
{
    // core models using classes 
    public class Transaction
    {
        public int Id { get; }
        public DateTime Date { get; }
        public decimal Amount { get; }
        public string Category { get; }

        public Transaction(int id, DateTime date, decimal amount, string category)
        {
            Id = id;
            Date = date;
            Amount = amount;
            Category = category;
        }
    }

    public interface ITransactionProcessor
    {
        void Process(Transaction transaction);
    }

    public class BankTransferProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[BankTransfer] Processing {transaction.Amount:C} for {transaction.Category} on {transaction.Date:d}.");
        }
    }

    public class MobileMoneyProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[MobileMoney] Processing {transaction.Amount:C} for {transaction.Category} on {transaction.Date:d}.");
        }
    }

    public class CryptoWalletProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[CryptoWallet] Processing {transaction.Amount:C} equivalent for {transaction.Category} on {transaction.Date:d}.");
        }
    }

    public class Account
    {
        public string AccountNumber { get; }
        public decimal Balance { get; protected set; }

        public Account(string accountNumber, decimal initialBalance)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException("Account number is required.", nameof(accountNumber));
            if (initialBalance < 0)
                throw new ArgumentOutOfRangeException(nameof(initialBalance), "Initial balance cannot be negative.");

            AccountNumber = accountNumber;
            Balance = initialBalance;
        }

        public virtual void ApplyTransaction(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (transaction.Amount <= 0)
            {
                Console.WriteLine("Transaction amount must be positive.");
                return;
            }

            Balance -= transaction.Amount;
            Console.WriteLine($"[Account] Applied {transaction.Amount:C}. New balance: {Balance:C}");
        }
    }

    public sealed class SavingsAccount : Account
    {
        public SavingsAccount(string accountNumber, decimal initialBalance) : base(accountNumber, initialBalance) { }

        public override void ApplyTransaction(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (transaction.Amount <= 0)
            {
                Console.WriteLine("Transaction amount must be positive.");
                return;
            }

            if (transaction.Amount > Balance)
            {
                Console.WriteLine("Insufficient funds");
                return;
            }

            Balance -= transaction.Amount;
            Console.WriteLine($"[SavingsAccount] Deducted {transaction.Amount:C}. Updated balance: {Balance:C}");
        }
    }

    public class FinanceApp
    {
        private readonly List<Transaction> _transactions = new List<Transaction>();

        public void Run()
        {
            var savings = new SavingsAccount("SA-0001", 1000m);
            Console.WriteLine($"Account {savings.AccountNumber} starting balance: {savings.Balance:C}");

            var t1 = new Transaction(1, DateTime.Now, 150m, "Groceries");
            var t2 = new Transaction(2, DateTime.Now, 250m, "Utilities");
            var t3 = new Transaction(3, DateTime.Now, 120m, "Entertainment");

            ITransactionProcessor mobileMoney = new MobileMoneyProcessor();
            ITransactionProcessor bankTransfer = new BankTransferProcessor();
            ITransactionProcessor cryptoWallet = new CryptoWalletProcessor();

            mobileMoney.Process(t1);
            bankTransfer.Process(t2);
            cryptoWallet.Process(t3);

            savings.ApplyTransaction(t1);
            savings.ApplyTransaction(t2);
            savings.ApplyTransaction(t3);

            _transactions.AddRange(new[] { t1, t2, t3 });

            Console.WriteLine("\n--- Transaction Summary ---");
            foreach (var tx in _transactions)
            {
                Console.WriteLine($"Id={tx.Id}, Date={tx.Date:g}, Amount={tx.Amount:C}, Category={tx.Category}");
            }

            Console.WriteLine($"\nFinal balance for {savings.AccountNumber}: {savings.Balance:C}");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            new FinanceApp().Run();
        }
    }
}
