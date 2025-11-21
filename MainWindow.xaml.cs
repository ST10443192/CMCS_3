using System;
using System.Windows;
using Microsoft.VisualBasic;
using ContractMonthlyClaimSystem2.Database;
using ContractMonthlyClaimSystem2.Helpers;
using ContractMonthlyClaimSystem2.Models;

namespace ContractMonthlyClaimSystem2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text?.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both email and password.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var user = JsonDatabaseManager.Instance.AuthenticateUser(email, password);

            if (user != null)
            {
                MessageBox.Show($"Welcome {user.FullName}!", "Login Successful",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                NavigateToDashboard(user);
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid email or password.",
                    "Login Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void NavigateToDashboard(User user)
        {
            if (user == null)
            {
                MessageBox.Show("User is null.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Window dashboard = null;
            string role = (user.Role ?? string.Empty).Trim();

            switch (role)
            {
                case "Lecturer":
                    dashboard = new LecturerDashboard(user.Email, user.FullName);
                    break;
                case "Coordinator":
                    dashboard = new CoordinatorDashboard(user.Email, user.FullName);
                    break;
                case "Manager":
                    dashboard = new ManagerDashboard(user.Email, user.FullName);
                    break;
                case "Admin":
                    dashboard = new AdminDashboard(user.Email, user.FullName);
                    break;
                default:
                    MessageBox.Show("Invalid user role.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            dashboard.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtEmail.Text = "";
            txtPassword.Password = "";

            MessageBox.Show(
                "Default Login Credentials:\n\n" +
                "Admin: admin@university.ac.za / Admin@123\n" +
                "Lecturer: lecturer@university.ac.za / Lecturer@123\n" +
                "Coordinator: coordinator@university.ac.za / Coordinator@123\n" +
                "Manager: manager@university.ac.za / Manager@123",
                "Login Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = Interaction.InputBox("Enter email:", "Register", "user@example.com").Trim();
                if (string.IsNullOrWhiteSpace(email))
                    return;

                string fullName = Interaction.InputBox("Full name:", "Register", "John Doe").Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                    return;

                string password = Interaction.InputBox("Password:", "Register", "");
                if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                {
                    MessageBox.Show("Password must be at least 8 characters.");
                    return;
                }

                string role = Interaction.InputBox("Role (Lecturer / Coordinator / Manager / Admin):",
                    "Register", "Lecturer").Trim();

                if (role != "Lecturer" && role != "Coordinator" && role != "Manager" && role != "Admin")
                {
                    MessageBox.Show("Invalid role.");
                    return;
                }

                bool created = JsonDatabaseManager.Instance.CreateUser(email, password, fullName, role);

                if (created)
                {
                    var newUser = new User
                    {
                        Email = email,
                        FullName = fullName,
                        Role = role
                    };

                    MessageBox.Show("User created successfully!");
                    NavigateToDashboard(newUser);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("User creation failed. Email may already exist.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
