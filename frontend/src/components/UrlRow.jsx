import React, { useState } from "react";
import {
  TableRow,
  TableCell,
  IconButton,
  Tooltip,
  Chip,
  Link,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
} from "@mui/material";
import {
  Delete,
  ContentCopy,
  OpenInNew,
  Visibility,
  CalendarToday,
  Link as LinkIcon,
} from "@mui/icons-material";
import api, { API_BASE_URL } from "../api/axios";

export default function UrlRow({ item, onDeleted }) {
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const shortLink = `${API_BASE_URL}/api/urls/${item.shortCode}`;

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(shortLink);
    } catch (err) {
      console.error('Failed to copy: ', err);
    }
  };

  const handleDelete = async () => {
    setDeleting(true);
    try {
      await api.delete(`/api/urls/${item.id}`);
      onDeleted(item.id);
      setDeleteDialogOpen(false);
    } catch (e) {
      console.error(e);
      alert("Delete failed");
    } finally {
      setDeleting(false);
    }
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  return (
    <>
      <TableRow hover>
        <TableCell>
          <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
            <LinkIcon color="primary" fontSize="small" />
            <Link
              href={shortLink}
              target="_blank"
              rel="noreferrer"
              sx={{ 
                fontWeight: 500,
                textDecoration: "none",
                "&:hover": { textDecoration: "underline" }
              }}
            >
              {item.shortCode}
            </Link>
            <Tooltip title="Copy link">
              <IconButton size="small" onClick={handleCopy}>
                <ContentCopy fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
        </TableCell>
        
        <TableCell>
          <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
            <Link
              href={item.originalUrl}
              target="_blank"
              rel="noreferrer"
              sx={{ 
                maxWidth: 300,
                overflow: "hidden",
                textOverflow: "ellipsis",
                whiteSpace: "nowrap",
                textDecoration: "none",
                "&:hover": { textDecoration: "underline" }
              }}
            >
              {item.originalUrl}
            </Link>
            <Tooltip title="Open original URL">
              <IconButton size="small" href={item.originalUrl} target="_blank" rel="noreferrer">
                <OpenInNew fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
        </TableCell>
        
        <TableCell>
          <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
            <CalendarToday fontSize="small" color="action" />
            <Typography variant="body2">
              {formatDate(item.createdAtUtc)}
            </Typography>
          </Box>
        </TableCell>
        
        <TableCell>
          <Chip
            icon={<Visibility />}
            label={item.visitCount}
            size="small"
            color="primary"
            variant="outlined"
          />
        </TableCell>
        
        <TableCell>
          <Tooltip title="Delete URL">
            <IconButton
              size="small"
              color="error"
              onClick={() => setDeleteDialogOpen(true)}
            >
              <Delete fontSize="small" />
            </IconButton>
          </Tooltip>
        </TableCell>
      </TableRow>

      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Delete Short URL</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete the short URL <strong>{item.shortCode}</strong>?
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            This action cannot be undone. The short URL will no longer work.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleDelete}
            color="error"
            variant="contained"
            disabled={deleting}
          >
            {deleting ? "Deleting..." : "Delete"}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}