import { Link } from 'react-router-dom';

export default function Layout({ children, token, onLogout }) {
  return (
    <div>
      <nav style={{ borderBottom: '1px solid #ccc', paddingBottom: '0.5rem', marginBottom: '1rem' }}>
        <Link to="/">Home</Link>
        {' | '}
        <Link to="/quotes">Quotes</Link>
        {' | '}
        {token ? (
          <button type="button" onClick={onLogout}>Log out</button>
        ) : (
          <Link to="/login">Log in</Link>
        )}
      </nav>
      <main>{children}</main>
    </div>
  );
}
