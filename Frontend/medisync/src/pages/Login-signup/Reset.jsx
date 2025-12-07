"use client"

import { useState, useEffect } from "react"
import { Link, useSearchParams } from "react-router-dom"
import { apiRequest } from "../../api"

export default function Reset() {
  const [searchParams] = useSearchParams()
  const [token, setToken] = useState("")
  const [password, setPassword] = useState("")
  const [message, setMessage] = useState("")
  const [error, setError] = useState("")
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    const urlToken = searchParams.get('token')
    if (urlToken) {
      setToken(urlToken)
    }
  }, [searchParams])

  async function submit(e) {
    e.preventDefault()
    setMessage("")
    setError("")
    setLoading(true)

    try {
      const res = await apiRequest("/api/Auth/reset-password", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ token, newPassword: password }),
      })
      setMessage(res.message || "Password reset successful! You can now sign in with your new password.")
    } catch (e) {
      setError(e.message || "Password reset failed. Please try again.")
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-container">
      <div className="auth-left">
        <div className="card">
          <div className="auth-header">
            <h2>Reset Password</h2>
            <p>Enter your reset token and new password</p>
          </div>

          <form onSubmit={submit} className="form">
            <div className="form-group">
              <label>Reset Token</label>
              <input
                value={token}
                onChange={(e) => setToken(e.target.value)}
                placeholder="Enter the reset token from your email"
                required
              />
            </div>

            <div className="form-group">
              <label>New Password</label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Enter your new password"
                required
                minLength={6}
              />
            </div>

            {message && <div className="success">✅ {message}</div>}
            {error && <div className="error">⚠️ {error}</div>}

            <button className="btn primary" disabled={loading}>
              {loading ? "Resetting..." : "Reset Password"}
            </button>
          </form>

          <div className="auth-footer">
            <p>
              <Link to="/login">Back to Sign In</Link>
            </p>
          </div>
        </div>
      </div>

      <div className="auth-right">
        <div className="auth-right-content">
          <h3>Almost There</h3>
          <p>Create a new secure password to regain access to your account.</p>
        </div>
      </div>
    </div>
  )
}

