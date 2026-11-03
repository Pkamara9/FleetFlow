import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { api, canManage, getSession } from '../api';
import { Badge, fmtDate, fmtMoney, State, useLoad } from '../components/ui';
import { VehicleForm } from './Vehicles';

export default function VehicleDetail() {
  const id = Number(useParams().id);
  const nav = useNavigate();
  const [editing, setEditing] = useState(false);
  const manage = canManage(getSession()?.user.role);
  const { data, error, loading, reload } = useLoad(async () => {
    const [vehicle, maintenance, records, orders] = await Promise.all([
      api.vehicle(id), api.maintenance(), api.serviceRecords(id), api.workOrders(id),
    ]);
    return { vehicle, schedules: maintenance.filter((m) => m.vehicleId === id), records, orders };
  }, [id]);

  async function remove() {
    if (!confirm('Delete this vehicle?')) return;
    await api.deleteVehicle(id);
    nav('/vehicles');
  }

  return (
    <State loading={loading} error={error}>
      {data && (
        <>
          <div className="head">
            <h2>{data.vehicle.vehicleNumber} — {data.vehicle.year} {data.vehicle.make} {data.vehicle.model}</h2>
            {manage && (
              <div>
                <button onClick={() => setEditing(true)}>Edit</button>
                <button className="danger" onClick={remove}>Delete</button>
              </div>
            )}
          </div>
          {editing && (
            <VehicleForm id={id} initial={data.vehicle} onCancel={() => setEditing(false)}
              onSaved={() => { setEditing(false); reload(); }} />
          )}
          <div className="card info">
            <div><span>Status</span><Badge value={data.vehicle.status} /></div>
            <div><span>VIN</span>{data.vehicle.vin}</div>
            <div><span>Plate</span>{data.vehicle.licensePlate || '—'}</div>
            <div><span>Mileage</span>{data.vehicle.currentMileage.toLocaleString()}</div>
            <div><span>Driver</span>{data.vehicle.assignedDriver || '—'}</div>
          </div>

          <h3>Maintenance schedule</h3>
          <table>
            <thead><tr><th>Service</th><th>Last</th><th>Due date</th><th>Due mileage</th><th>Status</th></tr></thead>
            <tbody>
              {data.schedules.map((s) => (
                <tr key={s.scheduleId}>
                  <td>{s.serviceType}</td><td>{fmtDate(s.lastServiceDate)}</td><td>{fmtDate(s.dueDate)}</td>
                  <td>{s.dueMileage?.toLocaleString() ?? '—'}</td><td><Badge value={s.status} /></td>
                </tr>
              ))}
              {data.schedules.length === 0 && <tr><td colSpan={5} className="muted">No schedules.</td></tr>}
            </tbody>
          </table>

          <h3>Service history</h3>
          <table>
            <thead><tr><th>Date</th><th>Service</th><th>Mileage</th><th>Cost</th><th>Notes</th></tr></thead>
            <tbody>
              {data.records.map((r) => (
                <tr key={r.serviceRecordId}>
                  <td>{fmtDate(r.serviceDate)}</td><td>{r.serviceType}</td><td>{r.mileage.toLocaleString()}</td>
                  <td>{fmtMoney(r.cost)}</td><td>{r.notes}</td>
                </tr>
              ))}
              {data.records.length === 0 && <tr><td colSpan={5} className="muted">No service records.</td></tr>}
            </tbody>
          </table>

          <h3>Work orders</h3>
          <table>
            <thead><tr><th>Title</th><th>Priority</th><th>Status</th><th>Created</th></tr></thead>
            <tbody>
              {data.orders.map((w) => (
                <tr key={w.workOrderId}>
                  <td>{w.title}</td><td><Badge value={w.priority} /></td><td><Badge value={w.status} /></td><td>{fmtDate(w.createdDate)}</td>
                </tr>
              ))}
              {data.orders.length === 0 && <tr><td colSpan={4} className="muted">No work orders.</td></tr>}
            </tbody>
          </table>
          <p><Link to="/vehicles">← Back to vehicles</Link></p>
        </>
      )}
    </State>
  );
}
