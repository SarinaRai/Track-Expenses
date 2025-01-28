using Microsoft.AspNetCore.Components;
using MauiApp1.Components.Model;
using static MauiApp1.Components.Model.ApplicationModel;

namespace MauiApp1.Components.Pages
{
    public partial class Cashin : ComponentBase
    {
        private string Tag = "";
        private string? Notes = "";
        private int? Amount = null; // The actual integer value after validation
        private string? AmountInput = ""; // The raw input from the user
        private string? AmountError = null; // Error message for invalid input
        private string? Message = "";
        private ApplicationModel Data;  // Corrected: Data is of type AppData
        private List<Transaction> AllTrans =>Data?.Transactions ?? new List<Transaction>();
        protected override async Task OnInitializedAsync()
        {
            Data = await MainService.LoadDataAsync();
        }

        private void ValidateAmount(ChangeEventArgs e)
        {
            AmountInput = e.Value?.ToString();

            // Validate input as a positive integer
            if (!int.TryParse(AmountInput, out int parsedAmount) || parsedAmount <= 0)
            {
                Message = "Please enter a valid positive integer.";
                Amount = null;

                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000); // Delay for error message timeout
                    Message = null;
                    StateHasChanged(); // Update the UI
                });
            }
            else
            {
                Message = null; // Clear error if input is valid
                Amount = parsedAmount; // Update Amount with the valid value
            }
        }
        public async Task Cashinasync()
        {
            try
            {
                // Ensure the validated Amount is available
                if (!Amount.HasValue || Amount <= 0)
                {
                    Message = "Enter a valid positive number for Amount.";
                    StateHasChanged();
                    return;
                }

                // Validate that the Label is not empty
                if (string.IsNullOrEmpty(Tag))
                {
                    Message = "Label is required.";
                    StateHasChanged();
                    return;
                }

                // Create a new transaction
                var newTransaction = new Transaction()
                {

                    Amount = Amount.Value,
                    Notes = Notes,
                    Tags = Tag,
                    TransactionDate = DateTime.Now,
                    TransactionType = "Credit",
                    Username = StateService.UserName,
                };

                // Add transaction to the list
                AllTrans.Add(newTransaction);

                // Save the updated AppData with the new transaction
                Data.Transactions = AllTrans;  // Update Transactions in AppData
                await MainService.SaveDataAsync(Data); // Save the entire AppData, not just Transactions

                // Reset inputs and show success message
                Amount = null;
                AmountInput = "";
                Tag = string.Empty;
                Notes = string.Empty;
                Message = "Transaction successfully created!";
                StateHasChanged();
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                Message = $"Error: {ex.Message}";
                StateHasChanged();
            }
        }
    }
}
