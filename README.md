# Contract Monthly Claim System (CMCS)

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-Core%206.0-purple.svg)
![Status](https://img.shields.io/badge/status-active-success.svg)

## 📋 Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [System Architecture](#system-architecture)
- [Database Schema](#database-schema)
- [User Roles](#user-roles)
- [Screenshots](#screenshots)
- [Testing](#testing)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## 🎯 Overview

The **Contract Monthly Claim System (CMCS)** is a comprehensive web-based automation solution designed to streamline the entire lifecycle of lecturer claim submissions, verification, and payment processing for educational institutions.

The system eliminates manual paperwork, reduces processing time, and ensures accurate payment calculations through automated workflows.

### Key Benefits
- ⚡ **Automated Calculations** - Hours × Rate = Payment (no manual errors)
- 🔄 **Workflow Automation** - Streamlined approval process
- 📊 **Comprehensive Reporting** - Real-time insights and analytics
- ✅ **Policy Compliance** - Automatic validation checks
- 🔒 **Secure & Auditable** - Complete transaction history

## ✨ Features

### For Lecturers
- 📝 **Easy Claim Submission** - Intuitive form with auto-calculations
- 💾 **Save as Draft** - Complete claims at your own pace
- 📎 **Document Upload** - Attach timesheets and supporting documents
- 📈 **Status Tracking** - Real-time claim status updates
- 📜 **Claim History** - View all past submissions and payments

### For Coordinators/Managers
- ✅ **Approval Workflow** - Review and approve/reject claims
- 🔍 **Policy Validation** - Automated compliance checks
- 💬 **Comment System** - Provide feedback on claims
- 📊 **Department Dashboard** - Overview of all department claims
- 🔔 **Notifications** - Alerts for pending approvals

### For HR Department
- 💰 **Payment Processing** - Automated invoice generation
- 📄 **Report Generation** - Comprehensive payment reports
- 👥 **Lecturer Management** - Manage lecturer profiles and rates
- 📊 **Analytics Dashboard** - Budget tracking and forecasting
- 📥 **Export Functions** - PDF, Excel, CSV exports

### Technical Features
- 🔐 **Authentication & Authorization** - Role-based access control
- ✔️ **Client-side Validation** - jQuery validation for instant feedback
- 🗄️ **Entity Framework** - Robust data persistence
- 📱 **Responsive Design** - Works on all devices
- 🔄 **Audit Trail** - Complete transaction logging

## 🛠️ Technology Stack

### Backend
- **Framework:** ASP.NET Core MVC 6.0 / ASP.NET MVC 5
- **ORM:** Entity Framework Core 6.0
- **Authentication:** ASP.NET Identity
- **Database:** SQL Server 2019+
- **Language:** C# 10.0
- **API:** RESTful Web API

### Frontend
- **View Engine:** Razor Pages / Web Forms
- **UI Framework:** Bootstrap 5
- **JavaScript:** jQuery 3.6+
- **Validation:** jQuery Validation
- **Charts:** Chart.js / D3.js
- **Icons:** Font Awesome

### Reporting
- **Tools:** SSRS (SQL Server Reporting Services)
- **Alternative:** Crystal Reports
- **Data Query:** LINQ to Entities
- **Export Formats:** PDF, Excel, CSV

### Additional Tools
- **Version Control:** Git
- **Package Manager:** NuGet
- **Build Tool:** MSBuild
- **Testing:** xUnit / NUnit
- **CI/CD:** Azure DevOps / GitHub Actions

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community, Professional, or Enterprise)
- [SQL Server 2019](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or later
- [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- [Node.js](https://nodejs.org/) (for frontend package management - optional)
- [Git](https://git-scm.com/downloads)

### Recommended Extensions (Visual Studio)
- Web Essentials
- ReSharper (optional)
- Entity Framework Power Tools

## 🚀 Installation

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/cmcs.git
cd cmcs
```

### 2. Restore NuGet Packages
```bash
dotnet restore
```

Or in Visual Studio:
- Right-click on Solution → **Restore NuGet Packages**

### 3. Update Database Connection String

Open `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=CMCS_DB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 4. Run Database Migrations

```bash
dotnet ef database update
```

Or in Visual Studio Package Manager Console:
```powershell
Update-Database
```

### 5. Seed Initial Data (Optional)

Run the seed script to populate initial data:
```bash
dotnet run --seed
```

### 6. Build and Run

```bash
dotnet build
dotnet run
```

Or in Visual Studio:
- Press `F5` or click **Start Debugging**

The application will launch at: `https://localhost:5001`

## ⚙️ Configuration

### Application Settings

Edit `appsettings.json` to configure:

```json
{
  "AppSettings": {
    "MaxClaimAmount": 50000,
    "MaxHoursPerMonth": 200,
    "DefaultHourlyRate": 500,
    "RequireDocumentUpload": true,
    "AllowedFileTypes": ".pdf,.docx,.xlsx,.jpg,.png",
    "MaxFileSize": 5242880
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@yrosebank.edu",
    "SenderName": "CMCS System"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### User Roles Configuration

Default roles are created during database initialization:
- **Administrator** - Full system access
- **HR** - Payment processing and reporting
- **Coordinator** - Claim approval and department management
- **Lecturer** - Claim submission and tracking

### Email Notifications

Configure SMTP settings for email notifications in `appsettings.json`.

## 📖 Usage

### For Lecturers

1. **Login** to the system with your credentials
2. Navigate to **Submit Claim**
3. Fill in the claim form:
   - Select month and year
   - Enter hours worked
   - System auto-calculates payment
   - Upload supporting documents
4. **Save as Draft** or **Submit** for approval
5. Track claim status in **My Claims**

### For Coordinators

1. **Login** with coordinator credentials
2. Navigate to **Pending Claims**
3. Review claim details:
   - Verify hours worked
   - Check supporting documents
   - System shows policy compliance status
4. **Approve** or **Reject** with comments
5. View department reports in **Dashboard**

### For HR

1. **Login** with HR credentials
2. Navigate to **Claims for Payment**
3. Review approved claims
4. **Generate Invoices**
5. Process payments
6. Generate reports:
   - Monthly payment summary
   - Lecturer payment history
   - Department budget reports

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────┐
│                   Presentation Layer                 │
│  (Razor Views, JavaScript, jQuery, Bootstrap)       │
└────────────────┬────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────┐
│                  Application Layer                   │
│     (Controllers, ViewModels, Validation)           │
└────────────────┬────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────┐
│                   Business Layer                     │
│   (Services, Workflow Engine, Business Logic)       │
└────────────────┬────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────┐
│                    Data Layer                        │
│   (Entity Framework, Repositories, DbContext)       │
└────────────────┬────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────┐
│                   Database Layer                     │
│              (SQL Server Database)                   │
└─────────────────────────────────────────────────────┘
```

## 🗄️ Database Schema

### Key Tables

**Claims**
- ClaimId (PK)
- LecturerId (FK)
- MonthYear
- HoursWorked
- HourlyRate
- TotalAmount
- Status (Pending/Approved/Rejected/Paid)
- SubmissionDate
- ApprovalDate

**Lecturers**
- LecturerId (PK)
- FirstName
- LastName
- Email
- PhoneNumber
- DepartmentId (FK)
- HourlyRate
- ContractStartDate
- ContractEndDate

**Approvals**
- ApprovalId (PK)
- ClaimId (FK)
- ApproverId (FK)
- ApprovalDate
- Status
- Comments

**Documents**
- DocumentId (PK)
- ClaimId (FK)
- FileName
- FilePath
- FileType
- UploadDate

## 👥 User Roles

| Role          | Permissions                                          |
|---------------|------------------------------------------------------|
| Administrator | Full system access, user management, system settings |
| HR           | Payment processing, reporting, lecturer management   |
| Coordinator  | Claim approval, department oversight                 |
| Lecturer     | Claim submission, view own claims                    |


## 🧪 Testing

### Run Unit Tests
```bash
dotnet test
```

### Run Integration Tests
```bash
dotnet test --filter Category=Integration
```

### Test Coverage
```bash
dotnet test /p:CollectCoverage=true
```

### Test User Accounts

**Administrator:**
- Username: `admin@institution.edu`
- Password: `Admin@123`

**Coordinator:**
- Username: `coordinator@institution.edu`
- Password: `Coord@123`

**Lecturer:**
- Username: `lecturer@institution.edu`
- Password: `Lect@123`

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Coding Standards
- Follow C# coding conventions
- Write unit tests for new features
- Update documentation
- Use meaningful commit messages

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Contact

**Project Maintainer:** Your Name
- Email: your.email@institution.edu
- GitHub: [@yourusername](https://github.com/St10443192)

**Institution:** Your Institution Name
- Website: https://www.yourinstitution.edu
- Support: support@yourinstitution.edu

## 🙏 Acknowledgments

- ASP.NET Core Team for the excellent framework
- Entity Framework Team for the ORM
- Bootstrap Team for the UI framework
- All contributors who helped build this system

---



*Last Updated: November 2025*
