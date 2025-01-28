using Microsoft.AspNetCore.Components;
using MauiApp1.Components.Model;
using static MauiApp1.Components.Model.ApplicationModel;

namespace MauiApp1.Components.Pages
{
    public partial class Cashout : ComponentBase
    {
        private string Tag = "";
        private string? Notes = "";
        private int? Amount = null; // The actual integer value after validation
        private string? AmountInput = ""; // The raw input from the user
        private string? AmountError = null; // Error message for invalid input
        private string Message = "";
        private ApplicationModel Data;  // Corrected: Data is of type AppData
        private List<Transaction> AllTrans => Data?.Transactions ?? new List<Transaction>();
        private int TotalAmount = 0;
        private int Status = 0;

        protected override async Task OnInitializedAsync()
        {
            Data = await MainService.LoadDataAsync();

            if (!string.IsNullOrEmpty(StateService.UserName))
            {
                // Filter transactions for the current user
                var userTransactions = AllTrans.Where(t => t.Username == StateService.UserName).ToList();

                if (userTransactions.Any())
                {
                    // Sum credit and debit transactions
                    var totalCredit = userTransactions
                        .Where(t => t.TransactionType == "Credit")
                        .Sum(t => t.Amount);

                    var totalDebit = userTransactions
                        .Where(t => t.TransactionType == "Debit")
                        .Sum(t => t.Amount);

                    //var totalUnpaidDebt = userDebts
                    //    .Where(d => d.PaidAmount < d.Amount) // Unpaid debt condition
                    //    .Sum(d => d.Amount - d.PaidAmount);

                    // Calculate current balance (credit - debit)
                    var currentBalance = totalCredit - totalDebit;

                    TotalAmount = currentBalance;
                    //// Calculate balance including unpaid debts
                    //IncludingDebt = currentBalance + totalUnpaidDebt;
                }
            }
        }

        private void ValidateAmount(ChangeEventArgs e)
        {
            AmountInput = e.Value?.ToString();

            // Validate input as a positive integer
            if (!int.TryParse(AmountInput, out int parsedAmount) || parsedAmount <= 0)
            {
                AmountError = "Please enter a valid positive number.";
                Amount = null;
            }
            else
            {
                AmountError = null; // Clear error if input is valid
                Amount = parsedAmount; // Update Amount with the valid value
            }
        }

        public async Task Cashoutasync()
        {
            try
            {
                if (!Amount.HasValue || Amount <= 0)
                {
                    Message = "Enter a valid positive number for Amount.";
                    StateHasChanged();
                    return;
                }

                if (string.IsNullOrEmpty(Tag))
                {
                    Message = "Label is required.";
                    StateHasChanged();
                    return;
                }
                if (Amount > TotalAmount)
                {
                    Status = 402;
                    Message = "Insufficient balance";// Insufficient balance (debt > amount)
                    StateHasChanged();
                    return;
                }
                //if (Amount > IncludingDebt)
                //{
                //    Status = 402; // Insufficient balance (debt > amount)
                //    StateHasChanged();
                //    return;
                //}

                var newTransaction = new Transaction
                {
                    Amount = Amount.Value,
                    Notes = Notes,
                    Tags = Tag,
                    TransactionDate = DateTime.Now,
                    TransactionType = "Debit",
                    Username = StateService.UserName,
                };

                AllTrans.Add(newTransaction);

                Data.Transactions = AllTrans;
                await MainService.SaveDataAsync(Data);

                // Reset fields after successful cashout
                Amount = null;
                AmountInput = "";
                Tag = string.Empty;
                Notes = string.Empty;
                Message = "Transaction successfully created!";
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Message = $"Error: {ex.Message}";
                StateHasChanged();
            }
        }

        private void CloseModal()
        {
            Status = 0;
            StateHasChanged();
        }


    }
}

