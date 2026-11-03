const BASE = (import.meta.env.VITE_API_URL as string | undefined) ?? '';
const STORAGE_KEY = 'fleetflow.session';

export interface User {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  companyName: string;
}
export interface Session {
  accessToken: string;
  refreshToken: string;
  user: User;
}
export interface Vehicle {
  vehicleId: number;
  vehicleNumber: string;
  vin: string;
  year: number;
  make: string;
  model: string;
  licensePlate?: string;
  currentMileage: number;
  status: string;
  assignedDriver?: string;
}
export type VehicleInput = Omit<Vehicle, 'vehicleId'>;
export interface MaintenanceItem {
  scheduleId: number;
  vehicleId: number;
  vehicleNumber: string;
  serviceType: string;
  lastServiceDate?: string;
  dueDate?: string;
  currentMileage: number;
  dueMileage?: number;
  status: 'Current' | 'Due Soon' | 'Overdue';
}
export interface ServiceType {
  serviceTypeId: number;
  name: string;
  intervalMiles?: number;
  intervalDays?: number;
}
export interface ServiceRecord {
  serviceRecordId: number;
  vehicleId: number;
  vehicleNumber: string;
  serviceTypeId: number;
  serviceType: string;
  serviceDate: string;
  mileage: number;
  cost: number;
  notes?: string;
}
export interface WorkOrder {
  workOrderId: number;
  vehicleId: number;
  assignedUserId?: number;
  vehicleNumber: string;
  title: string;
  description?: string;
  priority: string;
  status: string;
  totalCost: number;
  createdDate: string;
  completedDate?: string;
}
export interface Assignee {
  userId: number;
  firstName: string;
  lastName: string;
  role: string;
}
export interface ChartValue {
  name: string;
  value: number;
}
export interface Dashboard {
  totalVehicles: number;
  openWorkOrders: number;
  dueIn30Days: number;
  overdueMaintenance: number;
  monthlyMaintenanceCost: number;
  vehicleStatusChart: ChartValue[];
  maintenanceCostChart: ChartValue[];
  workOrderStatusChart: ChartValue[];
}

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}

let session: Session | null = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? 'null');
const listeners = new Set<() => void>();

export const getSession = () => session;
export function setSession(s: Session | null) {
  session = s;
  if (s) localStorage.setItem(STORAGE_KEY, JSON.stringify(s));
  else localStorage.removeItem(STORAGE_KEY);
  listeners.forEach((l) => l());
}
export function onSessionChange(fn: () => void) {
  listeners.add(fn);
  return () => {
    listeners.delete(fn);
  };
}

async function raw(path: string, init: RequestInit = {}, token?: string) {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };
  if (token) headers.Authorization = `Bearer ${token}`;
  return fetch(BASE + path, { ...init, headers: { ...headers, ...(init.headers as object) } });
}

let refreshing: Promise<boolean> | null = null;
function refresh(): Promise<boolean> {
  if (!session) return Promise.resolve(false);
  refreshing ??= (async () => {
    try {
      const res = await raw('/api/auth/refresh', {
        method: 'POST',
        body: JSON.stringify({ accessToken: session!.accessToken, refreshToken: session!.refreshToken }),
      });
      if (!res.ok) return false;
      setSession(await res.json());
      return true;
    } catch {
      return false;
    } finally {
      refreshing = null;
    }
  })();
  return refreshing;
}

async function parseError(res: Response) {
  let message = res.statusText;
  try {
    message = (await res.json()).message ?? message;
  } catch {
    /* no body */
  }
  return new ApiError(res.status, message);
}

export async function request<T>(path: string, method = 'GET', body?: unknown): Promise<T> {
  const init: RequestInit = { method, body: body === undefined ? undefined : JSON.stringify(body) };
  let res = await raw(path, init, session?.accessToken);
  if (res.status === 401 && session && (await refresh())) {
    res = await raw(path, init, session?.accessToken);
  }
  if (res.status === 401 && session) setSession(null);
  if (!res.ok) throw await parseError(res);
  return res.status === 204 ? (undefined as T) : res.json();
}

export async function login(email: string, password: string) {
  const res = await raw('/api/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) });
  if (!res.ok) throw await parseError(res);
  setSession(await res.json());
}

export const api = {
  dashboard: () => request<Dashboard>('/api/reports/dashboard'),
  vehicles: () => request<Vehicle[]>('/api/vehicles'),
  vehicle: (id: number) => request<Vehicle>(`/api/vehicles/${id}`),
  saveVehicle: (v: VehicleInput, id?: number) =>
    id ? request<Vehicle>(`/api/vehicles/${id}`, 'PUT', v) : request<Vehicle>('/api/vehicles', 'POST', v),
  deleteVehicle: (id: number) => request<void>(`/api/vehicles/${id}`, 'DELETE'),
  maintenance: () => request<MaintenanceItem[]>('/api/maintenance'),
  addSchedule: (b: unknown) => request<MaintenanceItem>('/api/maintenance', 'POST', b),
  serviceTypes: () => request<ServiceType[]>('/api/servicetypes'),
  addServiceType: (b: unknown) => request<ServiceType>('/api/servicetypes', 'POST', b),
  serviceRecords: (vehicleId?: number) =>
    request<ServiceRecord[]>('/api/servicerecords' + (vehicleId ? `?vehicleId=${vehicleId}` : '')),
  addServiceRecord: (b: unknown) => request<unknown>('/api/servicerecords', 'POST', b),
  workOrders: (vehicleId?: number) =>
    request<WorkOrder[]>('/api/workorders' + (vehicleId ? `?vehicleId=${vehicleId}` : '')),
  createWorkOrder: (b: unknown) => request<WorkOrder>('/api/workorders', 'POST', b),
  setWorkOrderStatus: (id: number, status: string, assignedUserId?: number | null, reassign = false) =>
    request<WorkOrder>(`/api/workorders/${id}/status`, 'PATCH', { status, assignedUserId, reassign }),
  users: () => request<Assignee[]>('/api/users'),
};

export const canManage = (role?: string) => role === 'Admin' || role === 'Manager';
