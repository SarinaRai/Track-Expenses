using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MauiApp1.Components.Model;
using Microsoft.AspNetCore.Components;
using static MauiApp1.Components.Model.ApplicationModel;

namespace MauiApp1.Components.Pages
{
    public partial class ClearDebt : ComponentBase
    {
        private string? AmountInput = null; // Initialize with null for clarity
        private string? Message = null; // Use nullable to ensure proper state handling
        private int? Amount = null;

        private ApplicationModel Data = new ApplicationModel();

        // Using null safety with null-coalescing operator
        private List<ApplicationModel.Debt> AllDebt => Data?.Debts ?? new List<ApplicationModel.Debt>();

        private decimal RemainingDebt { get; set; }
        private decimal ClearedDebt { get; set; }
        private decimal PaidAmount { get; set; }
        private decimal TotalDebtAmount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // Load data asynchronously
            Data = await MainService.LoadDataAsync();

            if (!string.IsNullOrEmpty(StateService.UserName))
            {
                // Filter debts for the logged-in user
                var userDebts = AllDebt
                    .Where(d => d.Username == StateService.UserName)
                    .ToList();

                if (userDebts.Any())
                {
                    // Retrieve the latest debt information
                    var latestDebt = userDebts
                        .OrderByDescending(d => d.DebtDate)
                        .FirstOrDefault();

                    if (latestDebt != null)
                    {
                        PaidAmount = latestDebt.PaidAmount;
                        TotalDebtAmount = latestDebt.Amount;
                    }

                    // Calculate remaining and cleared debts
                    RemainingDebt = userDebts.Sum(d => d.Amount - d.PaidAmount);
                    ClearedDebt = userDebts.Sum(d => d.PaidAmount);
                }
            }
        }

        private void ValidateAmount(ChangeEventArgs e)
        {
            AmountInput = e.Value?.ToString();

            // Validate the amount input
            if (!int.TryParse(AmountInput, out int parsedAmount) || parsedAmount <= 0)
            {
                Message = "Please enter a valid positive number.";
                Amount = null;
            }
            else
            {
                Message = null;
                Amount = parsedAmount;
            }
        }

        public async Task ClearOut()
        {
            // Validate the amount
            if (!Amount.HasValue || Amount <= 0)
            {
                Message = "Enter a valid positive number for Amount.";
                StateHasChanged();
                return;
            }

            if (Amount > RemainingDebt)
            {
                Message = "Amount exceeds the remaining debt.";
                StateHasChanged();
                return;
            }

            // Get unpaid debts for the logged-in user
            var userDebts = AllDebt
                .Where(d => d.Username == StateService.UserName && d.PaidAmount < d.Amount)
                .OrderBy(d => d.DebtDate)
                .ToList();

            int remainingAmount = Amount.Value;

            foreach (var debt in userDebts)
            {
                // Calculate the unpaid balance for each debt
                int debtBalance = debt.Amount - debt.PaidAmount;

                if (remainingAmount <= debtBalance)
                {
                    debt.PaidAmount += remainingAmount;

                    // Update debt status if fully paid
                    if (debt.PaidAmount == debt.Amount)
                    {
                        debt.DebtStatus = "Paid";
                    }

                    break;
                }

                // Deduct the remaining amount and mark debt as paid
                remainingAmount -= debtBalance;
                debt.PaidAmount = debt.Amount;
                debt.DebtStatus = "Paid";
            }

            // Create a debit transaction for the cleared debt
            var debitTransaction = new Transaction
            {
                Amount = Amount.Value, // Amount being cleared
                Tags = "Debt Clearance", // Tag to identify the transaction
                Notes = $"Cleared debt of Rs. {Amount.Value}", // Optional notes
                TransactionType = "Debit", // Transaction type (Debit for clearing debt)
                Username = StateService.UserName, // Username of the logged-in user
                TransactionDate = DateTime.Now // Current date and time
            };

            // Add the transaction to the list
            Data.Transactions.Add(debitTransaction);

            // Save updated data
            await MainService.SaveDataAsync(Data);

            // Update remaining and cleared debts
            RemainingDebt -= Amount.Value;
            ClearedDebt += Amount.Value;

            // Reset the form and display success message
            Message = $"Successfully cleared Rs. {Amount.Value:N2} from your debt.";
            AmountInput = null;
            Amount = null;

            // Navigate back to the dashboard to reflect the updated balance
            NavigationManager.NavigateTo("/dashboard", forceLoad: true);
        }
    }
}
