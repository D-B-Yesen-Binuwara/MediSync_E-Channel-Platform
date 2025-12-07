import React from 'react';
import '../styles/LoadingSpinner.css';

export default function LoadingSpinner({ size = 'md' }) {
  return (
    <div className={`spinner spinner-${size}`} />
  );
}
