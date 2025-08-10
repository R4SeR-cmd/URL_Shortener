import React, { useState } from "react";
import {
  Box,
  TextField,
  Button,
  Alert,
  InputAdornment,
  IconButton,
  Typography,
} from "@mui/material";
import { Link, Close } from "@mui/icons-material";
import api from "../api/axios";

export default function AddUrlForm({ onAdded, onCancel }) {
  const [url, setUrl] = useState("");
  const [err, setErr] = useState(null);
  const [loading, setLoading] = useState(false);

  const submit = async (e) => {
    e.preventDefault();
    setErr(null);
    setLoading(true);
    try {
      const res = await api.post("/api/urls", { originalUrl: url });
      onAdded(res.data);
      setUrl("");
    } catch (error) {
      console.error(error);
      setErr(error?.response?.data?.message || "Failed to create short URL. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    setUrl("");
    setErr(null);
    if (onCancel) {
      onCancel();
    }
  };

  return (
    <Box component="form" onSubmit={submit} sx={{ p: 2 }}>
      <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between", mb: 2 }}>
        <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
          <Link color="primary" />
          <Typography variant="h6">Add New URL</Typography>
        </Box>
        {onCancel && (
          <IconButton onClick={handleCancel} size="small">
            <Close />
          </IconButton>
        )}
      </Box>

      {err && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {err}
        </Alert>
      )}

      <Box sx={{ display: "flex", gap: 2, alignItems: "flex-start" }}>
        <TextField
          fullWidth
          label="Enter URL"
          placeholder="https://example.com/very-long-url"
          value={url}
          onChange={(e) => setUrl(e.target.value)}
          required
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Link color="action" />
              </InputAdornment>
            ),
          }}
          sx={{ flexGrow: 1 }}
        />
        
        <Button
          type="submit"
          variant="contained"
          disabled={loading || !url.trim()}
          sx={{ minWidth: 120, height: 56 }}
        >
          {loading ? "Creating..." : "Create"}
        </Button>
      </Box>
    </Box>
  );
}
