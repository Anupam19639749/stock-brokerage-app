import React, { useState, useEffect } from "react";
import { Typography, TextField, Button, Box, Alert, CircularProgress, Grid } from "@mui/material";
import { Link as RouterLink, useNavigate } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../hooks/redux-hooks";
import { registerUser } from "../features/auth/authSlice";
import type { RegisterUserDto } from "../types/authTypes";
import { toast } from "react-toastify";
import AuthLayout from "../components/layout/AuthLayout";

const RegisterPage = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { isAuthenticated, status, error } = useAppSelector((state) => state.auth);

  const [formData, setFormData] = useState<RegisterUserDto>({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    phoneNumber: "",
  });

  const { firstName, lastName, email, password, phoneNumber } = formData;

  useEffect(() => {
    if (isAuthenticated) {
      navigate("/");
    }
  }, [isAuthenticated, navigate]);

  const onChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prevState) => ({
      ...prevState,
      [e.target.name]: e.target.value,
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    dispatch(registerUser(formData))
      .unwrap()
      .then((registerResponse) => {
        toast.success(registerResponse.message, {
          autoClose: 500,
          onClose: () => {
            navigate("/login");
          }
        });
      })
      .catch(() => {});
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
          START FOR FREE
        </Typography>
        <Typography
          component="h1"
          variant="h4"
          sx={{ color: 'white', fontWeight: 700, mb: 1 }}
        >
          Create new account
        </Typography>
        <Typography sx={{ color: 'rgba(255,255,255,0.6)', mb: 3, fontSize: '0.9rem' }}>
          Already A Member?{" "}
          <RouterLink 
            to="/login" 
            style={{ 
              color: '#5c6bc0', 
              textDecoration: 'none',
              fontWeight: 500
            }}
          >
            Log in
          </RouterLink>
        </Typography>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        
        <Box component="form" onSubmit={handleSubmit}>
          <Grid container spacing={2}>
            <Grid size={{xs:12, sm:6}}>
              <TextField
                autoComplete="given-name"
                name="firstName"
                required
                fullWidth
                id="firstName"
                label="First Name"
                autoFocus
                value={firstName}
                onChange={onChange}
                sx={textFieldStyles}
              />
            </Grid>
            <Grid size={{xs:12, sm:6}}>
              <TextField
                required
                fullWidth
                id="lastName"
                label="Last Name"
                name="lastName"
                autoComplete="family-name"
                value={lastName}
                onChange={onChange}
                sx={textFieldStyles}
              />
            </Grid>
            <Grid size={{xs:12}}>
              <TextField
                required
                fullWidth
                id="email"
                label="Email Address"
                name="email"
                autoComplete="email"
                value={email}
                onChange={onChange}
                sx={textFieldStyles}
              />
            </Grid>
            <Grid size={{xs:12}}>
              <TextField
                required
                fullWidth
                id="phoneNumber"
                label="Phone Number"
                name="phoneNumber"
                autoComplete="tel"
                value={phoneNumber}
                onChange={onChange}
                sx={textFieldStyles}
              />
            </Grid>
            <Grid size={{xs:12}}>
              <TextField
                required
                fullWidth
                name="password"
                label="Password"
                type="password"
                id="password"
                autoComplete="new-password"
                value={password}
                onChange={onChange}
                sx={textFieldStyles}
              />
            </Grid>
          </Grid>
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
            {status === 'pending' ? <CircularProgress size={24} /> : "Create Account"}
          </Button>
        </Box>
      </Box>
    </AuthLayout>
  );
};

export default RegisterPage;