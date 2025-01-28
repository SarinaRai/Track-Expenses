using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MauiApp1.Components.Model;
using Microsoft.AspNetCore.Components;
using static MauiApp1.Components.Model.ApplicationModel;

namespace MauiApp1.Components.Pages
{
    public partial class Register : ComponentBase
    {
        private string Username { get; set; } = string.Empty;
        private string Password { get; set; } = string.Empty;
        private string ConfirmPassword { get; set; } = string.Empty;
        private string Email { get; set; } = string.Empty;

        private string Message { get; set; } = string.Empty;
        private ApplicationModel? Data { get; set; }
        private List<User> Users => Data?.Users ?? new List<User>();

        protected override async Task OnInitializedAsync()
        {
            // Load the application data
            Data = await MainService.LoadDataAsync();
        }

        private async Task RegisterUser()
        {
            // Validate that all required fields are filled
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Email))
            {
                Message = "All fields are required.";
                StateHasChanged();
                return;
            }

            // Validate email format
            if (!IsValidEmail(Email))
            {
                Message = "Please enter a valid email address.";
                StateHasChanged();
                return;
            }

            // Check if passwords match
            if (Password != ConfirmPassword)
            {
                Message = "Passwords do not match.";
                StateHasChanged();
                return;
            }

            // Check if the username already exists in the list of users
            if (Users.Any(user => user.Username.Equals(Username, StringComparison.OrdinalIgnoreCase)))
            {
                Message = "Username already exists.";
                StateHasChanged();
                return;
            }

            // Create a new user object with hashed password
            var hashedPassword = await MainService.HashPasswordAsync(Password);
            var newUser = new User
            {
                Username = Username,
                Password = hashedPassword,
                Email = Email
            };

            // Add the new user to the database
            Data?.Users.Add(newUser);
            await MainService.SaveDataAsync(Data);

            // Display success message
            Message = "Registration successful!";
            StateHasChanged();

            // Delay and redirect to the login or home page
            await Task.Delay(2000);
            NavigationManager.NavigateTo("/");
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
