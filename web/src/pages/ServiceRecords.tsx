import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api';
import { fmtDate, fmtMoney, State, useLoad } from '../components/ui';

export default function ServiceRecords() {
  const { data, error, loading, reload } = useLoad(async () => {
    const [records, vehicles, types] = await Promise.all([api.serviceRecords(), api.vehicles(), api.serviceTypes()]);
    return { records, vehicles, types };
  });
  const [adding, setAdding] = useState(false);
  const [f, setF] = useState({ vehicleId: '', serviceTypeId: '', serviceDate: new Date().toISOString().slice(0, 10), mileage: 0, cost: 0, notes: '' });
  const [formError, setFormError] = useState('');

  async function submit(e: FormEvent) {
    e.preventDefault();
    try {
      await api.addServiceRecord({
        vehicleId: +f.vehicleId, serviceTypeId: +f.serviceTypeId, serviceDate: f.serviceDate,
        mileage: f.mileage, cost: f.cost, notes: f.notes || null,
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
        <h2>Service records</h2>
        <button className="primary" onClick={() => setAdding(true)}>Log service</button>
      </div>
      {adding && data && (
        <form className="card form" onSubmit={submit}>
          {formError && <div className="alert">{formError}</div>}
          <label>Vehicle
            <select required value={f.vehicleId} onChange={(e) => setF({ ...f, vehicleId: e.target.value })}>
              <option value="">Select…</option>
              {data.vehicles.map((v) => <option key={v.vehicleId} value={v.vehicleId}>{v.vehicleNumber}</option>)}
            </select>
          </label>
          <label>Service type
            <select required value={f.serviceTypeId} onChange={(e) => setF({ ...f, serviceTypeId: e.target.value })}>
              <option value="">Select…</option>
              {data.types.map((t) => <option key={t.serviceTypeId} value={t.serviceTypeId}>{t.name}</option>)}
            </select>
          </label>
          <label>Date<input type="date" required value={f.serviceDate} onChange={(e) => setF({ ...f, serviceDate: e.target.value })} /></label>
          <label>Mileage<input type="number" min={0} value={f.mileage} onChange={(e) => setF({ ...f, mileage: +e.target.value })} /></label>
          <label>Cost<input type="number" min={0} step="0.01" value={f.cost} onChange={(e) => setF({ ...f, cost: +e.target.value })} /></label>
          <label>Notes<input value={f.notes} onChange={(e) => setF({ ...f, notes: e.target.value })} /></label>
          <div className="actions"><button className="primary">Save</button><button type="button" onClick={() => setAdding(false)}>Cancel</button></div>
        </form>
      )}
      <State loading={loading} error={error}>
        <table>
          <thead><tr><th>Date</th><th>Vehicle</th><th>Service</th><th>Mileage</th><th>Cost</th><th>Notes</th></tr></thead>
          <tbody>
            {data?.records.map((r) => (
              <tr key={r.serviceRecordId}>
                <td>{fmtDate(r.serviceDate)}</td>
                <td><Link to={`/vehicles/${r.vehicleId}`}>{r.vehicleNumber}</Link></td>
                <td>{r.serviceType}</td><td>{r.mileage.toLocaleString()}</td><td>{fmtMoney(r.cost)}</td><td>{r.notes}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </State>
    </>
  );
}
