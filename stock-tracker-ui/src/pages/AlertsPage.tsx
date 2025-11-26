import { useState, useEffect } from 'react';
import { Container, Typography, Paper, CircularProgress, Box, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, IconButton, Chip, Card, CardContent, Grid, useMediaQuery, useTheme, Button } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import NotificationsActiveIcon from '@mui/icons-material/NotificationsActive';
import http from '../api/axiosInstance';
import type { AlertDetailsDto } from '../types/alertTypes';
import { toast } from 'react-toastify';

const AlertsPage = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const [alerts, setAlerts] = useState<AlertDetailsDto[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchAlerts = async () => {
    try {
      setLoading(true);
      const response = await http.get<{ data: AlertDetailsDto[] }>("/alert/my-alerts");
      setAlerts(response.data.data);
    } catch (error) {
      toast.error("Failed to fetch alerts.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAlerts();
  }, []);

  const handleDelete = async (alertId: number) => {
    try {
      await http.delete(`/alert/${alertId}`);
      toast.success("Alert deleted.");
      setAlerts((prev) => prev.filter((alert) => alert.id !== alertId));
    } catch (error) {
      toast.error("Failed to delete alert.");
    }
  };

  const getStatusColor = (status: string): "success" | "warning" | "default" => {
    if (status === "Triggered") return "success";
    if (status === "Active") return "warning";
    return "default";
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <NotificationsActiveIcon sx={{ mr: 1.5, color: '#5c6bc0', fontSize: 28 }} />
        <Typography variant="h5" sx={{ fontWeight: 600 }}>
          My Price Alerts
        </Typography>
      </Box>

      {/* Desktop Table View */}
      {!isMobile ? (
        <TableContainer component={Paper} sx={{ backgroundColor: 'rgba(30, 30, 30, 0.6)', border: '1px solid rgba(92, 107, 192, 0.15)' }}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Stock</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Condition</TableCell>
                <TableCell align="right" sx={{ fontWeight: 600 }}>Target Price</TableCell>
                <TableCell align="center" sx={{ fontWeight: 600 }}>Status</TableCell>
                <TableCell align="center" sx={{ fontWeight: 600 }}>Created At</TableCell>
                <TableCell align="center" sx={{ fontWeight: 600 }}>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {alerts.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                    You have not set any price alerts.
                  </TableCell>
                </TableRow>
              ) : (
                alerts.map((alert) => (
                  <TableRow key={alert.id} sx={{ '&:hover': { backgroundColor: 'rgba(92, 107, 192, 0.05)' } }}>
                    <TableCell>
                      <Typography variant="body1" sx={{ fontWeight: 'bold' }}>
                        {alert.ticker}
                      </Typography>
                    </TableCell>
                    <TableCell>Price {alert.condition === "ABOVE" ? ">" : "<"}</TableCell>
                    <TableCell align="right">${alert.targetPrice.toFixed(2)}</TableCell>
                    <TableCell align="center">
                      <Chip label={alert.status} color={getStatusColor(alert.status)} size="small" />
                    </TableCell>
                    <TableCell align="center">
                      {new Date(alert.createdAt).toLocaleDateString()}
                    </TableCell>
                    <TableCell align="center">
                      <IconButton color="error" onClick={() => handleDelete(alert.id)}>
                        <DeleteIcon />
                      </IconButton>
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
          {alerts.length === 0 ? (
            <Paper sx={{ p: 3, textAlign: 'center' }}>
              You have not set any price alerts.
            </Paper>
          ) : (
            alerts.map((alert) => (
              <Card key={alert.id} sx={{ mb: 2, backgroundColor: 'rgba(30, 30, 30, 0.6)', border: '1px solid rgba(92, 107, 192, 0.15)' }}>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                    <Typography variant="h6" sx={{ fontWeight: 'bold' }}>
                      {alert.ticker}
                    </Typography>
                    <Chip label={alert.status} color={getStatusColor(alert.status)} size="small" />
                  </Box>
                  <Grid container spacing={1.5} sx={{ mb: 2 }}>
                    <Grid size={{xs:6}}>
                      <Typography variant="caption" color="text.secondary">Condition</Typography>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>
                        Price {alert.condition === "ABOVE" ? ">" : "<"}
                      </Typography>
                    </Grid>
                    <Grid size={{xs:6}} sx={{ textAlign: 'right' }}>
                      <Typography variant="caption" color="text.secondary">Target Price</Typography>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>
                        ${alert.targetPrice.toFixed(2)}
                      </Typography>
                    </Grid>
                    <Grid size={{xs:12}}>
                      <Typography variant="caption" color="text.secondary">Created</Typography>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>
                        {new Date(alert.createdAt).toLocaleDateString()}
                      </Typography>
                    </Grid>
                  </Grid>
                  <Button 
                    variant="outlined" 
                    color="error" 
                    onClick={() => handleDelete(alert.id)} 
                    fullWidth
                    startIcon={<DeleteIcon />}
                  >
                    Delete Alert
                  </Button>
                </CardContent>
              </Card>
            ))
          )}
        </Box>
      )}
    </Container>
  );
};

export default AlertsPage;