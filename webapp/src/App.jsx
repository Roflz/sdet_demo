import { Routes, Route, Navigate } from 'react-router-dom';
import { useState } from 'react';
import Layout from './Layout';
import Login from './pages/Login';
import Quote from './pages/Quote';
import Home from './pages/Home';

export default function App() {
  const [token, setToken] = useState(() => sessionStorage.getItem('apiToken') || '');

  const handleLogin = (t) => {
    setToken(t);
    sessionStorage.setItem('apiToken', t);
  };

  const handleLogout = () => {
    setToken('');
    sessionStorage.removeItem('apiToken');
  };

  return (
    <Layout token={token} onLogout={handleLogout}>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login onLogin={handleLogin} />} />
        <Route path="/quotes" element={<Quote token={token} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Layout>
  );
}
