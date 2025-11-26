import React, { useState} from "react";
import { Container, Typography, TextField, Button, Box, Alert, CircularProgress, Grid, Paper } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../hooks/redux-hooks";
import type { KycSubmitDto } from "../types/userTypes"; // We'll add this to userTypes
import http from "../api/axiosInstance";
import { toast } from "react-toastify";
import { setLogin } from "../features/auth/authSlice"; // To update the user's kycStatus in Redux

const KycPage = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);

  const [formData, setFormData] = useState<KycSubmitDto>({
    panNumber: "",
    bankName: "",
    bankAccountNumber: "",
    bankIfscCode: "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // If user's KYC is already approved, just show a message
  if (user?.kycStatus === "Approved") {
    return (
      <Container maxWidth="sm">
        <Paper sx={{ p: 4, mt: 8, textAlign: 'center' }}>
          <Typography variant="h5" gutterBottom>KYC Verified</Typography>
          <Typography>Your KYC details are approved. You are ready to trade.</Typography>
          <Button variant="contained" sx={{ mt: 3 }} onClick={() => navigate('/')}>
            Go to Dashboard
          </Button>
        </Paper>
      </Container>
    );
  }
  
  // If it's pending, show a different message
    if (user?.kycStatus === "Pending") {
        return (
        <Container maxWidth="sm">
            <Paper sx={{ p: 4, mt: 8, textAlign: 'center' }}>
            <Typography variant="h5" gutterBottom>KYC Pending Approval</Typography>
            <Typography>Your details have been submitted and are awaiting admin review.</Typography>
            </Paper>
        </Container>
        );
    }

  // If it's Rejected or NotSubmitted, we show the form.
  // We can add a special message for 'Rejected'.
  const pageTitle = user?.kycStatus === "Rejected" 
    ? "KYC Verification Rejected" 
    : "KYC Verification";
  
  const pageSubtitle = user?.kycStatus === "Rejected"
    ? "Your previous submission was rejected. Please review your details and resubmit."
    : "Please submit your details to get approved for trading.";

  // Handle form change
  const onChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prevState) => ({
      ...prevState,
      [e.target.name]: e.target.value.toUpperCase(), // Good to uppercase PAN/IFSC
    }));
  };

  // Handle form submit
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    // Create FormData for [FromForm]
    const formDataObj = new FormData();
    formDataObj.append("panNumber", formData.panNumber);
    formDataObj.append("bankName", formData.bankName);
    formDataObj.append("bankAccountNumber", formData.bankAccountNumber);
    formDataObj.append("bankIfscCode", formData.bankIfscCode);

    try {
      // We don't need a thunk for this, we can call http directly
      const response = await http.post("/users/kyc", formDataObj, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      
      toast.success(response.data.message);
      
      // Update the user's state in Redux to "Pending"
      if (user) {
        dispatch(setLogin({ ...user, kycStatus: "Pending" }));
      }
      
      setLoading(false);
      // We don't navigate, the component will re-render and show the "Pending" message
    } catch (err: any) {
      const errorMsg = err.response?.data?.message || "KYC submission failed";
      setError(errorMsg);
      toast.error(errorMsg);
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="sm">
      <Box sx={{ mt: 8, display: "flex", flexDirection: "column", alignItems: "center" }}>
        <Typography component="h1" variant="h4" gutterBottom>
          {pageTitle}
        </Typography>
        <Typography>
          {pageSubtitle}
        </Typography>
        
        {error && <Alert severity="error" sx={{ mt: 2, width: '100%' }}>{error}</Alert>}
        
        <Box component="form" onSubmit={handleSubmit} sx={{ mt: 3 }}>
          <Grid container spacing={2}>
            <Grid size={{xs:12}}>
              <TextField
                required
                fullWidth
                id="panNumber"
                label="PAN Number"
                name="panNumber"
                value={formData.panNumber}
                onChange={onChange}
                inputProps={{ maxLength: 10 }}
              />
            </Grid>
            <Grid size={{xs:12}}>
              <TextField
                required
                fullWidth
                id="bankName"
                label="Bank Name"
                name="bankName"
                value={formData.bankName}
                onChange={onChange}
              />
            </Grid>
            <Grid size={{xs:12, sm:6}}>
              <TextField
                required
                fullWidth
                id="bankAccountNumber"
                label="Bank Account Number"
                name="bankAccountNumber"
                value={formData.bankAccountNumber}
                onChange={onChange}
              />
            </Grid>
            <Grid size={{xs:12, sm:6}}>
              <TextField
                required
                fullWidth
                id="bankIfscCode"
                label="IFSC Code"
                name="bankIfscCode"
                value={formData.bankIfscCode}
                onChange={onChange}
                inputProps={{ maxLength: 11 }}
              />
            </Grid>
          </Grid>
          <Button
            type="submit"
            fullWidth
            variant="contained"
            sx={{ mt: 3, mb: 2 }}
            disabled={loading}
          >
            {loading ? <CircularProgress size={24} /> : "Submit for Verification"}
          </Button>
        </Box>
      </Box>
    </Container>
  );
};

export default KycPage;