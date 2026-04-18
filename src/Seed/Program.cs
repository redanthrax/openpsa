using Common.Authorization;
using Common.Database;
using Common.Modules;
using Common.Security;
using Contracts.Agreements;
using Contracts.Assets;
using Contracts.Clients;
using Contracts.Expenses;
using Contracts.Invoicing;
using Contracts.Projects;
using Contracts.Sla;
using Contracts.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenPsa.Modules.Agreements;
using OpenPsa.Modules.Agreements.Models;
using OpenPsa.Modules.Assets;
using OpenPsa.Modules.Assets.Models;
using OpenPsa.Modules.Authentication;
using OpenPsa.Modules.Authentication.Models;
using OpenPsa.Modules.Authentication.Services;
using OpenPsa.Modules.Clients;
using OpenPsa.Modules.Clients.Models;
using OpenPsa.Modules.Contacts;
using OpenPsa.Modules.Contacts.Models;
using OpenPsa.Modules.Dashboard;
using OpenPsa.Modules.Email;
using OpenPsa.Modules.Expenses;
using OpenPsa.Modules.Expenses.Models;
using OpenPsa.Modules.Invoicing;
using OpenPsa.Modules.Invoicing.Models;
using OpenPsa.Modules.Notes;
using OpenPsa.Modules.Notes.Models;
using OpenPsa.Modules.Projects;
using OpenPsa.Modules.Projects.Models;
using OpenPsa.Modules.Security;
using OpenPsa.Modules.Settings;
using OpenPsa.Modules.Settings.Models;
using OpenPsa.Modules.Sla;
using OpenPsa.Modules.Sla.Models;
using OpenPsa.Modules.Tickets;
using OpenPsa.Modules.Tickets.Models;
using OpenPsa.Modules.TimeEntries;
using OpenPsa.Modules.TimeEntries.Models;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

var connectionString = config.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is required");

var moduleAssemblies = new[] {
    typeof(AuthenticationModule).Assembly,
    typeof(ClientsModule).Assembly,
    typeof(ContactsModule).Assembly,
    typeof(ProjectsModule).Assembly,
    typeof(TicketsModule).Assembly,
    typeof(TimeEntriesModule).Assembly,
    typeof(InvoicingModule).Assembly,
    typeof(NotesModule).Assembly,
    typeof(SettingsModule).Assembly,
    typeof(DashboardModule).Assembly,
    typeof(SecurityModule).Assembly,
    typeof(AgreementsModule).Assembly,
    typeof(SlaModule).Assembly,
    typeof(AssetsModule).Assembly,
    typeof(EmailModule).Assembly,
    typeof(ExpensesModule).Assembly,
};

var services = new ServiceCollection();
services.AddSingleton<IPermissionRegistry, PermissionRegistry>();
services.AddModules(moduleAssemblies);
services.AddSingleton<IPiiEncryptionService, NullPiiEncryptionService>();
services.AddDbContext<OpenPsaDbContext>(opts =>
    opts.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly("Api")));

await using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<OpenPsaDbContext>();

Console.WriteLine("Applying migrations...");
await db.Database.MigrateAsync();

if (await db.Set<User>().AnyAsync()) {
    Console.WriteLine("Users already exist — skipping seed.");
    return 0;
}

Console.WriteLine("Seeding comprehensive MSP test data...");
var now = DateTime.UtcNow;
var rng = new Random(42);

// ─── General Settings ───────────────────────────────────────────────
var settings = new GeneralSettings {
    CompanyName = "Apex Technology Solutions",
    CompanyEmail = "info@apextech.io",
    CompanyPhone = "(555) 100-2000",
    CompanyWebsite = "https://apextech.io",
    DefaultCurrency = "USD",
    DefaultPaymentTermsDays = 30,
    CreatedAt = now,
};
db.Set<GeneralSettings>().Add(settings);

// ─── Users ──────────────────────────────────────────────────────────
var admin = new User {
    Email = "admin@openpsa.dev",
    Name = "Admin",
    IsActive = true,
    IsSuperAdmin = true,
    LocalPasswordHash = PasswordHasher.Hash("admin"),
    CreatedAt = now,
};

var sarah = new User {
    Email = "sarah.chen@apextech.io",
    Name = "Sarah Chen",
    IsActive = true,
    LocalPasswordHash = PasswordHasher.Hash("password"),
    CreatedAt = now,
};

var marcus = new User {
    Email = "marcus.williams@apextech.io",
    Name = "Marcus Williams",
    IsActive = true,
    LocalPasswordHash = PasswordHasher.Hash("password"),
    CreatedAt = now,
};

var jennifer = new User {
    Email = "jennifer.garcia@apextech.io",
    Name = "Jennifer Garcia",
    IsActive = true,
    LocalPasswordHash = PasswordHasher.Hash("password"),
    CreatedAt = now,
};

var david = new User {
    Email = "david.kim@apextech.io",
    Name = "David Kim",
    IsActive = true,
    LocalPasswordHash = PasswordHasher.Hash("password"),
    CreatedAt = now,
};

var techs = new[] { sarah, marcus, jennifer, david };
db.Set<User>().AddRange(admin, sarah, marcus, jennifer, david);

// ─── SLA Policies ───────────────────────────────────────────────────
var standardSla = new SlaPolicy {
    Name = "Standard",
    Description = "Default SLA for all clients",
    IsDefault = true,
    CreatedAt = now,
    Targets = [
        new SlaTarget { Priority = SlaPriorityLevel.Critical, ResponseTimeMinutes = 60, ResolutionTimeMinutes = 480, CreatedAt = now },
        new SlaTarget { Priority = SlaPriorityLevel.High, ResponseTimeMinutes = 120, ResolutionTimeMinutes = 960, CreatedAt = now },
        new SlaTarget { Priority = SlaPriorityLevel.Medium, ResponseTimeMinutes = 480, ResolutionTimeMinutes = 2880, CreatedAt = now },
        new SlaTarget { Priority = SlaPriorityLevel.Low, ResponseTimeMinutes = 1440, ResolutionTimeMinutes = 5760, CreatedAt = now },
    ],
};

var premiumSla = new SlaPolicy {
    Name = "Premium",
    Description = "Premium SLA for VIP clients with faster response times",
    IsDefault = false,
    CreatedAt = now,
    Targets = [
        new SlaTarget { Priority = SlaPriorityLevel.Critical, ResponseTimeMinutes = 15, ResolutionTimeMinutes = 240, CreatedAt = now },
        new SlaTarget { Priority = SlaPriorityLevel.High, ResponseTimeMinutes = 60, ResolutionTimeMinutes = 480, CreatedAt = now },
        new SlaTarget { Priority = SlaPriorityLevel.Medium, ResponseTimeMinutes = 240, ResolutionTimeMinutes = 1440, CreatedAt = now },
        new SlaTarget { Priority = SlaPriorityLevel.Low, ResponseTimeMinutes = 480, ResolutionTimeMinutes = 2880, CreatedAt = now },
    ],
};
db.Set<SlaPolicy>().AddRange(standardSla, premiumSla);

// ─── Ticket Queues ──────────────────────────────────────────────────
var triageQueue = new TicketQueue {
    Name = "Triage",
    Description = "Incoming tickets awaiting assignment",
    AssignmentStrategy = TicketQueueAssignmentStrategy.Manual,
    DefaultSlaPolicyId = standardSla.Id,
    SortOrder = 0,
    CreatedAt = now,
};

var networkQueue = new TicketQueue {
    Name = "Network & Infrastructure",
    Description = "Network, firewall, and server issues",
    AssignmentStrategy = TicketQueueAssignmentStrategy.RoundRobin,
    DefaultSlaPolicyId = standardSla.Id,
    SortOrder = 1,
    CreatedAt = now,
};

var securityQueue = new TicketQueue {
    Name = "Security",
    Description = "Security incidents and compliance",
    AssignmentStrategy = TicketQueueAssignmentStrategy.LeastBusy,
    DefaultSlaPolicyId = premiumSla.Id,
    SortOrder = 2,
    CreatedAt = now,
};

var onsiteQueue = new TicketQueue {
    Name = "Onsite",
    Description = "Issues requiring physical presence",
    AssignmentStrategy = TicketQueueAssignmentStrategy.Manual,
    DefaultSlaPolicyId = standardSla.Id,
    SortOrder = 3,
    CreatedAt = now,
};

db.Set<TicketQueue>().AddRange(triageQueue, networkQueue, securityQueue, onsiteQueue);

db.Set<TicketQueueMember>().AddRange(
    new TicketQueueMember { QueueId = triageQueue.Id, UserId = sarah.Id.ToString(), CreatedAt = now },
    new TicketQueueMember { QueueId = triageQueue.Id, UserId = marcus.Id.ToString(), CreatedAt = now },
    new TicketQueueMember { QueueId = networkQueue.Id, UserId = marcus.Id.ToString(), CreatedAt = now },
    new TicketQueueMember { QueueId = networkQueue.Id, UserId = david.Id.ToString(), CreatedAt = now },
    new TicketQueueMember { QueueId = securityQueue.Id, UserId = sarah.Id.ToString(), CreatedAt = now },
    new TicketQueueMember { QueueId = securityQueue.Id, UserId = jennifer.Id.ToString(), CreatedAt = now },
    new TicketQueueMember { QueueId = onsiteQueue.Id, UserId = marcus.Id.ToString(), CreatedAt = now },
    new TicketQueueMember { QueueId = onsiteQueue.Id, UserId = jennifer.Id.ToString(), CreatedAt = now }
);

// ─── Rate Cards ─────────────────────────────────────────────────────
var defaultRateCard = new RateCard {
    Name = "Standard Rates",
    IsDefault = true,
    CreatedAt = now,
    Entries = [
        new RateCardEntry { ServiceType = "General Support", HourlyRate = 150m, AfterHoursRate = 225m, CreatedAt = now },
        new RateCardEntry { ServiceType = "Network Engineering", HourlyRate = 175m, AfterHoursRate = 262.50m, CreatedAt = now },
        new RateCardEntry { ServiceType = "Security Consulting", HourlyRate = 200m, AfterHoursRate = 300m, CreatedAt = now },
        new RateCardEntry { ServiceType = "Project Management", HourlyRate = 165m, AfterHoursRate = 247.50m, CreatedAt = now },
        new RateCardEntry { ServiceType = "Onsite Visit", HourlyRate = 185m, AfterHoursRate = 277.50m, CreatedAt = now },
    ],
};
db.Set<RateCard>().Add(defaultRateCard);

// ─── Clients ────────────────────────────────────────────────────────
var clientData = new (string Name, string Website, string Phone, string Email, string Notes, ClientStatus Status)[] {
    ("Whitfield & Associates LLP", "https://whitfieldlaw.com", "(555) 200-1001", "office@whitfieldlaw.com", "25-person law firm, 3rd floor of Downtown Center. Primary contact: Rebecca Whitfield.", ClientStatus.Active),
    ("Bright Smile Dental Group", "https://brightsmile.dental", "(555) 200-1002", "admin@brightsmile.dental", "4 dentist offices across metro area. HIPAA compliance critical.", ClientStatus.Active),
    ("Meridian Accounting", "https://meridianaccounting.com", "(555) 200-1003", "it@meridianaccounting.com", "Tax season is October-April, no maintenance windows during that period.", ClientStatus.Active),
    ("Cascade Manufacturing", "https://cascademfg.com", "(555) 200-1004", "support@cascademfg.com", "100-employee manufacturing plant. OT/SCADA network isolated. 24/7 operations.", ClientStatus.Active),
    ("Pineview Community Church", "https://pineviewchurch.org", "(555) 200-1005", "office@pineviewchurch.org", "Non-profit. Volunteer-run IT committee. Budget-conscious.", ClientStatus.Active),
    ("TechStart Innovations", "https://techstart.io", "(555) 200-1006", "hello@techstart.io", "Startup. Was evaluating our services but went with in-house IT.", ClientStatus.Churned),
};

var clients = clientData.Select(c => new Client {
    Name = c.Name,
    Website = c.Website,
    Phone = c.Phone,
    Email = c.Email,
    Notes = c.Notes,
    Status = c.Status,
    CreatedAt = now.AddDays(-rng.Next(60, 365)),
}).ToArray();

db.Set<Client>().AddRange(clients);

// ─── Sites ──────────────────────────────────────────────────────────
var siteData = new (int ClientIdx, string Name, string Address, string City, string State, string Zip, bool IsPrimary)[] {
    (0, "Main Office", "400 Downtown Center Dr, Suite 300", "Portland", "OR", "97201", true),
    (1, "Bright Smile – Downtown", "1220 SW Morrison St", "Portland", "OR", "97205", true),
    (1, "Bright Smile – Beaverton", "3500 Cedar Hills Blvd", "Beaverton", "OR", "97005", false),
    (1, "Bright Smile – Lake Oswego", "100 A Ave", "Lake Oswego", "OR", "97034", false),
    (1, "Bright Smile – Tigard", "11600 SW Pacific Hwy", "Tigard", "OR", "97223", false),
    (2, "Meridian HQ", "888 NW 5th Ave", "Portland", "OR", "97209", true),
    (3, "Cascade Plant", "2200 Industrial Pkwy", "Tualatin", "OR", "97062", true),
    (3, "Cascade Admin Office", "2210 Industrial Pkwy", "Tualatin", "OR", "97062", false),
    (4, "Church Campus", "5500 SE Stark St", "Portland", "OR", "97215", true),
    (5, "TechStart Office", "1400 NW Everett St, Suite 200", "Portland", "OR", "97209", true),
};

var sites = siteData.Select(s => new Site {
    ClientId = clients[s.ClientIdx].Id,
    Name = s.Name,
    Address = s.Address,
    City = s.City,
    State = s.State,
    PostalCode = s.Zip,
    Country = "US",
    Timezone = "America/Los_Angeles",
    IsPrimary = s.IsPrimary,
    CreatedAt = clients[s.ClientIdx].CreatedAt.AddMinutes(1),
}).ToArray();

db.Set<Site>().AddRange(sites);

// ─── Contacts ───────────────────────────────────────────────────────
var contactData = new (int ClientIdx, string First, string Last, string? Title, string Email, string Phone, bool IsPrimary)[] {
    (0, "Rebecca", "Whitfield", "Managing Partner", "rebecca@whitfieldlaw.com", "(555) 201-0001", true),
    (0, "James", "Morton", "Office Manager", "james.morton@whitfieldlaw.com", "(555) 201-0002", false),
    (1, "Dr. Lisa", "Tran", "Owner", "lisa.tran@brightsmile.dental", "(555) 202-0001", true),
    (1, "Mike", "Hernandez", "Office Administrator", "mike.h@brightsmile.dental", "(555) 202-0002", false),
    (2, "Patricia", "Nguyen", "Managing Director", "patricia@meridianaccounting.com", "(555) 203-0001", true),
    (2, "Kevin", "O'Brien", "IT Coordinator", "kobrien@meridianaccounting.com", "(555) 203-0002", false),
    (3, "Frank", "Kowalski", "Plant Manager", "frank.k@cascademfg.com", "(555) 204-0001", true),
    (3, "Angela", "Rivera", "IT Director", "angela.r@cascademfg.com", "(555) 204-0002", false),
    (3, "Tom", "Bradley", "Shift Supervisor", "tom.b@cascademfg.com", "(555) 204-0003", false),
    (4, "Pastor Dave", "Simmons", "Senior Pastor", "dave@pineviewchurch.org", "(555) 205-0001", true),
    (4, "Linda", "Foster", "Volunteer IT Lead", "linda.f@pineviewchurch.org", "(555) 205-0002", false),
    (5, "Ryan", "Park", "CTO", "ryan@techstart.io", "(555) 206-0001", true),
};

var contacts = contactData.Select(c => new Contact {
    ClientId = clients[c.ClientIdx].Id,
    FirstName = c.First,
    LastName = c.Last,
    Title = c.Title,
    Email = c.Email,
    Phone = c.Phone,
    IsPrimary = c.IsPrimary,
    CreatedAt = clients[c.ClientIdx].CreatedAt.AddMinutes(2),
}).ToArray();

db.Set<Contact>().AddRange(contacts);

// ─── Agreements ─────────────────────────────────────────────────────
var agreementStart = now.AddMonths(-6);
var agreements = new Agreement[] {
    new() {
        Name = "Whitfield Managed Services",
        Description = "Unlimited remote support + quarterly onsite visits",
        Type = AgreementType.Retainer,
        Status = AgreementStatus.Active,
        ClientId = clients[0].Id,
        StartDate = agreementStart,
        EndDate = agreementStart.AddYears(1),
        MonthlyAmount = 2500m,
        TotalValue = 30000m,
        SlaPolicyId = premiumSla.Id,
        CreatedAt = agreementStart.AddDays(-7),
    },
    new() {
        Name = "Bright Smile Block Hours",
        Description = "200 hours block for all 4 locations, HIPAA compliant support",
        Type = AgreementType.BlockHours,
        Status = AgreementStatus.Active,
        ClientId = clients[1].Id,
        StartDate = agreementStart.AddMonths(1),
        EndDate = agreementStart.AddMonths(1).AddYears(1),
        TotalValue = 28000m,
        BlockHoursTotal = 200m,
        BlockHoursUsed = 87.5m,
        HourlyRate = 140m,
        SlaPolicyId = standardSla.Id,
        CreatedAt = agreementStart.AddMonths(1).AddDays(-5),
    },
    new() {
        Name = "Meridian T&M Support",
        Description = "Time and materials, billed monthly. No maintenance Oct-Apr without approval.",
        Type = AgreementType.TimeAndMaterials,
        Status = AgreementStatus.Active,
        ClientId = clients[2].Id,
        StartDate = agreementStart.AddMonths(-2),
        EndDate = agreementStart.AddMonths(-2).AddYears(2),
        HourlyRate = 150m,
        SlaPolicyId = standardSla.Id,
        CreatedAt = agreementStart.AddMonths(-2).AddDays(-3),
    },
    new() {
        Name = "Cascade Full Managed IT",
        Description = "24/7 monitoring, patching, helpdesk, and quarterly security reviews",
        Type = AgreementType.FixedFee,
        Status = AgreementStatus.Active,
        ClientId = clients[3].Id,
        StartDate = agreementStart.AddMonths(-4),
        EndDate = agreementStart.AddMonths(-4).AddYears(3),
        MonthlyAmount = 8500m,
        TotalValue = 306000m,
        SlaPolicyId = premiumSla.Id,
        RenewalNoticeDays = 90,
        CreatedAt = agreementStart.AddMonths(-4).AddDays(-14),
    },
    new() {
        Name = "Pineview Basic Support",
        Description = "Break/fix support, billed hourly. Non-profit discount applied.",
        Type = AgreementType.TimeAndMaterials,
        Status = AgreementStatus.Active,
        ClientId = clients[4].Id,
        StartDate = agreementStart.AddMonths(2),
        EndDate = agreementStart.AddMonths(2).AddYears(1),
        HourlyRate = 100m,
        SlaPolicyId = standardSla.Id,
        CreatedAt = agreementStart.AddMonths(2).AddDays(-2),
    },
};

var cascadeRateCard = new RateCard {
    Name = "Cascade Manufacturing Rates",
    ClientId = clients[3].Id,
    IsDefault = false,
    CreatedAt = agreements[3].CreatedAt,
    Entries = [
        new RateCardEntry { ServiceType = "General Support", HourlyRate = 135m, AfterHoursRate = 202.50m, CreatedAt = now },
        new RateCardEntry { ServiceType = "Network Engineering", HourlyRate = 160m, AfterHoursRate = 240m, CreatedAt = now },
        new RateCardEntry { ServiceType = "Security Consulting", HourlyRate = 185m, AfterHoursRate = 277.50m, CreatedAt = now },
        new RateCardEntry { ServiceType = "Onsite Visit", HourlyRate = 170m, AfterHoursRate = 255m, CreatedAt = now },
    ],
};
db.Set<RateCard>().Add(cascadeRateCard);
db.Set<Agreement>().AddRange(agreements);

// ─── Projects ───────────────────────────────────────────────────────
var projects = new Project[] {
    new() {
        Name = "Whitfield Office 365 Migration",
        Description = "Migrate from on-prem Exchange to Office 365 with data migration and user training",
        Status = ProjectStatus.Active,
        ClientId = clients[0].Id,
        ManagerUserId = sarah.Id.ToString(),
        StartDate = now.AddDays(-45),
        EndDate = now.AddDays(15),
        BudgetHours = 120m,
        BudgetAmount = 18000m,
        LoggedHours = 78.5m,
        CreatedAt = now.AddDays(-50),
    },
    new() {
        Name = "Bright Smile HIPAA Security Audit",
        Description = "Annual HIPAA compliance review across all 4 locations",
        Status = ProjectStatus.Active,
        ClientId = clients[1].Id,
        ManagerUserId = jennifer.Id.ToString(),
        StartDate = now.AddDays(-20),
        EndDate = now.AddDays(40),
        BudgetHours = 80m,
        BudgetAmount = 16000m,
        LoggedHours = 22m,
        CreatedAt = now.AddDays(-25),
    },
    new() {
        Name = "Cascade Network Refresh",
        Description = "Replace aging Cisco switches and upgrade firewall. Plant floor and admin office.",
        Status = ProjectStatus.Planning,
        ClientId = clients[3].Id,
        ManagerUserId = marcus.Id.ToString(),
        StartDate = now.AddDays(14),
        EndDate = now.AddDays(60),
        BudgetHours = 200m,
        BudgetAmount = 45000m,
        LoggedHours = 0m,
        CreatedAt = now.AddDays(-5),
    },
    new() {
        Name = "Pineview Website Redesign",
        Description = "New church website with event calendar and online giving integration",
        Status = ProjectStatus.Completed,
        ClientId = clients[4].Id,
        ManagerUserId = david.Id.ToString(),
        StartDate = now.AddDays(-120),
        EndDate = now.AddDays(-30),
        BudgetHours = 60m,
        BudgetAmount = 6000m,
        LoggedHours = 52m,
        CreatedAt = now.AddDays(-130),
    },
    new() {
        Name = "Meridian Tax Season Prep",
        Description = "Pre-season server maintenance, backup verification, and VPN scaling",
        Status = ProjectStatus.Completed,
        ClientId = clients[2].Id,
        ManagerUserId = sarah.Id.ToString(),
        StartDate = now.AddDays(-180),
        EndDate = now.AddDays(-150),
        BudgetHours = 40m,
        BudgetAmount = 6000m,
        LoggedHours = 36m,
        CreatedAt = now.AddDays(-185),
    },
};

db.Set<Project>().AddRange(projects);

// ─── Tickets ────────────────────────────────────────────────────────
var ticketDefs = new (string Title, string Desc, TicketStatus Status, TicketPriority Priority, TicketType Type,
    int ClientIdx, int? ProjectIdx, User? Assigned, TicketQueue? Queue, int DaysAgo, int? ResolvedDaysAgo)[] {
    // Active tickets
    ("Outlook keeps crashing for Rebecca", "Outlook 365 crashes every 15 minutes. Tried safe mode, still crashes. Need to check add-ins and profile.", TicketStatus.InProgress, TicketPriority.High, TicketType.Incident, 0, 0, sarah, null, 2, null),
    ("New hire laptop setup – James Morton", "New paralegal starting Monday. Need laptop with standard law firm image, VPN, and O365.", TicketStatus.Open, TicketPriority.Medium, TicketType.ServiceRequest, 0, null, marcus, null, 4, null),
    ("X-ray machine PC won't boot – Beaverton", "Front desk reports the PC controlling the dental X-ray at Beaverton location shows BSOD on startup.", TicketStatus.InProgress, TicketPriority.Critical, TicketType.Incident, 1, null, jennifer, onsiteQueue, 0, null),
    ("Pano software update required", "Patterson Imaging software v4.2 update required for HIPAA compliance. All 4 locations.", TicketStatus.Open, TicketPriority.Medium, TicketType.Change, 1, 1, jennifer, null, 5, null),
    ("VPN slow during peak hours", "Remote staff reporting VPN disconnects and slowness between 9-11am and 1-3pm.", TicketStatus.PendingCustomer, TicketPriority.Medium, TicketType.Incident, 2, null, david, networkQueue, 7, null),
    ("SQL Server backup failures", "Nightly SQL backup job failing since Tuesday. Error: VSS writer timeout.", TicketStatus.InProgress, TicketPriority.High, TicketType.Incident, 2, null, sarah, null, 3, null),
    ("SCADA HMI screen frozen", "HMI Panel #3 on Line 2 frozen. Operators manually controlling. Need immediate response.", TicketStatus.InProgress, TicketPriority.Critical, TicketType.Incident, 3, null, marcus, securityQueue, 0, null),
    ("Deploy new security cameras", "Install 4 new cameras at loading dock per Frank's request. Cabling already run.", TicketStatus.Open, TicketPriority.Low, TicketType.ServiceRequest, 3, null, null, onsiteQueue, 10, null),
    ("Church Wi-Fi not reaching fellowship hall", "Congregation members can't connect in the fellowship hall. Signal drops past the kitchen.", TicketStatus.New, TicketPriority.Low, TicketType.Incident, 4, null, null, triageQueue, 1, null),
    ("Projector HDMI issue in sanctuary", "HDMI cable from laptop to projector shows no signal. Tried 2 different cables.", TicketStatus.New, TicketPriority.Medium, TicketType.Incident, 4, null, null, triageQueue, 0, null),
    // Resolved tickets
    ("Set up MFA for all Whitfield users", "Enable MFA on all O365 accounts per new firm policy.", TicketStatus.Closed, TicketPriority.High, TicketType.Change, 0, 0, sarah, null, 30, 25),
    ("Replace reception printer", "HP LaserJet at reception jammed permanently. Replace with new unit.", TicketStatus.Resolved, TicketPriority.Low, TicketType.ServiceRequest, 0, null, marcus, null, 20, 18),
    ("HIPAA risk assessment – Downtown", "Completed annual risk assessment for the downtown location.", TicketStatus.Closed, TicketPriority.Medium, TicketType.ServiceRequest, 1, 1, jennifer, null, 15, 10),
    ("QuickBooks update to 2025", "Update QuickBooks Enterprise to 2025 version across all workstations.", TicketStatus.Closed, TicketPriority.Medium, TicketType.Change, 2, 4, david, null, 160, 155),
    ("Firewall firmware update", "Update SonicWall TZ670 to latest firmware. Schedule during maintenance window.", TicketStatus.Closed, TicketPriority.High, TicketType.Change, 3, null, marcus, securityQueue, 45, 42),
    ("PLC network segmentation", "Isolate PLC network from corporate LAN per security audit recommendation.", TicketStatus.Closed, TicketPriority.Critical, TicketType.Change, 3, null, sarah, securityQueue, 60, 50),
    ("Website SSL certificate renewal", "SSL cert expiring in 2 weeks. Renew and install.", TicketStatus.Closed, TicketPriority.Medium, TicketType.ServiceRequest, 4, 3, david, null, 40, 38),
    ("Email migration – Phase 1 mailboxes", "Migrate first batch of 10 mailboxes from on-prem to O365.", TicketStatus.Closed, TicketPriority.High, TicketType.Change, 0, 0, sarah, null, 35, 32),
    ("Dental chair PC replacement", "Replace the aging PC at Chair 3 in Lake Oswego with new mini-PC.", TicketStatus.Resolved, TicketPriority.Medium, TicketType.ServiceRequest, 1, null, marcus, onsiteQueue, 12, 9),
    ("Configure backup for new file server", "Set up Veeam backup job for the new file server at Cascade admin office.", TicketStatus.Closed, TicketPriority.High, TicketType.ServiceRequest, 3, null, david, null, 55, 52),
};

var tickets = ticketDefs.Select(t => {
    var created = now.AddDays(-t.DaysAgo).AddHours(-rng.Next(1, 10));
    var ticket = new Ticket {
        Title = t.Title,
        Description = t.Desc,
        Status = t.Status,
        Priority = t.Priority,
        Type = t.Type,
        ClientId = clients[t.ClientIdx].Id,
        ProjectId = t.ProjectIdx.HasValue ? projects[t.ProjectIdx.Value].Id : null,
        AssignedToUserId = t.Assigned?.Id.ToString(),
        QueueId = t.Queue?.Id,
        ContractId = t.ClientIdx < agreements.Length ? agreements[t.ClientIdx].Id : null,
        CreatedAt = created,
        FirstResponseAt = t.Status != TicketStatus.New ? created.AddMinutes(rng.Next(10, 120)) : null,
        ResolvedAt = t.ResolvedDaysAgo.HasValue ? now.AddDays(-t.ResolvedDaysAgo.Value) : null,
    };
    if (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed)
        ticket.DueDate = created.AddDays(7);
    return ticket;
}).ToArray();

db.Set<Ticket>().AddRange(tickets);

// ─── Time Entries ───────────────────────────────────────────────────
var timeEntries = new List<TimeEntry>();
void AddTime(Guid clientId, Guid? projectId, Guid? ticketId, Guid userId, int daysAgo, decimal hours, string desc, bool billable = true) {
    timeEntries.Add(new TimeEntry {
        ClientId = clientId,
        ProjectId = projectId,
        TicketId = ticketId,
        UserId = userId,
        Date = now.AddDays(-daysAgo),
        Hours = hours,
        Description = desc,
        Billable = billable,
        CreatedAt = now.AddDays(-daysAgo).AddHours(17),
    });
}

// Whitfield O365 project time
AddTime(clients[0].Id, projects[0].Id, tickets[17].Id, sarah.Id, 33, 4m, "Migrated first 5 mailboxes to O365, verified sync");
AddTime(clients[0].Id, projects[0].Id, tickets[17].Id, sarah.Id, 32, 6m, "Migrated remaining 5 mailboxes, configured autodiscover");
AddTime(clients[0].Id, projects[0].Id, tickets[10].Id, sarah.Id, 28, 3m, "Enabled MFA for all 25 users, documented recovery codes");
AddTime(clients[0].Id, projects[0].Id, null, sarah.Id, 20, 5m, "SharePoint Online setup and permissions configuration");
AddTime(clients[0].Id, projects[0].Id, null, sarah.Id, 15, 4m, "OneDrive migration and user training sessions");
AddTime(clients[0].Id, projects[0].Id, null, sarah.Id, 10, 3.5m, "Teams configuration and meeting room setup");
AddTime(clients[0].Id, projects[0].Id, tickets[0].Id, sarah.Id, 1, 2m, "Troubleshooting Outlook crashes, disabled suspect add-ins");

// Whitfield non-project time
AddTime(clients[0].Id, null, tickets[11].Id, marcus.Id, 19, 1.5m, "Replaced reception printer, configured on network");

// Bright Smile HIPAA audit time
AddTime(clients[1].Id, projects[1].Id, tickets[12].Id, jennifer.Id, 14, 6m, "HIPAA risk assessment at downtown location");
AddTime(clients[1].Id, projects[1].Id, null, jennifer.Id, 12, 5m, "HIPAA risk assessment at Beaverton location");
AddTime(clients[1].Id, projects[1].Id, null, jennifer.Id, 8, 5m, "Documented findings and remediation recommendations");
AddTime(clients[1].Id, projects[1].Id, null, jennifer.Id, 5, 6m, "HIPAA risk assessment at Lake Oswego and Tigard");

// Bright Smile ticket time
AddTime(clients[1].Id, null, tickets[2].Id, jennifer.Id, 0, 2m, "Onsite at Beaverton, diagnosed BSOD on X-ray PC, replacing drive");
AddTime(clients[1].Id, null, tickets[18].Id, marcus.Id, 11, 2.5m, "Replaced dental chair PC at Lake Oswego, imaged and configured");

// Meridian time
AddTime(clients[2].Id, projects[4].Id, null, sarah.Id, 175, 4m, "Server maintenance and Windows updates");
AddTime(clients[2].Id, projects[4].Id, null, sarah.Id, 170, 6m, "Verified backup integrity, tested restore procedure");
AddTime(clients[2].Id, projects[4].Id, null, david.Id, 168, 8m, "VPN capacity testing and configuration scaling");
AddTime(clients[2].Id, projects[4].Id, null, sarah.Id, 160, 5m, "Final checks and documentation update");
AddTime(clients[2].Id, projects[4].Id, tickets[13].Id, david.Id, 158, 3m, "QuickBooks 2025 upgrade on 12 workstations");
AddTime(clients[2].Id, null, tickets[5].Id, sarah.Id, 2, 3m, "Investigating SQL backup VSS failures, cleared shadow copies");
AddTime(clients[2].Id, null, tickets[4].Id, david.Id, 6, 2m, "Ran VPN diagnostics, sent bandwidth test instructions to client");

// Cascade time
AddTime(clients[3].Id, null, tickets[14].Id, marcus.Id, 44, 3m, "Firewall firmware update during maintenance window");
AddTime(clients[3].Id, null, tickets[15].Id, sarah.Id, 58, 8m, "Designed PLC network segmentation plan");
AddTime(clients[3].Id, null, tickets[15].Id, sarah.Id, 55, 10m, "Implemented PLC VLAN isolation and firewall rules");
AddTime(clients[3].Id, null, tickets[15].Id, sarah.Id, 52, 4m, "Tested PLC segmentation, verified HMI connectivity");
AddTime(clients[3].Id, null, tickets[19].Id, david.Id, 53, 4m, "Configured Veeam backup for new file server");
AddTime(clients[3].Id, null, tickets[6].Id, marcus.Id, 0, 1.5m, "Emergency response to frozen HMI, performing diagnostics");

// Pineview time
AddTime(clients[4].Id, projects[3].Id, null, david.Id, 115, 8m, "WordPress setup and theme customization");
AddTime(clients[4].Id, projects[3].Id, null, david.Id, 100, 10m, "Event calendar plugin and online giving integration");
AddTime(clients[4].Id, projects[3].Id, null, david.Id, 85, 8m, "Content migration from old site");
AddTime(clients[4].Id, projects[3].Id, null, david.Id, 70, 6m, "User testing and final adjustments");
AddTime(clients[4].Id, projects[3].Id, tickets[16].Id, david.Id, 39, 1m, "SSL certificate renewal and installation");
AddTime(clients[4].Id, null, null, david.Id, 50, 3m, "General maintenance and WordPress updates", false);

db.Set<TimeEntry>().AddRange(timeEntries);

// ─── Invoices ───────────────────────────────────────────────────────
var invoices = new Invoice[] {
    new() {
        InvoiceNumber = "INV-2026-001",
        ClientId = clients[0].Id,
        Status = InvoiceStatus.Paid,
        InvoiceDate = now.AddDays(-60),
        DueDate = now.AddDays(-30),
        TaxRate = 0m,
        AmountPaid = 2500m,
        Notes = "January managed services retainer",
        CreatedAt = now.AddDays(-60),
        LineItems = [
            new InvoiceLineItem { Description = "Monthly Managed Services Retainer – January 2026", Quantity = 1m, UnitPrice = 2500m, CreatedAt = now.AddDays(-60) },
        ],
    },
    new() {
        InvoiceNumber = "INV-2026-002",
        ClientId = clients[0].Id,
        Status = InvoiceStatus.Paid,
        InvoiceDate = now.AddDays(-30),
        DueDate = now.AddDays(0),
        TaxRate = 0m,
        AmountPaid = 2500m,
        Notes = "February managed services retainer",
        CreatedAt = now.AddDays(-30),
        LineItems = [
            new InvoiceLineItem { Description = "Monthly Managed Services Retainer – February 2026", Quantity = 1m, UnitPrice = 2500m, CreatedAt = now.AddDays(-30) },
        ],
    },
    new() {
        InvoiceNumber = "INV-2026-003",
        ClientId = clients[0].Id,
        Status = InvoiceStatus.Sent,
        InvoiceDate = now.AddDays(-5),
        DueDate = now.AddDays(25),
        TaxRate = 0m,
        AmountPaid = 0m,
        Notes = "March managed services retainer + project hours",
        CreatedAt = now.AddDays(-5),
        LineItems = [
            new InvoiceLineItem { Description = "Monthly Managed Services Retainer – March 2026", Quantity = 1m, UnitPrice = 2500m, CreatedAt = now.AddDays(-5) },
            new InvoiceLineItem { Description = "O365 Migration – Project hours (12.5 hrs)", Quantity = 12.5m, UnitPrice = 150m, CreatedAt = now.AddDays(-5) },
        ],
    },
    new() {
        InvoiceNumber = "INV-2026-004",
        ClientId = clients[1].Id,
        Status = InvoiceStatus.Paid,
        InvoiceDate = now.AddDays(-45),
        DueDate = now.AddDays(-15),
        TaxRate = 0m,
        AmountPaid = 3780m,
        Notes = "Block hours usage – January",
        CreatedAt = now.AddDays(-45),
        LineItems = [
            new InvoiceLineItem { Description = "Block Hours – Support (18 hrs @ $140)", Quantity = 18m, UnitPrice = 140m, CreatedAt = now.AddDays(-45) },
            new InvoiceLineItem { Description = "Block Hours – HIPAA Audit (9 hrs @ $140)", Quantity = 9m, UnitPrice = 140m, CreatedAt = now.AddDays(-45) },
        ],
    },
    new() {
        InvoiceNumber = "INV-2026-005",
        ClientId = clients[2].Id,
        Status = InvoiceStatus.Overdue,
        InvoiceDate = now.AddDays(-50),
        DueDate = now.AddDays(-20),
        TaxRate = 0m,
        AmountPaid = 0m,
        Notes = "T&M support – tax season prep",
        CreatedAt = now.AddDays(-50),
        LineItems = [
            new InvoiceLineItem { Description = "Server Maintenance (10 hrs @ $150)", Quantity = 10m, UnitPrice = 150m, CreatedAt = now.AddDays(-50) },
            new InvoiceLineItem { Description = "VPN Configuration (8 hrs @ $175)", Quantity = 8m, UnitPrice = 175m, CreatedAt = now.AddDays(-50) },
            new InvoiceLineItem { Description = "QuickBooks Upgrade (3 hrs @ $150)", Quantity = 3m, UnitPrice = 150m, CreatedAt = now.AddDays(-50) },
        ],
    },
    new() {
        InvoiceNumber = "INV-2026-006",
        ClientId = clients[3].Id,
        Status = InvoiceStatus.Paid,
        InvoiceDate = now.AddDays(-30),
        DueDate = now.AddDays(0),
        TaxRate = 0m,
        AmountPaid = 8500m,
        Notes = "March managed IT monthly",
        CreatedAt = now.AddDays(-30),
        LineItems = [
            new InvoiceLineItem { Description = "Full Managed IT – Monthly Fee – March 2026", Quantity = 1m, UnitPrice = 8500m, CreatedAt = now.AddDays(-30) },
        ],
    },
    new() {
        InvoiceNumber = "INV-2026-007",
        ClientId = clients[4].Id,
        Status = InvoiceStatus.Paid,
        InvoiceDate = now.AddDays(-35),
        DueDate = now.AddDays(-5),
        TaxRate = 0m,
        AmountPaid = 5200m,
        Notes = "Website redesign project – final invoice",
        CreatedAt = now.AddDays(-35),
        LineItems = [
            new InvoiceLineItem { Description = "Website Redesign – Development (32 hrs @ $100)", Quantity = 32m, UnitPrice = 100m, CreatedAt = now.AddDays(-35) },
            new InvoiceLineItem { Description = "Website Redesign – Content Migration (12 hrs @ $100)", Quantity = 12m, UnitPrice = 100m, CreatedAt = now.AddDays(-35) },
            new InvoiceLineItem { Description = "Website Redesign – Testing & Launch (8 hrs @ $100)", Quantity = 8m, UnitPrice = 100m, CreatedAt = now.AddDays(-35) },
        ],
    },
    new() {
        InvoiceNumber = "INV-2026-008",
        ClientId = clients[3].Id,
        Status = InvoiceStatus.Draft,
        InvoiceDate = now,
        DueDate = now.AddDays(30),
        TaxRate = 0m,
        AmountPaid = 0m,
        Notes = "April managed IT monthly + security project hours",
        CreatedAt = now,
        LineItems = [
            new InvoiceLineItem { Description = "Full Managed IT – Monthly Fee – April 2026", Quantity = 1m, UnitPrice = 8500m, CreatedAt = now },
            new InvoiceLineItem { Description = "PLC Network Segmentation (22 hrs @ $185)", Quantity = 22m, UnitPrice = 185m, CreatedAt = now },
        ],
    },
};

db.Set<Invoice>().AddRange(invoices);

// ─── Assets ─────────────────────────────────────────────────────────
var assets = new Asset[] {
    // Whitfield
    new() { Name = "WF-DC01", Type = AssetType.Server, Status = AssetStatus.Deployed, ClientId = clients[0].Id, SiteId = sites[0].Id, Manufacturer = "Dell", Model = "PowerEdge R750", SerialNumber = "DELL-R750-WF001", OperatingSystem = "Windows Server 2022", IpAddress = "10.10.1.10", PurchaseDate = now.AddYears(-2), WarrantyExpiry = now.AddYears(1), PurchasePrice = 8500m, CreatedAt = now.AddDays(-200) },
    new() { Name = "WF-FW01", Type = AssetType.NetworkDevice, Status = AssetStatus.Deployed, ClientId = clients[0].Id, SiteId = sites[0].Id, Manufacturer = "Fortinet", Model = "FortiGate 60F", SerialNumber = "FGT60F-WF001", IpAddress = "10.10.1.1", PurchaseDate = now.AddYears(-1), WarrantyExpiry = now.AddYears(2), PurchasePrice = 1200m, CreatedAt = now.AddDays(-200) },
    new() { Name = "Rebecca's Laptop", Type = AssetType.Laptop, Status = AssetStatus.Deployed, ClientId = clients[0].Id, SiteId = sites[0].Id, Manufacturer = "Lenovo", Model = "ThinkPad X1 Carbon Gen 11", SerialNumber = "LNV-X1C-WF001", OperatingSystem = "Windows 11 Pro", PurchaseDate = now.AddMonths(-8), WarrantyExpiry = now.AddMonths(28), PurchasePrice = 1800m, CreatedAt = now.AddMonths(-8) },

    // Bright Smile
    new() { Name = "BS-SRV01", Type = AssetType.Server, Status = AssetStatus.Deployed, ClientId = clients[1].Id, SiteId = sites[1].Id, Manufacturer = "HP", Model = "ProLiant DL360 Gen10", SerialNumber = "HP-DL360-BS001", OperatingSystem = "Windows Server 2019", IpAddress = "192.168.1.10", PurchaseDate = now.AddYears(-3), WarrantyExpiry = now.AddMonths(-6), PurchasePrice = 6500m, Notes = "Warranty expired — renewal pending", CreatedAt = now.AddDays(-300) },
    new() { Name = "X-Ray PC – Beaverton", Type = AssetType.Workstation, Status = AssetStatus.InRepair, ClientId = clients[1].Id, SiteId = sites[2].Id, Manufacturer = "Dell", Model = "OptiPlex 7090", SerialNumber = "DELL-OPT-BS002", OperatingSystem = "Windows 10 Pro", IpAddress = "192.168.2.50", PurchaseDate = now.AddYears(-2), WarrantyExpiry = now.AddMonths(10), PurchasePrice = 1200m, Notes = "BSOD issue — ticket #3", CreatedAt = now.AddDays(-300) },
    new() { Name = "Chair 3 PC – Lake Oswego", Type = AssetType.Workstation, Status = AssetStatus.Deployed, ClientId = clients[1].Id, SiteId = sites[3].Id, Manufacturer = "Intel", Model = "NUC 12 Pro", SerialNumber = "NUC12-BS003", OperatingSystem = "Windows 11 Pro", IpAddress = "192.168.3.30", PurchaseDate = now.AddDays(-10), WarrantyExpiry = now.AddYears(3), PurchasePrice = 750m, Notes = "Replacement unit installed", CreatedAt = now.AddDays(-10) },

    // Meridian
    new() { Name = "MER-FS01", Type = AssetType.Server, Status = AssetStatus.Deployed, ClientId = clients[2].Id, SiteId = sites[5].Id, Manufacturer = "Dell", Model = "PowerEdge T550", SerialNumber = "DELL-T550-MER01", OperatingSystem = "Windows Server 2022", IpAddress = "10.20.1.10", PurchaseDate = now.AddYears(-1), WarrantyExpiry = now.AddYears(2), PurchasePrice = 5500m, CreatedAt = now.AddDays(-250) },
    new() { Name = "MER-FW01", Type = AssetType.NetworkDevice, Status = AssetStatus.Deployed, ClientId = clients[2].Id, SiteId = sites[5].Id, Manufacturer = "SonicWall", Model = "TZ470", SerialNumber = "SW-TZ470-MER01", IpAddress = "10.20.1.1", PurchaseDate = now.AddYears(-1), WarrantyExpiry = now.AddYears(2), PurchasePrice = 900m, CreatedAt = now.AddDays(-250) },

    // Cascade
    new() { Name = "CAS-DC01", Type = AssetType.Server, Status = AssetStatus.Deployed, ClientId = clients[3].Id, SiteId = sites[6].Id, Manufacturer = "Dell", Model = "PowerEdge R750", SerialNumber = "DELL-R750-CAS01", OperatingSystem = "Windows Server 2022", IpAddress = "10.30.1.10", PurchaseDate = now.AddYears(-1), WarrantyExpiry = now.AddYears(2), PurchasePrice = 9200m, CreatedAt = now.AddDays(-350) },
    new() { Name = "CAS-DC02", Type = AssetType.Server, Status = AssetStatus.Deployed, ClientId = clients[3].Id, SiteId = sites[6].Id, Manufacturer = "Dell", Model = "PowerEdge R750", SerialNumber = "DELL-R750-CAS02", OperatingSystem = "Windows Server 2022", IpAddress = "10.30.1.11", PurchaseDate = now.AddYears(-1), WarrantyExpiry = now.AddYears(2), PurchasePrice = 9200m, CreatedAt = now.AddDays(-350) },
    new() { Name = "CAS-FW01", Type = AssetType.NetworkDevice, Status = AssetStatus.Deployed, ClientId = clients[3].Id, SiteId = sites[6].Id, Manufacturer = "SonicWall", Model = "TZ670", SerialNumber = "SW-TZ670-CAS01", IpAddress = "10.30.1.1", PurchaseDate = now.AddMonths(-18), WarrantyExpiry = now.AddMonths(18), PurchasePrice = 2800m, CreatedAt = now.AddDays(-350) },
    new() { Name = "CAS-SW01", Type = AssetType.NetworkDevice, Status = AssetStatus.Deployed, ClientId = clients[3].Id, SiteId = sites[6].Id, Manufacturer = "Cisco", Model = "Catalyst 9200L-48", SerialNumber = "CISCO-C9200-CAS01", IpAddress = "10.30.1.2", PurchaseDate = now.AddYears(-4), WarrantyExpiry = now.AddMonths(-12), PurchasePrice = 3500m, Notes = "Scheduled for replacement in network refresh project", CreatedAt = now.AddDays(-350) },
    new() { Name = "CAS-SW02", Type = AssetType.NetworkDevice, Status = AssetStatus.Deployed, ClientId = clients[3].Id, SiteId = sites[6].Id, Manufacturer = "Cisco", Model = "Catalyst 9200L-24", SerialNumber = "CISCO-C9200-CAS02", IpAddress = "10.30.1.3", PurchaseDate = now.AddYears(-4), WarrantyExpiry = now.AddMonths(-12), PurchasePrice = 2800m, Notes = "Scheduled for replacement in network refresh project", CreatedAt = now.AddDays(-350) },
    new() { Name = "HMI Panel #3 – Line 2", Type = AssetType.Workstation, Status = AssetStatus.InRepair, ClientId = clients[3].Id, SiteId = sites[6].Id, Manufacturer = "Siemens", Model = "SIMATIC HMI TP1500", SerialNumber = "SIE-HMI-CAS03", IpAddress = "10.30.50.103", PurchaseDate = now.AddYears(-2), WarrantyExpiry = now.AddMonths(10), PurchasePrice = 4200m, Notes = "Frozen — active incident ticket", CreatedAt = now.AddDays(-300) },

    // Pineview
    new() { Name = "PV-AP01", Type = AssetType.NetworkDevice, Status = AssetStatus.Deployed, ClientId = clients[4].Id, SiteId = sites[8].Id, Manufacturer = "Ubiquiti", Model = "UniFi U6 Pro", SerialNumber = "UI-U6P-PV01", IpAddress = "10.40.1.20", PurchaseDate = now.AddYears(-1), WarrantyExpiry = now.AddYears(1), PurchasePrice = 150m, Notes = "Covers main sanctuary only", CreatedAt = now.AddDays(-100) },
    new() { Name = "Projector – Sanctuary", Type = AssetType.Peripheral, Status = AssetStatus.Deployed, ClientId = clients[4].Id, SiteId = sites[8].Id, Manufacturer = "Epson", Model = "Pro EX9240", SerialNumber = "EPS-EX9-PV01", PurchaseDate = now.AddYears(-2), WarrantyExpiry = now.AddMonths(-3), PurchasePrice = 700m, Notes = "HDMI issue reported", CreatedAt = now.AddDays(-100) },
};

db.Set<Asset>().AddRange(assets);

// ─── Expenses ───────────────────────────────────────────────────────
var expenses = new Expense[] {
    new() { Description = "Replacement SSD for X-Ray PC – Beaverton", Category = ExpenseCategory.Hardware, Status = ExpenseStatus.Approved, Amount = 89.99m, ExpenseDate = now, Billable = true, ClientId = clients[1].Id, TicketId = tickets[2].Id, UserId = jennifer.Id.ToString(), CreatedAt = now },
    new() { Description = "Intel NUC 12 Pro for Chair 3 – Lake Oswego", Category = ExpenseCategory.Hardware, Status = ExpenseStatus.Invoiced, Amount = 750m, ExpenseDate = now.AddDays(-12), Billable = true, ClientId = clients[1].Id, TicketId = tickets[18].Id, UserId = marcus.Id.ToString(), CreatedAt = now.AddDays(-12) },
    new() { Description = "HP LaserJet M404n – Whitfield reception", Category = ExpenseCategory.Hardware, Status = ExpenseStatus.Invoiced, Amount = 349m, ExpenseDate = now.AddDays(-19), Billable = true, ClientId = clients[0].Id, TicketId = tickets[11].Id, UserId = marcus.Id.ToString(), CreatedAt = now.AddDays(-19) },
    new() { Description = "Mileage – Onsite visit Cascade plant", Category = ExpenseCategory.Travel, Status = ExpenseStatus.Approved, Amount = 45.50m, ExpenseDate = now, Billable = false, ClientId = clients[3].Id, TicketId = tickets[6].Id, UserId = marcus.Id.ToString(), CreatedAt = now },
    new() { Description = "Veeam Backup license – 1 socket", Category = ExpenseCategory.Licensing, Status = ExpenseStatus.Approved, Amount = 540m, ExpenseDate = now.AddDays(-54), Billable = true, ClientId = clients[3].Id, TicketId = tickets[19].Id, UserId = david.Id.ToString(), CreatedAt = now.AddDays(-54) },
    new() { Description = "Security camera cable and connectors", Category = ExpenseCategory.Hardware, Status = ExpenseStatus.Submitted, Amount = 127.45m, ExpenseDate = now.AddDays(-3), Billable = true, ClientId = clients[3].Id, TicketId = tickets[7].Id, UserId = marcus.Id.ToString(), CreatedAt = now.AddDays(-3) },
    new() { Description = "WordPress theme license – Pineview", Category = ExpenseCategory.Software, Status = ExpenseStatus.Invoiced, Amount = 59m, ExpenseDate = now.AddDays(-110), Billable = true, ClientId = clients[4].Id, ProjectId = projects[3].Id, UserId = david.Id.ToString(), CreatedAt = now.AddDays(-110) },
    new() { Description = "Team lunch – quarterly planning", Category = ExpenseCategory.Meals, Status = ExpenseStatus.Approved, Amount = 142.30m, ExpenseDate = now.AddDays(-14), Billable = false, UserId = sarah.Id.ToString(), CreatedAt = now.AddDays(-14) },
};

db.Set<Expense>().AddRange(expenses);

// ─── Notes ──────────────────────────────────────────────────────────
var notes = new Note[] {
    new() { EntityType = "Ticket", EntityId = tickets[0].Id, Content = "Disabled Grammarly and Adobe Acrobat add-ins. Monitoring to see if crashes continue. Rebecca will report back EOD.", UserId = sarah.Id, CreatedAt = now.AddDays(-1) },
    new() { EntityType = "Ticket", EntityId = tickets[2].Id, Content = "Arrived onsite. BSOD shows CRITICAL_PROCESS_DIED. Running SSD diagnostics — SMART shows reallocated sectors. Ordering replacement drive.", UserId = jennifer.Id, CreatedAt = now.AddHours(-3) },
    new() { EntityType = "Ticket", EntityId = tickets[4].Id, Content = "Sent client instructions for running bandwidth test from VPN. Waiting for results before making firewall changes.", UserId = david.Id, CreatedAt = now.AddDays(-5) },
    new() { EntityType = "Ticket", EntityId = tickets[5].Id, Content = "VSS writer was in a failed state. Ran 'vssadmin delete shadows /all' and restarted Volume Shadow Copy service. Will verify tonight's backup tomorrow.", UserId = sarah.Id, CreatedAt = now.AddDays(-1) },
    new() { EntityType = "Ticket", EntityId = tickets[6].Id, Content = "HMI panel not responding to touch or keyboard. Power cycled — boots to Siemens runtime but freezes after 30s. Suspect firmware or memory issue. Contacting Siemens support.", UserId = marcus.Id, CreatedAt = now.AddHours(-1) },
    new() { EntityType = "Ticket", EntityId = tickets[15].Id, Content = "Segmentation complete. PLC VLAN 50 isolated from corporate VLAN 10. Firewall rules allow only HMI stations and SCADA server. Operators confirmed all 4 lines running normally.", UserId = sarah.Id, CreatedAt = now.AddDays(-51) },
    new() { EntityType = "Client", EntityId = clients[0].Id, Content = "Whitfield is expanding — hiring 5 new associates in Q2. Will need additional O365 licenses and laptop procurement.", UserId = sarah.Id, CreatedAt = now.AddDays(-10) },
    new() { EntityType = "Client", EntityId = clients[3].Id, Content = "Frank mentioned they're considering a second shift starting in September. Will need to discuss 24/7 monitoring expansion.", UserId = marcus.Id, CreatedAt = now.AddDays(-7) },
    new() { EntityType = "Project", EntityId = projects[0].Id, Content = "Migration going smoothly. 20 of 25 mailboxes complete. Last 5 are partners with large mailboxes (50GB+), scheduling for this weekend.", UserId = sarah.Id, CreatedAt = now.AddDays(-8) },
    new() { EntityType = "Project", EntityId = projects[2].Id, Content = "Cisco quotes received. C9300L-48 x2 and C9300L-24 x2 plus FortiGate 100F. Total hardware: ~$28K. Scheduling kickoff meeting with Frank.", UserId = marcus.Id, CreatedAt = now.AddDays(-3) },
};

db.Set<Note>().AddRange(notes);

// ─── Save Everything ────────────────────────────────────────────────
await db.SaveChangesAsync();

var ticketCount = tickets.Length;
var clientCount = clients.Length;
Console.WriteLine($"Seeded: {clientCount} clients, {contacts.Length} contacts, {sites.Length} sites, {ticketCount} tickets");
Console.WriteLine($"        {projects.Length} projects, {timeEntries.Count} time entries, {invoices.Length} invoices");
Console.WriteLine($"        {assets.Length} assets, {expenses.Length} expenses, {notes.Length} notes");
Console.WriteLine($"        {agreements.Length} agreements, 2 SLA policies, 4 queues, 2 rate cards");
Console.WriteLine($"        5 users (admin/admin, sarah/password, marcus/password, jennifer/password, david/password)");
Console.WriteLine("Done.");
return 0;

internal sealed class NullPiiEncryptionService : IPiiEncryptionService {
    public string Encrypt(string plaintext) => plaintext;
    public string Decrypt(string ciphertext) => ciphertext;
}
