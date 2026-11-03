import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { api, canManage, getSession, type Vehicle, type VehicleInput } from '../api';
import { Badge, State, useLoad } from '../components/ui';

const STATUSES = ['Active', 'In Service', 'Maintenance', 'Retired'];
const blank: VehicleInput = {
  vehicleNumber: '', vin: '', year: new Date().getFullYear(), make: '', model: '',
  licensePlate: '', currentMileage: 0, status: 'Active', assignedDriver: '',
};

export function VehicleForm({ initial, onSaved, onCancel, id }: {
  initial: VehicleInput; id?: number; onSaved: () => void; onCancel: () => void;
}) {
  const [v, setV] = useState(initial);
  const [error, setError] = useState('');
  const set = (k: keyof VehicleInput, val: string | number) => setV({ ...v, [k]: val });

  async function submit(e: FormEvent) {
    e.preventDefault();
    try {
      await api.saveVehicle(v, id);
      onSaved();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  return (
    <form className="card form" onSubmit={submit}>
      <h3>{id ? 'Edit vehicle' : 'New vehicle'}</h3>
      {error && <div className="alert">{error}</div>}
      <label>Vehicle #<input value={v.vehicleNumber} onChange={(e) => set('vehicleNumber', e.target.value)} required /></label>
      <label>VIN<input value={v.vin} onChange={(e) => set('vin', e.target.value)} required /></label>
      <label>Year<input type="number" value={v.year} onChange={(e) => set('year', +e.target.value)} required /></label>
      <label>Make<input value={v.make} onChange={(e) => set('make', e.target.value)} required /></label>
      <label>Model<input value={v.model} onChange={(e) => set('model', e.target.value)} required /></label>
      <label>Plate<input value={v.licensePlate ?? ''} onChange={(e) => set('licensePlate', e.target.value)} /></label>
      <label>Mileage<input type="number" min={0} value={v.currentMileage} onChange={(e) => set('currentMileage', +e.target.value)} /></label>
      <label>Status
        <select value={v.status} onChange={(e) => set('status', e.target.value)}>
          {STATUSES.map((s) => <option key={s}>{s}</option>)}
        </select>
      </label>
      <label>Driver<input value={v.assignedDriver ?? ''} onChange={(e) => set('assignedDriver', e.target.value)} /></label>
      <div className="actions">
        <button className="primary">Save</button>
        <button type="button" onClick={onCancel}>Cancel</button>
      </div>
    </form>
  );
}

export default function Vehicles() {
  const { data, error, loading, reload } = useLoad(api.vehicles);
  const [q, setQ] = useState('');
  const [adding, setAdding] = useState(false);
  const manage = canManage(getSession()?.user.role);

  const rows = (data ?? []).filter((v: Vehicle) =>
    `${v.vehicleNumber} ${v.make} ${v.model} ${v.licensePlate ?? ''} ${v.assignedDriver ?? ''}`.toLowerCase().includes(q.toLowerCase()));

  return (
    <>
      <div className="head">
        <h2>Vehicles</h2>
        <div>
          <input placeholder="Search…" value={q} onChange={(e) => setQ(e.target.value)} />
          {manage && <button className="primary" onClick={() => setAdding(true)}>Add vehicle</button>}
        </div>
      </div>
      {adding && <VehicleForm initial={blank} onCancel={() => setAdding(false)} onSaved={() => { setAdding(false); reload(); }} />}
      <State loading={loading} error={error}>
        <table>
          <thead><tr><th>#</th><th>Vehicle</th><th>Plate</th><th>Mileage</th><th>Driver</th><th>Status</th></tr></thead>
          <tbody>
            {rows.map((v) => (
              <tr key={v.vehicleId}>
                <td><Link to={`/vehicles/${v.vehicleId}`}>{v.vehicleNumber}</Link></td>
                <td>{v.year} {v.make} {v.model}</td>
                <td>{v.licensePlate}</td>
                <td>{v.currentMileage.toLocaleString()}</td>
                <td>{v.assignedDriver}</td>
                <td><Badge value={v.status} /></td>
              </tr>
            ))}
            {rows.length === 0 && <tr><td colSpan={6} className="muted">No vehicles found.</td></tr>}
          </tbody>
        </table>
      </State>
    </>
  );
}
