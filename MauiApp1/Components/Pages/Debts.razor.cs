using Microsoft.AspNetCore.Components;

using static MauiApp1.Components.Model.ApplicationModel;
using MauiApp1.Services;
using MauiApp1.Components.Model;

namespace MauiApp1.Components.Pages
{
    public partial class Debts : ComponentBase
    {
        private string Notes { get; set; } = string.Empty; // Description for the debt
        private int? Amount { get; set; } = null; // The validated integer value
        private string? AmountInput { get; set; } = string.Empty; // The raw input from the user
        private string? AmountError { get; set; } = null; // Error message for invalid input
        private string Message { get; set; } = string.Empty; // Feedback message for the user
        private ApplicationModel Data { get; set; } = new(); // Holds the application's data
        private List<ApplicationModel.Debt> AllDebt => Data?.Debts ?? new List<ApplicationModel.Debt>();
        private string CheckStatus { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            Data = await MainService.LoadDataAsync();

            if (!string.IsNullOrEmpty(StateService.UserName))
            {
                // Get all debts associated with the current user
                var userDebts = AllDebt
                    .Where(d => d.Username == StateService.UserName)
                    .OrderByDescending(d => d.DebtDate) // Ensure debts are ordered by date
                    .ToList();

                // Check the most recent debt status, if any
                CheckStatus = userDebts.FirstOrDefault()?.DebtStatus ?? string.Empty;
            }
        }

        private void ValidateAmount(ChangeEventArgs e)
        {
            AmountInput = e.Value?.ToString();

            if (!int.TryParse(AmountInput, out int parsedAmount) || parsedAmount <= 0)
            {
                AmountError = "Please enter a valid positive integer.";
                Amount = null;

                // Clear the error message after a short delay
                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000); // Delay for error message timeout
                    AmountError = null;
                    await InvokeAsync(StateHasChanged); // Update the UI
                });
            }
            else
            {
                AmountError = null; // Clear error if input is valid
                Amount = parsedAmount; // Update Amount with the valid value
            }
        }

        public async Task TakeDebt()
        {
            // Input validations
            if (Amount == null || Amount <= 0)
            {
                Message = "Please enter a valid amount before proceeding.";
                StateHasChanged();
                return;
            }

            if (string.IsNullOrWhiteSpace(Notes))
            {
                Message = "Description is required.";
                StateHasChanged();
                return;
            }

            // Check if the most recent debt is still unpaid
            if (CheckStatus == "UnPaid")
            {
                Message = "Your previous debt has not been paid.";
                StateHasChanged();
                return;
            }

            // Create a new debt entry
            var newDebt = new Debt
            {
                Amount = Amount.Value,
                Description = Notes,
                DebtDate = DateTime.Now,
                Username = StateService.UserName,
                DebtStatus = "UnPaid"
            };

            // Add the new debt to the list
            Data.Debts.Add(newDebt);

            // Create a transaction for the debt as a credit (inflow)
            var debtTransaction = new Transaction
            {
                Amount = Amount.Value,
                TransactionType = "Credit",
                Tags = "Debt",
                TransactionDate = DateTime.Now,
                Username = StateService.UserName
            };

            // Add the transaction to the list
            Data.Transactions.Add(debtTransaction);

            // Save the updated data
            await MainService.SaveDataAsync(Data);

            // Save the amount to preferences and navigate to the dashboard
            Preferences.Set("DebtAmount", Amount.Value);
            NavigationManager.NavigateTo("/dashboard");

            // Clear the form and update the state
            Notes = string.Empty;
            Amount = null;
            Message = "Debt added successfully!";
            StateHasChanged();
        }
    }
}
