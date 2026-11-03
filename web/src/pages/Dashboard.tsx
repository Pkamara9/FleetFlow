import { Bar, BarChart, CartesianGrid, Cell, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { api } from '../api';
import { fmtMoney, State, useLoad } from '../components/ui';

const COLORS = ['#2563eb', '#f59e0b', '#ef4444', '#10b981', '#8b5cf6'];

export default function Dashboard() {
  const { data, error, loading } = useLoad(api.dashboard);
  return (
    <>
      <h2>Dashboard</h2>
      <State loading={loading} error={error}>
        {data && (
          <>
            <div className="kpis">
              <div className="card kpi"><span>Vehicles</span><b>{data.totalVehicles}</b></div>
              <div className="card kpi"><span>Open work orders</span><b>{data.openWorkOrders}</b></div>
              <div className="card kpi"><span>Due in 30 days</span><b>{data.dueIn30Days}</b></div>
              <div className="card kpi warn"><span>Overdue</span><b>{data.overdueMaintenance}</b></div>
              <div className="card kpi"><span>Cost (30 days)</span><b>{fmtMoney(data.monthlyMaintenanceCost)}</b></div>
            </div>
            <div className="grid2">
              <div className="card">
                <h3>Maintenance cost by month</h3>
                <ResponsiveContainer width="100%" height={240}>
                  <BarChart data={data.maintenanceCostChart}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="name" />
                    <YAxis />
                    <Tooltip formatter={(v) => fmtMoney(Number(v))} />
                    <Bar dataKey="value" fill="#2563eb" />
                  </BarChart>
                </ResponsiveContainer>
              </div>
              <div className="card">
                <h3>Work orders by status</h3>
                <ResponsiveContainer width="100%" height={240}>
                  <PieChart>
                    <Pie data={data.workOrderStatusChart} dataKey="value" nameKey="name" label>
                      {data.workOrderStatusChart.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            </div>
          </>
        )}
      </State>
    </>
  );
}
