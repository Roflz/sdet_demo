import { useState } from 'react';

const API_BASE = import.meta.env.VITE_API_URL || '/api';

export default function Quote({ token }) {
  const [quoteId, setQuoteId] = useState('');
  const [quote, setQuote] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const fetchQuote = async (e) => {
    e?.preventDefault();
    const id = quoteId.trim();
    if (!id || !token) {
      setError(token ? 'Enter a quote ID.' : 'Log in first.');
      return;
    }
    setError('');
    setLoading(true);
    setQuote(null);
    try {
      const res = await fetch(`${API_BASE}/quotes/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) {
        if (res.status === 401) setError('Invalid or missing API key.');
        else if (res.status === 404) setError('Quote not found.');
        else setError(`Error ${res.status}`);
        return;
      }
      const data = await res.json();
      setQuote(data);
    } catch (err) {
      setError('Network error. Is the API running?');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1>Get Quote</h1>
      <p>Enter a quote ID to fetch details (e.g. 1 if you ran seed data).</p>
      <form onSubmit={fetchQuote}>
        <input
          type="number"
          min="1"
          value={quoteId}
          onChange={(e) => setQuoteId(e.target.value)}
          placeholder="Quote ID"
        />
        <button type="submit" disabled={!token || loading}>
          {loading ? 'Loading…' : 'Get Quote'}
        </button>
      </form>
      {error && <p style={{ color: 'crimson' }}>{error}</p>}
      {quote && (
        <div style={{ marginTop: '1rem', padding: '1rem', background: '#f5f5f5', borderRadius: 4 }}>
          <p><strong>ID:</strong> {quote.id}</p>
          <p><strong>Customer ID:</strong> {quote.customerId}</p>
          <p><strong>Product Code:</strong> {quote.productCode ?? '—'}</p>
          <p><strong>Premium:</strong> ${quote.premium}</p>
          <p><strong>Status:</strong> {quote.status}</p>
        </div>
      )}
    </div>
  );
}
