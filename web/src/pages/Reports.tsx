import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { api } from '../api';
import { fmtMoney, State, useLoad } from '../components/ui';

export default function Reports() {
  const { data, error, loading } = useLoad(async () => {
    const [dash, orders, maintenance] = await Promise.all([api.dashboard(), api.workOrders(), api.maintenance()]);
    return { dash, orders, maintenance };
  });

  const costByVehicle = Object.values(
    (data?.orders ?? []).reduce<Record<string, { name: string; value: number }>>((acc, w) => {
      acc[w.vehicleNumber] ??= { name: w.vehicleNumber, value: 0 };
      acc[w.vehicleNumber].value += w.totalCost;
      return acc;
    }, {}),
  ).sort((a, b) => b.value - a.value);

  return (
    <>
      <h2>Reports</h2>
      <State loading={loading} error={error}>
        {data && (
          <>
            <div className="grid2">
              <div className="card">
                <h3>Maintenance cost (last 6 months)</h3>
                <ResponsiveContainer width="100%" height={240}>
                  <BarChart data={data.dash.maintenanceCostChart}>
                    <CartesianGrid strokeDasharray="3 3" /><XAxis dataKey="name" /><YAxis />
                    <Tooltip formatter={(v) => fmtMoney(Number(v))} />
                    <Bar dataKey="value" fill="#2563eb" />
                  </BarChart>
                </ResponsiveContainer>
              </div>
              <div className="card">
                <h3>Cost by vehicle (all work orders)</h3>
                <ResponsiveContainer width="100%" height={240}>
                  <BarChart data={costByVehicle}>
                    <CartesianGrid strokeDasharray="3 3" /><XAxis dataKey="name" /><YAxis />
                    <Tooltip formatter={(v) => fmtMoney(Number(v))} />
                    <Bar dataKey="value" fill="#10b981" />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>
            <h3>Fleet summary</h3>
            <table>
              <tbody>
                <tr><td>Vehicles</td><td>{data.dash.totalVehicles}</td></tr>
                <tr><td>Open work orders</td><td>{data.dash.openWorkOrders}</td></tr>
                <tr><td>Overdue services</td><td>{data.maintenance.filter((m) => m.status === 'Overdue').length}</td></tr>
                <tr><td>Due soon</td><td>{data.maintenance.filter((m) => m.status === 'Due Soon').length}</td></tr>
                <tr><td>Total work order cost</td><td>{fmtMoney(data.orders.reduce((s, w) => s + w.totalCost, 0))}</td></tr>
              </tbody>
            </table>
          </>
        )}
      </State>
    </>
  );
}
