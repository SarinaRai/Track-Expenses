using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MauiApp1.Components.Model
{
    public class ApplicationModel
    {
        public List<User> Users { get; set; } = new();
        public List<Transaction> Transactions { get; set; } = new();
        public List<Debt> Debts { get; set; } = new();

        public class User
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
        }

        public class Transaction
        {
            public int Amount { get; set; }
            public string Tags { get; set; }
            public string? Notes { get; set; }
            public string TransactionType { get; set; }

            [ForeignKey("Username")]
            public string Username { get; set; }

            public DateTime TransactionDate { get; set; }
        }

        public class Debt
        {
            [ForeignKey("Username")] // Corrected typo from "Usename" to "Username"
            public string Username { get; set; }

            public int Amount { get; set; }
            public string DebtStatus { get; set; }
            public int PaidAmount { get; set; }
            public DateTime DebtDate { get; set; }
            public string Description { get; set; }
        }
    }
}
