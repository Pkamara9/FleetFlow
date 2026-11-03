import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { api, canManage, getSession } from '../api';
import { Badge, fmtDate, fmtMoney, State, useLoad } from '../components/ui';

export const WO_STATUSES = ['Open', 'In Progress', 'Waiting Parts', 'Completed'];
const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'];

export default function WorkOrders() {
  const { data, error, loading, reload } = useLoad(async () => {
    const [orders, vehicles, users] = await Promise.all([api.workOrders(), api.vehicles(), api.users()]);
    return { orders, vehicles, users };
  });
  const [status, setStatus] = useState('All');
  const [adding, setAdding] = useState(false);
  const [f, setF] = useState({ vehicleId: '', title: '', description: '', priority: 'Medium', assignedUserId: '' });
  const [formError, setFormError] = useState('');
  const [actionError, setActionError] = useState('');
  const me = getSession()?.user;
  const manage = canManage(me?.role);
  const userName = (id?: number) => {
    const u = data?.users.find((x) => x.userId === id);
    return u ? `${u.firstName} ${u.lastName}` : 'Unassigned';
  };

  async function create(e: FormEvent) {
    e.preventDefault();
    try {
      await api.createWorkOrder({
        vehicleId: +f.vehicleId, title: f.title, description: f.description || null, priority: f.priority,
        assignedUserId: f.assignedUserId ? +f.assignedUserId : null, status: 'Open',
      });
      setAdding(false);
      setF({ vehicleId: '', title: '', description: '', priority: 'Medium', assignedUserId: '' });
      reload();
    } catch (err) {
      setFormError((err as Error).message);
    }
  }

  async function change(id: number, newStatus: string, assignee?: number | null, reassign = false) {
    try {
      setActionError('');
      await api.setWorkOrderStatus(id, newStatus, assignee, reassign);
      reload();
    } catch (err) {
      setActionError((err as Error).message);
    }
  }

  const rows = (data?.orders ?? []).filter((w) => status === 'All' || w.status === status);

  return (
    <>
      <div className="head">
        <h2>Work orders</h2>
        <div>
          <select value={status} onChange={(e) => setStatus(e.target.value)}>
            {['All', ...WO_STATUSES].map((s) => <option key={s}>{s}</option>)}
          </select>
          {manage && <button className="primary" onClick={() => setAdding(true)}>New work order</button>}
        </div>
      </div>
      {actionError && <div className="alert">{actionError}</div>}
      {adding && data && (
        <form className="card form" onSubmit={create}>
          {formError && <div className="alert">{formError}</div>}
          <label>Vehicle
            <select required value={f.vehicleId} onChange={(e) => setF({ ...f, vehicleId: e.target.value })}>
              <option value="">Select…</option>
              {data.vehicles.map((v) => <option key={v.vehicleId} value={v.vehicleId}>{v.vehicleNumber}</option>)}
            </select>
          </label>
          <label>Title<input required value={f.title} onChange={(e) => setF({ ...f, title: e.target.value })} /></label>
          <label>Description<input value={f.description} onChange={(e) => setF({ ...f, description: e.target.value })} /></label>
          <label>Priority
            <select value={f.priority} onChange={(e) => setF({ ...f, priority: e.target.value })}>
              {PRIORITIES.map((p) => <option key={p}>{p}</option>)}
            </select>
          </label>
          <label>Assign to
            <select value={f.assignedUserId} onChange={(e) => setF({ ...f, assignedUserId: e.target.value })}>
              <option value="">Unassigned</option>
              {data.users.map((u) => <option key={u.userId} value={u.userId}>{u.firstName} {u.lastName} ({u.role})</option>)}
            </select>
          </label>
          <div className="actions"><button className="primary">Create</button><button type="button" onClick={() => setAdding(false)}>Cancel</button></div>
        </form>
      )}
      <State loading={loading} error={error}>
        <table>
          <thead><tr><th>Title</th><th>Vehicle</th><th>Priority</th><th>Assignee</th><th>Status</th><th>Cost</th><th>Created</th></tr></thead>
          <tbody>
            {rows.map((w) => (
              <tr key={w.workOrderId}>
                <td>{w.title}</td>
                <td><Link to={`/vehicles/${w.vehicleId}`}>{w.vehicleNumber}</Link></td>
                <td><Badge value={w.priority} /></td>
                <td>
                  {manage ? (
                    <select value={w.assignedUserId ?? ''} aria-label="Assignee"
                      onChange={(e) => change(w.workOrderId, w.status, e.target.value ? +e.target.value : null, true)}>
                      <option value="">Unassigned</option>
                      {data?.users.map((u) => <option key={u.userId} value={u.userId}>{u.firstName} {u.lastName}</option>)}
                    </select>
                  ) : userName(w.assignedUserId)}
                </td>
                <td>
                  <select value={w.status} aria-label="Status" onChange={(e) => change(w.workOrderId, e.target.value)}>
                    {WO_STATUSES.map((s) => <option key={s}>{s}</option>)}
                  </select>
                </td>
                <td>{fmtMoney(w.totalCost)}</td>
                <td>{fmtDate(w.createdDate)}</td>
              </tr>
            ))}
            {rows.length === 0 && <tr><td colSpan={7} className="muted">No work orders.</td></tr>}
          </tbody>
        </table>
      </State>
    </>
  );
}
