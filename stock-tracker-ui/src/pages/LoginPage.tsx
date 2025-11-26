import React, { useState, useEffect } from "react";
import { Typography, TextField, Button, Box, Alert, CircularProgress } from "@mui/material";
import { Link as RouterLink, useNavigate } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../hooks/redux-hooks";
import { loginUser } from "../features/auth/authSlice";
import type { LoginDto } from "../types/authTypes";
import { toast } from "react-toastify";
import AuthLayout from "../components/layout/AuthLayout";

const LoginPage = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { isAuthenticated, status, error, user } = useAppSelector((state) => state.auth);

  const [formData, setFormData] = useState<LoginDto>({
    email: "",
    password: "",
  });

  const { email, password } = formData;

  useEffect(() => {
    if (isAuthenticated) {
      if (user?.role === "Admin") {
        navigate("/admin");
      } else {
        navigate("/");
      }
    }
  }, [isAuthenticated, user, navigate]);

  const onChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prevState) => ({
      ...prevState,
      [e.target.name]: e.target.value,
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    dispatch(loginUser(formData))
      .unwrap()
      .then((loginResponse) => {
        toast.success(loginResponse.message, {
          autoClose: 500,
          onClose: () => {
            if (loginResponse.data.user.role === "Admin") {
              navigate("/admin");
            } else {
              navigate("/");
            }
          }
        });
      })
      .catch(() => {});
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
          START FOR FREE
        </Typography>
        <Typography
          component="h1"
          variant="h4"
          sx={{ color: 'white', fontWeight: 700, mb: 1 }}
        >
          Sign In
        </Typography>
        <Typography sx={{ color: 'rgba(255,255,255,0.6)', mb: 3, fontSize: '0.9rem' }}>
          Welcome back! Please login to your account.
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
            onChange={onChange}
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
          <TextField
            margin="normal"
            required
            fullWidth
            name="password"
            label="Password"
            type="password"
            id="password"
            autoComplete="current-password"
            value={password}
            onChange={onChange}
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
            disabled={status === 'pending'}
          >
            {status === 'pending' ? <CircularProgress size={24} /> : "Sign In"}
          </Button>

          <Box sx={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: 1 }}>
            <RouterLink
              to="/forgot-password"
              style={{ 
                color: "#5c6bc0", 
                textDecoration: 'none', 
                fontSize: '0.875rem',
                fontWeight: 500
              }}
            >
              Forgot password?
            </RouterLink>
            <RouterLink
              to="/register"
              style={{ 
                color: "#5c6bc0", 
                textDecoration: 'none', 
                fontSize: '0.875rem',
                fontWeight: 500
              }}
            >
              Don't have an account? Sign Up
            </RouterLink>
          </Box>
        </Box>
      </Box>
    </AuthLayout>
  );
};

export default LoginPage;