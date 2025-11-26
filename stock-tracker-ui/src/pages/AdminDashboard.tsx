import { Box, Container, Tabs, Tab } from '@mui/material';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import DashboardIcon from '@mui/icons-material/Dashboard';
import PeopleIcon from '@mui/icons-material/People';
import PendingActionsIcon from '@mui/icons-material/PendingActions';
import AssignmentTurnedInIcon from '@mui/icons-material/AssignmentTurnedIn';

const AdminDashboard = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const [currentTab, setCurrentTab] = useState(0);

  const tabs = [
    { label: 'Analytics', path: '/admin/analytics', icon: <DashboardIcon /> },
    { label: 'KYC Requests', path: '/admin/kyc-requests', icon: <AssignmentTurnedInIcon /> },
    { label: 'Pending Orders', path: '/admin/pending-orders', icon: <PendingActionsIcon /> },
    { label: 'User Management', path: '/admin/user-management', icon: <PeopleIcon /> },
  ];

  useEffect(() => {
    const currentPath = location.pathname;
    const tabIndex = tabs.findIndex(tab => currentPath.startsWith(tab.path));
    if (tabIndex !== -1) {
      setCurrentTab(tabIndex);
    }
  }, [location.pathname]);

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setCurrentTab(newValue);
    navigate(tabs[newValue].path);
  };

  return (
    <Container maxWidth="lg">
      <Box sx={{ mb: 3 }}>
        
        <Tabs 
          value={currentTab} 
          onChange={handleTabChange}
          variant="scrollable"
          scrollButtons="auto"
          allowScrollButtonsMobile
          sx={{
            borderBottom: '1px solid rgba(92, 107, 192, 0.2)',
            '& .MuiTab-root': {
              textTransform: 'none',
              fontSize: '0.95rem',
              fontWeight: 500,
              minHeight: '48px',
              color: 'rgba(255,255,255,0.7)',
              transition: 'all 0.3s ease',
              '&:hover': {
                color: '#5c6bc0',
                backgroundColor: 'rgba(92, 107, 192, 0.05)',
              },
              '&.Mui-selected': {
                color: '#5c6bc0',
                fontWeight: 600,
              },
            },
            '& .MuiTabs-indicator': {
              backgroundColor: '#5c6bc0',
              height: '3px',
            },
            '& .MuiTabs-scrollButtons': {
              color: 'rgba(255,255,255,0.7)',
              '&.Mui-disabled': {
                opacity: 0.3,
              },
            },
          }}
        >
          {tabs.map((tab, index) => (
            <Tab 
              key={index}
              label={tab.label} 
              icon={tab.icon}
              iconPosition="start"
            />
          ))}
        </Tabs>
      </Box>

      <Box sx={{ mt: 3 }}>
        <Outlet />
      </Box>
    </Container>
  );
};

export default AdminDashboard;