import { Routes, Route } from 'react-router-dom';
import DashboardLayout from './layouts/DashboardLayout';

function App() {
  return (
    <Routes>
      <Route path="/" element={<DashboardLayout />}>
        <Route index element={
          <div style={{ padding: '2rem', fontFamily: 'sans-serif' }}>
            <p>Welcome to Wissler Admin.</p>
            <div style={{ marginTop: '1rem', padding: '1rem', border: '1px solid #ddd', borderRadius: '8px', display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))' }}>
              <div style={{ padding: '1rem', backgroundColor: '#fdf2f8', borderRadius: '8px' }}>
                <h3>New Matches</h3>
                <p style={{ fontSize: '2rem', fontWeight: 'bold', color: '#db2777' }}>450</p>
              </div>
              <div style={{ padding: '1rem', backgroundColor: '#fdf2f8', borderRadius: '8px' }}>
                <h3>Dates Planned</h3>
                <p style={{ fontSize: '2rem', fontWeight: 'bold', color: '#db2777' }}>89</p>
              </div>
            </div>
          </div>
        } />
        <Route path="users" element={<div>Profiles List (Coming Soon)</div>} />
        <Route path="matches" element={<div>Matches List (Coming Soon)</div>} />
      </Route>
    </Routes>
  );
}

export default App;
