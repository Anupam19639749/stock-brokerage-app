import React, { useState } from 'react';
import { Modal, Box, Typography, TextField, Button, CircularProgress, Alert } from '@mui/material';
import { useAppSelector, useAppDispatch } from '../../hooks/redux-hooks';
import { setWalletBalance } from '../../features/wallet/walletSlice';
import type { AddMoneyRequestDto, WalletBalanceDto } from '../../types/walletTypes';
import http from '../../api/axiosInstance';
import { toast } from 'react-toastify';

// MUI styles for the modal
const modalStyle = {
  position: 'absolute' as 'absolute',
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 400,
  bgcolor: 'background.paper',
  border: '2px solid #000',
  boxShadow: 24,
  p: 4,
};

interface AddMoneyModalProps {
  open: boolean;
  onClose: () => void;
}

const AddMoneyModal = ({ open, onClose }: AddMoneyModalProps) => {
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const [formData, setFormData] = useState({ amount: '', password: '' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prevState) => ({
      ...prevState,
      [e.target.name]: e.target.value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    const requestDto: AddMoneyRequestDto = {
      amount: parseFloat(formData.amount),
      password: formData.password,
    };
    
    // 1. The 3-second "theater"
    await new Promise((resolve) => setTimeout(resolve, 3000));

    try {
      // 2. The real API call
      const formDataObj = new FormData();
      formDataObj.append("amount", requestDto.amount.toString());
      formDataObj.append("password", requestDto.password);

      const response = await http.post<{ data: WalletBalanceDto }>("/wallet/deposit", formDataObj, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      
      // 3. Success
      dispatch(setWalletBalance(response.data.data));
      toast.success("Funds added successfully!");
      setLoading(false);
      onClose(); // Close the modal
      setFormData({ amount: '', password: '' }); // Reset form
    } catch (err: any) {
      // 4. Failure
      const errorMsg = err.response?.data?.message || "Failed to add funds";
      setError(errorMsg);
      toast.error(errorMsg);
      setLoading(false);
    }
  };

  return (
    <Modal open={open} onClose={onClose}>
      <Box sx={modalStyle}>
        <Typography variant="h6" component="h2">
          Add Money to Wallet
        </Typography>
        <Typography sx={{ mt: 2, mb: 2, fontSize: '0.9rem' }}>
          From: {user?.bankName} (A/c: ...{user?.bankAccountNumber?.slice(-4)})
        </Typography>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Box component="form" onSubmit={handleSubmit}>
          <TextField
            margin="normal"
            required
            fullWidth
            id="amount"
            label="Amount (e.g., 50000)"
            name="amount"
            type="number"
            autoFocus
            value={formData.amount}
            onChange={onChange}
          />
          <TextField
            margin="normal"
            required
            fullWidth
            name="password"
            label="Confirm with Password"
            type="password"
            id="password"
            value={formData.password}
            onChange={onChange}
          />
          <Button
            type="submit"
            fullWidth
            variant="contained"
            sx={{ mt: 3, mb: 2 }}
            disabled={loading}
          >
            {loading ? <CircularProgress size={24} /> : `Add ₹${formData.amount || '0'}`}
          </Button>
        </Box>
      </Box>
    </Modal>
  );
};

export default AddMoneyModal;