import { useState, useEffect } from 'react';
import { Typography, Paper, CircularProgress, Box, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Button, Chip, Card, CardContent, Grid, useMediaQuery, useTheme } from '@mui/material';
import http from '../../api/axiosInstance';
import type { AdminUserListDto } from '../../types/adminTypes';
import { toast } from 'react-toastify';
import BlockIcon from '@mui/icons-material/Block';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { useNavigate } from 'react-router-dom';

const UserManagementPage = () => {
  const [users, setUsers] = useState<AdminUserListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const response = await http.get<{ data: AdminUserListDto[] }>("/admin/users");
      setUsers(response.data.data);
    } catch (error) {
      toast.error("Failed to fetch users.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  const handleToggleBlock = async (userId: number, isCurrentlyActive: boolean) => {
    const action = isCurrentlyActive ? "block" : "unblock";
    try {
      await http.post(`/admin/users/${userId}/${action}`);
      toast.success(`User ${action}ed successfully.`);
      setUsers((prevUsers) =>
        prevUsers.map((user) =>
          user.id === userId ? { ...user, isActive: !isCurrentlyActive } : user
        )
      );
    } catch (error) {
      toast.error(`Failed to ${action} user.`);
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      {/* Desktop Table View */}
      {!isMobile ? (
        <TableContainer component={Paper} sx={{ backgroundColor: 'rgba(30, 30, 30, 0.6)', border: '1px solid rgba(92, 107, 192, 0.15)' }}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Full Name</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Email</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>KYC Status</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Account Status</TableCell>
                <TableCell align="center" sx={{ fontWeight: 600 }}>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {users.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} align="center" sx={{ py: 4 }}>No users found.</TableCell>
                </TableRow>
              ) : (
                users.map((user) => (
                  <TableRow key={user.id} sx={{ '&:hover': { backgroundColor: 'rgba(92, 107, 192, 0.05)' } }}>
                    <TableCell sx={{ fontWeight: 600 }}>{user.fullName}</TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>
                      <Chip label={user.kycStatus} size="small" />
                    </TableCell>
                    <TableCell>
                      <Chip label={user.isActive ? "Active" : "Blocked"} color={user.isActive ? "success" : "error"} size="small" />
                    </TableCell>
                    <TableCell align="center">
                      <Box sx={{ display: 'flex', gap: 1, justifyContent: 'center' }}>
                        <Button
                          size="small"
                          variant="outlined"
                          color={user.isActive ? "error" : "success"}
                          startIcon={user.isActive ? <BlockIcon /> : <CheckCircleIcon />}
                          onClick={() => handleToggleBlock(user.id, user.isActive)}
                        >
                          {user.isActive ? "Block" : "Unblock"}
                        </Button>
                        <Button
                          size="small"
                          variant="outlined"
                          startIcon={<VisibilityIcon />}
                          onClick={() => navigate(`/admin/user-management/${user.id}`)}
                        >
                          View
                        </Button>
                      </Box>
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
          {users.length === 0 ? (
            <Paper sx={{ p: 3, textAlign: 'center' }}>No users found.</Paper>
          ) : (
            users.map((user) => (
              <Card key={user.id} sx={{ mb: 2, backgroundColor: 'rgba(30, 30, 30, 0.6)', border: '1px solid rgba(92, 107, 192, 0.15)' }}>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                    <Typography variant="h6" sx={{ fontWeight: 'bold' }}>
                      {user.fullName}
                    </Typography>
                    <Chip label={user.isActive ? "Active" : "Blocked"} color={user.isActive ? "success" : "error"} size="small" />
                  </Box>
                  <Grid container spacing={1.5} sx={{ mb: 2 }}>
                    <Grid size={{xs:12}}>
                      <Typography variant="caption" color="text.secondary">Email</Typography>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>{user.email}</Typography>
                    </Grid>
                    <Grid size={{xs:6}}>
                      <Typography variant="caption" color="text.secondary">KYC Status</Typography>
                      <Box sx={{ mt: 0.5 }}>
                        <Chip label={user.kycStatus} size="small" />
                      </Box>
                    </Grid>
                  </Grid>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <Button
                      size="small"
                      variant="outlined"
                      color={user.isActive ? "error" : "success"}
                      startIcon={user.isActive ? <BlockIcon /> : <CheckCircleIcon />}
                      onClick={() => handleToggleBlock(user.id, user.isActive)}
                      fullWidth
                    >
                      {user.isActive ? "Block" : "Unblock"}
                    </Button>
                    <Button
                      size="small"
                      variant="outlined"
                      startIcon={<VisibilityIcon />}
                      onClick={() => navigate(`/admin/user-management/${user.id}`)}
                      fullWidth
                    >
                      View
                    </Button>
                  </Box>
                </CardContent>
              </Card>
            ))
          )}
        </Box>
      )}
    </Box>
  );
};

export default UserManagementPage;