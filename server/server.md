# Server

## Tech Stack
- Python3   (>3.11.x)
- SQLite3   (built-in)
- Flask     (pip install flask)

## Architecture Decisions
- Timestamps are stores as a `UNIX TIMESTAMP`
- Data is stored as a BLOB, not as a filepath
- The server runs on port `26834`