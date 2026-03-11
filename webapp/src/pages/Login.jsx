import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

export default function Login({ onLogin }) {
  const [apiKey, setApiKey] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = (e) => {
    e.preventDefault();
    const key = apiKey.trim();
    if (!key) {
      setError('Enter the API key.');
      return;
    }
    onLogin(key);
    setError('');
    navigate('/');
  };

  return (
    <div>
      <h1>Log in</h1>
      <p>Use the test API key: <code>test-api-key-12345</code></p>
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="username">API Key </label>
          <input
            id="username"
            type="password"
            value={apiKey}
            onChange={(e) => setApiKey(e.target.value)}
            placeholder="API key"
            autoComplete="off"
          />
        </div>
        {error && <p style={{ color: 'crimson' }}>{error}</p>}
        <button type="submit">Log in</button>
      </form>
    </div>
  );
}
