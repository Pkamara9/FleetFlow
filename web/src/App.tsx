import { useEffect, useState } from 'react';
import { BrowserRouter, Navigate, NavLink, Outlet, Route, Routes } from 'react-router-dom';
import { getSession, onSessionChange, setSession } from './api';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Vehicles from './pages/Vehicles';
import VehicleDetail from './pages/VehicleDetail';
import Maintenance from './pages/Maintenance';
import ServiceRecords from './pages/ServiceRecords';
import WorkOrders from './pages/WorkOrders';
import Reports from './pages/Reports';

function useSession() {
  const [s, setS] = useState(getSession());
  useEffect(() => onSessionChange(() => setS(getSession())), []);
  return s;
}

function Layout() {
  const session = useSession();
  if (!session) return <Navigate to="/login" replace />;
  return (
    <div className="shell">
      <aside>
        <h1>FleetFlow</h1>
        <nav>
          <NavLink to="/" end>Dashboard</NavLink>
          <NavLink to="/vehicles">Vehicles</NavLink>
          <NavLink to="/maintenance">Maintenance</NavLink>
          <NavLink to="/service-records">Service records</NavLink>
          <NavLink to="/work-orders">Work orders</NavLink>
          <NavLink to="/reports">Reports</NavLink>
        </nav>
        <div className="user">
          <div>{session.user.firstName} {session.user.lastName}</div>
          <small>{session.user.role} · {session.user.companyName}</small>
          <button onClick={() => setSession(null)}>Sign out</button>
        </div>
      </aside>
      <main><Outlet /></main>
    </div>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route element={<Layout />}>
          <Route index element={<Dashboard />} />
          <Route path="vehicles" element={<Vehicles />} />
          <Route path="vehicles/:id" element={<VehicleDetail />} />
          <Route path="maintenance" element={<Maintenance />} />
          <Route path="service-records" element={<ServiceRecords />} />
          <Route path="work-orders" element={<WorkOrders />} />
          <Route path="reports" element={<Reports />} />
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
