# Hearth — ISP Customer Portal & Ticket Management System
 
A full-stack web application that lets broadband customers manage their subscriptions and raise support tickets — while automatically routing those tickets to the right internal team the moment they're submitted.
 
Built as a portfolio project targeting ISP internships, but designed to reflect how these systems actually work in production: normalized relational data, clean separation between business logic and data access, and a UI that takes user psychology into account (people filing support tickets are usually already frustrated — the interface shouldn't make that worse).

## The problem it solves
 
Most ISP support portals dump every incoming ticket into a single queue. A billing complaint sits next to a network outage report. Staff have to read each one before they can route it, which adds latency and means urgent issues don't always surface first.
 
Hearth's ticket router eliminates that step. When a customer submits a ticket, a keyword-scoring algorithm scans the subject and description, assigns a department (`Network_Tier_1`, `Billing_Department`, or `General_Queue`), and flags high-priority issues automatically — before any staff member has looked at it. The result is a sorted, prioritized queue that staff can act on immediately instead of triaging first.

## Architecture
ISP_Portal_Frontend/        vanilla HTML/CSS/JS, no framework
├── index.html              login
├── register.html
├── dashboard.html          customer home — plan + ticket history
├── submit-ticket.html      ticket form + post-submit routing reveal
├── ticket-detail.html      comment thread + staff status controls
├── staff-queue.html        kanban triage view (Staff/Admin only)
├── css/style.css           shared design system
└── js/
    ├── api.js              fetch wrapper — attaches JWT, handles 401 redirect
    └── ui.js               shared tag rendering, date formatting, nav init
 
ISP_Portal_API/             ASP.NET Core Web API (.NET 8)
├── Controllers/
│   ├── AuthController      register, login
│   ├── SubscriptionsController
│   └── TicketsController   submit, list, get by id, queue, comments, status update
├── Services/
│   ├── TicketRouter        keyword-scoring engine — pure logic, no DB access
│   ├── TicketManager       orchestrates routing + persistence
│   └── TokenService        JWT generation
├── Models/
│   ├── Entities            User, Plan, NetworkNode, Department,
│   │                       Subscription, SupportTicket, TicketComment
│   └── Dtos                request/response shapes kept separate from entities
└── Data/
    └── AppDbContext         EF Core — maps C# models onto the existing MySQL schema
     
The router and manager are deliberately separate classes. `TicketRouter` only reads text and returns a routing decision — it has no database dependency and can be unit tested in isolation. `TicketManager` takes that decision and handles persistence. Neither leaks into the `User` or `Ticket` entity.

Tech stack

LayerTechnologyFrontendHTML5, CSS3, JavaScript (vanilla — no framework)BackendASP.NET Core Web API (.NET 8), C#ORMEntity Framework Core 8 with Pomelo MySQL providerDatabaseMySQL 8AuthJWT (JSON Web Tokens) via Microsoft.AspNetCore.Authentication.JwtBearerPassword hashingBCrypt.Net-NextAPI testingPostman collection included

## Database schema 
Seven tables, normalized to 3NF.
 
- `Plans` holds pricing and speed. `Subscriptions` references it by FK — price is never duplicated per subscription row.
- `Departments` is a lookup table. The router writes a `DepartmentID`, not a repeated free-text string on every ticket.
- `Support_Tickets.SubscriptionID` is nullable — a customer can file a general inquiry that isn't tied to a specific subscription.
- `Support_Tickets.DepartmentID` starts null and gets assigned by the router on submission.
- `Ticket_Comments` cascade-deletes when the parent ticket is deleted; subscription FK on tickets sets null on delete.
<img width="1918" height="862" alt="image" src="https://github.com/user-attachments/assets/02730edc-a831-42fe-9cbf-bb0d5533dafb" />

## Screenshots
<img width="1918" height="965" alt="Screenshot 2026-06-22 121516" src="https://github.com/user-attachments/assets/945fe746-2aca-4a47-bfc5-5178610c4251" />
<img width="1918" height="962" alt="image" src="https://github.com/user-attachments/assets/a10218bc-6cbd-4fd2-9adf-5744b697e318" />
<img width="1918" height="962" alt="image" src="https://github.com/user-attachments/assets/c60e968d-0f63-487d-bf89-0d04f6e8028c" />
<img width="1918" height="968" alt="image" src="https://github.com/user-attachments/assets/56b2df4c-960f-4954-b992-428206e4db6e" />
