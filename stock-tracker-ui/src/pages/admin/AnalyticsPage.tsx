import { useState, useEffect } from 'react';
import { Typography, Paper, CircularProgress, Box, Grid } from '@mui/material';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import http from '../../api/axiosInstance';
import type { AdminStatsDto } from '../../types/adminTypes';
import { toast } from 'react-toastify';
import PeopleIcon from '@mui/icons-material/People';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';

const AnalyticsPage = () => {
  const [stats, setStats] = useState<AdminStatsDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        setLoading(true);
        const response = await http.get<{ data: AdminStatsDto }>("/admin/stats");
        setStats(response.data.data);
      } catch (error) {
        toast.error("Failed to fetch analytics.");
      } finally {
        setLoading(false);
      }
    };
    fetchStats();
  }, []);

  if (loading || !stats) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      {/* Stat Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{xs:12, sm:6}}>
          <Paper sx={{ 
            p: 3, 
            textAlign: 'center',
            backgroundColor: 'rgba(0, 0, 0, 0.6)', 
            border: '1px solid rgba(92, 107, 192, 0.15)',
            transition: 'all 0.3s ease',
            '&:hover': {
              border: '1px solid rgba(92, 107, 192, 0.3)',
              transform: 'translateY(-2px)',
            }
          }}>
            <Box sx={{ display: 'flex', justifyContent: 'center', mb: 1 }}>
              <PeopleIcon sx={{ fontSize: 40, color: '#5c6bc0' }} />
            </Box>
            <Typography variant="body2" color="text.secondary">Total Users</Typography>
            <Typography variant="h4" sx={{ fontWeight: 'bold', mt: 1 }}>
              {stats.totalUsers}
            </Typography>
          </Paper>
        </Grid>
        <Grid size={{xs:12, sm:6}}>
          <Paper sx={{ 
            p: 3, 
            textAlign: 'center',
            backgroundColor: 'rgba(0, 0, 0, 0.6)', 
            border: '1px solid rgba(92, 107, 192, 0.15)',
            transition: 'all 0.3s ease',
            '&:hover': {
              border: '1px solid rgba(92, 107, 192, 0.3)',
              transform: 'translateY(-2px)',
            }
          }}>
            <Box sx={{ display: 'flex', justifyContent: 'center', mb: 1 }}>
              <TrendingUpIcon sx={{ fontSize: 40, color: '#4caf50' }} />
            </Box>
            <Typography variant="body2" color="text.secondary">Active Users (30-day)</Typography>
            <Typography variant="h4" sx={{ fontWeight: 'bold', mt: 1 }}>
              {stats.activeUsers}
            </Typography>
          </Paper>
        </Grid>
      </Grid>

      {/* Charts */}
      <Grid container spacing={3}>
        <Grid size={{xs:12, md:6}}>
          <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
            Top 10 Held Stocks
          </Typography>
          <Paper 
            sx={{ 
              p: 2, 
              height: '400px',
              backgroundColor: 'rgba(0, 0, 0, 0.6)',
              border: '1px solid rgba(92, 107, 192, 0.15)',
            }}
          >
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={stats.topHeldStocks} margin={{ top: 5, right: 20, left: -20, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.1)" />
                <XAxis dataKey="ticker" stroke="rgba(255,255,255,0.7)" />
                <YAxis allowDecimals={false} stroke="rgba(255,255,255,0.7)" />
                <Tooltip 
                  contentStyle={{ 
                    backgroundColor: 'rgba(0, 0, 0, 0.95)', 
                    border: '1px solid rgba(92, 107, 192, 0.3)',
                    borderRadius: '4px',
                  }} 
                />
                <Legend />
                <Bar dataKey="count" fill="#5c6bc0" name="Users Holding" />
              </BarChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>
        <Grid size={{xs:12, md:6}}>
          <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
            Top 10 Alerted Stocks
          </Typography>
          <Paper 
            sx={{ 
              p: 2, 
              height: '400px',
              backgroundColor: 'rgba(0, 0, 0, 0.6)',
              border: '1px solid rgba(92, 107, 192, 0.15)',
            }}
          >
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={stats.topAlertedStocks} margin={{ top: 5, right: 20, left: -20, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.1)" />
                <XAxis dataKey="ticker" stroke="rgba(255,255,255,0.7)" />
                <YAxis allowDecimals={false} stroke="rgba(255,255,255,0.7)" />
                <Tooltip 
                  contentStyle={{ 
                    backgroundColor: 'rgba(0, 0, 0, 0.95)', 
                    border: '1px solid rgba(92, 107, 192, 0.3)',
                    borderRadius: '4px',
                  }} 
                />
                <Legend />
                <Bar dataKey="count" fill="#4caf50" name="Alerts Set" />
              </BarChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default AnalyticsPage;