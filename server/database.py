### Handles database connection and crud updates

import sqlite3

# establish DB connection
conn = sqlite3.connect("fe_data.db")  # Creates the file if it doesn't exist
c = conn.cursor()

## Healthcheck for database connection 
def healthcheck():
    print("## DATABASE HEALTHCHECK ##")
    if (conn == None or c == None):
        print("ERROR: Connection or database cursor not initialized.")
    else:
        print("SUCCESS: Database connection successful")

## Create the database tables
def create_tables():

    print("## CREATING DATABASE TABLES ##")

    # foreign key support
    c.execute("PRAGMA foreign_keys = ON")

    # User table
    c.execute("""
    CREATE TABLE IF NOT EXISTS users (
        id INTEGER PRIMARY KEY,
        name TEXT,
        accesskey TEXT,
        op INTEGER
    )
    """)

    # Messages table
    c.execute("""
    CREATE TABLE IF NOT EXISTS messages (
        id INTEGER PRIMARY KEY,
        sender_id INTEGER,
        receiver_id INTEGER,
        timestamp INTEGER,
        file_name TEXT,
        file_type TEXT,
        file_contents BLOB,
        queue_deletion INTEGER,
        FOREIGN KEY(sender_id) REFERENCES users(id),
        FOREIGN KEY(receiver_id) REFERENCES users(id)
    )
    """)

    conn.commit()  # Save changes

    print("SUCCESS: Table creation successful")
