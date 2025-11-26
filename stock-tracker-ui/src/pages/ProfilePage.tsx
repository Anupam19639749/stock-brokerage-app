import React, { useState, useEffect } from 'react';
import { Container, Typography, TextField, Button, Box, Paper, Grid, CircularProgress, Avatar, IconButton, Select, MenuItem, InputLabel, FormControl, FormHelperText, Stack } from '@mui/material';
import { PhotoCamera } from '@mui/icons-material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { useAppSelector, useAppDispatch } from '../hooks/redux-hooks';
import http from '../api/axiosInstance';
import { toast } from 'react-toastify';
import type { ProfileUpdateDto } from '../types/userTypes';
import { setLogin } from '../features/auth/authSlice';
import { useNavigate } from 'react-router-dom';

const ProfilePage = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);

  const [formData, setFormData] = useState<ProfileUpdateDto>({
    firstName: '',
    lastName: '',
    phoneNumber: '',
    gender: '',
    dateOfBirth: null,
  });
  const [loading, setLoading] = useState(false);
  const [imgLoading, setImgLoading] = useState(false);
  const [avatarSrc, setAvatarSrc] = useState<string>('');
  const fakeDelay = (ms: number) => new Promise(res => setTimeout(res, ms));

  // This key is a trick to force the Avatar to reload after an upload
  const [avatarKey, setAvatarKey] = useState(Date.now());

  // When the component loads, fill the form with the user's data from Redux
  useEffect(() => {
    if (user) {
      setFormData({
        firstName: user.firstName,
        lastName: user.lastName,
        phoneNumber: user.phoneNumber || '',
        gender: user.gender || '',
        dateOfBirth: user.dateOfBirth ? new Date(user.dateOfBirth) : null,
      });
      // Set the initial avatar URL
      setAvatarSrc(`https://localhost:7290/api/users/my-image?${avatarKey}`);
    }
  }, [user, avatarKey]); // Re-run if user or avatarKey changes

  // Handler for text, select, etc.
  const onChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement> | { target: { name: string, value: unknown } }) => {
    setFormData((prevState) => ({
      ...prevState,
      [e.target.name]: e.target.value,
    }));
  };

  // Handler for the Date Picker
  const onDateChange = (newDate: Date | null) => {
    setFormData((prevState) => ({
      ...prevState,
      dateOfBirth: newDate,
    }));
  };

  // Handler for the Profile Image Upload
  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setImgLoading(true);
    const formData = new FormData();
    formData.append("file", file);

    try {
      await http.post("/users/profile/image", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      toast.success("Profile image updated!");
      // Force the Avatar to re-fetch the new image
      setAvatarKey(Date.now());
    } catch (err: any) {
      toast.error(err.response?.data?.message || "Image upload failed");
    }
    setImgLoading(false);
  };

  // Handler for the main profile form
  const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault();
  setLoading(true);

  const formDataObj = new FormData();
  formDataObj.append("firstName", formData.firstName);
  formDataObj.append("lastName", formData.lastName);
  formDataObj.append("phoneNumber", formData.phoneNumber || '');
  formDataObj.append("gender", formData.gender || '');
  if (formData.dateOfBirth) {
    // Format the date correctly for the backend
    formDataObj.append("dateOfBirth", formData.dateOfBirth.toISOString().split('T')[0]);
  }

  try {
    // 1. Create the API call promise
    const apiCall = http.put<{ data: any, message: string }>("/users/profile", formDataObj, {
       headers: { "Content-Type": "multipart/form-data" },
    });

    // 2. Run the API call and our 500ms fake delay in parallel
    const [response] = await Promise.all([
      apiCall,
      fakeDelay(500) // 0.5 second delay
    ]);

    // 3. Update the user state in Redux
    dispatch(setLogin(response.data.data));
    toast.success(response.data.message);

    // 4. Redirect to the home page
    navigate('/');

  } catch (err: any) {
    toast.error(err.response?.data?.message || "Profile update failed");
  }
  setLoading(false);
};


  return (
    <Container maxWidth="md">
      <Typography variant="h4" gutterBottom>My Profile</Typography>
      <Paper sx={{ p: 4 }}>
        <Grid container spacing={3}>
          
          {/* --- AVATAR UPLOAD --- */}
          <Grid size={{xs:12}} sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
            <Box sx={{ position: 'relative' }}>
              <Avatar
                src={avatarSrc}
                // Handle image load error
                onError={() => setAvatarSrc('')} // Fallback to initials if load fails
                sx={{ width: 120, height: 120, fontSize: '3rem' }}
              >
                {user?.firstName[0]}
              </Avatar>
              <IconButton
                color="primary"
                component="label"
                sx={{ position: 'absolute', bottom: 0, right: 0, backgroundColor: 'background.paper' }}
              >
                <input type="file" hidden accept="image/*" onChange={handleImageUpload} />
                {imgLoading ? <CircularProgress size={24} /> : <PhotoCamera />}
              </IconButton>
            </Box>
            <FormHelperText sx={{ textAlign: 'center', mt: 1 }}>
                Allowed: JPG, PNG, GIF. Max size: 2 MB
            </FormHelperText>
          </Grid>
          
          {/* --- PROFILE FORM --- */}
          <Grid size={{xs:12}}>
            <Box component="form" onSubmit={handleSubmit}>
              <Grid container spacing={2}>
                <Grid size={{xs:12, sm:6}}>
                  <TextField
                    required
                    fullWidth
                    id="firstName"
                    label="First Name"
                    name="firstName"
                    value={formData.firstName}
                    onChange={onChange}
                  />
                </Grid>
                <Grid size={{xs:12, sm:6}}>
                  <TextField
                    required
                    fullWidth
                    id="lastName"
                    label="Last Name"
                    name="lastName"
                    value={formData.lastName}
                    onChange={onChange}
                  />
                </Grid>
                <Grid size={{xs:12, sm:6}}>
                  <TextField
                    fullWidth
                    id="phoneNumber"
                    label="Phone Number"
                    name="phoneNumber"
                    value={formData.phoneNumber}
                    onChange={onChange}
                  />
                </Grid>
                <Grid size={{xs:12, sm:6}}>
                  <FormControl fullWidth>
                    <InputLabel id="gender-label">Gender</InputLabel>
                    <Select
                      labelId="gender-label"
                      id="gender"
                      name="gender"
                      label="Gender"
                      value={formData.gender}
                      onChange={(e) => onChange(e as any)} // Cast for Select
                    >
                      <MenuItem value=""><em>None</em></MenuItem>
                      <MenuItem value="Male">Male</MenuItem>
                      <MenuItem value="Female">Female</MenuItem>
                      <MenuItem value="Other">Other</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid size={{xs:12, sm:6}}>
                  <DatePicker
                    label="Date of Birth"
                    value={formData.dateOfBirth}
                    onChange={onDateChange}
                    sx={{ width: '100%' }}
                  />
                  <FormHelperText>Must be at least 12 years old.</FormHelperText>
                </Grid>
                <Grid size={{xs:12, sm:6}}>
                  <TextField
                    disabled // User cannot change their email
                    fullWidth
                    id="email"
                    label="Email"
                    name="email"
                    value={user?.email || ''}
                  />
                </Grid>
                <Grid size={{xs:12}}>
                  <Stack direction="row" spacing={2} sx={{ mt: 3, mb: 2 }}>
                    <Button
                        type="submit"
                        variant="contained"
                        fullWidth
                        disabled={loading}
                    >
                        {loading ? <CircularProgress size={24} /> : "Save Changes"}
                    </Button>
                    <Button
                        type="button" // Not a submit button
                        variant="outlined" // Gives it a "secondary" look
                        fullWidth
                        onClick={() => navigate('/')}
                    >
                        Cancel
                    </Button>
                </Stack>
                </Grid>
              </Grid>
            </Box>
          </Grid>
        </Grid>
      </Paper>
    </Container>
  );
};

export default ProfilePage;