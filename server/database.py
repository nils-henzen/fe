### Handles database connection and crud updates

import sqlite3

import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), '..'))
import shared.datatypes as t


## Healthcheck for database connection 
def healthcheck():
    print("## DATABASE HEALTHCHECK ##")
    #if (conn == None or c == None):
    #    print("ERROR: Connection or database cursor not initialized.")
    #else:
    #    print("SUCCESS: Database connection successful")

## Create the database tables
def create_tables():

    print("## CREATING DATABASE TABLES ##")


    # establish DB connection
    conn = sqlite3.connect("fe_data.db")  # Creates the file if it doesn't exist
    c = conn.cursor()

    # foreign key support
    c.execute("PRAGMA foreign_keys = ON")

    # User table
    c.execute("""
    CREATE TABLE IF NOT EXISTS users (
        username TEXT PRIMARY KEY,
        accesskey TEXT,
        op INTEGER
    )
    """)

    # Messages table
    c.execute("""
    CREATE TABLE IF NOT EXISTS messages (
        id INTEGER PRIMARY KEY,
        sender_id TEXT,
        receiver_id TEXT,
        timestamp INTEGER,
        file_name TEXT,
        file_type TEXT,
        file_contents BLOB,
        queue_deletion INTEGER,
        FOREIGN KEY(sender_id) REFERENCES users(username),
        FOREIGN KEY(receiver_id) REFERENCES users(username)
    )
    """)

    conn.commit()  # Save changes

    conn.close() # close connection

    print("SUCCESS: Table creation successful")

def fetch_user(username: str) -> t.User | None:
    """
    Fetch a user by username.
    Returns a User object if found, else None.
    """
    conn = sqlite3.connect("fe_data.db")
    c = conn.cursor()

    query = """
    SELECT username, accesskey, op
    FROM users
    WHERE username = ?
    """

    c.execute(query, (username,))
    row = c.fetchone()
    conn.close()

    if row:
        return t.User(False, row[0], row[1], row[2])
    else:
        return None

def register_user(user: t.User):
    """
    Register a new user in the database.
    """

    # SQL INSERT statement
    query = """
    INSERT INTO users (username, accesskey, op)
    VALUES (?, ?, ?)
    """


    # establish DB connection
    conn = sqlite3.connect("fe_data.db")  # Creates the file if it doesn't exist
    c = conn.cursor()

    try:
        c.execute(query, (user.name, user.access_key, user.op))
        conn.commit()
        print(f"User {user.name} registered successfully.")
    except sqlite3.IntegrityError:
        print(f"User {user.name} already exists.")
    finally:
        conn.close()

def fetch_messages_for_user(user: t.User) -> t.Messages | None:
    if not user.verified:
        return None

    conn = sqlite3.connect("fe_data.db")
    c = conn.cursor()

    query = """
    SELECT id, sender_id, receiver_id, timestamp, file_name, file_type, queue_deletion
    FROM messages
    WHERE receiver_id = ? OR sender_id = ?
    ORDER BY timestamp ASC
    """
    c.execute(query, (user.name,user.name))
    rows = c.fetchall()
    conn.close()

    # fill file_contents with None explicitly
    messages = t.Messages([
        t.Message(
            message_id=row[0],
            sender_id=row[1],
            receiver_id=row[2],
            timestamp=row[3],
            file_name=row[4][:50] if row[4] else None,
            file_type=row[5],
            file_contents=None,      # skipped
            queue_deletion=row[6]
        )
        for row in rows
    ])

    return messages



def fetch_message_by_id(message_id: int) -> t.Message | None:

    """
    Fetch a single message by its ID.
    Returns a Message object if found, else None.
    """
    conn = sqlite3.connect("fe_data.db")
    c = conn.cursor()

    query = """
    SELECT id, sender_id, receiver_id, timestamp,
           file_name, file_type, file_contents, queue_deletion
    FROM messages
    WHERE id = ?
    """

    c.execute(query, (message_id,))
    row = c.fetchone()
    conn.close()

    if row:
        # match constructor: message_id, sender_id, receiver_id, timestamp, file_name, file_type, file_contents, queue_deletion
        return t.Message(*row)
    else:
        return None


def save_message(message: t.Message):
    """
    Save a message to the database.
    - sending_user: User object (sender)
    - receiving_user: User object (receiver)
    - message: Message object
    """
    conn = sqlite3.connect("fe_data.db")
    c = conn.cursor()

    query = """
    INSERT INTO messages (
        sender_id, receiver_id, timestamp,
        file_name, file_type, file_contents,
        queue_deletion
    )
    VALUES (?, ?, ?, ?, ?, ?, ?)
    """

    try:
        c.execute(query, (
            message.sender_id,
            message.receiver_id,
            message.timestamp,
            message.file_name,
            message.file_type,
            message.file_contents,
            message.queue_deletion
        ))
        conn.commit()
        print(f"Message from {message.sender_id} to {message.receiver_id} saved.")
    except sqlite3.Error as e:
        print(f"Error saving message: {e}")
    finally:
        conn.close()
