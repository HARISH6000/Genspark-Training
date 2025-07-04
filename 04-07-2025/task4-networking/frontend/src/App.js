import React, { useEffect, useState } from 'react';

function App() {
  const [message, setMessage] = useState("Loading...");

  useEffect(() => {
    fetch('/api/') 
      .then(res => res.json())
      .then(data => setMessage(data.message))
      .catch(err => {
        console.error('Error fetching from backend:', err);
        setMessage('Failed to load.');
      });
  }, []);

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Frontend React App</h1>
      <p>{message}</p>
    </div>
  );
}

export default App;
