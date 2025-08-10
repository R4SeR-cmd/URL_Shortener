import React, { useState, useContext } from "react";
import { Routes, Route, useNavigate } from "react-router-dom";
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  Container,
  Box,
  IconButton,
  Menu,
  MenuItem,
  Chip,
} from "@mui/material";
import {
  Link as LinkIcon,
  AccountCircle,
  Logout,
  Home,
  Add,
} from "@mui/icons-material";
import { ThemeProvider } from "@mui/material/styles";
import { theme } from "./theme";
import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";

import Login from "./pages/Login";
import Register from "./pages/Register";
import ShortUrls from "./pages/ShortUrls";
import ShortUrlInfo from "./pages/ShortUrlInfo";
import ProtectedRoute from "./components/ProtectedRoute.jsx";
import { AuthContext } from "./contexts/AuthContext";
import "./App.css";

function App() {
  const navigate = useNavigate();
  const { token, logout } = useContext(AuthContext);
  const [anchorEl, setAnchorEl] = useState(null);

  const handleLogout = () => {
    logout();
    setAnchorEl(null);
    navigate("/login");
  };

  const handleMenu = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleNavigation = (path) => {
    navigate(path);
    handleClose();
  };

  return (
    <ThemeProvider theme={theme}>
      <Box sx={{ flexGrow: 1, minHeight: '100vh', backgroundColor: 'background.default' }}>
        <AppBar position="static" elevation={0}>
          <Toolbar>
            <IconButton
              size="large"
              edge="start"
              color="inherit"
              aria-label="menu"
              sx={{ mr: 2 }}
              onClick={() => navigate("/")}
            >
              <LinkIcon />
            </IconButton>
            <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
              URL Shortener
            </Typography>
            
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Button
                color="inherit"
                startIcon={<Home />}
                onClick={() => navigate("/")}
              >
                Home
              </Button>
              
              <Button
                color="inherit"
                startIcon={<LinkIcon />}
                onClick={() => navigate("/links")}
              >
                Short URLs
              </Button>

              {!token ? (
                <>
                  <Button
                    color="inherit"
                    variant="outlined"
                    onClick={() => navigate("/login")}
                    sx={{ 
                      borderColor: 'rgba(255, 255, 255, 0.5)',
                      color: 'white',
                      '&:hover': {
                        borderColor: 'white',
                        backgroundColor: 'rgba(255, 255, 255, 0.1)'
                      }
                    }}
                  >
                    Login
                  </Button>
                  <Button
                    color="inherit"
                    variant="contained"
                    onClick={() => navigate("/register")}
                    sx={{ 
                      backgroundColor: 'rgba(255, 255, 255, 0.2)',
                      '&:hover': {
                        backgroundColor: 'rgba(255, 255, 255, 0.3)'
                      }
                    }}
                  >
                    Register
                  </Button>
                </>
              ) : (
                <>
                  <Chip
                    label="Authenticated"
                    color="success"
                    size="small"
                    variant="filled"
                  />
                  <IconButton
                    size="large"
                    aria-label="account of current user"
                    aria-controls="menu-appbar"
                    aria-haspopup="true"
                    onClick={handleMenu}
                    color="inherit"
                  >
                    <AccountCircle />
                  </IconButton>
                  <Menu
                    id="menu-appbar"
                    anchorEl={anchorEl}
                    anchorOrigin={{
                      vertical: 'top',
                      horizontal: 'right',
                    }}
                    keepMounted
                    transformOrigin={{
                      vertical: 'top',
                      horizontal: 'right',
                    }}
                    open={Boolean(anchorEl)}
                    onClose={handleClose}
                  >
                    <MenuItem onClick={() => handleNavigation("/links")}>
                      <LinkIcon sx={{ mr: 1 }} />
                      My URLs
                    </MenuItem>
                    <MenuItem onClick={handleLogout}>
                      <Logout sx={{ mr: 1 }} />
                      Logout
                    </MenuItem>
                  </Menu>
                </>
              )}
            </Box>
          </Toolbar>
        </AppBar>

        <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
          <Routes>
            <Route 
              path="/" 
              element={
                <Box sx={{ textAlign: 'center', py: 8 }}>
                  <Typography variant="h2" component="h1" gutterBottom>
                    Welcome to URL Shortener
                  </Typography>
                  <Typography variant="h5" color="text.secondary" paragraph>
                    Create short, memorable links for your long URLs
                  </Typography>
                  <Button
                    variant="contained"
                    size="large"
                    startIcon={<Add />}
                    onClick={() => navigate(token ? "/links" : "/login")}
                    sx={{ mt: 3 }}
                  >
                    {token ? "Get Started" : "Sign In to Get Started"}
                  </Button>
                </Box>
              } 
            />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/links" element={<ShortUrls />} />
            <Route
              path="/links/:id"
              element={
                <ProtectedRoute>
                  <ShortUrlInfo />
                </ProtectedRoute>
              }
            />
          </Routes>
        </Container>
      </Box>
    </ThemeProvider>
  );
}

export default App;
