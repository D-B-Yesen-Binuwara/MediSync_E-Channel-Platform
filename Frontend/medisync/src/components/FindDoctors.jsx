import React, { useState, useEffect } from 'react';
import { Search, Filter } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import FavoriteButton from './FavoriteButton';
import { API_BASE } from '../api';


export default function FindDoctors({
  searchTerm,
  setSearchTerm,
  selectedSpecialization,
  setSelectedSpecialization,
  doctors,
  loading,
  displayMode,
  doctorsByName = [],
  doctorsBySpec = [],
  searchMessage = ''
}) {
  const navigate = useNavigate();
  const [showDropdown, setShowDropdown] = useState(false);
  const [appointmentDate, setAppointmentDate] = useState('');
  const [localSearchTerm, setLocalSearchTerm] = useState(searchTerm);
  const [localSpecialization, setLocalSpecialization] = useState(selectedSpecialization);
  const [specializations, setSpecializations] = useState([]);

  // Fetch specializations from backend
  useEffect(() => {
    async function fetchSpecs() {
      try {
        const res = await fetch(`${API_BASE}/api/specializations`);
        const data = await res.json();
        setSpecializations(['All Specializations', ...data]);
      } catch {
        setSpecializations(['All Specializations']);
      }
    }
    fetchSpecs();
  }, []);

  // Handler to trigger backend fetch with all filters
  const handleSearch = () => {
    setSearchTerm(localSearchTerm);
    setSelectedSpecialization(localSpecialization);
    // Optionally: pass appointmentDate to parent and use in backend fetch
  };

  // Filter doctors for dropdown (from backend data)
  const filteredDropdown = localSearchTerm
    ? doctors.filter((doctor) =>
        (doctor.fullName || doctor.name)?.toLowerCase().includes(localSearchTerm.toLowerCase())
      )
    : [];

  return (
    <div className="card">
      <div className="card-header">
        <h2 className="text-xl font-semibold text-gray-800">Find Doctors</h2>
      </div>
      
      <div className="card-body">
        {/* Search Filters */}
        <div className="flex gap-4 mb-6">
          <div style={{ position: 'relative', flex: '2' }}>
            <div className="flex items-center" style={{ padding: '10px 12px', border: '1px solid #ddd', borderRadius: '8px', backgroundColor: 'white', height: '44px' }}>
              <Search size={18} style={{ color: '#999', marginRight: '8px' }} />
              <input
                type="text"
                placeholder="Search doctors..."
                value={localSearchTerm}
                onChange={(e) => {
                  setLocalSearchTerm(e.target.value);
                  setShowDropdown(!!e.target.value);
                }}
                onBlur={() => setTimeout(() => setShowDropdown(false), 150)}
                onFocus={() => setShowDropdown(!!localSearchTerm)}
                autoComplete="off"
                style={{ border: 'none', outline: 'none', flex: 1, background: 'transparent' }}
              />
            </div>
            {showDropdown && filteredDropdown.length > 0 && (
              <ul style={{
                position: 'absolute',
                top: '100%',
                left: 0,
                right: 0,
                zIndex: 10,
                background: 'white',
                border: '1px solid #ddd',
                borderRadius: '8px',
                boxShadow: '0 4px 6px rgba(0,0,0,0.1)',
                maxHeight: '200px',
                overflowY: 'auto',
                marginTop: '2px'
              }}>
                {filteredDropdown.map((doctor) => (
                  <li
                    key={doctor.userId || doctor.id}
                    onMouseDown={() => {
                      setLocalSearchTerm(doctor.fullName || doctor.name);
                      setShowDropdown(false);
                    }}
                    style={{
                      padding: '12px',
                      cursor: 'pointer',
                      borderBottom: '1px solid #f0f0f0'
                    }}
                  >
                    {doctor.fullName || doctor.name}
                  </li>
                ))}
              </ul>
            )}
          </div>
          
          <div className="flex items-center" style={{ padding: '10px 12px', border: '1px solid #ddd', borderRadius: '8px', backgroundColor: 'white', minWidth: '200px', height: '44px' }}>
            <Filter size={18} style={{ color: '#999', marginRight: '8px' }} />
            <select
              value={localSpecialization}
              onChange={(e) => setLocalSpecialization(e.target.value)}
              style={{ border: 'none', outline: 'none', flex: 1, background: 'transparent' }}
            >
              {specializations.map((spec) => (
                <option key={spec} value={spec}>{spec}</option>
              ))}
            </select>
          </div>
          
          <input
            type="date"
            value={appointmentDate}
            onChange={(e) => setAppointmentDate(e.target.value)}
            style={{ padding: '10px 12px', border: '1px solid #ddd', borderRadius: '8px', backgroundColor: 'white', minWidth: '180px', height: '44px' }}
          />
          
          <button className="btn btn-primary" onClick={handleSearch}>Search</button>
        </div>
        
        {/* Loading State */}
        {loading && (
          <div className="flex items-center justify-center py-8">
            <div className="loading-spinner"></div>
            <span className="ml-2 text-gray-600">Loading doctors...</span>
          </div>
        )}
        
        {/* Search Message */}
        {searchMessage && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-600 font-medium text-center">{searchMessage}</p>
          </div>
        )}
        
        {/* Doctors Grid */}
        {!loading && (
          <div className="grid grid-cols-3">
            {doctors.map((doctor) => {
              const name = doctor.fullName || doctor.name || 'Unknown Doctor';
              const specialization = doctor.specialization || 'General Practitioner';
              const image = doctor.profileImage || '/src/assets/Elogo.png';
              return (
                <div key={doctor.doctorId || doctor.id} className="card" style={{ height: '350px', display: 'flex', flexDirection: 'column', position: 'relative' }}>
                  <div style={{ position: 'absolute', top: '10px', right: '10px', zIndex: 1 }}>
                    <FavoriteButton
                      doctorId={doctor.doctorId || doctor.id}
                      initialIsFavorite={doctor.isFavorite || false}
                      onToggle={(isFav, msg) => {
                        console.log(msg);
                      }}
                    />
                  </div>
                  
                  <div className="card-body" style={{ flex: 1, display: 'flex', flexDirection: 'column', textAlign: 'center' }}>
                    <div style={{
                      width: '120px',
                      height: '120px',
                      borderRadius: '50%',
                      overflow: 'hidden',
                      margin: '0 auto 15px',
                      backgroundColor: '#f5f5f5'
                    }}>
                      <img 
                        src={image} 
                        alt={name} 
                        style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                        onError={(e) => { e.currentTarget.src = '/src/assets/Elogo.png' }} 
                      />
                    </div>
                    
                    <h3 style={{ fontSize: '1.2rem', lineHeight: '1.3', marginBottom: '8px', color: '#1976D2', fontWeight: '600' }}>{name}</h3>
                    <p style={{ color: '#666', marginBottom: '20px' }}>{specialization}</p>
                    
                    <button 
                      className="btn btn-primary"
                      onClick={() => navigate(`/book/${doctor.doctorId || doctor.id}`)}
                      style={{ width: '100%', marginTop: 'auto', background: 'linear-gradient(135deg, #1976D2 0%, #0D47A1 100%)', border: 'none' }}
                    >
                      Book Now
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}
        
        {/* No Results */}
        {!loading && doctors.length === 0 && (
          <div className="text-center py-8 text-gray-500">
            No doctors found.
          </div>
        )}
      </div>
    </div>
  );
}
