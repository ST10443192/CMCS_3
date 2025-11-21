using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ContractMonthlyClaimSystem2.Models;
using ContractMonthlyClaimSystem2.Helpers;

namespace ContractMonthlyClaimSystem2.Database
{
    public sealed class JsonDatabaseManager
    {
        private static readonly Lazy<JsonDatabaseManager> _instance =
            new Lazy<JsonDatabaseManager>(() => new JsonDatabaseManager());
        public static JsonDatabaseManager Instance => _instance.Value;

        private readonly string dataFolder;
        private readonly string usersPath;
        private readonly string claimsPath;

        private List<User> usersCache;
        private List<Claim> claimsCache;

        private readonly JsonSerializerSettings jsonSettings;

        private JsonDatabaseManager()
        {
            // Configure JSON settings for better compatibility
            jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // Set up data folder in a common location
            dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                "ContractMonthlyClaimSystem2", "JsonDatabase"
            );

            // Ensure directory exists
            Directory.CreateDirectory(dataFolder);

            usersPath = Path.Combine(dataFolder, "users.json");
            claimsPath = Path.Combine(dataFolder, "claims.json");

            // Load existing data
            LoadUsers();
            LoadClaims();
            InitializeDefaultUsers();
        }

        // =====================================================
        // ============== JSON SERIALIZATION HELPERS ===========
        // =====================================================

        private T Deserialize<T>(string path) where T : class, new()
        {
            try
            {
                if (!File.Exists(path))
                    return new T();

                string json = File.ReadAllText(path);

                if (string.IsNullOrWhiteSpace(json))
                    return new T();

                return JsonConvert.DeserializeObject<T>(json, jsonSettings) ?? new T();
            }
            catch (Exception ex)
            {
                AuditLogger.LogAction("Deserialization Error", $"Failed to deserialize {path}: {ex.Message}");
                return new T();
            }
        }

        private void Serialize<T>(string path, T data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data, jsonSettings);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                AuditLogger.LogAction("Serialization Error", $"Failed to serialize to {path}: {ex.Message}");
                throw;
            }
        }

        // =====================================================
        // ================ LOAD & SAVE USERS ==================
        // =====================================================

        private void LoadUsers()
        {
            usersCache = Deserialize<List<User>>(usersPath);
            if (usersCache == null)
                usersCache = new List<User>();
        }

        private void SaveUsers()
        {
            try
            {
                Serialize(usersPath, usersCache);
            }
            catch (Exception ex)
            {
                AuditLogger.LogAction("Save Users Error", $"Failed to save users: {ex.Message}");
            }
        }

        // =====================================================
        // ================ LOAD & SAVE CLAIMS =================
        // =====================================================

        private void LoadClaims()
        {
            claimsCache = Deserialize<List<Claim>>(claimsPath);
            if (claimsCache == null)
                claimsCache = new List<Claim>();
        }

        private void SaveClaims()
        {
            try
            {
                Serialize(claimsPath, claimsCache);
            }
            catch (Exception ex)
            {
                AuditLogger.LogAction("Save Claims Error", $"Failed to save claims: {ex.Message}");
            }
        }

        // =====================================================
        // ====================== USERS ========================
        // =====================================================

        public User AuthenticateUser(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var user = usersCache.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                AuditLogger.LogAction("User Authentication", $"User {email} authenticated successfully");
            }

            return user;
        }

        public bool CreateUser(string email, string password, string fullName, string role)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if (usersCache.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                AuditLogger.LogAction("User Creation Failed", $"User {email} already exists");
                return false;
            }

            var user = new User
            {
                Id = GenerateUserId(),
                Email = email,
                FullName = fullName ?? "Unknown User",
                Role = role ?? "Lecturer"
            };

            usersCache.Add(user);
            SaveUsers();

            AuditLogger.LogAction("User Created", $"User {email} created with role {role}");
            return true;
        }

        public List<User> GetAllUsers()
        {
            return usersCache.ToList();
        }

        public User GetUserById(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return usersCache.FirstOrDefault(u => u.Id == userId);
        }

        public bool UpdateUser(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
                return false;

            var existingUser = usersCache.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser == null)
                return false;

            existingUser.Email = user.Email;
            existingUser.FullName = user.FullName;
            existingUser.Role = user.Role;

            SaveUsers();
            AuditLogger.LogAction("User Updated", $"User {user.Id} updated");
            return true;
        }

        public bool DeleteUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var user = usersCache.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return false;

            usersCache.Remove(user);
            SaveUsers();

            AuditLogger.LogAction("User Deleted", $"User {userId} deleted");
            return true;
        }

        // =====================================================
        // ====================== CLAIMS =======================
        // =====================================================

        public void SaveClaim(Claim claim)
        {
            if (claim == null)
                return;

            if (claim.Id == 0)
            {
                claim.Id = GenerateClaimId();
                claimsCache.Add(claim);
                AuditLogger.LogAction("Claim Created", $"New claim {claim.Id} created for {claim.LecturerEmail}");
            }
            else
            {
                var existingClaim = claimsCache.FirstOrDefault(c => c.Id == claim.Id);
                if (existingClaim != null)
                {
                    int index = claimsCache.IndexOf(existingClaim);
                    claimsCache[index] = claim;
                    AuditLogger.LogAction("Claim Updated", $"Claim {claim.Id} updated");
                }
                else
                {
                    claimsCache.Add(claim);
                    AuditLogger.LogAction("Claim Added", $"Claim {claim.Id} added to database");
                }
            }

            SaveClaims();
        }

        public List<Claim> GetAllClaims()
        {
            return claimsCache.ToList();
        }

        public List<Claim> GetClaimsByLecturer(string lecturerEmail)
        {
            if (string.IsNullOrWhiteSpace(lecturerEmail))
                return new List<Claim>();

            return claimsCache.Where(c =>
                c.LecturerEmail != null &&
                c.LecturerEmail.Equals(lecturerEmail, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<Claim> GetClaimsByStatus(ClaimStatus status)
        {
            return claimsCache.Where(c => c.Status == status).ToList();
        }

        public Claim GetClaimById(int claimId)
        {
            return claimsCache.FirstOrDefault(c => c.Id == claimId);
        }

        public void UpdateClaim(Claim claim)
        {
            if (claim == null)
                return;

            var existingClaim = claimsCache.FirstOrDefault(c => c.Id == claim.Id);
            if (existingClaim != null)
            {
                int index = claimsCache.IndexOf(existingClaim);
                claimsCache[index] = claim;
                SaveClaims();
                AuditLogger.LogAction("Claim Updated", $"Claim {claim.Id} status changed to {claim.Status}");
            }
        }

        public bool DeleteClaim(int claimId)
        {
            var claim = claimsCache.FirstOrDefault(c => c.Id == claimId);
            if (claim == null)
                return false;

            claimsCache.Remove(claim);
            SaveClaims();

            AuditLogger.LogAction("Claim Deleted", $"Claim {claimId} deleted");
            return true;
        }

        // =====================================================
        // ==================== HELPERS ========================
        // =====================================================

        private string GenerateUserId()
        {
            if (usersCache == null || usersCache.Count == 0)
                return "U0001";

            var lastId = usersCache
                .Select(u => u.Id)
                .Where(id => !string.IsNullOrWhiteSpace(id) && id.StartsWith("U") && id.Length == 5)
                .OrderByDescending(id => id)
                .FirstOrDefault();

            if (lastId == null)
                return "U0001";

            if (int.TryParse(lastId.Substring(1), out int number))
                return $"U{(number + 1):D4}";

            return $"U{usersCache.Count + 1:D4}";
        }

        private int GenerateClaimId()
        {
            if (claimsCache == null || claimsCache.Count == 0)
                return 1001;

            return claimsCache.Max(c => c.Id) + 1;
        }

        private void InitializeDefaultUsers()
        {
            if (usersCache.Count > 0)
                return;

            CreateUser("admin@university.ac.za", "Admin@123", "System Administrator", "Admin");
            CreateUser("lecturer@university.ac.za", "Lecturer@123", "Dr. John Lecturer", "Lecturer");
            CreateUser("coordinator@university.ac.za", "Coordinator@123", "Academic Coordinator", "Coordinator");
            CreateUser("manager@university.ac.za", "Manager@123", "Programme Manager", "Manager");

            AuditLogger.LogAction("System Initialization", "Default users created successfully");
        }

        // =====================================================
        // ================== DATABASE STATS ===================
        // =====================================================

        public Dictionary<string, int> GetDatabaseStats()
        {
            return new Dictionary<string, int>
            {
                { "TotalUsers", usersCache?.Count ?? 0 },
                { "TotalClaims", claimsCache?.Count ?? 0 },
                { "SubmittedClaims", claimsCache?.Count(c => c.Status == ClaimStatus.Submitted) ?? 0 },
                { "UnderReviewClaims", claimsCache?.Count(c => c.Status == ClaimStatus.UnderReview) ?? 0 },
                { "ApprovedClaims", claimsCache?.Count(c => c.Status == ClaimStatus.Approved) ?? 0 },
                { "RejectedClaims", claimsCache?.Count(c => c.Status == ClaimStatus.Rejected) ?? 0 },
                { "PaidClaims", claimsCache?.Count(c => c.Status == ClaimStatus.Paid) ?? 0 }
            };
        }

        public void ClearAllData()
        {
            usersCache.Clear();
            claimsCache.Clear();
            SaveUsers();
            SaveClaims();
            InitializeDefaultUsers();

            AuditLogger.LogAction("Database Cleared", "All data cleared and defaults restored");
        }

        // =====================================================
        // ================ BACKUP & RESTORE ===================
        // =====================================================

        public string BackupDatabase()
        {
            try
            {
                string backupFolder = Path.Combine(dataFolder, "Backups");
                Directory.CreateDirectory(backupFolder);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = Path.Combine(backupFolder, $"Backup_{timestamp}");
                Directory.CreateDirectory(backupPath);

                File.Copy(usersPath, Path.Combine(backupPath, "users.json"), true);
                File.Copy(claimsPath, Path.Combine(backupPath, "claims.json"), true);

                AuditLogger.LogAction("Database Backup", $"Database backed up to {backupPath}");
                return backupPath;
            }
            catch (Exception ex)
            {
                AuditLogger.LogAction("Backup Error", $"Failed to backup database: {ex.Message}");
                return null;
            }
        }

        public bool RestoreDatabase(string backupPath)
        {
            try
            {
                string backupUsersPath = Path.Combine(backupPath, "users.json");
                string backupClaimsPath = Path.Combine(backupPath, "claims.json");

                if (!File.Exists(backupUsersPath) || !File.Exists(backupClaimsPath))
                    return false;

                File.Copy(backupUsersPath, usersPath, true);
                File.Copy(backupClaimsPath, claimsPath, true);

                LoadUsers();
                LoadClaims();

                AuditLogger.LogAction("Database Restore", $"Database restored from {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                AuditLogger.LogAction("Restore Error", $"Failed to restore database: {ex.Message}");
                return false;
            }
        }

        // =====================================================
        // =================== DATA EXPORT =====================
        // =====================================================

        public string ExportToJson(string exportPath)
        {
            try
            {
                var exportData = new
                {
                    ExportDate = DateTime.Now,
                    Users = usersCache,
                    Claims = claimsCache,
                    Statistics = GetDatabaseStats()
                };

                string json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                File.WriteAllText(exportPath, json);

                AuditLogger.LogAction("Data Export", $"Data exported to {exportPath}");
                return exportPath;
            }
            catch (Exception ex)
            {
                AuditLogger.LogAction("Export Error", $"Failed to export data: {ex.Message}");
                return null;
            }
        }

        // =====================================================
        // ================ DATABASE INFO ======================
        // =====================================================

        public string GetDatabasePath()
        {
            return dataFolder;
        }

        public long GetDatabaseSize()
        {
            try
            {
                long totalSize = 0;

                if (File.Exists(usersPath))
                    totalSize += new FileInfo(usersPath).Length;

                if (File.Exists(claimsPath))
                    totalSize += new FileInfo(claimsPath).Length;

                return totalSize;
            }
            catch
            {
                return 0;
            }
        }

        public DateTime? GetLastModifiedDate()
        {
            try
            {
                DateTime? lastModified = null;

                if (File.Exists(usersPath))
                {
                    var usersMod = File.GetLastWriteTime(usersPath);
                    if (!lastModified.HasValue || usersMod > lastModified.Value)
                        lastModified = usersMod;
                }

                if (File.Exists(claimsPath))
                {
                    var claimsMod = File.GetLastWriteTime(claimsPath);
                    if (!lastModified.HasValue || claimsMod > lastModified.Value)
                        lastModified = claimsMod;
                }

                return lastModified;
            }
            catch
            {
                return null;
            }
        }
    }
}