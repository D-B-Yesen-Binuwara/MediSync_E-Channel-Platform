import React from 'react';
import { Calendar, Clock, Star, ArrowRight } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import '../styles/QuickStats.css';

export default function QuickStats() {
  const navigate = useNavigate();

  return (
    <div className="quick-stats-grid">
      <div className="quick-stat-card clickable" onClick={() => navigate('/appointments')}>
        <div className="quick-stat-icon blue"><Calendar size={24} /></div>
        <div className="quick-stat-content">
          <p className="quick-stat-label">My Appointments</p>
          <p className="quick-stat-value">0</p>
        </div>
        <div className="quick-stat-arrow">
          <ArrowRight size={16} />
        </div>
      </div>

      <div className="quick-stat-card">
        <div className="quick-stat-icon green"><Clock size={24} /></div>
        <div>
          <p className="quick-stat-label">Total Appointments</p>
          <p className="quick-stat-value">0</p>
        </div>
      </div>

      <div className="quick-stat-card clickable" onClick={() => navigate('/favorites')}>
        <div className="quick-stat-icon purple"><Star size={24} /></div>
        <div className="quick-stat-content">
          <p className="quick-stat-label">Favorite Doctors</p>
        </div>
        <div className="quick-stat-arrow">
          <ArrowRight size={16} />
        </div>
      </div>
    </div>
  );
}