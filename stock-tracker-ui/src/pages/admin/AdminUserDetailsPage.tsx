import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Typography, Paper, CircularProgress, Box, Grid, Card, CardContent, Avatar, Chip, Button, Table, TableBody, TableCell, TableContainer, TableHead, TableRow } from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import http from '../../api/axiosInstance';
import type { AdminUserDetailsDto } from '../../types/adminTypes';
import { toast } from 'react-toastify';

const AdminUserDetailsPage = () => {
  const { userId } = useParams<{ userId: string }>();
  const navigate = useNavigate();
  const [user, setUser] = useState<AdminUserDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!userId) {
      toast.error("User ID not found.");
      navigate("/admin/user-management");
      return;
    }

    const fetchUserDetails = async () => {
      try {
        setLoading(true);
        const response = await http.get<{ data: AdminUserDetailsDto }>(`/admin/users/${userId}`);
        setUser(response.data.data);
      } catch (error) {
        toast.error("Failed to fetch user details.");
      } finally {
        setLoading(false);
      }
    };

    fetchUserDetails();
  }, [userId, navigate]);

  if (loading) {
    return <Box display="flex" justifyContent="center"><CircularProgress /></Box>;
  }

  if (!user) {
    return <Typography>User not found.</Typography>;
  }

  const getKycChipColor = (status: string) => {
    if (status === "Approved") return "success";
    if (status === "Pending") return "warning";
    if (status === "Rejected") return "error";
    return "default";
  };
  
  const getStatusColor = (status: string): "success" | "warning" | "error" | "default" => {
    if (status === "Approved") return "success";
    if (status === "Pending") return "warning";
    if (status === "Rejected") return "error";
    return "default";
  };

  return (
    <Box>
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={() => navigate("/admin/user-management")}
        sx={{ mb: 2 }}
      >
        Back to User List
      </Button>

      <Grid container spacing={3}>
        {/* Profile & KYC Card */}
        <Grid size={{xs:12, md:5}}>
          <Paper sx={{ p: 3, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
            <Avatar
              src={`https://localhost:7290/api/admin/users/${user.id}/image?${Date.now()}`}
              sx={{ width: 100, height: 100, fontSize: '3rem', mb: 2 }}
            >
              {user.firstName[0]}
            </Avatar>
            <Typography variant="h5" sx={{ fontWeight: 'bold' }}>{user.firstName} {user.lastName}</Typography>
            <Typography color="text.secondary">{user.email}</Typography>
            <Chip
              label={user.isActive ? "Active" : "Blocked"}
              color={user.isActive ? "success" : "error"}
              size="small"
              sx={{ mt: 1 }}
            />
          </Paper>
          <Paper sx={{ p: 3, mt: 3 }}>
            <Typography variant="h6" gutterBottom>KYC Details</Typography>
            <Chip label={user.kycStatus} color={getKycChipColor(user.kycStatus)} sx={{ mb: 2 }} />
            <Typography variant="body2"><strong>PAN:</strong> {user.panNumber || 'N/A'}</Typography>
            <Typography variant="body2"><strong>Bank:</strong> {user.bankName || 'N/A'}</Typography>
            <Typography variant="body2"><strong>Account:</strong> {user.bankAccountNumber || 'N/A'}</Typography>
            <Typography variant="body2"><strong>IFSC:</strong> {user.bankIfscCode || 'N/A'}</Typography>
          </Paper>
        </Grid>

        {/* Financial Info */}
        <Grid size={{xs:12, md:7}} >
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>Financials</Typography>
            <Card variant="outlined" sx={{ mb: 2 }}>
              <CardContent>
                <Typography variant="body2" color="text.secondary">Wallet Balance</Typography>
                <Typography variant="h5" sx={{ fontWeight: 'bold' }}>
                  ₹{user.wallet?.balance?.toLocaleString('en-IN') ?? '0.00'}
                </Typography>
              </CardContent>
            </Card>

            <Typography variant="h6" gutterBottom sx={{ mt: 3 }}>Portfolio Holdings</Typography>
            <TableContainer>
              <Table size="small">
                <TableHead><TableRow><TableCell>Stock</TableCell><TableCell align="right">Quantity</TableCell><TableCell align="right">Avg. Cost</TableCell></TableRow></TableHead>
                <TableBody>
                  {user.portfolio.length === 0 ? (
                    <TableRow><TableCell colSpan={3} align="center">No holdings</TableCell></TableRow>
                  ) : (
                    user.portfolio.map(h => (
                      <TableRow key={h.id}>
                        <TableCell sx={{ fontWeight: 'bold' }}>{h.ticker}</TableCell>
                        <TableCell align="right">{h.quantity}</TableCell>
                        <TableCell align="right">${h.averageCostPrice.toFixed(2)}</TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>

            <Typography variant="h6" gutterBottom sx={{ mt: 3 }}>Order History</Typography>
            <TableContainer sx={{ maxHeight: 300 }}>
              <Table size="small" stickyHeader>
                <TableHead><TableRow><TableCell>Date</TableCell><TableCell>Stock</TableCell><TableCell>Type</TableCell><TableCell align="center">Status</TableCell></TableRow></TableHead>
                <TableBody>
                  {user.orders.length === 0 ? (
                    <TableRow><TableCell colSpan={4} align="center">No orders</TableCell></TableRow>
                  ) : (
                    user.orders.map(o => (
                      <TableRow key={o.id}>
                        <TableCell>{new Date(o.timestamp).toLocaleDateString()}</TableCell>
                        <TableCell sx={{ fontWeight: 'bold' }}>{o.ticker}</TableCell>
                        <TableCell color={o.type === "BUY" ? "success.main" : "error.main"}>{o.type}</TableCell>
                        <TableCell align="center">
                          <Chip label={o.status} color={getStatusColor(o.status)} size="small" />
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default AdminUserDetailsPage;