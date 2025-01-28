using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using MauiApp1.Components.Model;
using Microsoft.AspNetCore.Components;
using static MauiApp1.Components.Model.ApplicationModel;

namespace MauiApp1.Components.Pages
{
    public partial class Home : ComponentBase
    {
        private string TotalAmount = "Loading...";
        private string TotalCredit = "Loading...";
        private string TotalDebit = "Loading...";
        private string TotalDebt = "Loading...";
        private string RemainingDebt = "Loading...";
        private string ClearedDebt = "Loading...";
        private string HighestInflow = "Loading...";
        private string HighestOutflow = "Loading...";
        private string HighestDebt = "Loading...";
        private int TotalTransactions = 0;
        private DateTime? StartDate { get; set; }
        private DateTime? EndDate { get; set; }
        private ApplicationModel Data;
        private List<Transaction> AllTrans => Data?.Transactions ?? new List<Transaction>();
        private List<ApplicationModel.Debt> AllDebt => Data?.Debts ?? new List<ApplicationModel.Debt>();
        private List<Transaction> Top5HighestTransactions { get; set; } = new List<Transaction>();
        private List<Transaction> Top5LowestTransactions { get; set; } = new List<Transaction>();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (Preferences.ContainsKey("DebtAmount"))
                {
                    int debtAmount = Preferences.Get("DebtAmount", 0);

                    if (debtAmount > 0)
                    {
                        var toastMessage = $"You have taken a debt of amount: {debtAmount}";
                        var toast = Toast.Make(toastMessage, CommunityToolkit.Maui.Core.ToastDuration.Short);
                        await toast.Show();
                        Preferences.Remove("DebtAmount");
                    }
                }

                Data = await DataService.LoadDataAsync();

                if (Data == null)
                {
                    TotalAmount = "No data found.";
                    return;
                }

                if (!string.IsNullOrEmpty(StateService.UserName))
                {
                    var userTransactions = AllTrans.Where(t => t.Username == StateService.UserName).ToList();
                    var userDebts = AllDebt.Where(d => d.Username == StateService.UserName).ToList();

                    if (userTransactions.Any() || userDebts.Any())
                    {
                        TotalTransactions = userTransactions.Count;
                        TotalCredit = "Rs. " + userTransactions.Where(t => t.TransactionType == "Credit").Sum(t => (decimal?)t.Amount)?.ToString("N2") ?? "0.00";
                        TotalDebit = "Rs. " + userTransactions.Where(t => t.TransactionType == "Debit").Sum(t => (decimal?)t.Amount)?.ToString("N2") ?? "0.00";

                        HighestInflow = "Rs. " + userTransactions.Where(t => t.TransactionType == "Credit").Max(t => (decimal?)t.Amount)?.ToString("N2") ?? "0.00";
                        HighestOutflow = "Rs. " + userTransactions.Where(t => t.TransactionType == "Debit").Max(t => (decimal?)t.Amount)?.ToString("N2") ?? "0.00";

                        // Calculate Highest Debt
                        var highestDebtAmount = userDebts.Max(d => (decimal?)d.Amount) ?? 0;
                        HighestDebt = "Rs. " + highestDebtAmount.ToString("N2");

                        // Calculate Remaining Debt
                        var remainingDebtAmount = userDebts.Sum(d => d.Amount - d.PaidAmount);
                        RemainingDebt = "Rs. " + remainingDebtAmount.ToString("N2");

                        var latestDebt = userDebts.OrderByDescending(d => d.DebtDate).FirstOrDefault();
                        if (latestDebt != null)
                        {
                            TotalDebt = "Rs. " + latestDebt.Amount.ToString("N2");
                            ClearedDebt = "Rs. " + latestDebt.PaidAmount.ToString("N2");
                        }
                        else
                        {
                            TotalDebt = "No debts found.";
                            ClearedDebt = "Rs. 0.00";
                        }

                        Top5HighestTransactions = userTransactions.OrderByDescending(t => t.Amount).Take(5).ToList();
                        Top5LowestTransactions = userTransactions.OrderBy(t => t.Amount).Take(5).ToList();

                        // Calculate Current Balance
                        var currentBalance = userTransactions.Where(t => t.TransactionType == "Credit").Sum(t => t.Amount)
                                            - userTransactions.Where(t => t.TransactionType == "Debit").Sum(t => t.Amount);
                        TotalAmount = "Rs. " + currentBalance.ToString("N2");
                    }
                    else
                    {
                        ResetValues();
                    }
                }
                else
                {
                    TotalAmount = "User not found.";
                    ResetValues();
                }
            }
            catch (Exception ex)
            {
                // Log the error for debugging purposes
                Console.WriteLine($"An error occurred: {ex.Message}");
                await Toast.Make("An unexpected error occurred. Please try again.", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
            }
        }

        private void ResetValues()
        {
            TotalCredit = "Rs. 0.00";
            TotalDebit = "Rs. 0.00";
            TotalDebt = "Rs. 0.00";
            RemainingDebt = "Rs. 0.00";
            ClearedDebt = "Rs. 0.00";
            HighestDebt = "Rs. 0.00";
            Top5HighestTransactions = new List<Transaction>();
            Top5LowestTransactions = new List<Transaction>();
            HighestInflow = "Rs. 0.00";
            HighestOutflow = "Rs. 0.00";
        }


        private async Task FilterByDateRange()
        {
            if (StartDate.HasValue && EndDate.HasValue)
            {
                // Filter transactions by date range
                var filteredTransactions = AllTrans
                    .Where(t => t.TransactionDate >= StartDate.Value && t.TransactionDate <= EndDate.Value)
                    .ToList();

                // Filter debts by date range
                var filteredDebts = AllDebt
                    .Where(d => d.DebtDate >= StartDate.Value && d.DebtDate <= EndDate.Value)
                    .ToList();

                // Update calculations based on the filtered data
                if (filteredTransactions.Any() || filteredDebts.Any())
                {
                    // Recalculate total credit, debit, and other statistics
                    var totalCredit = filteredTransactions
                        .Where(t => t.TransactionType == "Credit")
                        .Sum(t => t.Amount);

                    var totalDebit = filteredTransactions
                        .Where(t => t.TransactionType == "Debit")
                        .Sum(t => t.Amount);

                    TotalTransactions = filteredTransactions.Count;

                    // Calculate the highest inflow and outflow
                    HighestInflow = "Rs. " + filteredTransactions
                        .Where(t => t.TransactionType == "Credit")
                        .Max(t => (decimal?)t.Amount)
                        ?.ToString("N2") ?? "0.00";

                    HighestOutflow = "Rs. " + filteredTransactions
                        .Where(t => t.TransactionType == "Debit")
                        .Max(t => (decimal?)t.Amount)
                        ?.ToString("N2") ?? "0.00";

                    // Recalculate debts
                    var latestDebt = filteredDebts.OrderByDescending(d => d.DebtDate).FirstOrDefault();
                    if (latestDebt != null)
                    {
                        TotalDebt = "Rs. " + latestDebt.Amount.ToString("N2");
                        RemainingDebt = "Rs. " + (latestDebt.Amount - latestDebt.PaidAmount).ToString("N2");
                        ClearedDebt = "Rs. " + latestDebt.PaidAmount.ToString("N2");
                    }
                    else
                    {
                        TotalDebt = "No recent debts found.";
                        RemainingDebt = "No recent debts found.";
                        ClearedDebt = "No recent debts found.";
                    }

                    // Recalculate top 5 highest and lowest transactions
                    Top5HighestTransactions = filteredTransactions
                        .OrderByDescending(t => t.Amount)
                        .Take(5)
                        .ToList();

                    Top5LowestTransactions = filteredTransactions
                        .OrderBy(t => t.Amount)
                        .Take(5)
                        .ToList();

                    // Recalculate current balance
                    var currentBalance = filteredTransactions
                        .Where(t => t.TransactionType == "Credit")
                        .Sum(t => t.Amount)
                        - filteredTransactions
                        .Where(t => t.TransactionType == "Debit")
                        .Sum(t => t.Amount);

                    TotalAmount = "Rs. " + currentBalance.ToString("N2");
                }
                else
                {
                    // Reset values if no transactions or debts found
                    TotalCredit = "Rs. 0.00";
                    TotalDebit = "Rs. 0.00";
                    TotalDebt = "Rs. 0.00";
                    RemainingDebt = "Rs. 0.00";
                    ClearedDebt = "Rs. 0.00";
                    HighestDebt = "Rs. 0.00";
                    Top5HighestTransactions = new List<Transaction>();
                    Top5LowestTransactions = new List<Transaction>();
                    HighestInflow = "Rs. 0.00";
                    HighestOutflow = "Rs. 0.00";
                    TotalAmount = "Rs. 0.00";
                }
            }
            else
            {
                await Toast.Make("Please select both start and end dates.", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
            }
        }
    }
}
