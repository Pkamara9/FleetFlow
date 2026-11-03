import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import Login from './pages/Login';
import { Badge } from './components/ui';

describe('FleetFlow web', () => {
  it('renders the login form', () => {
    render(<MemoryRouter><Login /></MemoryRouter>);
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
  });

  it('renders status badges', () => {
    render(<Badge value="Due Soon" />);
    expect(screen.getByText('Due Soon')).toHaveClass('b-due-soon');
  });
});
