using System;


namespace MauiApp1.Services
{
    public class StateService
    {

        public string? UserName { get; set; }


        // Event to notify users about state changes
        public event Action OnChange;

        // Method to set the username and notify any listeners
        public void SetData(string username)
        {
            UserName = username;

            NotifyStateChanged();
        }

        // Private method to notify about state changes
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}