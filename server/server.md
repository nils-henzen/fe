# Server

## Tech Stack
- Python3   (>3.11.x)
- SQLite3   (built-in)
- Flask     (pip install flask)

## Architecture Decisions
- Timestamps are stores as a `UNIX TIMESTAMP`
- Data is stored as a BLOB, not as a filepath
- The server runs on port `26834`
- each request gets a signature, that is calculated from the `access_key` hashed via ...
- user messages are stored using the `FETXT` MIME. file contents are empty, only the file_name contains the message. this is to make the query for multiple messages more effective
- when retrieving multiple messages using `fetch`, the `file_contents` are **NOT SENT**, only `file_name`s. Also file_names are truncated to `50` characters
- For `fetch` command the signature key is `FTCH`
- For `read` command the signature key is the `message_id`
- For `send` command **(IF MESSAGE)** the signature key is the `message_text`
- For `send` command **(IF FILE)** the signature key is the `file_name + file_content`