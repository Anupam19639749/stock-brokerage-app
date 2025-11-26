import React, { useState } from 'react';
import { Typography, TextField, Button, Box, Alert, CircularProgress } from '@mui/material';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import http from '../api/axiosInstance';
import { toast } from 'react-toastify';
import AuthLayout from '../components/layout/AuthLayout';

const ForgotPasswordPage = () => {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const formData = new FormData();
      formData.append("email", email);

      const response = await http.post("/auth/forgot-password", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      toast.success(response.data.message);
      navigate(`/reset-password?email=${encodeURIComponent(email)}`);

    } catch (err: any) {
      const errorMsg = err.response?.data?.message || "Request failed";
      setError(errorMsg);
      toast.error(errorMsg);
    }
    setLoading(false);
  };

  return (
    <AuthLayout>
      <Box>
        <Typography
          sx={{ 
            color: 'rgba(255,255,255,0.5)', 
            fontSize: '0.75rem',
            fontWeight: 600,
            letterSpacing: '1.5px',
            textTransform: 'uppercase',
            mb: 1
          }}
        >
          PASSWORD RECOVERY
        </Typography>
        <Typography
          component="h1"
          variant="h4"
          sx={{ color: 'white', fontWeight: 700, mb: 1 }}
        >
          Forgot Password
        </Typography>
        <Typography sx={{ color: 'rgba(255,255,255,0.6)', mb: 3, fontSize: '0.9rem' }}>
          Enter your email address and we'll send you a 6-digit code to reset your password.
        </Typography>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Box component="form" onSubmit={handleSubmit}>
          <TextField
            margin="normal"
            required
            fullWidth
            id="email"
            label="Email Address"
            name="email"
            autoComplete="email"
            autoFocus
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            sx={{
              '& .MuiOutlinedInput-root': {
                backgroundColor: 'rgba(255, 255, 255, 0.08)',
                color: 'white',
                '& fieldset': { 
                  borderColor: 'rgba(255,255,255,0.2)',
                },
                '&:hover fieldset': { 
                  borderColor: 'rgba(255,255,255,0.3)',
                },
                '&.Mui-focused fieldset': { 
                  borderColor: '#5c6bc0',
                },
              },
              '& .MuiInputLabel-root': { 
                color: 'rgba(255,255,255,0.6)',
              },
              '& .MuiInputLabel-root.Mui-focused': { 
                color: '#5c6bc0',
              },
            }}
          />
          <Button
            type="submit"
            fullWidth
            variant="contained"
            sx={{
              mt: 3,
              mb: 2,
              py: 1.5,
              fontWeight: 600,
              textTransform: 'none',
              fontSize: '1rem',
              backgroundColor: '#5c6bc0',
              '&:hover': {
                backgroundColor: '#4a5bb5',
              }
            }}
            disabled={loading}
          >
            {loading ? <CircularProgress size={24} /> : "Send Reset Code"}
          </Button>
          <Box sx={{ textAlign: 'center' }}>
            <RouterLink 
              to="/login" 
              style={{ 
                color: '#5c6bc0', 
                textDecoration: 'none', 
                fontSize: '0.875rem',
                fontWeight: 500
              }}
            >
              Back to Login
            </RouterLink>
          </Box>
        </Box>
      </Box>
    </AuthLayout>
  );
};

export default ForgotPasswordPage;