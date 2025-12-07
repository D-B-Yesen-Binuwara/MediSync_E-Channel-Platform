import React, { useState, useEffect } from 'react';
import { apiRequest } from '../../api';
import '../../styles/AdminTransactions.css';

const AdminTransactions = () => {
    const [transactions, setTransactions] = useState([]);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState('');

    useEffect(() => {
        fetchTransactions();
    }, []);

    const fetchTransactions = async (searchTerm = '') => {
        try {
            const url = searchTerm ? `/api/admin/admintransactions?search=${encodeURIComponent(searchTerm)}` : '/api/admin/admintransactions';
            const data = await apiRequest(url);
            setTransactions(data || []);
        } catch (error) {
            console.error('Error fetching transactions:', error);
            setTransactions([]);
        } finally {
            setLoading(false);
        }
    };

    const handleSearch = (e) => {
        e.preventDefault();
        fetchTransactions(search);
    };

    const getStatusBadge = (status, statusValue) => {
        const badgeClass = statusValue === 1 ? 'badge-success' : statusValue === 0 ? 'badge-warning' : 'badge-error';
        return <span className={`badge ${badgeClass}`}>{status}</span>;
    };

    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('en-LK', {
            style: 'currency',
            currency: 'LKR'
        }).format(amount);
    };

    if (loading) return <div className="loading">Loading transactions...</div>;

    return (
        <div className="page-wrapper">
            <div className="content-wrapper">
                {/* Header Section */}
                <div className="flex items-center justify-between mb-6">
                    <h1 className="text-3xl font-bold text-gray-800">Manage Transactions</h1>
                    <div className="flex gap-4">
                        <div className="card p-4 text-center">
                            <div className="text-sm text-gray-500 font-medium">Total Transactions</div>
                            <div className="text-2xl font-bold" style={{ color: 'var(--primary-600)' }}>{transactions.length}</div>
                        </div>
                        <div className="card p-4 text-center">
                            <div className="text-sm text-gray-500 font-medium">Total Revenue</div>
                            <div className="text-2xl font-bold" style={{ color: 'var(--success-600)' }}>
                                {formatCurrency(transactions.reduce((sum, t) => sum + (t.amount || 0), 0))}
                            </div>
                        </div>
                    </div>
                </div>

                {/* Search Section */}
                <div className="card mb-6">
                    <div className="card-body">
                        <form onSubmit={handleSearch} style={{ display: 'flex', flexDirection: 'row', gap: '16px', alignItems: 'center', width: '100%' }}>
                            <input
                                type="text"
                                placeholder="Search by Patient Name, NIC, or Payment ID..."
                                value={search}
                                onChange={(e) => setSearch(e.target.value)}
                                style={{ 
                                    flex: 1, 
                                    padding: '0 16px', 
                                    fontSize: '16px',
                                    border: '1px solid #ddd', 
                                    borderRadius: '8px',
                                    outline: 'none',
                                    height: '48px',
                                    boxSizing: 'border-box'
                                }}
                            />
                            <button 
                                type="submit" 
                                style={{
                                    padding: '0 32px',
                                    fontSize: '16px',
                                    background: 'linear-gradient(135deg, #1976D2 0%, #0D47A1 100%)',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '8px',
                                    cursor: 'pointer',
                                    fontWeight: '500',
                                    minWidth: '120px',
                                    height: '48px',
                                    boxSizing: 'border-box'
                                }}
                            >
                                Search
                            </button>
                            <button 
                                type="button" 
                                onClick={() => { setSearch(''); fetchTransactions(); }}
                                style={{
                                    padding: '0 32px',
                                    fontSize: '16px',
                                    backgroundColor: '#78909C',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '8px',
                                    cursor: 'pointer',
                                    fontWeight: '500',
                                    minWidth: '120px',
                                    height: '48px',
                                    boxSizing: 'border-box'
                                }}
                            >
                                Clear
                            </button>
                        </form>
                    </div>
                </div>

                {/* Transactions Table */}
                <div className="card">
                    <div style={{ overflowX: 'auto' }}>
                        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                            <thead>
                                <tr style={{ backgroundColor: 'var(--gray-50)', borderBottom: '1px solid var(--gray-200)' }}>
                                    <th style={{ padding: 'var(--space-3)', textAlign: 'left', fontWeight: 600, color: 'var(--gray-700)' }}>ID</th>
                                    <th style={{ padding: 'var(--space-3)', textAlign: 'left', fontWeight: 600, color: 'var(--gray-700)' }}>Patient Name</th>
                                    <th style={{ padding: 'var(--space-3)', textAlign: 'left', fontWeight: 600, color: 'var(--gray-700)' }}>Appointment</th>
                                    <th style={{ padding: 'var(--space-3)', textAlign: 'left', fontWeight: 600, color: 'var(--gray-700)' }}>Amount</th>
                                    <th style={{ padding: 'var(--space-3)', textAlign: 'left', fontWeight: 600, color: 'var(--gray-700)' }}>Payment Method</th>
                                    <th style={{ padding: 'var(--space-3)', textAlign: 'left', fontWeight: 600, color: 'var(--gray-700)' }}>Date</th>
                                    <th style={{ padding: 'var(--space-3)', textAlign: 'left', fontWeight: 600, color: 'var(--gray-700)' }}>Status</th>
                                    <th style={{ padding: 'var(--space-3)', textAlign: 'left', fontWeight: 600, color: 'var(--gray-700)' }}>Bank</th>
                                    <th style={{ padding: 'var(--space-3)', textAlign: 'left', fontWeight: 600, color: 'var(--gray-700)' }}>Email</th>
                                </tr>
                            </thead>
                            <tbody>
                                {transactions.length === 0 ? (
                                    <tr>
                                        <td colSpan="9" style={{ textAlign: 'center', padding: 'var(--space-8)', color: 'var(--gray-500)' }}>
                                            No transactions found
                                        </td>
                                    </tr>
                                ) : (
                                    transactions.map(transaction => (
                                        <tr key={transaction.transactionId} style={{ borderBottom: '1px solid var(--gray-200)' }}>
                                            <td style={{ padding: 'var(--space-3)' }}>{transaction.transactionId}</td>
                                            <td style={{ padding: 'var(--space-3)' }}>{transaction.patientName}</td>
                                            <td style={{ padding: 'var(--space-3)' }}>#{transaction.appointmentId}</td>
                                            <td style={{ padding: 'var(--space-3)', fontWeight: 600 }}>{formatCurrency(transaction.amount || 0)}</td>
                                            <td style={{ padding: 'var(--space-3)' }}>{transaction.paymentMethod || 'Bank Transfer'}</td>
                                            <td style={{ padding: 'var(--space-3)' }}>{new Date(transaction.paymentDate).toLocaleDateString()}</td>
                                            <td style={{ padding: 'var(--space-3)' }}>{getStatusBadge(transaction.status, transaction.statusValue)}</td>
                                            <td style={{ padding: 'var(--space-3)' }}>{transaction.bankName || 'N/A'}</td>
                                            <td style={{ padding: 'var(--space-3)' }}>{transaction.email}</td>
                                        </tr>
                                    ))
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AdminTransactions;