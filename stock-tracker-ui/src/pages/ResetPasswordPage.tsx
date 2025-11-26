import React, { useState, useEffect } from 'react';
import { Typography, TextField, Button, Box, Alert, CircularProgress } from '@mui/material';
import { useNavigate, useSearchParams } from 'react-router-dom';
import http from '../api/axiosInstance';
import { toast } from 'react-toastify';
import type { ResetPasswordDto } from '../types/authTypes';
import AuthLayout from '../components/layout/AuthLayout';

const ResetPasswordPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const emailFromQuery = searchParams.get('email');

  const [formData, setFormData] = useState({
    code: "",
    newPassword: "",
    confirmPassword: ""
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!emailFromQuery) {
      toast.error("Invalid session. Please try again.");
      navigate("/forgot-password");
    }
  }, [emailFromQuery, navigate]);

  const onChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prevState) => ({
      ...prevState,
      [e.target.name]: e.target.value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (formData.newPassword !== formData.confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    setLoading(true);
    setError(null);

    const resetDto: ResetPasswordDto = {
      email: emailFromQuery!,
      code: formData.code,
      newPassword: formData.newPassword
    };

    try {
      const formDataObj = new FormData();
      formDataObj.append("email", resetDto.email);
      formDataObj.append("code", resetDto.code);
      formDataObj.append("newPassword", resetDto.newPassword);

      const response = await http.post("/auth/reset-password", formDataObj, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      toast.success(response.data.message, {
        autoClose: 2000,
        onClose: () => navigate("/login")
      });

    } catch (err: any) {
      const errorMsg = err.response?.data?.message || "Reset failed. Invalid or expired code.";
      setError(errorMsg);
      toast.error(errorMsg);
      setLoading(false);
    }
  };

  const textFieldStyles = {
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
          PASSWORD RESET
        </Typography>
        <Typography
          component="h1"
          variant="h4"
          sx={{ color: 'white', fontWeight: 700, mb: 1 }}
        >
          Reset Your Password
        </Typography>
        <Typography sx={{ color: 'rgba(255,255,255,0.6)', mb: 3, fontSize: '0.9rem' }}>
          Check your email for the 6-digit code we sent to {emailFromQuery}.
        </Typography>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Box component="form" onSubmit={handleSubmit}>
          <TextField
            margin="normal"
            required
            fullWidth
            id="code"
            label="6-Digit Code"
            name="code"
            autoFocus
            value={formData.code}
            onChange={onChange}
            sx={textFieldStyles}
          />
          <TextField
            margin="normal"
            required
            fullWidth
            name="newPassword"
            label="New Password"
            type="password"
            id="newPassword"
            value={formData.newPassword}
            onChange={onChange}
            sx={textFieldStyles}
          />
          <TextField
            margin="normal"
            required
            fullWidth
            name="confirmPassword"
            label="Confirm New Password"
            type="password"
            id="confirmPassword"
            value={formData.confirmPassword}
            onChange={onChange}
            sx={textFieldStyles}
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
            {loading ? <CircularProgress size={24} /> : "Reset Password"}
          </Button>
        </Box>
      </Box>
    </AuthLayout>
  );
};

export default ResetPasswordPage;