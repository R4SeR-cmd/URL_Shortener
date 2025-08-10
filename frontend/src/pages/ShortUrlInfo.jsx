import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Chip,
  Divider,
  Grid,
  IconButton,
  Tooltip,
  Alert,
  Paper,
} from "@mui/material";
import {
  ArrowBack,
  Link,
  OpenInNew,
  ContentCopy,
  CalendarToday,
  Visibility,
  Share,
} from "@mui/icons-material";
import api, { API_BASE_URL } from "../api/axios";

export default function ShortUrlInfo() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [item, setItem] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        const res = await api.get(`/api/urls/${id}`);
        setItem(res.data);
      } catch (e) {
        console.error(e);
        setError("Failed to load URL information. Please try again.");
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [id]);

  const handleCopy = async (text) => {
    try {
      await navigator.clipboard.writeText(text);
    } catch (err) {
      console.error('Failed to copy: ', err);
    }
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  if (loading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
        <Typography variant="h6" color="text.secondary">
          Loading...
        </Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mt: 2 }}>
        {error}
      </Alert>
    );
  }

  if (!item) {
    return (
      <Alert severity="warning" sx={{ mt: 2 }}>
        URL not found
      </Alert>
    );
  }

  const shortLink = `${API_BASE_URL}/api/urls/${item.shortCode}`;

  return (
    <Box sx={{ py: 2 }}>
      <Box sx={{ display: "flex", alignItems: "center", mb: 3 }}>
        <IconButton onClick={() => navigate("/links")} sx={{ mr: 2 }}>
          <ArrowBack />
        </IconButton>
        <Typography variant="h4" component="h1">
          URL Details
        </Typography>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={8}>
          <Card sx={{ boxShadow: "0px 4px 20px rgba(0, 0, 0, 0.1)" }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h5" gutterBottom>
                Short URL Information
              </Typography>
              
              <Divider sx={{ my: 2 }} />
              
              <Box sx={{ mb: 3 }}>
                <Typography variant="h6" color="primary" gutterBottom>
                  Short URL
                </Typography>
                <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
                  <Link color="primary" />
                  <Typography variant="body1" sx={{ fontFamily: "monospace", fontWeight: 500 }}>
                    {item.shortCode}
                  </Typography>
                </Box>
                <Box sx={{ display: "flex", gap: 1 }}>
                  <Button
                    variant="outlined"
                    size="small"
                    startIcon={<ContentCopy />}
                    onClick={() => handleCopy(shortLink)}
                  >
                    Copy Link
                  </Button>
                  <Button
                    variant="outlined"
                    size="small"
                    startIcon={<OpenInNew />}
                    href={shortLink}
                    target="_blank"
                    rel="noreferrer"
                  >
                    Test Link
                  </Button>
                </Box>
              </Box>

              <Divider sx={{ my: 2 }} />

              <Box sx={{ mb: 3 }}>
                <Typography variant="h6" color="primary" gutterBottom>
                  Original URL
                </Typography>
                <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
                  <Typography
                    variant="body1"
                    sx={{
                      wordBreak: "break-all",
                      color: "text.secondary",
                    }}
                  >
                    {item.originalUrl}
                  </Typography>
                </Box>
                <Button
                  variant="outlined"
                  size="small"
                  startIcon={<OpenInNew />}
                  href={item.originalUrl}
                  target="_blank"
                  rel="noreferrer"
                >
                  Visit Original
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={4}>
          <Card sx={{ boxShadow: "0px 4px 20px rgba(0, 0, 0, 0.1)" }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Statistics
              </Typography>
              
              <Divider sx={{ my: 2 }} />
              
              <Box sx={{ mb: 2 }}>
                <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
                  <CalendarToday color="action" />
                  <Typography variant="body2" color="text.secondary">
                    Created
                  </Typography>
                </Box>
                <Typography variant="body1">
                  {formatDate(item.createdAtUtc)}
                </Typography>
              </Box>

              <Box sx={{ mb: 2 }}>
                <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
                  <Visibility color="action" />
                  <Typography variant="body2" color="text.secondary">
                    Total Visits
                  </Typography>
                </Box>
                <Chip
                  label={item.visitCount}
                  color="primary"
                  variant="outlined"
                  size="large"
                />
              </Box>

              <Divider sx={{ my: 2 }} />

              <Button
                fullWidth
                variant="contained"
                startIcon={<Share />}
                onClick={() => handleCopy(shortLink)}
                sx={{ mt: 2 }}
              >
                Share URL
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
