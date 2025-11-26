import { Routes, Route, useNavigate, useLocation } from "react-router-dom";
import { ToastContainer } from "react-toastify";
import { Box, CircularProgress, Container } from "@mui/material";
import { useEffect, useRef } from "react";
import { useAppDispatch, useAppSelector } from "./hooks/redux-hooks"; 
import { checkAuthStatus } from "./features/auth/authSlice";
import { fetchWalletBalance } from "./features/wallet/walletThunks";

import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import ForgotPasswordPage from "./pages/ForgotPasswordPage"; 
import ResetPasswordPage from "./pages/ResetPasswordPage";
import HomePage from "./pages/HomePage";
import ProtectedRoute from "./components/layout/ProtectedRoute";
import AdminRoute from "./components/layout/AdminRoute";
import AdminDashboard from "./pages/AdminDashboard";
import Navbar from "./components/layout/Navbar";
import KycPage from "./pages/KycPage";
import WalletPage from "./pages/WalletPage";
import ProfilePage from "./pages/ProfilePage";
import PortfolioPage from "./pages/PortfolioPage"; 
import OrderHistoryPage from "./pages/OrderHistoryPage";
import KycRequestsPage from "./pages/admin/KycRequestsPage"; 
import PendingOrdersPage from "./pages/admin/PendingOrdersPage";
import AlertsPage from "./pages/AlertsPage";
import signalRService from "./services/signalRService";
import UserManagementPage from "./pages/admin/UserManagementPage"; 
import AnalyticsPage from "./pages/admin/AnalyticsPage";
import AdminUserDetailsPage from "./pages/admin/AdminUserDetailsPage";


function App() {
  const dispatch = useAppDispatch();
  const { status, isAuthenticated, user } = useAppSelector((state) => state.auth);
  const navigate = useNavigate();
  const location = useLocation();
  const signalRStarted = useRef(false);

  // Define auth pages where Navbar should NOT be shown
  const authPages = ['/login', '/register', '/forgot-password', '/reset-password'];
  const showNavbar = !authPages.includes(location.pathname);

  useEffect(() => {
    if (status === "idle") {
      dispatch(checkAuthStatus());
    }

    if (isAuthenticated && status === "succeeded" && user) {
      
      if (!signalRStarted.current) {
        signalRService.startConnection();
        signalRStarted.current = true;
      }
      
      if (user.role === "User") {
        dispatch(fetchWalletBalance());
      }

      if (user.role === "Admin" && window.location.pathname === "/") {
        navigate("/admin", { replace: true });
      }
    }

    if (!isAuthenticated && status === "succeeded" && signalRStarted.current) {
      signalRService.stopConnection();
      signalRStarted.current = false;
    }
    
  }, [status, isAuthenticated, user, dispatch, navigate]);

  if (status === "pending" || status === "idle") {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight="100vh"
      >
        <CircularProgress />
      </Box>
    );
  }

  return (
    <>
      {showNavbar && <Navbar />}
      {showNavbar ? (
        <Container maxWidth="lg" sx={{ mt: 4 }}>
          <Routes>
            {/* Public Routes */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/forgot-password" element={<ForgotPasswordPage />} />
            <Route path="/reset-password" element={<ResetPasswordPage />} />

            {/* User Protected Routes */}
            <Route element={<ProtectedRoute />}>
              <Route path="/" element={<HomePage />} />
              <Route path="/kyc" element={<KycPage />} />
              <Route path="/wallet" element={<WalletPage />} />
              <Route path="/profile" element={<ProfilePage />} />
              <Route path="/portfolio" element={<PortfolioPage />} />
              <Route path="/portfolio/orders" element={<OrderHistoryPage />} />
              <Route path="/alerts" element={<AlertsPage />} />
            </Route>

            {/* Admin Protected Routes */}
            <Route element={<AdminRoute />}>
              <Route path="/admin" element={<AdminDashboard />}>
                <Route index element={<AnalyticsPage />} /> 
                <Route path="analytics" element={<AnalyticsPage />} />
                <Route path="kyc-requests" element={<KycRequestsPage />} />
                <Route path="pending-orders" element={<PendingOrdersPage />} />
                <Route path="user-management" element={<UserManagementPage />} />
                <Route path="user-management/:userId" element={<AdminUserDetailsPage />} />
              </Route>
            </Route>
          </Routes>
        </Container>
      ) : (
        <Routes>
          {/* Auth routes without navbar/container */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />
        </Routes>
      )}
      <ToastContainer theme="dark" />
    </>
  );
}

export default App;