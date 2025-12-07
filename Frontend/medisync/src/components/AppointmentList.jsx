import React from 'react';
import { Calendar, Clock, MapPin } from 'lucide-react';
import '../styles/AppointmentList.css';

export default function AppointmentsList({ appointments, cancelAppointment }) {
  const getStatusClass = (status) => {
    switch (status) {
      case 'scheduled': return 'status scheduled';
      case 'completed': return 'status completed';
      case 'cancelled': return 'status cancelled';
      default: return 'status';
    }
  };

  return (
    <div className="appointments-list">
      <h2 className="appointments-title">Upcoming Appointments</h2>
      {appointments.map((apt) => (
        <div key={apt.id} className="appointment-card">
          <div>
            <h3 className="appointment-doctor">{apt.doctor ? (apt.doctor.name || apt.doctor.fullName) : 'Unknown Doctor'}</h3>
            <p className="appointment-specialization">{apt.doctor ? apt.doctor.specialization : ''}</p>
            <div className="appointment-details">
              <span><Calendar size={16} /> {new Date(apt.date).toLocaleDateString()}</span>
              <span><Clock size={16} /> {apt.time}</span>
              <span><MapPin size={16} /> Ward {apt.doctor ? apt.doctor.wardRoom : ''}</span>
            </div>
          </div>
          <div className="appointment-actions">
            <span className={getStatusClass(apt.status)}>
              {apt.status.charAt(0).toUpperCase() + apt.status.slice(1)}
            </span>
            <button onClick={() => cancelAppointment(apt.id)} className="appointment-cancel-btn">
              Cancel
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}
