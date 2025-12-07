import React, { useState, useEffect } from 'react';
import { API_BASE } from '../api';
import { ArrowLeft, Calendar, Clock, MapPin, User, DollarSign } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import '../styles/AppointmentsDone.css';

export default function AppointmentsDone() {
  const navigate = useNavigate();
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [sortOrder, setSortOrder] = useState('desc'); // 'asc' or 'desc'

  useEffect(() => {
    fetchAppointments();
  }, []);

  const fetchAppointments = async () => {
    try {
      const token = localStorage.getItem('token');
      if (!token) {
        navigate('/login');
        return;
      }

      const response = await fetch(`${API_BASE}/api/booking/user`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setAppointments(data);
      } else {
        console.error('Failed to fetch appointments');
      }
    } catch (error) {
      console.error('Error fetching appointments:', error);
    } finally {
      setLoading(false);
    }
  };

  const sortAppointments = () => {
    const newOrder = sortOrder === 'desc' ? 'asc' : 'desc';
    setSortOrder(newOrder);
    
    const sorted = [...appointments].sort((a, b) => {
      const dateA = new Date(a.date);
      const dateB = new Date(b.date);
      return newOrder === 'desc' ? dateB - dateA : dateA - dateB;
    });
    
    setAppointments(sorted);
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  const formatTime = (timeString) => {
    const time = new Date(`2000-01-01T${timeString}`);
    return time.toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });
  };

  if (loading) {
    return (
      <div className="appointments-page">
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Loading appointments...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="appointments-page">
      <div className="appointments-header">
        <button onClick={() => navigate(-1)} className="btn-back">
          <ArrowLeft size={16} /> Back
        </button>
        <h1>My Appointments</h1>
        <button onClick={sortAppointments} className="btn-sort">
          Sort by Date ({sortOrder === 'desc' ? 'Newest First' : 'Oldest First'})
        </button>
      </div>

      <div className="appointments-container">
        {appointments.length === 0 ? (
          <div className="no-appointments">
            <Calendar size={48} />
            <h3>No Appointments Found</h3>
            <p>You haven't made any appointments yet.</p>
            <button onClick={() => navigate('/patient')} className="btn-primary">
              Book an Appointment
            </button>
          </div>
        ) : (
          <div className="appointments-grid">
            {appointments.map((appointment) => (
              <div key={appointment.appointmentId} className="appointment-card">
                <div className="appointment-header">
                  <div className="doctor-info">
                    <h3>{appointment.doctor}</h3>
                    <p className="specialization">{appointment.specialization}</p>
                  </div>
                  <div className="appointment-status">
                    <span className={`status-badge ${appointment.status === 0 ? 'booked' : 'cancelled'}`}>
                      {appointment.status === 0 ? 'Booked' : 'Cancelled'}
                    </span>
                  </div>
                </div>
                
                <div className="appointment-details">
                  <div className="detail-row">
                    <Calendar size={16} />
                    <span>{formatDate(appointment.date)}</span>
                  </div>
                  <div className="detail-row">
                    <Clock size={16} />
                    <span>{formatTime(appointment.time)}</span>
                  </div>
                  <div className="detail-row">
                    <MapPin size={16} />
                    <span>Ward {appointment.ward} - Slot {appointment.slot}</span>
                  </div>
                  <div className="detail-row">
                    <User size={16} />
                    <span>Patient ID: {appointment.appointmentId}</span>
                  </div>
                  <div className="detail-row price">
                    <DollarSign size={16} />
                    <span>Rs. {appointment.price.toFixed(2)}</span>
                  </div>
                </div>

                <div className="appointment-footer">
                  <small>Payment ID: {appointment.paymentId}</small>
                  <small>Booked on: {formatDate(appointment.paymentDate)}</small>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}