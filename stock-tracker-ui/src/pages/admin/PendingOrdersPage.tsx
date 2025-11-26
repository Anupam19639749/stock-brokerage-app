import { useState, useEffect } from 'react';
import { Typography, Paper, CircularProgress, Box, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Button, Stack, Chip } from '@mui/material';
import http from '../../api/axiosInstance';
import type { OrderDetailsDto } from '../../types/tradeTypes';
import { toast } from 'react-toastify';

type LoadingState = {
  action: 'approve' | 'reject';
  orderId: number;
}

const PendingOrdersPage = () => {
  const [orders, setOrders] = useState<OrderDetailsDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingState, setLoadingState] = useState<LoadingState | null>(null);

  const fetchOrders = async () => {
    try {
      setLoading(true);
      const response = await http.get<{ data: OrderDetailsDto[] }>("/admin/orders/pending");
      setOrders(response.data.data);
    } catch (error) {
      toast.error("Failed to fetch pending orders.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrders();
  }, []);

  const fakeDelay = (ms: number) => new Promise(res => setTimeout(res, ms));

  const handleApprove = async (orderId: number) => {
    setLoadingState({ action: 'approve', orderId });
    try {
      const apiCall = http.post(`/admin/orders/approve/${orderId}`);
      await Promise.all([apiCall, fakeDelay(750)]);
      
      toast.success(`Order #${orderId} Approved!`);
      setOrders((prev) => prev.filter((order) => order.id !== orderId));
    } catch (error) {
      toast.error("Failed to approve order.");
    }
    setLoadingState(null);
  };

  const handleReject = async (orderId: number) => {
    setLoadingState({ action: 'reject', orderId });
    try {
      const apiCall = http.post(`/admin/orders/reject/${orderId}`);
      await Promise.all([apiCall, fakeDelay(750)]);
      
      toast.warn(`Order #${orderId} Rejected.`);
      setOrders((prev) => prev.filter((order) => order.id !== orderId));
    } catch (error) {
      toast.error("Failed to reject order.");
    }
    setLoadingState(null);
  };

  if (loading) {
    return <Box display="flex" justifyContent="center"><CircularProgress /></Box>;
  }

  return (
    <Box>
      <Typography variant="h5" gutterBottom>Pending Trade Orders</Typography>
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell>Stock</TableCell>
              <TableCell>Type</TableCell>
              <TableCell align="right">Quantity</TableCell>
              <TableCell align="right">Price</TableCell>
              <TableCell align="right">Total Value</TableCell>
              <TableCell align="center">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {orders.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center">No pending orders.</TableCell>
              </TableRow>
            ) : (
              orders.map((order) => {
                // --- NEW: Check loading state for this specific row ---
                const isApproving = loadingState?.action === 'approve' && loadingState?.orderId === order.id;
                const isRejecting = loadingState?.action === 'reject' && loadingState?.orderId === order.id;
                const isProcessing = isApproving || isRejecting;

                return (
                  <TableRow key={order.id}>
                    <TableCell>{new Date(order.timestamp).toLocaleString()}</TableCell>
                    <TableCell>{order.ticker}</TableCell>
                    <TableCell>
                      <Chip
                        label={order.type}
                        color={order.type === "BUY" ? "success" : "error"}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="right">{order.quantity}</TableCell>
                    <TableCell align="right">${order.pricePerShare.toFixed(2)}</TableCell>
                    <TableCell align="right">${order.totalValue.toFixed(2)}</TableCell>
                    <TableCell align="center">
                      <Stack direction="row" spacing={1} justifyContent="center">
                        <Button
                          size="small"
                          variant="contained"
                          color="success"
                          onClick={() => handleApprove(order.id)}
                          disabled={isProcessing}
                        >
                          {isApproving ? <CircularProgress size={22} color="inherit" /> : "Approve"}
                        </Button>
                        <Button
                          size="small"
                          variant="contained"
                          color="error"
                          onClick={() => handleReject(order.id)}
                          disabled={isProcessing}
                        >
                          {isRejecting ? <CircularProgress size={22} color="inherit" /> : "Reject"}
                        </Button>
                      </Stack>
                    </TableCell>
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};

export default PendingOrdersPage;