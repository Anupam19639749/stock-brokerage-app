import React, { useState, useEffect } from 'react';
import { Modal, Box, Typography, TextField, Button, CircularProgress, Stack, FormControl, InputLabel, Select, MenuItem, FormHelperText, Alert } from '@mui/material';
import http from '../../api/axiosInstance';
import type { AlertCreateDto } from '../../types/alertTypes';
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
  borderRadius: 2,
};

interface AlertsModalProps {
  open: boolean;
  onClose: () => void;
  ticker: string; // The stock we are setting an alert for
}

const AlertsModal = ({ open, onClose, ticker }: AlertsModalProps) => {
  const [condition, setCondition] = useState<"ABOVE" | "BELOW">("ABOVE");
  const [targetPrice, setTargetPrice] = useState<number | string>("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Reset form when modal opens or ticker changes
  useEffect(() => {
    if (open) {
      setCondition("ABOVE");
      setTargetPrice("");
      setError(null);
    }
  }, [open, ticker]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    const alertDto: AlertCreateDto = {
      ticker,
      condition,
      targetPrice: Number(targetPrice),
    };

    // Use FormData for [FromForm]
    const formData = new FormData();
    formData.append("ticker", alertDto.ticker);
    formData.append("condition", alertDto.condition);
    formData.append("targetPrice", alertDto.targetPrice.toString());

    try {
      await http.post("/alert", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      toast.success("Alert set successfully!");
      setLoading(false);
      onClose();
    } catch (err: any) {
      const errorMsg = err.response?.data?.message || "Failed to set alert";
      setError(errorMsg);
      toast.error(errorMsg);
      setLoading(false);
    }
  };

  return (
    <Modal open={open} onClose={onClose}>
      <Box sx={modalStyle}>
        <Typography variant="h6" component="h2" sx={{ fontWeight: 'bold' }}>
          Set New Alert for {ticker}
        </Typography>

        {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}

        <Box component="form" onSubmit={handleSubmit} sx={{ mt: 2 }}>
          <FormControl fullWidth margin="normal">
            <InputLabel id="condition-label">Condition</InputLabel>
            <Select
              labelId="condition-label"
              id="condition"
              name="condition"
              label="Condition"
              value={condition}
              onChange={(e) => setCondition(e.target.value as "ABOVE" | "BELOW")}
            >
              <MenuItem value="ABOVE">Price is Above</MenuItem>
              <MenuItem value="BELOW">Price is Below</MenuItem>
            </Select>
            <FormHelperText>Trigger when the price moves above or below your target.</FormHelperText>
          </FormControl>
          
          <TextField
            margin="normal"
            required
            fullWidth
            id="targetPrice"
            label="Target Price"
            name="targetPrice"
            type="number"
            value={targetPrice}
            onChange={(e) => setTargetPrice(e.target.value)}
            InputProps={{ inputProps: { min: 0.01, step: "0.01" } }}
          />

          <Stack direction="row" spacing={2} sx={{ mt: 3 }}>
            <Button
              type="button"
              fullWidth
              variant="outlined"
              onClick={onClose}
              disabled={loading}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              fullWidth
              variant="contained"
              disabled={loading || !targetPrice}
            >
              {loading ? <CircularProgress size={24} /> : "Set Alert"}
            </Button>
          </Stack>
        </Box>
      </Box>
    </Modal>
  );
};

export default AlertsModal;