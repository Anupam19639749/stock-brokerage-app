import React, { useState, useEffect } from 'react';
import { Modal, Box, Typography, TextField, Button, CircularProgress, Stack } from '@mui/material';
import type { OrderRequestDto } from '../../types/tradeTypes';
import { useAppSelector } from '../../hooks/redux-hooks';

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

interface BuySellModalProps {
  open: boolean;
  onClose: () => void;
  orderType: "BUY" | "SELL";
  ticker: string;
  livePrice: number;
  onSubmit: (order: OrderRequestDto) => void;
}

const BuySellModal = ({ open, onClose, orderType, ticker, livePrice, onSubmit }: BuySellModalProps) => {
  const [quantity, setQuantity] = useState(1);
  const [totalCost, setTotalCost] = useState(0);
  const { status } = useAppSelector((state) => state.portfolio); // Get loading status
  const loading = status === 'pending';
  
  // Recalculate total cost whenever quantity or price changes
  useEffect(() => {
    if (livePrice > 0 && quantity > 0) {
      setTotalCost(livePrice * quantity);
    } else {
      setTotalCost(0);
    }
  }, [quantity, livePrice]);

  const handleQuantityChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = parseInt(e.target.value, 10);
    if (val > 0) {
      setQuantity(val);
    } else {
      setQuantity(0); // Or 1, depending on desired behavior
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (quantity <= 0) return;

    onSubmit({
      ticker,
      quantity,
      type: orderType,
    });
  };
  
  const isBuy = orderType === "BUY";
  const buttonColor = isBuy ? "success" : "error";

  return (
    <Modal open={open} onClose={onClose}>
      <Box sx={modalStyle}>
        <Typography variant="h6" component="h2" sx={{ fontWeight: 'bold' }}>
          {isBuy ? "Buy" : "Sell"} {ticker}
        </Typography>
        
        <Box sx={{ my: 2 }}>
          <Typography>Live Price: ${livePrice.toFixed(2)}</Typography>
        </Box>

        <Box component="form" onSubmit={handleSubmit}>
          <TextField
            margin="normal"
            required
            fullWidth
            id="quantity"
            label="Quantity"
            name="quantity"
            type="number"
            autoFocus
            value={quantity}
            onChange={handleQuantityChange}
            InputProps={{ inputProps: { min: 1 } }}
          />

          <Typography variant="h6" sx={{ mt: 2, fontWeight: 'bold' }}>
            Total Estimated Cost: ${totalCost.toFixed(2)}
          </Typography>

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
              color={buttonColor}
              disabled={loading || quantity <= 0}
            >
              {loading ? <CircularProgress size={24} /> : `Confirm ${orderType}`}
            </Button>
          </Stack>
        </Box>
      </Box>
    </Modal>
  );
};

export default BuySellModal;