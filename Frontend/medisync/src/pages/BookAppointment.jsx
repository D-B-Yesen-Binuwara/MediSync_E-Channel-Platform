import React, { useState, useEffect } from 'react';
import { API_BASE } from '../api';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Clock, Calendar, MapPin, Star, Check } from 'lucide-react';

import '../styles/BookAppointment.css';
import ClientBookingModal from '../components/ClientBookingModal';

export default function BookAppointment() {
  const { doctorId } = useParams();
  const navigate = useNavigate();
  
  // State
  const [doctor, setDoctor] = useState(null);
  const [availableSlots, setAvailableSlots] = useState([]);
  const [selectedDate, setSelectedDate] = useState('');
  const [selectedTime, setSelectedTime] = useState('');
  const [patientName, setPatientName] = useState('');
  const [patientNIC, setPatientNIC] = useState('');
  const [patientEmail, setPatientEmail] = useState('');
  const [patientContactNo, setPatientContactNo] = useState('');
  const [patientNotes, setPatientNotes] = useState('');
  const [isBooking, setIsBooking] = useState(false);
  const [bookingSuccess, setBookingSuccess] = useState(false);
  const [bookingSlotId, setBookingSlotId] = useState(null);
  const [showClientForm, setShowClientForm] = useState(false);
  const [selectedSlot, setSelectedSlot] = useState(null);
  const [loading, setLoading] = useState(true);


  // Load doctor data and time slots
  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      try {
        // Fetch doctor from API
        const response = await fetch(`${API_BASE}/api/doctors`);
        const doctors = await response.json();
        const foundDoctor = doctors.find(d => d.doctorId == doctorId);
        if (foundDoctor) {
          // Add missing fields for compatibility
          foundDoctor.consultationFee = foundDoctor.consultationFee || 2000;
          foundDoctor.profileImage = foundDoctor.profileImage || '/images/unnamed.png';
          foundDoctor.wardRoom = foundDoctor.wardRoom || 'A-101';
          foundDoctor.rating = foundDoctor.rating || 4.5;
          foundDoctor.reviews = foundDoctor.reviews || 25;
          setDoctor(foundDoctor);
        }
        
        // Fetch schedules from API
        const scheduleResponse = await fetch(`${API_BASE}/api/admin/AdminSchedules`);
        const allSchedules = await scheduleResponse.json();
        const twoWeeksAgo = new Date();
        twoWeeksAgo.setDate(twoWeeksAgo.getDate() - 14);
        
        const doctorSchedules = allSchedules
          .filter(s => s.doctorId === parseInt(doctorId))
          .filter(s => new Date(s.scheduleDate) >= twoWeeksAgo)
          .map(s => ({
            id: s.scheduleId,
            doctorId: s.doctorId,
            date: s.scheduleDate.split('T')[0],
            time: s.startTime + ' - ' + s.endTime,
            totalSlots: s.totalSlots,
            availableSlots: s.availableSlots,
            wardNo: 'A-101',
            price: 2500
          }));
        setAvailableSlots(doctorSchedules);
      } catch (error) {
        console.error('Failed to fetch data:', error);
        setAvailableSlots([]);
      }
      setLoading(false);
    };
    loadData();
  }, [doctorId]);

  // Get available slots for selected date
  const getAvailableSlotsForDoctor = () => {
    // Return all schedule entries for this doctor (we'll show availableSlots in the UI)
    return (availableSlots || []).filter(s => (s.doctorId == doctor.doctorId || s.doctorId == doctor.id));
  };

  // Handle booking confirmation
  const handleConfirmBooking = async (slot) => {
    // Accept a slot object {date?, time, id}
    if (!slot) return;
    const date = slot.date || slot.day || '';
    const time = slot.time || slot.start || '';
    // Prepare the client form flow
    setSelectedDate(date);
    setSelectedTime(time);
    setSelectedSlot(slot);
    setShowClientForm(true);
  };

  const handleCancelClientForm = () => {
    setShowClientForm(false);
    // clear client fields (optional)
    setPatientName('');
    setPatientNIC('');
    setPatientEmail('');
    setPatientContactNo('');
    setSelectedSlot(null);
  };

  const handlePayNow = async (patient) => {
    // patient: { name, nic, email, contact, payment: { details: paymentPayload } }
    if (!patient || !patient.name || !patient.nic || !patient.email || !patient.contact) {
      alert('Please fill in all required patient details');
      return;
    }

    if (!selectedSlot) {
      alert('No slot selected');
      return;
    }

    // persist patient values to parent state for success screen
    setPatientName(patient.name);
    setPatientNIC(patient.nic);
    setPatientEmail(patient.email);
    setPatientContactNo(patient.contact);

    setIsBooking(true);
    setBookingSlotId(selectedSlot.id);
    
    try {
      // Get JWT token from localStorage
      const token = localStorage.getItem('token');
      if (!token) {
        alert('Please login to book an appointment');
        return;
      }

      // Prepare booking request
      const bookingRequest = {
        scheduleId: selectedSlot.id,
        patientName: patient.name,
        nic: patient.nic,
        email: patient.email,
        contactNo: patient.contact,
        payment: {
          accountName: patient.payment?.details?.accName || patient.name,
          accountNumber: patient.payment?.details?.accNo || '1234567890',
          bankName: patient.payment?.details?.bankName || 'Default Bank',
          bankBranch: patient.payment?.details?.bankBranch || 'Main Branch',
          amount: selectedSlot.price || doctor.consultationFee || 2500
        }
      };

      // Call backend API
      const response = await fetch(`${API_BASE}/api/booking`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(bookingRequest)
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Booking failed');
      }

      const result = await response.json();
      console.log('Booking successful:', result);

      // on success decrement availableSlots for the selected slot
      setAvailableSlots(prev => prev.map(s => s.id === selectedSlot.id ? { ...s, availableSlots: Math.max(0, (s.availableSlots || 0) - 1) } : s));
      // clear client form state and hide modal
      setShowClientForm(false);
      setSelectedSlot(null);
      setBookingSuccess(true);
    } catch (err) {
      console.error('Payment/booking failed', err);
      alert(`Payment failed: ${err.message}`);
    } finally {
      setIsBooking(false);
      setBookingSlotId(null);
    }
  };

  if (loading) {
    return (
      <div className="booking-page">
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Loading doctor information...</p>
        </div>
      </div>
    );
  }

  if (!doctor) {
    return (
      <div className="booking-page">
        <div className="error-container">
          <h2>Doctor not found</h2>
          <p>The requested doctor could not be found.</p>
          <button onClick={() => navigate(-1)} className="btn-back">
            <ArrowLeft size={16} /> Go Back
          </button>
        </div>
      </div>
    );
  }

  if (bookingSuccess) {
    return (
      <div className="booking-page">
        <div className="success-container">
          <div className="success-icon">
            <Check size={48} />
          </div>
          <h2>Appointment Booked Successfully!</h2>
          <p>Your appointment has been confirmed.</p>
          <div className="booking-details">
            <p><strong>Doctor:</strong> {doctor.fullName}</p>
            <p><strong>Specialization:</strong> {doctor.specialization}</p>
            {/* show date and time separately using slot or selected values directly */}
            <p><strong>Date:</strong> {selectedSlot?.date || selectedSlot?.day || selectedDate}</p>
            <p><strong>Time:</strong> {selectedSlot?.time || selectedSlot?.start || selectedTime}</p>
            <p><strong>Ward:</strong> {selectedSlot?.wardNo ?? doctor.wardRoom}</p>
            <p><strong>Fee:</strong> Rs. {selectedSlot?.price ?? doctor.consultationFee}</p>
            <hr />
            <p><strong>Patient:</strong> {patientName}</p>
            <p><strong>NIC:</strong> {patientNIC}</p>
            <p><strong>Contact:</strong> {patientContactNo}</p>
            <p><strong>Email:</strong> {patientEmail}</p>
          </div>
          <div className="success-actions">
            <button onClick={() => navigate('/patient')} className="btn-primary">
              Back to Dashboard
            </button>
            <button onClick={() => navigate(-1)} className="btn-secondary">
              Book Another
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="booking-page">
      <div className="booking-container">
        {/* Header */}
        <div className="booking-header">
          <button onClick={() => navigate(-1)} className="btn-back">
            <ArrowLeft size={16} /> Back
          </button>
          <h1>Book Your Appointments Here</h1>
        </div>

        <div className="booking-content">
          {/* Left: Doctor profile panel (moved left) */}
          <aside className="doctor-profile">
            <div className="doctor-image">
              <img src={doctor.profileImage} alt={doctor.fullName} onError={(e) => { e.currentTarget.src = '/src/assets/Elogo.png'; }} />
            </div>
            <div className="doctor-info">
              <h2>{doctor.fullName}</h2>
              <div className="specialization">{doctor.specialization}</div>
              <div><button className="btn-secondary">Details</button></div>
            </div>
          </aside>

          {/* Right: schedule list and book buttons */}
          <div className="booking-form schedule-panel">
            <h3>Available Schedule</h3>
            <div className="schedule-list">
              {getAvailableSlotsForDoctor().length === 0 && (
                <div className="no-slots">No available slots.</div>
              )}
              {getAvailableSlotsForDoctor().map(slot => (
                <div key={slot.id} className="slot-card">
                  <div className="slot-meta">
                    <div className="slot-row">
                      <div className="slot-field">
                        <span className="slot-label">Date:</span>
                        <span className="slot-value">{slot?.date || slot?.day || ''}</span>
                      </div>
                      <div className="slot-field">
                        <span className="slot-label">Session time:</span>
                        <span className="slot-value">{slot?.time || slot?.start || ''}</span>
                      </div>
                    </div>

                    <div className="slot-row" style={{ marginTop: 6 }}>
                      <div className="slot-field">
                        <span className="slot-label">Available slot:</span>
                        <span className="slot-value"><strong>{slot.availableSlots ?? slot.totalSlots}</strong></span>
                      </div>
                      <div className="slot-field">
                        <span className="slot-label">Ward room:</span>
                        <span className="slot-value">{slot.wardNo}</span>
                      </div>
                    </div>
                  </div>
                  <div className="slot-actions">
                    <div className="slot-fee">Rs. {slot.price ?? doctor.consultationFee}</div>
                    <button
                      className="btn-primary"
                      disabled={(slot.availableSlots ?? 0) <= 0 || (isBooking && bookingSlotId === slot.id)}
                      onClick={() => handleConfirmBooking(slot)}
                    >
                      {isBooking && bookingSlotId === slot.id ? 'Processing...' : ((slot.availableSlots ?? 0) <= 0 ? 'Full' : 'Book Now')}
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Client booking modal popup */}
        {showClientForm && selectedSlot && (
          <ClientBookingModal
            doctor={doctor}
            slot={selectedSlot}
            onClose={() => { setShowClientForm(false); setSelectedSlot(null); }}
            onPay={handlePayNow}
            isProcessing={isBooking && bookingSlotId === selectedSlot.id}
          />
        )}
      </div>
    </div>
  );
}