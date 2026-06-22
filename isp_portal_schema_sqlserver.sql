-- ISP customer portal db

CREATE DATABASE isp_portal;
GO

USE isp_portal;
GO

-- customers + staff in one table, role decides what they can do
CREATE TABLE Users (
    UserID      INT IDENTITY(1,1) PRIMARY KEY,
    FullName    NVARCHAR(100) NOT NULL,
    Email       NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role        NVARCHAR(20) NOT NULL DEFAULT 'Customer'
                CHECK (Role IN ('Customer', 'Staff', 'Admin')),
    Phone       NVARCHAR(20),
    Address     NVARCHAR(255),
    CreatedAt   DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- internet plans, kept separate so price/speed isn't duplicated on every subscription row
CREATE TABLE Plans (
    PlanID       INT IDENTITY(1,1) PRIMARY KEY,
    PlanName     NVARCHAR(100) NOT NULL,
    SpeedMbps    INT NOT NULL,
    MonthlyPrice DECIMAL(10,2) NOT NULL,
    Description  NVARCHAR(255)
);
GO

CREATE TABLE Network_Nodes (
    NodeID    INT IDENTITY(1,1) PRIMARY KEY,
    NodeName  NVARCHAR(100) NOT NULL,
    Region    NVARCHAR(100) NOT NULL,
    Capacity  INT NOT NULL,
    Status    NVARCHAR(20) NOT NULL DEFAULT 'Active'
              CHECK (Status IN ('Active', 'Maintenance', 'Offline'))
);
GO

CREATE TABLE Departments (
    DepartmentID   INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL UNIQUE,
    Description    NVARCHAR(255)
);
GO

CREATE TABLE Subscriptions (
    SubscriptionID INT IDENTITY(1,1) PRIMARY KEY,
    UserID         INT NOT NULL,
    PlanID         INT NOT NULL,
    NodeID         INT NOT NULL,
    StartDate      DATE NOT NULL,
    Status         NVARCHAR(20) NOT NULL DEFAULT 'Active'
                   CHECK (Status IN ('Active', 'Suspended', 'Cancelled')),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (PlanID) REFERENCES Plans(PlanID),
    FOREIGN KEY (NodeID) REFERENCES Network_Nodes(NodeID)
);
GO


CREATE TABLE Support_Tickets (
    TicketID       INT IDENTITY(1,1) PRIMARY KEY,
    UserID         INT NOT NULL,
    SubscriptionID INT NULL,
    DepartmentID   INT NULL,
    Subject        NVARCHAR(150) NOT NULL,
    Description    NVARCHAR(MAX) NOT NULL,
    Priority       NVARCHAR(10) NOT NULL DEFAULT 'Medium'
                   CHECK (Priority IN ('Low', 'Medium', 'High')),
    Status         NVARCHAR(20) NOT NULL DEFAULT 'Open'
                   CHECK (Status IN ('Open', 'In_Progress', 'Resolved', 'Closed')),
    CreatedAt      DATETIME2 NOT NULL DEFAULT GETDATE(),
    ResolvedAt     DATETIME2 NULL,
    FOREIGN KEY (UserID)         REFERENCES Users(UserID),
    FOREIGN KEY (SubscriptionID) REFERENCES Subscriptions(SubscriptionID) ON DELETE SET NULL,
    FOREIGN KEY (DepartmentID)   REFERENCES Departments(DepartmentID)
);
GO

CREATE TABLE Ticket_Comments (
    CommentID INT IDENTITY(1,1) PRIMARY KEY,
    TicketID  INT NOT NULL,
    UserID    INT NOT NULL,
    Message   NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (TicketID) REFERENCES Support_Tickets(TicketID) ON DELETE CASCADE,
    FOREIGN KEY (UserID)   REFERENCES Users(UserID)
);
GO

CREATE INDEX idx_ticket_status     ON Support_Tickets(Status);
CREATE INDEX idx_ticket_department ON Support_Tickets(DepartmentID);
CREATE INDEX idx_ticket_user       ON Support_Tickets(UserID);
CREATE INDEX idx_subscription_user ON Subscriptions(UserID);
GO

INSERT INTO Departments (DepartmentName, Description) VALUES
('Network_Tier_1',     'connectivity stuff - speed, buffering, ping, outages'),
('Billing_Department', 'billing and payment questions'),
('General_Queue',      'anything that does not match a keyword');

INSERT INTO Plans (PlanName, SpeedMbps, MonthlyPrice, Description) VALUES
('Starter',   25,  19.99, 'basic home plan'),
('Home Plus', 100, 39.99, 'good for streaming/gaming'),
('Pro Fiber', 500, 69.99, 'fiber, heavy usage');

INSERT INTO Network_Nodes (NodeName, Region, Capacity, Status) VALUES
('Node-YGN-01', 'Yangon Central', 500, 'Active'),
('Node-YGN-02', 'Yangon North',   350, 'Active'),
('Node-MDY-01', 'Mandalay',       400, 'Active');
GO
