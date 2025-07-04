const express = require('express');
const { MongoClient } = require('mongodb');

const app = express();
const port = 3000;
const mongoUrl = process.env.MONGO_URL || 'mongodb://localhost:27017/db';

app.get('/', async (req, res) => {
  try {
    const client = await MongoClient.connect(mongoUrl);
    const db = client.db();
    const collections = await db.collections();
    await client.close();
    res.send(`Connected to MongoDB. Collections: ${collections.length}`);
  } catch (err) {
    res.status(500).send('MongoDB connection failed: ' + err.message);
  }
});

app.listen(port, () => {
  console.log(`API running at http://localhost:${port}`);
});
