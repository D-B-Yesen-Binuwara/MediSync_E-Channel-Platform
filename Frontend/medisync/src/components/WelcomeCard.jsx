import React from 'react';
import '../styles/WelcomeCard.css';

export default function WelcomeCard({ name }) {
  return (
    <div className="welcome-card">
      <h1 className="welcome-title">Welcome back, {name}!</h1>
      <p className="welcome-subtitle">
        Find and book appointments with our qualified doctors
      </p>
    </div>
  );
}
