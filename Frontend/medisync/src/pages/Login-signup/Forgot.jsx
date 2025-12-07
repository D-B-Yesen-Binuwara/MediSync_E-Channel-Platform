"use client";

import { useState } from "react";
import { Link } from "react-router-dom";
import { apiRequest } from "../../api";

export default function Forgot() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function submit(e) {
    e.preventDefault();
    setMessage("");
    setError("");
    setLoading(true);

    try {
      const res = await apiRequest("/api/Auth/forgot-password", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email }),
      });

      // Backend should return something like { message: "..."} on success
      if (res.message) {
        setMessage(res.message);
      } else {
        setMessage("If the email exists, a reset link has been sent to your inbox.");
      }
    } catch (err) {
      setError(err.message || "Something went wrong");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-container">
      <div className="auth-left">
        <div className="card">
          <div className="auth-header">
            <h2>Forgot Password</h2>
            <p>Enter your email to receive a password reset link</p>
          </div>

          <form onSubmit={submit} className="form">
            <div className="form-group">
              <label>Email Address</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="Enter your email address"
                required
              />
            </div>

            {message && <div className="success">✅ {message}</div>}
            {error && <div className="error">⚠️ {error}</div>}

            <button className="btn primary" disabled={loading}>
              {loading ? "Sending..." : "Send Reset Link"}
            </button>
          </form>

          <div className="auth-footer">
            <p>
              Remember your password? <Link to="/login">Sign in here</Link>
            </p>
          </div>
        </div>
      </div>

      <div className="auth-right">
        <div className="auth-right-content">
          <h3>Password Recovery</h3>
          <p>We'll help you get back into your account quickly and securely.</p>
        </div>
      </div>
    </div>
  );
}

