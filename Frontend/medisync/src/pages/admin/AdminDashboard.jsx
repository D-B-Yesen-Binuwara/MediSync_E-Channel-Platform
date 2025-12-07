import React, { useState, useEffect } from 'react';
import { apiRequest, authHeaders } from '../../api';
import '../../styles/AdminDashboard.css';

const AdminDashboard = () => {
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchDashboardStats();
    }, []);

    const fetchDashboardStats = async () => {
        try {
            const data = await apiRequest('/api/admin/admindashboard/stats');
            setStats(data);
        } catch (error) {
            console.error('Error fetching dashboard stats:', error);
            setStats({
                totalDoctors: 0,
                totalPatients: 0,
                totalAppointments: 0,
                todayAppointments: 0,
                recentAppointments: []
            });
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div className="loading">Loading dashboard...</div>;

    return (
        <div className="admin-dashboard">
            <h1>Admin Dashboard</h1>
            
            <div className="stats-grid">
                <div className="stat-card">
                    <h3>Total Doctors</h3>
                    <p className="stat-number">{stats?.totalDoctors || 0}</p>
                </div>
                <div className="stat-card">
                    <h3>Total Patients</h3>
                    <p className="stat-number">{stats?.totalPatients || 0}</p>
                </div>
                <div className="stat-card">
                    <h3>Total Appointments</h3>
                    <p className="stat-number">{stats?.totalAppointments || 0}</p>
                </div>
                <div className="stat-card">
                    <h3>Today's Appointments</h3>
                    <p className="stat-number">{stats?.todayAppointments || 0}</p>
                </div>
            </div>

            <div className="recent-appointments">
                <h2>Recent Appointments</h2>
                <div className="appointments-table">
                    <table>
                        <thead>
                            <tr>
                                <th>Patient</th>
                                <th>Doctor</th>
                                <th>Schedule Date</th>
                                <th>Booking Date</th>
                                <th>Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            {stats?.recentAppointments?.length > 0 ? stats.recentAppointments.map(appointment => (
                                <tr key={appointment.appointmentId}>
                                    <td>{appointment.patientName}</td>
                                    <td>{appointment.doctorName}</td>
                                    <td>{new Date(appointment.scheduleDate).toLocaleDateString()}</td>
                                    <td>{new Date(appointment.bookingDate).toLocaleDateString()}</td>
                                    <td className={`status ${appointment.status?.toString().toLowerCase() || 'unknown'}`}>
                                        {appointment.status || 'Unknown'}
                                    </td>
                                </tr>
                            )) : (
                                <tr>
                                    <td colSpan="5" style={{textAlign: 'center', padding: '2rem'}}>
                                        No recent appointments found
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
};

export default AdminDashboard;