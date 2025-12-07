import React, { useState } from 'react';
import '../styles/PaymentGatewayForm.css';

export default function PaymentGatewayForm({ amount, onCancel, onCheckout }) {
  const [accName, setAccName] = useState('');
  const [accNo, setAccNo] = useState('');
  const [bankName, setBankName] = useState('');
  const [bankBranch, setBankBranch] = useState('');
  const [pin, setPin] = useState('');
  const [errors, setErrors] = useState({});
  const [isProcessing, setIsProcessing] = useState(false);

  const validate = () => {
    const e = {};
    if (!accName || accName.trim().length < 2) e.accName = 'Enter account name';
    if (!/^[0-9]+$/.test(accNo || '')) e.accNo = 'Account number must be digits only';
    // reasonable length check for account number: 6-24 digits
    if ((accNo || '').length < 6 || (accNo || '').length > 24) e.accNo = 'Account number length invalid';
  if (!bankName || bankName.trim().length < 2) e.bankName = 'Enter bank name';
  if (!bankBranch || bankBranch.trim().length < 1) e.bankBranch = 'Enter bank branch';
    if (!/^[0-9]{4}$/.test(pin || '')) e.pin = 'PIN must be 4 digits';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleCheckout = async () => {
    if (!validate()) return;
    setIsProcessing(true);
    try {
      if (onCheckout) {
        // pass minimal payment payload (do not include sensitive info in logs)
        await onCheckout({ accName, accNo, bankName, bankBranch, pin, amount });
      }
    } catch (err) {
      console.error('Checkout failed', err);
      // swallow; onCheckout should handle user-facing errors
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="payment-overlay">
      <div className="payment-box">
        <h4>Payment Details</h4>
        <div className="form-group"><label>Account Name</label><input className="form-input" value={accName} onChange={e=>setAccName(e.target.value)} /></div>
        <div className="form-group"><label>Account Number</label><input className="form-input" value={accNo} onChange={e=>setAccNo(e.target.value)} /></div>
        {errors.accNo && <div className="form-error">{errors.accNo}</div>}
        <div className="form-group"><label>Bank Name</label><input className="form-input" value={bankName} onChange={e=>setBankName(e.target.value)} /></div>
  <div className="form-group"><label>Bank Branch</label><input className="form-input" value={bankBranch} onChange={e=>setBankBranch(e.target.value)} /></div>
        <div className="form-group"><label>PIN</label><input className="form-input" value={pin} onChange={e=>setPin(e.target.value)} maxLength={4} /></div>
        {errors.pin && <div className="form-error">{errors.pin}</div>}

        <div className="payment-summary">
          <span className="label">Amount:</span>
          <span className="value">Rs. {amount}</span>
        </div>

        <div className="payment-actions">
          <button className="btn-secondary" onClick={onCancel} disabled={isProcessing}>Cancel</button>
          <button className="btn-primary" onClick={handleCheckout} disabled={isProcessing}>{isProcessing ? 'Processing...' : 'Checkout'}</button>
        </div>
      </div>
    </div>
  );
}
