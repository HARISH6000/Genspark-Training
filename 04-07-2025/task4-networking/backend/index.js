const express = require('express');
const app = express();
const PORT = 5000;

app.get('/api', (req, res) => {
  res.json({ message: 'Hello from the backend API!' });
});

app.listen(PORT, () => {
  console.log(`Backend API is running on port ${PORT}`);
});
