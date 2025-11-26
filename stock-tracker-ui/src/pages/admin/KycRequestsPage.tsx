import { useState, useEffect } from 'react';
import { Typography, Paper, CircularProgress, Box, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Button, Stack } from '@mui/material';
import http from '../../api/axiosInstance';
import type { KycRequestDetailsDto } from '../../types/adminTypes';
import { toast } from 'react-toastify';

type LoadingState = {
  action: 'approve' | 'reject';
  userId: number;
}

const KycRequestsPage = () => {
  const [requests, setRequests] = useState<KycRequestDetailsDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingState, setLoadingState] = useState<LoadingState | null>(null);

  const fetchRequests = async () => {
    try {
      setLoading(true);
      const response = await http.get<{ data: KycRequestDetailsDto[] }>("/admin/kyc-requests");
      setRequests(response.data.data);
    } catch (error) {
      toast.error("Failed to fetch KYC requests.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRequests();
  }, []);

  const fakeDelay = (ms: number) => new Promise(res => setTimeout(res, ms));
  const handleApprove = async (userId: number) => {
    setLoadingState({ action: 'approve', userId });
    try {
      const apiCall = http.post(`/admin/kyc/approve/${userId}`);
      // Run the API call and our 750ms fake delay at the same time
      await Promise.all([apiCall, fakeDelay(750)]);
      
      toast.success("KYC Approved!");
      setRequests((prev) => prev.filter((req) => req.userId !== userId));
    } catch (error) {
      toast.error("Failed to approve KYC.");
    }
    setLoadingState(null);
  };

  const handleReject = async (userId: number) => {
    setLoadingState({ action: 'reject', userId });
    try {
      const apiCall = http.post(`/admin/kyc/reject/${userId}`);
      await Promise.all([apiCall, fakeDelay(750)]);
      
      toast.warn("KYC Rejected.");
      setRequests((prev) => prev.filter((req) => req.userId !== userId));
    } catch (error) {
      toast.error("Failed to reject KYC.");
    }
    setLoadingState(null);
  };

  if (loading) {
    return <Box display="flex" justifyContent="center"><CircularProgress /></Box>;
  }

  return (
    <Box>
      <Typography variant="h5" gutterBottom>Pending KYC Requests</Typography>
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>User</TableCell>
              <TableCell>PAN</TableCell>
              <TableCell>Bank Name</TableCell>
              <TableCell>Account No.</TableCell>
              <TableCell>IFSC</TableCell>
              <TableCell align="center">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {requests.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center">No pending KYC requests.</TableCell>
              </TableRow>
            ) : (
              requests.map((req) => {
                // Check loading state for this specific row
                const isApproving = loadingState?.action === 'approve' && loadingState?.userId === req.userId;
                const isRejecting = loadingState?.action === 'reject' && loadingState?.userId === req.userId;
                const isProcessing = isApproving || isRejecting;

                return (
                  <TableRow key={req.userId}>
                    <TableCell>{req.fullName}<br/><small>{req.email}</small></TableCell>
                    <TableCell>{req.panNumber}</TableCell>
                    <TableCell>{req.bankName}</TableCell>
                    <TableCell>{req.bankAccountNumber}</TableCell>
                    <TableCell>{req.bankIfscCode}</TableCell>
                    <TableCell align="center">
                      <Stack direction="row" spacing={1} justifyContent="center">
                        <Button
                          size="small"
                          variant="contained"
                          color="success"
                          onClick={() => handleApprove(req.userId)}
                          disabled={isProcessing} // Disable if any action is happening
                        >
                          {/* Show spinner or text */}
                          {isApproving ? <CircularProgress size={22} color="inherit" /> : "Approve"}
                        </Button>
                        <Button
                          size="small"
                          variant="contained"
                          color="error"
                          onClick={() => handleReject(req.userId)}
                          disabled={isProcessing} // Disable if any action is happening
                        >
                          {/* Show spinner or text */}
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

export default KycRequestsPage;