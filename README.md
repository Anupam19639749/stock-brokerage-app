# Apex Stocks - Full-Stack Trading Platform

A comprehensive, full-stack web application that simulates a real-world stock brokerage platform. Built with a modern tech stack, it features a robust .NET backend, a responsive React frontend, and real-time market data integration.

The application features a unique **"Admin-as-Broker"** architecture where high-risk actions (KYC verification and Trade execution) require manual approval, creating a high-fidelity simulation of real-world brokerage operations.

---

##  Key Features

### 1. Real-Time Market Data
* **Live Quotes:** Uses the **Finnhub API** to fetch live stock prices for major US stocks (e.g., AAPL, MSFT, TSLA).
* **Market Overview:** The home page displays live-at-load indices (S&P 500, Dow Jones) and a grid of popular stocks.
* **Interactive Charts:** Integrated **TradingView** widgets provide professional-grade charts with 1D, 1W, 1M, and 1Y historical views.

### 2. Live Portfolio & P&L
* **Real-Time Updates:** The user's portfolio updates instantly without page refreshes.
* **SignalR Integration:** A backend worker broadcasts live price updates to all connected clients every 30 seconds.
* **Dynamic P&L:** The "Total Profit/Loss" and individual stock performance flash **green** (profit) or **red** (loss) in real-time as the market moves.

### 3. Admin-Controlled Ecosystem
* **KYC Verification:** New users must submit PAN and Bank details. These requests enter a "Pending" queue for Admin approval.
* **Trade Execution:** Buy and Sell orders are not executed immediately. They enter a "Pending" order book that simulates a real exchange. An Admin reviews and approves/rejects them.
* **User Management:** Admins can view all users, block suspicious accounts, and view detailed user profiles (including their portfolio and wallet history).

### 4. Complete Wallet System
* **Mock Payment Gateway:** A realistic "Add Money" modal that simulates a secure bank transaction.
* **Security:** Transactions require the user to re-enter their login password for confirmation.
* **Transaction History:** A full ledger of all deposits, trades (debits/credits), and adjustments.

### 5. Automated Notifications
The system sends professional HTML email notifications for critical events:
* **Security:** "Forgot Password" flow with a secure 6-digit OTP.
* **KYC Status:** Emails sent on Approval or Rejection.
* **Trade Confirmation:** Instant email when a Buy/Sell order is executed.
* **Price Alerts:** Users can set custom price targets (e.g., "AAPL > $200") and receive instant alerts when triggered.

---

## 🛠️ Tech Stack

### **Backend**
* **Framework:** ASP.NET Core Web API (.NET 8)
* **Architecture:** Layered Monolithic (Controllers, Services, Repositories)
* **Database:** SQL Server with Entity Framework Core
* **Real-Time:** SignalR
* **Authentication:** JWT (JSON Web Tokens) with HttpOnly Cookies
* **Background Tasks:** IHostedService (Worker Services)
* **Email:** MailKit (SMTP)
* **External API:** Finnhub (Stock Data)
* **Testing:** xUnit, Moq

### **Frontend**
* **Framework:** React + TypeScript + Vite
* **State Management:** Redux Toolkit
* **UI Library:** Material-UI (MUI) with custom Dark Theme
* **Charts:** TradingView Widgets (`react-ts-tradingview-widgets`)
* **HTTP Client:** Axios (with Interceptors)
* **Routing:** React Router DOM (with Protected Routes)

---

## ⚙️ Setup & Installation

### Prerequisites
* Node.js & npm
* .NET 8 SDK
* SQL Server
* Visual Studio or VS Code

### 1. Backend Setup
1.  Navigate to the `StockAlertTracker.API` folder.
2.  Open `appsettings.json` and configure your SQL Server connection string.
3.  **User Secrets:** Configure your sensitive keys (Finnhub API Key, Email Password, JWT Secret) using `dotnet user-secrets`.
4.  Run migrations to create the database:
    ```bash
    dotnet ef database update
    ```
5.  Start the API:
    ```bash
    dotnet run
    ```
   *(The API will run on `https://localhost:7290`)*

### 2. Frontend Setup
1.  Navigate to the `stock-tracker-ui` folder.
2.  Install dependencies:
    ```bash
    npm install
    ```
3.  Create a `.env.local` file in the root:
    ```env
    VITE_API_BASE_URL=https://localhost:7290/api
    ```
4.  Start the React app:
    ```bash
    npm run dev
    ```
   *(The app will run on `http://localhost:5173`)*

---

## 📖 Usage Guide

1.  **Register:** Create a new account.
2.  **KYC:** Submit your verification details (PAN, Bank).
3.  **Admin Login:** Log in as `admin@admin.com` (Password: `Admin@123`). Go to the Dashboard and **Approve** the KYC request.
4.  **Add Money:** Log back in as the User. Go to "Wallet" and add mock funds (password confirmation required).
5.  **Trade:** Search for a stock (e.g., "AAPL"), view the chart, and place a "Buy" order.
6.  **Approve Trade:** As Admin, approve the pending order.
7.  **Monitor:** As a User, watch your Portfolio update live!

---

## 👤 Author

**Anupam Agrawal**
* **Role:** Full-Stack Developer
* **Project Type:** Module Assessment Project
