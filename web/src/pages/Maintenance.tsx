import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { api, canManage, getSession } from '../api';
import { Badge, fmtDate, State, useLoad } from '../components/ui';

const FILTERS = ['All', 'Overdue', 'Due Soon', 'Current'];

export default function Maintenance() {
  const { data, error, loading, reload } = useLoad(async () => {
    const [items, vehicles, types] = await Promise.all([api.maintenance(), api.vehicles(), api.serviceTypes()]);
    return { items, vehicles, types };
  });
  const [filter, setFilter] = useState('All');
  const [adding, setAdding] = useState(false);
  const [form, setForm] = useState({ vehicleId: 0, serviceTypeId: 0, lastServiceMileage: 0, lastServiceDate: '', nextDueMileage: '', nextDueDate: '' });
  const [formError, setFormError] = useState('');
  const manage = canManage(getSession()?.user.role);

  const rows = (data?.items ?? [])
    .filter((m) => filter === 'All' || m.status === filter)
    .sort((a, b) => (a.dueDate ?? '9').localeCompare(b.dueDate ?? '9'));

  async function submit(e: FormEvent) {
    e.preventDefault();
    try {
      await api.addSchedule({
        vehicleId: form.vehicleId,
        serviceTypeId: form.serviceTypeId,
        lastServiceMileage: form.lastServiceMileage,
        lastServiceDate: form.lastServiceDate || null,
        nextDueMileage: form.nextDueMileage ? +form.nextDueMileage : null,
        nextDueDate: form.nextDueDate || null,
      });
      setAdding(false);
      reload();
    } catch (err) {
      setFormError((err as Error).message);
    }
  }

  return (
    <>
      <div className="head">
        <h2>Maintenance schedules</h2>
        <div>
          <select value={filter} onChange={(e) => setFilter(e.target.value)}>
            {FILTERS.map((f) => <option key={f}>{f}</option>)}
          </select>
          {manage && <button className="primary" onClick={() => setAdding(true)}>Add schedule</button>}
        </div>
      </div>
      {adding && data && (
        <form className="card form" onSubmit={submit}>
          {formError && <div className="alert">{formError}</div>}
          <label>Vehicle
            <select required value={form.vehicleId} onChange={(e) => setForm({ ...form, vehicleId: +e.target.value })}>
              <option value="">Select…</option>
              {data.vehicles.map((v) => <option key={v.vehicleId} value={v.vehicleId}>{v.vehicleNumber}</option>)}
            </select>
          </label>
          <label>Service type
            <select required value={form.serviceTypeId} onChange={(e) => setForm({ ...form, serviceTypeId: +e.target.value })}>
              <option value="">Select…</option>
              {data.types.map((t) => <option key={t.serviceTypeId} value={t.serviceTypeId}>{t.name}</option>)}
            </select>
          </label>
          <label>Last service mileage<input type="number" min={0} value={form.lastServiceMileage} onChange={(e) => setForm({ ...form, lastServiceMileage: +e.target.value })} /></label>
          <label>Last service date<input type="date" value={form.lastServiceDate} onChange={(e) => setForm({ ...form, lastServiceDate: e.target.value })} /></label>
          <label>Next due mileage<input type="number" min={0} value={form.nextDueMileage} onChange={(e) => setForm({ ...form, nextDueMileage: e.target.value })} /></label>
          <label>Next due date<input type="date" value={form.nextDueDate} onChange={(e) => setForm({ ...form, nextDueDate: e.target.value })} /></label>
          <div className="actions"><button className="primary">Save</button><button type="button" onClick={() => setAdding(false)}>Cancel</button></div>
        </form>
      )}
      <State loading={loading} error={error}>
        <table>
          <thead><tr><th>Vehicle</th><th>Service</th><th>Last service</th><th>Due date</th><th>Mileage / due</th><th>Status</th></tr></thead>
          <tbody>
            {rows.map((m) => (
              <tr key={m.scheduleId}>
                <td><Link to={`/vehicles/${m.vehicleId}`}>{m.vehicleNumber}</Link></td>
                <td>{m.serviceType}</td>
                <td>{fmtDate(m.lastServiceDate)}</td>
                <td>{fmtDate(m.dueDate)}</td>
                <td>{m.currentMileage.toLocaleString()} / {m.dueMileage?.toLocaleString() ?? '—'}</td>
                <td><Badge value={m.status} /></td>
              </tr>
            ))}
            {rows.length === 0 && <tr><td colSpan={6} className="muted">Nothing here.</td></tr>}
          </tbody>
        </table>
      </State>
    </>
  );
}
