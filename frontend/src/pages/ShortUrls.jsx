import React, { useContext, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Box,
  Typography,
  Card,
  CardContent,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Alert,
  Chip,
  IconButton,
  Tooltip,
  Fab,
  Button,
} from "@mui/material";
import {
  Add,
  Refresh,
  Visibility,
  Edit,
  Delete,
  ContentCopy,
  OpenInNew,
} from "@mui/icons-material";
import api from "../api/axios";
import AddUrlForm from "../components/AddUrlForm";
import UrlRow from "../components/UrlRow";
import { AuthContext } from "../contexts/AuthContext";

export default function ShortUrls() {
  const navigate = useNavigate();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const { token, user } = useContext(AuthContext);

  const load = async () => {
    if (!token) {
      setLoading(false);
      return;
    }
    
    setLoading(true);
    setError(null);
    try {
      const res = await api.get("/api/urls");
      setItems(res.data);
    } catch (e) {
      console.error(e);
      setError("Failed to load URLs. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [token]);

  const onAdded = (newItem) => {
    setItems(prev => [newItem, ...prev]);
    setShowAddForm(false);
  };

  const onDeleted = (id) => {
    setItems(prev => prev.filter(x => x.id !== id));
  };

  const handleRefresh = () => {
    load();
  };

  const copyToClipboard = (text) => {
    navigator.clipboard.writeText(text);
  };

  return (
    <Box sx={{ py: 2 }}>
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" gutterBottom>
            Short URLs
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Manage your shortened URLs and track their performance
          </Typography>
        </Box>
        
        <Box sx={{ display: "flex", gap: 2 }}>
          <Tooltip title="Refresh">
            <IconButton onClick={handleRefresh} disabled={loading}>
              <Refresh />
            </IconButton>
          </Tooltip>
          
          {token && (
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => setShowAddForm(true)}
              sx={{ borderRadius: 2 }}
            >
              Add URL
            </Button>
          )}
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {token && showAddForm && (
        <Card sx={{ mb: 3, boxShadow: "0px 4px 20px rgba(0, 0, 0, 0.1)" }}>
          <CardContent>
            <AddUrlForm onAdded={onAdded} onCancel={() => setShowAddForm(false)} />
          </CardContent>
        </Card>
      )}

      {loading ? (
        <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
          <Typography variant="h6" color="text.secondary">
            Loading...
          </Typography>
        </Box>
      ) : items.length === 0 ? (
        <Card sx={{ textAlign: "center", py: 8, boxShadow: "0px 4px 20px rgba(0, 0, 0, 0.1)" }}>
          <CardContent>
            <Typography variant="h6" color="text.secondary" gutterBottom>
              {token ? "No URLs found" : "Authentication Required"}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              {token ? "Create your first short URL to get started." : "Please sign in to create and manage your short URLs."}
            </Typography>
            {!token && (
              <Button
                variant="contained"
                onClick={() => navigate("/login")}
                sx={{ mt: 2 }}
              >
                Sign In
              </Button>
            )}
          </CardContent>
        </Card>
      ) : (
        <TableContainer component={Paper} sx={{ boxShadow: "0px 4px 20px rgba(0, 0, 0, 0.1)" }}>
          <Table>
            <TableHead>
              <TableRow sx={{ backgroundColor: "primary.main" }}>
                <TableCell sx={{ color: "white", fontWeight: 600 }}>Short URL</TableCell>
                <TableCell sx={{ color: "white", fontWeight: 600 }}>Original URL</TableCell>
                <TableCell sx={{ color: "white", fontWeight: 600 }}>Created</TableCell>
                <TableCell sx={{ color: "white", fontWeight: 600 }}>Visits</TableCell>
                <TableCell sx={{ color: "white", fontWeight: 600 }}>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map(item => (
                <UrlRow key={item.id} item={item} onDeleted={onDeleted} />
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {token && !showAddForm && (
        <Fab
          color="primary"
          aria-label="add"
          onClick={() => setShowAddForm(true)}
          sx={{
            position: "fixed",
            bottom: 24,
            right: 24,
            boxShadow: "0px 4px 20px rgba(0, 0, 0, 0.2)",
          }}
        >
          <Add />
        </Fab>
      )}
    </Box>
  );
}
