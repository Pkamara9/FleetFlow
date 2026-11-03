import { useCallback, useEffect, useState, type ReactNode } from 'react';

export function useLoad<T>(fn: () => Promise<T>, deps: unknown[] = []) {
  const [data, setData] = useState<T | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const reload = useCallback(() => {
    setLoading(true);
    fn()
      .then((d) => {
        setData(d);
        setError(null);
      })
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, deps);
  useEffect(reload, [reload]);
  return { data, error, loading, reload };
}

export function State({ loading, error, children }: { loading: boolean; error: string | null; children: ReactNode }) {
  if (error) return <div className="alert">{error}</div>;
  if (loading) return <p className="muted">Loading…</p>;
  return <>{children}</>;
}

export function Badge({ value }: { value: string }) {
  return <span className={`badge b-${value.toLowerCase().replace(/\s+/g, '-')}`}>{value}</span>;
}

export const fmtDate = (d?: string | null) => (d ? new Date(d).toLocaleDateString() : '—');
export const fmtMoney = (n: number) => n.toLocaleString(undefined, { style: 'currency', currency: 'USD' });
