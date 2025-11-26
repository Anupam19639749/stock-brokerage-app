import { List, ListItem, ListItemButton, ListItemIcon, ListItemText, Paper } from '@mui/material';
import { NavLink } from 'react-router-dom';
import DashboardIcon from '@mui/icons-material/Dashboard';
import PeopleIcon from '@mui/icons-material/People';
import PendingActionsIcon from '@mui/icons-material/PendingActions';
import AssignmentTurnedInIcon from '@mui/icons-material/AssignmentTurnedIn';

const AdminSidebar = () => {
  const menuItems = [
    { to: '/admin/analytics', icon: <DashboardIcon />, label: 'Analytics' },
    { to: '/admin/kyc-requests', icon: <AssignmentTurnedInIcon />, label: 'KYC Requests' },
    { to: '/admin/pending-orders', icon: <PendingActionsIcon />, label: 'Pending Orders' },
    { to: '/admin/user-management', icon: <PeopleIcon />, label: 'User Management' },
  ];

  return (
    <Paper 
      sx={{ 
        p: 1,
        backgroundColor: 'rgba(30, 30, 30, 0.6)',
        border: '1px solid rgba(92, 107, 192, 0.15)',
      }}
    >
      <List component="nav">
        {menuItems.map((item) => (
          <ListItem key={item.to} disablePadding>
            <ListItemButton
              component={NavLink}
              to={item.to}
              sx={{
                borderRadius: 1,
                mb: 0.5,
                transition: 'all 0.3s ease',
                '&:hover': {
                  backgroundColor: 'rgba(92, 107, 192, 0.08)',
                },
                '&.active': {
                  backgroundColor: 'rgba(92, 107, 192, 0.15)',
                  borderLeft: '3px solid #5c6bc0',
                  '& .MuiListItemIcon-root': {
                    color: '#5c6bc0',
                  },
                  '& .MuiListItemText-primary': {
                    color: '#5c6bc0',
                    fontWeight: 600,
                  },
                },
              }}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
    </Paper>
  );
};

export default AdminSidebar;