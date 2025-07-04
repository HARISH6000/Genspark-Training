const express = require("express");
const app = express();

app.get("/api", (req, res) => {
  res.json({ message: "Hello from the API!" });
});

app.listen(3000, () => console.log("API running on port 3000"));
