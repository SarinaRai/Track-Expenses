using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp1.Components.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static MauiApp1.Components.Model.ApplicationModel;

namespace MauiApp1.Components.Pages
{
    public partial class Trans : ComponentBase
    {
        private string sortOrder = "desc"; // Default sorting: New to old
        private string sortOrderType = "All"; // Default filter: All
        private string tags = ""; // For label-based search

        private List<TransactionDto> transactions = new();
        private ApplicationModel data;

        // Flags for sorting and filtering
        private bool IsAscending => sortOrder == "asc";
        private bool IsDescending => sortOrder == "desc";
        private bool IsCredit => sortOrderType == "Credit";
        private bool IsDebit => sortOrderType == "Debit";
        private bool IsAll => sortOrderType == "All";

        private void ExportTransactions()
        {
            if (data?.Transactions == null) return;

            var filteredTransactions = data.Transactions
                .Where(t => t.Username == StateService.UserName)
                .Where(t => sortOrderType == "All" ||
                            t.TransactionType.Equals(sortOrderType, StringComparison.OrdinalIgnoreCase))
                .Select(MapToTransactionDto)
                .ToList();

            var csvContent = new StringBuilder();
            csvContent.AppendLine("Transaction Id, Title, Tags, Debit, Credit, Description, Date");

            foreach (var transaction in filteredTransactions)
            {
                csvContent.AppendLine($"{transaction.Amount}, {transaction.Label}, {transaction.Notes}, {transaction.TransactionType}, {transaction.TransactionDateTime:MM/dd/yyyy}");
            }

            JSRuntime.InvokeVoidAsync("createDownloadLink", csvContent.ToString(), "transactions.csv");
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                data = await DataService.LoadDataAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            await UpdateTransactions();
            await base.OnParametersSetAsync();
        }

        private void SearchByTags()
        {
            if (data?.Transactions == null) return;

            var userTransactions = data.Transactions
                .Where(t => t.Username == StateService.UserName &&
                            t.Tags.Contains(tags, StringComparison.OrdinalIgnoreCase))
                .Select(MapToTransactionDto)
                .ToList();

            transactions = userTransactions;
            StateHasChanged();
        }

        private async Task UpdateTransactions()
        {
            if (data?.Transactions == null) return;

            var userTransactions = data.Transactions
                .Where(t => t.Username == StateService.UserName)
                .ToList();

            if (userTransactions.Any())
            {
                transactions = SortTransactions(userTransactions, sortOrder, sortOrderType);
                StateHasChanged();
            }
        }

        private List<TransactionDto> SortTransactions(List<Transaction> userTransactions, string sortOrder, string sortOrderType)
        {
            var filteredTransactions = sortOrderType == "All"
                ? userTransactions
                : userTransactions.Where(t => t.TransactionType.Equals(sortOrderType, StringComparison.OrdinalIgnoreCase));

            return sortOrder switch
            {
                "desc" => filteredTransactions
                    .OrderByDescending(t => t.TransactionDate)
                    .Select(MapToTransactionDto)
                    .ToList(),
                "asc" => filteredTransactions
                    .OrderBy(t => t.TransactionDate)
                    .Select(MapToTransactionDto)
                    .ToList(),
                _ => filteredTransactions
                    .Select(MapToTransactionDto)
                    .ToList(),
            };
        }

        private TransactionDto MapToTransactionDto(Transaction transaction)
        {
            return new TransactionDto
            {
                Amount = transaction.Amount,
                Label = transaction.Tags,
                Notes = transaction.Notes,
                TransactionType = transaction.TransactionType,
                TransactionDateTime = transaction.TransactionDate,
            };
        }

        private async Task OnSortOrderChanged(ChangeEventArgs e)
        {
            if (e?.Value is not null)
            {
                sortOrder = e.Value.ToString();
                await UpdateTransactions();
            }
        }

        private async Task OnTransactionTypeChanged(ChangeEventArgs e)
        {
            if (e?.Value is not null)
            {
                sortOrderType = e.Value.ToString();
                await UpdateTransactions();
            }
        }

        private class TransactionDto
        {
            public decimal Amount { get; set; }
            public string Label { get; set; }
            public string Notes { get; set; }
            public string TransactionType { get; set; }
            public DateTime TransactionDateTime { get; set; }
        }
    }
}
