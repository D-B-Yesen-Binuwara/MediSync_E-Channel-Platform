import React, { useState, useEffect } from 'react';
import { Link, Outlet, useNavigate } from 'react-router-dom';
import { userAPI } from '../../api';
import '../../styles/AdminLayout.css';

const AdminLayout = () => {
    const navigate = useNavigate();
    const [userName, setUserName] = useState('Admin');

    useEffect(() => {
        async function fetchUserName() {
            try {
                const profile = await userAPI.getProfile();
                setUserName(profile.fullName || profile.name || 'Admin');
            } catch (err) {
                console.error('Error fetching user:', err);
            }
        }
        fetchUserName();
    }, []);

    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('userRole');
        navigate('/login');
    };

    return (
        <div className="admin-layout">
            <nav className="admin-sidebar">
                <div className="admin-logo">
                    <h2>Welcome {userName}</h2>
                </div>
                <ul className="admin-nav">
                    <li>
                        <Link to="/admin/dashboard">Dashboard</Link>
                    </li>
                    <li>
                        <Link to="/admin/doctors">Manage Doctors</Link>
                    </li>
                    <li>
                        <Link to="/admin/schedules">Manage Schedules</Link>
                    </li>
                    <li>
                        <Link to="/admin/transactions">Manage Transactions</Link>
                    </li>
                </ul>
                <div className="admin-logout">
                    <button onClick={handleLogout} className="logout-btn">
                        Logout
                    </button>
                </div>
            </nav>
            <main className="admin-content">
                <Outlet />
            </main>
        </div>
    );
};

export default AdminLayout;