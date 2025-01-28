using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MauiApp1.Components.Model;
using MauiApp1.Services;
using Microsoft.AspNetCore.Components;
using static MauiApp1.Components.Model.ApplicationModel;

namespace MauiApp1.Components.Pages
{
    public partial class Login : ComponentBase
    {
        private string Username { get; set; } = string.Empty;
        private string Password { get; set; } = string.Empty;
        private string Message { get; set; } = string.Empty;
        private string SelectedCurrency { get; set; } = "Nrs";

        private ApplicationModel? Data { get; set; }
        private List<User> Users => Data?.Users ?? new List<User>();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Load application data
                Data = await MainService.LoadDataAsync();
            }
            catch (Exception ex)
            {
                // Log the error and set a user-friendly message
                Console.Error.WriteLine($"Error loading users: {ex.Message}");
                Message = "Unable to load users. Please try again later.";
            }
        }

        private async Task LoginUser()
        {
            // Reset the message field for each attempt
            Message = string.Empty;

            // Validate input fields
            if (string.IsNullOrWhiteSpace(Username))
            {
                Message = "Please enter your username.";
                StateHasChanged();
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                Message = "Please enter your password.";
                StateHasChanged();
                return;
            }

            try
            {
                // Check if the user exists
                var user = Users.FirstOrDefault(u =>
                    u.Username.Equals(Username, StringComparison.OrdinalIgnoreCase));

                if (user == null)
                {
                    Message = "Invalid username or password.";
                    StateHasChanged();
                    return;
                }

                // Validate the user's password
                var isPasswordValid = await MainService.ValidatePasswordAsync(Password, user.Password);
                if (isPasswordValid)
                {
                    // Set user data in state
                    StateService.SetData(user.Username);

                    // Navigate to the dashboard
                    NavigationManager.NavigateTo("/dashboard");
                }
                else
                {
                    Message = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                // Log the error and provide feedback to the user
                Console.Error.WriteLine($"Error during login: {ex.Message}");
                Message = "An unexpected error occurred. Please try again later.";
            }
            finally
            {
                // Trigger UI update
                StateHasChanged();
            }
        }
    }
}
