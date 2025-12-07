import React, { useState, useEffect } from 'react';
import { Heart } from 'lucide-react';
import { favoritesAPI } from '../api';

export default function FavoriteButton({ doctorId, initialIsFavorite = false, onToggle }) {
  const [isFavorite, setIsFavorite] = useState(initialIsFavorite);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const checkFavoriteStatus = async () => {
      const token = localStorage.getItem('token');
      if (!token) return;
      
      try {
        const result = await favoritesAPI.checkFavorite(doctorId);
        setIsFavorite(result.isFavorite);
      } catch (error) {
        console.error('Error checking favorite status:', error);
      }
    };
    
    checkFavoriteStatus();
  }, [doctorId]);

  const handleToggle = async (e) => {
    e.stopPropagation();
    e.preventDefault();
    
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Please login to add favorites');
      return;
    }

    setLoading(true);
    try {
      if (isFavorite) {
        await favoritesAPI.removeFavorite(doctorId);
        setIsFavorite(false);
        onToggle?.(false, 'Removed from favorites');
      } else {
        await favoritesAPI.addFavorite(doctorId);
        setIsFavorite(true);
        onToggle?.(true, 'Added to favorites');
      }
    } catch (error) {
      console.error('Error toggling favorite:', error);
      alert('Failed to update favorites. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <button
      onClick={handleToggle}
      disabled={loading}
      style={{
        background: 'none',
        border: 'none',
        cursor: loading ? 'not-allowed' : 'pointer',
        padding: '4px',
        borderRadius: '50%',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        transition: 'all 0.2s ease',
        opacity: loading ? 0.6 : 1
      }}
      title={isFavorite ? 'Remove from favorites' : 'Add to favorites'}
    >
      <Heart
        size={20}
        fill={isFavorite ? '#ef4444' : 'none'}
        color={isFavorite ? '#ef4444' : '#6b7280'}
        style={{ transition: 'all 0.2s ease' }}
      />
    </button>
  );
}