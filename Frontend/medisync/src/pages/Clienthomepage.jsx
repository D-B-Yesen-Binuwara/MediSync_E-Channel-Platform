import React, { useState, useMemo, useEffect } from 'react';
import FindDoctors from '../components/FindDoctors';
import { useNavigate } from 'react-router-dom';
import { API_BASE, userAPI, authHeaders } from '../api';

export default function PatientDashboard() {
  const navigate = useNavigate();
  // 🔹 State
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedSpecialization, setSelectedSpecialization] = useState('All Specializations');
  const [doctors, setDoctors] = useState([]);
  const [doctorsByName, setDoctorsByName] = useState([]);
  const [doctorsBySpec, setDoctorsBySpec] = useState([]);
  const [loadingDoctors, setLoadingDoctors] = useState(false);
  const [searchMessage, setSearchMessage] = useState('');
  const [displayMode, setDisplayMode] = useState('default');
  const [userName, setUserName] = useState('User');
  const [appointmentStats, setAppointmentStats] = useState({ upcoming: 3, total: 0, next: null });

  // Fetch user profile and appointments
  useEffect(() => {
    async function fetchUserData() {
      try {
        const profile = await userAPI.getProfile();
        setUserName(profile.fullName || profile.name || 'User');
        
        const res = await fetch(`${API_BASE}/api/Booking/user`, {
          headers: authHeaders()
        });
        const appointments = await res.json();
        
        const now = new Date();
        const upcoming = (appointments || []).filter(apt => new Date(apt.appointmentDate) >= now);
        const next = upcoming.sort((a, b) => new Date(a.appointmentDate) - new Date(b.appointmentDate))[0];
        
        setAppointmentStats({
          upcoming: upcoming.length,
          total: (appointments || []).length,
          next: next
        });
      } catch (err) {
        console.error('Error fetching user data:', err);
      }
    }
    fetchUserData();
  }, []);

  // Fetch doctors with custom rules
  React.useEffect(() => {
    async function fetchWithRules() {
      setLoadingDoctors(true);
      setSearchMessage('');
      setDisplayMode('default');
      setDoctorsByName([]);
      setDoctorsBySpec([]);

      const hasName = !!searchTerm.trim();
      const hasSpec = !!selectedSpecialization && selectedSpecialization !== 'All Specializations';

      try {
        if (hasName && hasSpec) {
          const [nameRes, specRes] = await Promise.all([
            fetch(`${API_BASE}/api/doctors?name=${encodeURIComponent(searchTerm)}`),
            fetch(`${API_BASE}/api/doctors?specialization=${encodeURIComponent(selectedSpecialization)}`)
          ]);
          const [nameData, specData] = await Promise.all([nameRes.json(), specRes.json()]);
          setDoctorsByName(nameData);
          setDoctorsBySpec(specData);

          // Rule 1: wrong name + correct specialization → show message + specialization doctors
          if ((!nameData || nameData.length === 0) && (specData && specData.length > 0)) {
            setSearchMessage('Sorry! We could not find results for your search query. You can try one of the below suggestions!');
            setDoctors(specData);
            setDisplayMode('specOnly');
          } else {
            // Rule 3: correct name + unmatched specialization → message + combine name + specialization (no split grids)
            const nameMatchesSelectedSpec = (nameData || []).some(d => (d.specialization || '').toLowerCase() === selectedSpecialization.toLowerCase());
            if (!nameMatchesSelectedSpec && nameData && nameData.length > 0) {
              setSearchMessage('Sorry! We could not find results for your search query. You can try one of the below suggestions!');
              const map = new Map();
              [...(nameData || []), ...(specData || [])].forEach(d => map.set(d.doctorId || d.id, d));
              setDoctors([...map.values()]);
              setDisplayMode('both');
            } else {
              // overlap or same spec → merge and show
              const map = new Map();
              [...(nameData || []), ...(specData || [])].forEach(d => map.set(d.doctorId || d.id, d));
              setDoctors([...map.values()]);
              setDisplayMode('default');
            }
          }
        } else if (hasName) {
          const res = await fetch(`${API_BASE}/api/doctors?name=${encodeURIComponent(searchTerm)}`);
          let data = await res.json();
          if (!data || data.length === 0) {
            const allRes = await fetch(`${API_BASE}/api/doctors`);
            const all = await allRes.json();
            const q = searchTerm.toLowerCase();
            data = (all || []).filter(d => (d.fullName || d.name || '').toLowerCase().includes(q));
          }
          // Rule 4: name only
          setDoctors(data || []);
          setDisplayMode('nameOnly');
        } else if (hasSpec) {
          const res = await fetch(`${API_BASE}/api/doctors?specialization=${encodeURIComponent(selectedSpecialization)}`);
          const data = await res.json();
          // Rule 2: just specialization
          setDoctorsBySpec(data || []);
          setDoctors(data || []);
          setDisplayMode('specOnly');
        } else {
          const res = await fetch(`${API_BASE}/api/doctors`);
          const data = await res.json();
          setDoctors(data || []);
          setDisplayMode('default');
        }
      } catch (err) {
        console.log('Search failed', err);
        setDoctors([]);
      } finally {
        setLoadingDoctors(false);
      }
    }
    fetchWithRules();
  }, [searchTerm, selectedSpecialization]);


  return (
    <div className="page-wrapper">
      <div className="container">
        <div className="content-wrapper">
        {/* Welcome Section */}
        <div className="card mb-6" style={{ background: 'linear-gradient(135deg, #1976D2 0%, #0D47A1 100%)' }}>
          <div className="card-body">
            <h1 className="text-3xl font-bold" style={{ color: 'white', margin: 0 }}>Welcome back, {userName}!</h1>
            <p className="text-lg" style={{ color: 'rgba(255,255,255,0.9)', margin: '0.5rem 0 0 0' }}>
              Find and book appointments with our qualified doctors
            </p>
          </div>
        </div>

        {/* Quick Stats */}
        <div className="grid grid-cols-3 mb-6">
          <div className="card cursor-pointer" onClick={() => navigate('/appointments')} style={{ transition: 'transform 0.2s' }}>
            <div className="card-body flex items-center gap-4">
              <div className="p-3" style={{ backgroundColor: 'var(--primary-100)', borderRadius: 'var(--radius-lg)', color: 'var(--primary-600)' }}>
                📅
              </div>
              <div>
                <div className="text-sm text-gray-500">My Appointments</div>
                <div className="text-xl font-bold">{appointmentStats.upcoming}</div>
              </div>
            </div>
          </div>
          
          <div className="card">
            <div className="card-body flex items-center gap-4">
              <div className="p-3" style={{ backgroundColor: 'var(--success-50)', borderRadius: 'var(--radius-lg)', color: 'var(--success-600)' }}>
                ⏰
              </div>
              <div>
                <div className="text-sm text-gray-500">Total Appointments</div>
                <div className="text-xl font-bold">{appointmentStats.total}</div>
              </div>
            </div>
          </div>
          
          <div className="card cursor-pointer" onClick={() => navigate('/favorites')} style={{ transition: 'transform 0.2s' }}>
            <div className="card-body flex items-center gap-4">
              <div className="p-3" style={{ backgroundColor: 'rgba(147, 51, 234, 0.1)', borderRadius: 'var(--radius-lg)', color: '#9333ea' }}>
                ⭐
              </div>
              <div>
                <div className="text-sm text-gray-500">Favorite Doctors</div>
                <div className="text-xl font-bold">View</div>
              </div>
            </div>
          </div>
        </div>

        {/* Next Appointment */}
        <div className="card mb-6">
          <div className="card-body flex items-center gap-4">
            <div className="p-3" style={{ backgroundColor: 'var(--primary-100)', borderRadius: 'var(--radius-lg)', color: 'var(--primary-600)' }}>
              📅
            </div>
            <div>
              <div className="text-sm text-gray-500">Next Appointment</div>
              <div className="text-lg font-medium">
                {appointmentStats.next 
                  ? `${appointmentStats.next.doctorName || 'Doctor'} - ${new Date(appointmentStats.next.appointmentDate).toLocaleDateString()}`
                  : 'No upcoming appointments'
                }
              </div>
            </div>
          </div>
        </div>

        {/* Find Doctors */}
        <FindDoctors
          searchTerm={searchTerm}
          setSearchTerm={setSearchTerm}
          selectedSpecialization={selectedSpecialization}
          setSelectedSpecialization={setSelectedSpecialization}
          doctors={doctors}
          loading={loadingDoctors}
          displayMode={displayMode}
          doctorsByName={doctorsByName}
          doctorsBySpec={doctorsBySpec}
          searchMessage={searchMessage}
        />
        </div>
      </div>
    </div>
  );
}
