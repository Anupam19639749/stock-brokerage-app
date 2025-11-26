import { AppBar, Toolbar, Typography, Button, Box, IconButton, Menu, MenuItem, Avatar, Badge, ListItemIcon, ListItemText, useMediaQuery, useTheme } from "@mui/material";
import { Link, useNavigate } from "react-router-dom";
import { useAppSelector, useAppDispatch } from "../../hooks/redux-hooks";
import { setLogout } from "../../features/auth/authSlice";
import { clearWallet } from "../../features/wallet/walletSlice";
import http from "../../api/axiosInstance";
import { toast } from "react-toastify";
import React from "react";
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import NotificationsIcon from '@mui/icons-material/Notifications';
import PersonIcon from '@mui/icons-material/Person';
import HistoryIcon from '@mui/icons-material/History';
import VerifiedUserIcon from '@mui/icons-material/VerifiedUser';
import LogoutIcon from '@mui/icons-material/Logout';
import { clearPortfolio } from "../../features/portfolio/portfolioSlice";
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
  
const Navbar = () => {
  const { isAuthenticated, user } = useAppSelector((state) => state.auth);
  const { balance } = useAppSelector((state) => state.wallet);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const open = Boolean(anchorEl);

  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = async () => {
    try {
      await http.post("/auth/logout");
      dispatch(setLogout());
      dispatch(clearWallet());
      dispatch(clearPortfolio());
      toast.success("Logged out successfully.");
      navigate("/login");
    } catch (error) {
      toast.error("Logout failed.");
    }
    handleClose();
  };

  const getKycChipColor = () => {
    switch (user?.kycStatus) {
      case "Approved":
        return "success";
      case "Pending":
        return "warning";
      case "Rejected":
        return "error";
      default:
        return "default";
    }
  };

  return (
    <AppBar 
      position="static" 
      elevation={0}
      sx={{ 
        mb: 4,
        backgroundColor: 'rgba(18, 18, 18, 0.95)',
        backdropFilter: 'blur(20px)',
        borderBottom: '1px solid rgba(63, 81, 181, 0.15)',
      }}
    >
      <Toolbar sx={{ py: 1 }}>
        {/* Logo */}
        <Box sx={{ display: 'flex', alignItems: 'center', position: 'relative', zIndex: 2 }}>
          <TrendingUpIcon sx={{ mr: 1, color: '#5c6bc0', fontSize: 28 }} />
          <Typography 
            variant="h6" 
            component={Link} 
            to={user?.role === "Admin" ? "/admin" : "/"}
            sx={{ 
              color: 'inherit', 
              textDecoration: 'none',
              fontWeight: 700,
              letterSpacing: '0.5px',
              fontSize: { xs: '1.1rem', sm: '1.25rem' }, // Smaller font on mobile
              transition: 'color 0.3s ease',
              '&:hover': {
                color: '#5c6bc0',
              }
            }}
          >
            {isMobile ? 'Apex' : 'Apex Stocks'}
          </Typography>
        </Box>

        {/* Portfolio Button - Only show on desktop, next to logo */}
        {isAuthenticated && user?.role === "User" && !isMobile && (
          <Box sx={{ ml: 2 }}>
            <Button 
              component={Link} 
              to="/portfolio" 
              sx={{ 
                color: 'rgba(255,255,255,0.9)',
                textTransform: 'none',
                fontSize: '0.95rem',
                fontWeight: 500,
                px: 2,
                transition: 'all 0.3s ease',
                '&:hover': {
                  color: '#5c6bc0',
                  backgroundColor: 'rgba(92, 107, 192, 0.08)',
                }
              }}
            >
              Portfolio
            </Button>
          </Box>
        )}

        <Box sx={{ flexGrow: 1 }} />

        {/* Portfolio Button - Mobile only, truly centered */}
        {isAuthenticated && user?.role === "User" && isMobile && (
          <Typography
            component={Link}
            to="/portfolio"
            sx={{
              position: 'absolute',
              left: '50%',
              transform: 'translateX(-50%)',
              color: 'rgba(255,255,255,0.9)',
              textDecoration: 'none',
              fontSize: '0.95rem',
              fontWeight: 600,
              whiteSpace: 'nowrap',
              transition: 'color 0.3s ease',
              '&:hover': {
                color: '#5c6bc0',
              }
            }}
          >
            Portfolio
          </Typography>
        )}

        <Box sx={{ flexGrow: 1 }} />

        {isAuthenticated && user ? (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            
            {/* Wallet - User Only, Hidden on mobile */}
            {user.role === "User" && !isMobile && (
              <Button
                component={Link}
                to="/wallet"
                sx={{
                  color: 'white',
                  backgroundColor: 'rgba(92, 107, 192, 0.15)',
                  px: 2.5,
                  py: 0.75,
                  borderRadius: 2,
                  fontWeight: 600,
                  fontSize: '0.9rem',
                  textTransform: 'none',
                  border: '1px solid rgba(92, 107, 192, 0.3)',
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    backgroundColor: 'rgba(92, 107, 192, 0.25)',
                    transform: 'translateY(-1px)',
                  }
                }}
              >
                ${balance?.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) ?? '0.00'}
              </Button>
            )}

            {/* Notifications - User Only */}
            {user.role === "User" && (
              <IconButton 
                component={Link}
                to="/alerts"
                sx={{ 
                  color: 'rgba(255,255,255,0.8)',
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    color: '#5c6bc0',
                    backgroundColor: 'rgba(92, 107, 192, 0.08)',
                  }
                }}
              >
                <NotificationsIcon />
              </IconButton>
            )}

            {/* User Avatar */}
            <IconButton 
              onClick={handleMenu} 
              sx={{ 
                p: 0,
                transition: 'transform 0.3s ease',
                '&:hover': {
                  transform: 'scale(1.05)',
                }
              }}
            >
              <Avatar 
                alt={user.firstName} 
                src={`https://localhost:7290/api/users/my-image?${new Date().getTime()}`}
                sx={{
                  border: '2px solid rgba(92, 107, 192, 0.3)',
                }}
              />
            </IconButton>
            
            {/* Dropdown Menu */}
            <Menu
              anchorEl={anchorEl}
              open={open}
              onClose={handleClose}
              PaperProps={{
                sx: {
                  mt: 1.5,
                  backgroundColor: 'rgba(30, 30, 30, 0.98)',
                  backdropFilter: 'blur(20px)',
                  border: '1px solid rgba(92, 107, 192, 0.2)',
                  minWidth: 200,
                }
              }}
            >
              <MenuItem 
                onClick={() => { navigate('/profile'); handleClose(); }}
                sx={{
                  py: 1.5,
                  transition: 'all 0.2s ease',
                  '&:hover': {
                    backgroundColor: 'rgba(92, 107, 192, 0.15)',
                  }
                }}
              >
                <ListItemIcon>
                  <PersonIcon sx={{ color: '#5c6bc0' }} />
                </ListItemIcon>
                <ListItemText>Profile</ListItemText>
              </MenuItem>

              {user.role === "User" && [
                <MenuItem 
                  key="orders" 
                  onClick={() => { navigate('/portfolio/orders'); handleClose(); }}
                  sx={{
                    py: 1.5,
                    transition: 'all 0.2s ease',
                    '&:hover': {
                      backgroundColor: 'rgba(92, 107, 192, 0.15)',
                    }
                  }}
                >
                  <ListItemIcon>
                    <HistoryIcon sx={{ color: '#5c6bc0' }} />
                  </ListItemIcon>
                  <ListItemText>Order History</ListItemText>
                </MenuItem>,
                isMobile && (
                  <MenuItem 
                    key="wallet" 
                    onClick={() => { navigate('/wallet'); handleClose(); }}
                    sx={{
                      py: 1.5,
                      transition: 'all 0.2s ease',
                      '&:hover': {
                        backgroundColor: 'rgba(92, 107, 192, 0.15)',
                      }
                    }}
                  >
                    <ListItemIcon>
                      <AccountBalanceWalletIcon sx={{ color: '#5c6bc0' }} />
                    </ListItemIcon>
                    <ListItemText>
                      Wallet
                      <Typography variant="caption" sx={{ display: 'block', color: '#5c6bc0', fontWeight: 600 }}>
                        ${balance?.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) ?? '0.00'}
                      </Typography>
                    </ListItemText>
                  </MenuItem>
                ),
                <MenuItem 
                  key="kyc" 
                  onClick={() => { navigate('/kyc'); handleClose(); }}
                  sx={{
                    py: 1.5,
                    transition: 'all 0.2s ease',
                    '&:hover': {
                      backgroundColor: 'rgba(92, 107, 192, 0.15)',
                    }
                  }}
                >
                  <ListItemIcon>
                    <VerifiedUserIcon sx={{ color: '#5c6bc0' }} />
                  </ListItemIcon>
                  <ListItemText>
                    KYC Status
                    <Badge 
                      badgeContent={user.kycStatus} 
                      color={getKycChipColor()} 
                      sx={{ ml: 4.5 }}
                    />
                  </ListItemText>
                </MenuItem>,
              ]}
              
              <MenuItem 
                onClick={handleLogout}
                sx={{
                  py: 1.5,
                  transition: 'all 0.2s ease',
                  '&:hover': {
                    backgroundColor: 'rgba(245, 0, 87, 0.15)',
                  }
                }}
              >
                <ListItemIcon>
                  <LogoutIcon sx={{ color: '#f50057' }} />
                </ListItemIcon>
                <ListItemText>Logout</ListItemText>
              </MenuItem>
            </Menu>
          </Box>
        ) : (
          <Button 
            component={Link} 
            to="/login"
            sx={{
              color: 'white',
              backgroundColor: 'rgba(92, 107, 192, 0.15)',
              px: 3,
              py: 1,
              borderRadius: 2,
              fontWeight: 600,
              textTransform: 'none',
              border: '1px solid rgba(92, 107, 192, 0.3)',
              transition: 'all 0.3s ease',
              '&:hover': {
                backgroundColor: 'rgba(92, 107, 192, 0.25)',
              }
            }}
          >
            Login
          </Button>
        )}
      </Toolbar>
    </AppBar>
  );
};

export default Navbar;