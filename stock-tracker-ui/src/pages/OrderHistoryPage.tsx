import { useEffect } from 'react';
import { Container, Typography, Paper, CircularProgress, Box, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Chip, Card, CardContent, Grid, useMediaQuery, useTheme } from '@mui/material';
import { useAppSelector, useAppDispatch } from '../hooks/redux-hooks';
import { fetchOrders } from '../features/portfolio/portfolioSlice';
import type { OrderDetailsDto } from '../types/tradeTypes';
import HistoryIcon from '@mui/icons-material/History';

const OrderHistoryPage = () => {
  const dispatch = useAppDispatch();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const { orders, status } = useAppSelector((state) => state.portfolio);

  useEffect(() => {
    dispatch(fetchOrders());
  }, [dispatch]);

  const getStatusColor = (status: string): "success" | "warning" | "error" | "default" => {
    if (status === "Approved") return "success";
    if (status === "Pending") return "warning";
    if (status === "Rejected") return "error";
    return "default";
  };

  if (status === 'pending') {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <HistoryIcon sx={{ mr: 1.5, color: '#5c6bc0', fontSize: 28 }} />
        <Typography variant="h5" sx={{ fontWeight: 600 }}>
          Order History
        </Typography>
      </Box>

      {/* Desktop Table View */}
      {!isMobile ? (
        <TableContainer component={Paper} sx={{ backgroundColor: 'rgba(30, 30, 30, 0.6)', border: '1px solid rgba(92, 107, 192, 0.15)' }}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Date</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Stock</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Type</TableCell>
                <TableCell align="right" sx={{ fontWeight: 600 }}>Quantity</TableCell>
                <TableCell align="right" sx={{ fontWeight: 600 }}>Price</TableCell>
                <TableCell align="right" sx={{ fontWeight: 600 }}>Total Value</TableCell>
                <TableCell align="center" sx={{ fontWeight: 600 }}>Status</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {orders.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                    You have not placed any orders.
                  </TableCell>
                </TableRow>
              ) : (
                orders.map((order: OrderDetailsDto) => (
                  <TableRow key={order.id} sx={{ '&:hover': { backgroundColor: 'rgba(92, 107, 192, 0.05)' } }}>
                    <TableCell>{new Date(order.timestamp).toLocaleString()}</TableCell>
                    <TableCell>
                      <Typography variant="body1" sx={{ fontWeight: 'bold' }}>
                        {order.ticker}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography color={order.type === "BUY" ? "success.main" : "error.main"} sx={{ fontWeight: 600 }}>
                        {order.type}
                      </Typography>
                    </TableCell>
                    <TableCell align="right">{order.quantity}</TableCell>
                    <TableCell align="right">${order.pricePerShare.toFixed(2)}</TableCell>
                    <TableCell align="right">${order.totalValue.toFixed(2)}</TableCell>
                    <TableCell align="center">
                      <Chip label={order.status} color={getStatusColor(order.status)} size="small" />
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      ) : (
        /* Mobile Card View */
        <Box>
          {orders.length === 0 ? (
            <Paper sx={{ p: 3, textAlign: 'center' }}>
              You have not placed any orders.
            </Paper>
          ) : (
            orders.map((order: OrderDetailsDto) => (
              <Card key={order.id} sx={{ mb: 2, backgroundColor: 'rgba(30, 30, 30, 0.6)', border: '1px solid rgba(92, 107, 192, 0.15)' }}>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                    <Box>
                      <Typography variant="h6" sx={{ fontWeight: 'bold' }}>
                        {order.ticker}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {new Date(order.timestamp).toLocaleString()}
                      </Typography>
                    </Box>
                    <Chip label={order.status} color={getStatusColor(order.status)} size="small" />
                  </Box>
                  <Grid container spacing={1.5}>
                    <Grid size={{xs:6}}>
                      <Typography variant="caption" color="text.secondary">Type</Typography>
                      <Typography variant="body2" color={order.type === "BUY" ? "success.main" : "error.main"} sx={{ fontWeight: 600 }}>
                        {order.type}
                      </Typography>
                    </Grid>
                    <Grid size={{xs:6}} sx={{ textAlign: 'right' }}>
                      <Typography variant="caption" color="text.secondary">Quantity</Typography>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>{order.quantity}</Typography>
                    </Grid>
                    <Grid size={{xs:6}}>
                      <Typography variant="caption" color="text.secondary">Price</Typography>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>${order.pricePerShare.toFixed(2)}</Typography>
                    </Grid>
                    <Grid size={{xs:6}} sx={{ textAlign: 'right' }}>
                      <Typography variant="caption" color="text.secondary">Total Value</Typography>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>${order.totalValue.toFixed(2)}</Typography>
                    </Grid>
                  </Grid>
                </CardContent>
              </Card>
            ))
          )}
        </Box>
      )}
    </Container>
  );
};

export default OrderHistoryPage;