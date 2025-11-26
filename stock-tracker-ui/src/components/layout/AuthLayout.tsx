import React from 'react';
import { Box, Paper, Typography } from '@mui/material';
import { Link } from 'react-router-dom';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';

interface AuthLayoutProps {
  children: React.ReactNode;
}

const AuthLayout: React.FC<AuthLayoutProps> = ({ children }) => {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'flex-start',
        background: '#000000ff',
        position: 'relative',
        overflow: 'hidden',
        '&::before': {
          content: '""',
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundImage: 'url(/bull-market.png)',
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          opacity: 0.2,
          zIndex: 0,
        }
      }}
    >
      {/* Logo at top left */}
      <Box
        sx={{
          position: 'absolute',
          top: 32,
          left: 48,
          display: 'flex',
          alignItems: 'center',
          zIndex: 2,
          color: 'white',
        }}
      >
        <TrendingUpIcon sx={{ fontSize: 32, mr: 1, color: '#5c6bc0' }} />
        <Typography
          variant="h5"
          component={Link}
          to="/"
          sx={{
            fontWeight: 700,
            color: 'inherit',
            textDecoration: 'none',
            letterSpacing: '0.5px',
          }}
        >
          Apex Stocks
        </Typography>
      </Box>

      {/* Form Container */}
      <Box
        sx={{
          ml: { xs: 2, sm: 6, md: 12 },
          mr: { xs: 2, sm: 0 },
          zIndex: 1,
          width: { xs: '90%', sm: '450px', md: '500px' },
        }}
      >
        <Paper
          elevation={0}
          sx={{
            p: { xs: 3, sm: 4, md: 5 },
            backgroundColor: 'transparent',
            backgroundImage: 'none',
          }}
        >
          {children}
        </Paper>
      </Box>
    </Box>
  );
};

export default AuthLayout;