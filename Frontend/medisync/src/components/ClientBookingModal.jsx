import React, { useState } from 'react';
import { X } from 'lucide-react';
import '../styles/BookingModal.css';
import PaymentGatewayForm from './PaymentGatewayForm';

export default function ClientBookingModal({ doctor, slot, onClose, onPay, isProcessing }) {
  const [name, setName] = useState('');
  const [nic, setNic] = useState('');
  const [email, setEmail] = useState('');
  const [contact, setContact] = useState('');
  const [errors, setErrors] = useState({});

  // Use date and time directly from the slot. Many schedule sources provide
  // date and time separately (e.g. { date: '2025-10-10', time: '10:00 AM' }).
  // Relying on these fields preserves any AM/PM text the backend or mock provides.
  const dateDisplay = slot?.date || slot?.day || '';
  const timeDisplay = slot?.time || slot?.start || '';

  const validateForm = () => {
    const newErrors = {};
    
    // Name validation - only letters and spaces
    if (!name.trim()) {
      newErrors.name = 'Full name is required';
    } else if (!/^[a-zA-Z\s]+$/.test(name.trim())) {
      newErrors.name = 'Name should contain only letters and spaces';
    }
    
    // NIC validation - exactly 12 digits
    if (!nic.trim()) {
      newErrors.nic = 'NIC is required';
    } else if (!/^\d{12}$/.test(nic.trim())) {
      newErrors.nic = 'NIC should be exactly 12 digits';
    }
    
    // Email validation
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!email.trim()) {
      newErrors.email = 'Email is required';
    } else if (!emailPattern.test(email.trim())) {
      newErrors.email = 'Please enter a valid email address';
    }
    
    // Contact validation - only numbers
    if (!contact.trim()) {
      newErrors.contact = 'Contact number is required';
    } else if (!/^\d+$/.test(contact.trim())) {
      newErrors.contact = 'Contact number should contain only numbers';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handlePay = async () => {
    if (!validateForm()) {
      return;
    }
    // open payment popup instead of directly calling onPay
    setShowPayment(true);
  };

  const [showPayment, setShowPayment] = React.useState(false);

  const handleCheckout = async (paymentPayload) => {
    // close payment overlay
    setShowPayment(false);
    // call parent onPay with combined patient info and simulated payment result
    if (onPay) {
      try {
        // include patient fields and a minimal payment receipt
        await onPay({ name, nic, email, contact, payment: { method: 'bank-transfer', details: paymentPayload } });
      } catch (err) {
        console.error('onPay failed', err);
      }
    }
  };

  return (
    <div className="modal-overlay">
  <div className={`modal-box modal-wide ${showPayment ? 'payment-active' : ''}`}>
        <div className="modal-header">
          <h3>Confirm & Pay</h3>
          <button className="modal-close" onClick={onClose}><X size={18} /></button>
        </div>

        <div className="modal-body modal-grid">
          <div>
            <h4>Patient Details</h4>
            <div className="form-group">
              <label>Full Name *</label>
              <input 
                className={`form-input ${errors.name ? 'error' : ''}`} 
                value={name} 
                onChange={e=>setName(e.target.value)}
                placeholder="Enter full name"
              />
              {errors.name && <div className="form-error">{errors.name}</div>}
            </div>
            <div className="form-group">
              <label>NIC *</label>
              <input 
                className={`form-input ${errors.nic ? 'error' : ''}`} 
                value={nic} 
                onChange={e=>setNic(e.target.value)}
                placeholder="12 digit NIC number"
                maxLength="12"
              />
              {errors.nic && <div className="form-error">{errors.nic}</div>}
            </div>
            <div className="form-group">
              <label>Email *</label>
              <input 
                className={`form-input ${errors.email ? 'error' : ''}`} 
                type="email"
                value={email} 
                onChange={e=>setEmail(e.target.value)}
                placeholder="example@email.com"
              />
              {errors.email && <div className="form-error">{errors.email}</div>}
            </div>
            <div className="form-group">
              <label>Contact No *</label>
              <input 
                className={`form-input ${errors.contact ? 'error' : ''}`} 
                value={contact} 
                onChange={e=>setContact(e.target.value)}
                placeholder="Contact number"
              />
              {errors.contact && <div className="form-error">{errors.contact}</div>}
            </div>
          </div>

          <div>
            <h4>Booking Details</h4>
            <div className="summary-item"><span>Doctor:</span><span>{doctor.fullName}</span></div>
            <div className="summary-item"><span>Specialization:</span><span>{doctor.specialization}</span></div>
            <div className="summary-item"><span>Price:</span><span>Rs. {slot.price ?? doctor.consultationFee}</span></div>
            <div className="summary-item"><span>Date:</span><span>{dateDisplay}</span></div>
            <div className="summary-item"><span>Time:</span><span>{timeDisplay}</span></div>
            <div className="summary-item"><span>Ward:</span><span>{slot.wardNo}</span></div>
            <div style={{marginTop:12}}>
              <button className="btn-secondary" onClick={onClose} disabled={isProcessing}>Cancel</button>
              <button className="btn-primary" onClick={handlePay} disabled={isProcessing} style={{marginLeft:8}}>{isProcessing ? 'Processing...' : 'Pay Now'}</button>
            </div>
          </div>
        </div>
        {showPayment && (
          <PaymentGatewayForm
            amount={slot.price ?? doctor.consultationFee}
            onCancel={() => setShowPayment(false)}
            onCheckout={handleCheckout}
          />
        )}
      </div>
    </div>
  );
}
