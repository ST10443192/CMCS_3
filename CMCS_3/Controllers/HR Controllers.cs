using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS_3.Data;
using CMCS_3.Models;

namespace CMCS_3.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HRController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: HR Dashboard
        public async Task<IActionResult> Index()
        {
            // Statistics
            ViewBag.TotalLecturers = await _context.Lecturers.CountAsync();
            ViewBag.ActiveLecturers = await _context.Lecturers.CountAsync(l => l.IsActive);
            ViewBag.PendingPayments = await _context.Claims.CountAsync(c => c.Status == "Approved");
            ViewBag.MonthlyClaimsCount = await _context.Claims
                .CountAsync(c => c.SubmissionDate.Month == DateTime.Now.Month
                              && c.SubmissionDate.Year == DateTime.Now.Year);

            // Recent Lecturers
            ViewBag.RecentLecturers = await _context.Lecturers
                .OrderByDescending(l => l.CreatedDate)
                .Take(5)
                .ToListAsync();

            // Payment Summary
            var currentMonthClaims = await _context.Claims
                .Where(c => c.SubmissionDate.Month == DateTime.Now.Month
                         && c.SubmissionDate.Year == DateTime.Now.Year)
                .ToListAsync();

            ViewBag.PendingAmount = currentMonthClaims.Where(c => c.Status == "Pending").Sum(c => c.TotalAmount);
            ViewBag.ApprovedAmount = currentMonthClaims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);
            ViewBag.PaidAmount = currentMonthClaims.Where(c => c.Status == "Paid").Sum(c => c.TotalAmount);
            ViewBag.RejectedAmount = currentMonthClaims.Where(c => c.Status == "Rejected").Sum(c => c.TotalAmount);

            return View();
        }

        // GET: All Lecturers
        public async Task<IActionResult> Lecturers(string search, string department, string status, int page = 1)
        {
            var query = _context.Lecturers.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l => l.FirstName.Contains(search)
                                      || l.LastName.Contains(search)
                                      || l.EmployeeId.Contains(search));
                ViewBag.SearchTerm = search;
            }

            // Department filter
            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(l => l.Department == department);
                ViewBag.SelectedDepartment = department;
            }

            // Status filter
            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "active";
                query = query.Where(l => l.IsActive == isActive);
                ViewBag.SelectedStatus = status;
            }

            // Get departments for filter dropdown
            ViewBag.Departments = await _context.Lecturers
                .Select(l => l.Department)
                .Distinct()
                .ToListAsync();

            // Pagination
            int pageSize = 10;
            int totalItems = await query.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentPage = page;

            var lecturers = await query
                .OrderByDescending(l => l.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(lecturers);
        }

        // GET: Create Lecturer
        public IActionResult CreateLecturer()
        {
            return View();
        }

        // POST: Create Lecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLecturer(Lecturer lecturer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists
                    var existingLecturer = await _context.Lecturers
                        .FirstOrDefaultAsync(l => l.Email == lecturer.Email);

                    if (existingLecturer != null)
                    {
                        TempData["ErrorMessage"] = "A lecturer with this email already exists.";
                        return View(lecturer);
                    }

                    // Check if Employee ID already exists
                    var existingEmployeeId = await _context.Lecturers
                        .FirstOrDefaultAsync(l => l.EmployeeId == lecturer.EmployeeId);

                    if (existingEmployeeId != null)
                    {
                        TempData["ErrorMessage"] = "A lecturer with this Employee ID already exists.";
                        return View(lecturer);
                    }

                    // Create user account for lecturer
                    var user = new ApplicationUser
                    {
                        UserName = lecturer.Email,
                        Email = lecturer.Email,
                        EmailConfirmed = true,
                        FirstName = lecturer.FirstName,
                        LastName = lecturer.LastName
                    };

                    string tempPassword = GenerateTemporaryPassword();
                    var result = await _userManager.CreateAsync(user, tempPassword);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Lecturer");

                        lecturer.UserId = user.Id;
                        lecturer.CreatedDate = DateTime.Now;
                        lecturer.IsActive = true;

                        _context.Lecturers.Add(lecturer);
                        await _context.SaveChangesAsync();

                        // TODO: Send email with temporary password
                        TempData["SuccessMessage"] = $"Lecturer {lecturer.FullName} created successfully. Temporary password: {tempPassword}";
                        return RedirectToAction(nameof(Lecturers));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        TempData["ErrorMessage"] = "Failed to create user account.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating lecturer: {ex.Message}";
                }
            }

            return View(lecturer);
        }

        // GET: Edit Lecturer
        public async Task<IActionResult> EditLecturer(int id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer == null)
            {
                return NotFound();
            }
            return View(lecturer);
        }

        // POST: Edit Lecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLecturer(int id, Lecturer lecturer)
        {
            if (id != lecturer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    lecturer.ModifiedDate = DateTime.Now;
                    _context.Update(lecturer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Lecturer updated successfully.";
                    return RedirectToAction(nameof(Lecturers));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Lecturers.AnyAsync(l => l.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating lecturer: {ex.Message}";
                }
            }
            return View(lecturer);
        }

        // POST: Delete Lecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLecturer(int id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer != null)
            {
                try
                {
                    // Delete associated user account
                    if (!string.IsNullOrEmpty(lecturer.UserId))
                    {
                        var user = await _userManager.FindByIdAsync(lecturer.UserId);
                        if (user != null)
                        {
                            await _userManager.DeleteAsync(user);
                        }
                    }

                    _context.Lecturers.Remove(lecturer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Lecturer deleted successfully.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error deleting lecturer: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Lecturer not found.";
            }
            return RedirectToAction(nameof(Lecturers));
        }

        // GET: Process Payments
        public async Task<IActionResult> ProcessPayments(string status = "approved", string? department = null,
            DateTime? fromDate = null, DateTime? toDate = null, int page = 1)
        {
            var query = _context.Claims.Include(c => c.Lecturer).AsQueryable();

            // Status filter
            if (status != "all")
            {
                string statusFilter = status switch
                {
                    "approved" => "Approved",
                    "processing" => "Processing",
                    "paid" => "Paid",
                    _ => "Approved"
                };
                query = query.Where(c => c.Status == statusFilter);
            }
            ViewBag.SelectedStatus = status;

            // Department filter
            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(c => c.Lecturer != null && c.Lecturer.Department == department);
                ViewBag.SelectedDepartment = department;
            }

            // Date filters
            if (fromDate.HasValue)
            {
                query = query.Where(c => c.ApprovedDate >= fromDate.Value);
                ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            }
            if (toDate.HasValue)
            {
                query = query.Where(c => c.ApprovedDate <= toDate.Value);
                ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            }

            // Summary statistics
            ViewBag.PendingCount = await _context.Claims.CountAsync(c => c.Status == "Approved");
            ViewBag.PendingAmount = await _context.Claims.Where(c => c.Status == "Approved").SumAsync(c => c.TotalAmount);
            ViewBag.ProcessingCount = await _context.Claims.CountAsync(c => c.Status == "Processing");
            ViewBag.ProcessingAmount = await _context.Claims.Where(c => c.Status == "Processing").SumAsync(c => c.TotalAmount);
            ViewBag.PaidCount = await _context.Claims.CountAsync(c => c.Status == "Paid"
                && c.PaidDate.HasValue && c.PaidDate.Value.Month == DateTime.Now.Month);
            ViewBag.PaidAmount = await _context.Claims.Where(c => c.Status == "Paid"
                && c.PaidDate.HasValue && c.PaidDate.Value.Month == DateTime.Now.Month).SumAsync(c => c.TotalAmount);
            ViewBag.UniqueLecturers = await _context.Claims.Where(c => c.Status == "Approved")
                .Select(c => c.LecturerId).Distinct().CountAsync();

            // Departments for filter
            ViewBag.Departments = await _context.Lecturers.Select(l => l.Department).Distinct().ToListAsync();

            // Pagination
            int pageSize = 15;
            ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);
            ViewBag.CurrentPage = page;

            var claims = await query
                .OrderByDescending(c => c.ApprovedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(claims);
        }

        // POST: Process Bulk Payments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessBulkPayments(int[] selectedClaims, string paymentReference,
            DateTime paymentDate, string notes)
        {
            if (selectedClaims == null || selectedClaims.Length == 0)
            {
                TempData["ErrorMessage"] = "No claims selected for payment.";
                return RedirectToAction(nameof(ProcessPayments));
            }

            try
            {
                var claims = await _context.Claims
                    .Where(c => selectedClaims.Contains(c.Id) && c.Status == "Approved")
                    .ToListAsync();

                foreach (var claim in claims)
                {
                    claim.Status = "Paid";
                    claim.PaidDate = paymentDate;
                    claim.PaymentReference = paymentReference;
                    claim.PaymentNotes = notes;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully processed payment for {claims.Count} claim(s).";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing payments: {ex.Message}";
            }

            return RedirectToAction(nameof(ProcessPayments));
        }

        // GET: Lecturer Details
        public async Task<IActionResult> LecturerDetails(int id)
        {
            var lecturer = await _context.Lecturers
                .Include(l => l.Claims)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lecturer == null)
            {
                return NotFound();
            }

            return View(lecturer);
        }

        // GET: Lecturer Claims
        public async Task<IActionResult> LecturerClaims(int id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer == null)
            {
                return NotFound();
            }

            ViewBag.Lecturer = lecturer;
            var claims = await _context.Claims
                .Where(c => c.LecturerId == id)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();

            return View(claims);
        }

        // Helper method to generate temporary password
        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}