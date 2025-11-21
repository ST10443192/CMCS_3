using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ContractMonthlyClaimSystem2.Database;
using ContractMonthlyClaimSystem2.Helpers;
using ContractMonthlyClaimSystem2.Models;

namespace ContractMonthlyClaimSystem2
{
    public partial class LecturerDashboard : Window
    {
        private string _email;
        private string _role;
        private ObservableCollection<Claim> _claims;
        private List<string> _uploadedDocuments;

        public LecturerDashboard()
        {
            InitializeComponent();
            _claims = new ObservableCollection<Claim>();
            _uploadedDocuments = new List<string>();
        }

        public LecturerDashboard(string email, string role) : this()
        {
            _email = email;
            _role = role;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblUserInfo.Text = $"Welcome, {_role}! ({_email})";
            LoadExistingClaims();
            claimsDataGrid.ItemsSource = _claims;
        }

        private void LoadExistingClaims()
        {
            _claims.Clear();

            // Load claims from database for this lecturer
            var existingClaims = JsonDatabaseManager.Instance.GetClaimsByLecturer(_email);

            foreach (var claim in existingClaims)
            {
                _claims.Add(claim);
            }

            // If no claims exist, add a sample claim for demonstration
            if (_claims.Count == 0)
            {
                var sampleClaim = new Claim
                {
                    Id = 1001,
                    LecturerId = "L-001",
                    LecturerName = "Dr. Alice Smith",
                    LecturerEmail = _email,
                    Amount = 4500.00m,
                    HoursWorked = 30m,
                    HourlyRate = 150m,
                    Status = ClaimStatus.Submitted,
                    SubmissionDate = DateTime.Now.AddDays(-5),
                    Description = "Teaching: Software Engineering - 30 hours",
                    Documents = new List<Document>()
                };

                _claims.Add(sampleClaim);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                UserSession.Logout();
                new MainWindow().Show();
                Close();
            }
        }

        private void NewClaim_Click(object sender, RoutedEventArgs e)
        {
            ClearClaimForm();
            MessageBox.Show("Form cleared. Ready to submit a new claim!", "New Claim",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UploadDocument_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Supporting Documents",
                Filter = "PDF Files (*.pdf)|*.pdf|Word Documents (*.docx)|*.docx|Excel Files (*.xlsx)|*.xlsx|Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All Files (*.*)|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                long maxBytes = 10 * 1024 * 1024; // 10 MB
                var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".pdf", ".docx", ".xlsx", ".jpg", ".jpeg", ".png" };

                int added = 0;
                foreach (string file in dialog.FileNames)
                {
                    string ext = System.IO.Path.GetExtension(file);
                    long size = new System.IO.FileInfo(file).Length;

                    if (!allowedExt.Contains(ext))
                    {
                        MessageBox.Show($"File type not allowed: {ext}\nAllowed types: PDF, DOCX, XLSX, JPG, PNG",
                            "Invalid File Type", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    if (size > maxBytes)
                    {
                        MessageBox.Show($"File too large: {System.IO.Path.GetFileName(file)}\nMaximum size: 10 MB",
                            "File Too Large", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    if (!_uploadedDocuments.Contains(file))
                    {
                        _uploadedDocuments.Add(file);
                        lstDocuments.Items.Add(System.IO.Path.GetFileName(file));
                        added++;
                    }
                }

                if (added > 0)
                {
                    MessageBox.Show($"{added} document(s) uploaded successfully.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void CalculateAmount(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtHoursWorked.Text, out decimal hours) &&
                decimal.TryParse(txtHourlyRate.Text, out decimal rate))
            {
                txtClaimAmount.Text = (hours * rate).ToString("F2");
            }
            else
            {
                txtClaimAmount.Text = "0.00";
            }
        }

        private void SubmitClaim_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateClaimForm())
                return;

            try
            {
                decimal.TryParse(txtClaimAmount.Text, out decimal amount);
                decimal.TryParse(txtHoursWorked.Text, out decimal hours);
                decimal.TryParse(txtHourlyRate.Text, out decimal rate);

                var newClaim = new Claim
                {
                    Id = 0, // Will be generated by database manager
                    LecturerId = _email,
                    LecturerName = _role,
                    LecturerEmail = _email,
                    Amount = amount,
                    HoursWorked = hours,
                    HourlyRate = rate,
                    Status = ClaimStatus.Submitted,
                    SubmissionDate = DateTime.Now,
                    Description = txtDescription.Text,
                    Documents = _uploadedDocuments.Select(f => new Document
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        FileName = System.IO.Path.GetFileName(f),
                        FileType = System.IO.Path.GetExtension(f),
                        FileSizeBytes = new System.IO.FileInfo(f).Length,
                        UploadDate = DateTime.Now,
                       
                    }).ToList()
                };

                // Save to database (this will generate the ID)
                JsonDatabaseManager.Instance.SaveClaim(newClaim);

                // Add to display collection
                _claims.Insert(0, newClaim);

                // Log the action
                AuditLogger.LogAction("SubmitClaim", $"ClaimId={newClaim.Id}, Amount={newClaim.Amount:C}");

                MessageBox.Show($"Claim submitted successfully!\n\nClaim ID: CLM-{newClaim.Id}\nAmount: R{newClaim.Amount:N2}\nStatus: {newClaim.Status}",
                    "Claim Submitted", MessageBoxButton.OK, MessageBoxImage.Information);

                ClearClaimForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting claim: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                AuditLogger.LogAction("SubmitClaimError", ex.Message);
            }
        }

        private void ClaimsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (claimsDataGrid.SelectedItem is Claim selectedClaim)
            {
                string documents = selectedClaim.Documents != null && selectedClaim.Documents.Any()
                    ? $"\nDocuments: {selectedClaim.Documents.Count}"
                    : "\nDocuments: None";

                string details =
                    $"Claim ID: CLM-{selectedClaim.Id}\n" +
                    $"Lecturer: {selectedClaim.LecturerName}\n" +
                    $"Email: {selectedClaim.LecturerEmail}\n" +
                    $"Amount: R{selectedClaim.Amount:N2}\n" +
                    $"Hours Worked: {selectedClaim.HoursWorked}\n" +
                    $"Hourly Rate: R{selectedClaim.HourlyRate:N2}\n" +
                    $"Status: {selectedClaim.Status}\n" +
                    $"Submitted: {selectedClaim.SubmissionDate:dd/MM/yyyy HH:mm}\n" +
                    $"Description: {selectedClaim.Description}" +
                    documents;

                MessageBox.Show(details, "Claim Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool ValidateClaimForm()
        {
            if (!decimal.TryParse(txtHoursWorked.Text, out decimal hours) || hours <= 0)
            {
                MessageBox.Show("Please enter valid hours worked (must be greater than 0).",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtHoursWorked.Focus();
                return false;
            }

            if (!decimal.TryParse(txtHourlyRate.Text, out decimal rate) || rate <= 0)
            {
                MessageBox.Show("Please enter a valid hourly rate (must be greater than 0).",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtHourlyRate.Focus();
                return false;
            }

            if (!decimal.TryParse(txtClaimAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid claim amount (must be greater than 0).",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtClaimAmount.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Description is required. Please provide details about your claim.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDescription.Focus();
                return false;
            }

            if (txtDescription.Text.Length < 10)
            {
                MessageBox.Show("Description must be at least 10 characters long.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDescription.Focus();
                return false;
            }

            if (_uploadedDocuments.Count == 0)
            {
                var result = MessageBox.Show("No supporting documents uploaded. Do you want to continue without documents?",
                    "No Documents", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return false;
            }

            return true;
        }

        private void ClearClaimForm()
        {
            txtClaimAmount.Clear();
            txtDescription.Clear();
            txtHoursWorked.Clear();
            txtHourlyRate.Clear();
            lstDocuments.Items.Clear();
            _uploadedDocuments.Clear();
        }

        private void ViewClaimDetails_Click(object sender, RoutedEventArgs e)
        {
            if (claimsDataGrid.SelectedItem is Claim claim)
            {
                if (claim.Documents != null && claim.Documents.Any())
                {
                    string docs = string.Join("\n\n", claim.Documents.Select((d, index) =>
                        $"Document {index + 1}:\n" +
                        $"  Name: {d.FileName}\n" +
                        $"  Type: {d.FileType}\n" +
                        $"  Size: {FormatFileSize(d.FileSizeBytes)}\n" +
                        $"  Uploaded: {d.UploadDate:dd/MM/yyyy HH:mm}"));

                    MessageBox.Show(docs, $"Documents for Claim CLM-{claim.Id}",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No documents attached to this claim.", "No Documents",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a claim first.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}