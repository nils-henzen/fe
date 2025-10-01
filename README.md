# FeCli (Fast Exchange)


Similar to email - but easier
Used to simply and fast send messages or files to your buddy!

## Client Side / Local commands: 

- Fe (fetch) > fetches for [unread/new] messages and displays previews (max 50 characters) in a table with ids
- Fe read [id] > displays message and gets marked for deletion on the server
- Fe send [username;username2] [“string”.txt or file.pdf]
- Fe explore > opens file system folder with stored messages and files

Identification via local config file:
    - Keeps track of corresponding server token (UUID/secret)
    - IP and Port of server instance

Not important:
- Fe chatroom [username] > live chatroom (login via username) + messages don’t get stored, only relayed)
- Fe token set [UUID] > local, automatically store in config
- Fe token new > calls the server for a new UUID needed for future auth and stores it in config file
- Fe unread [id] > unmarks for deletion
- Fe list > show all messages [also read] with ids
- Fe register [username] [serverpass] > creates a new user on the server and outputs his auth-key
Op system:
- Fe server pass change [old-serverpass] [new-serverpass] (only changeable if your local key is considered op)
- Fe op [username] (only changeable if your local key is considered op)
- Fe deop [username] [serverpass]  (only changeable if your local key is considered op)



## Server Side:
- SQLite DB
    - User table that stores name and auth-key, op/admin flag
    - Message table that stores sender, receiver, timestamp, file blob, queuedForDeletionFlag
- HTTP Endpoint that checks for client commands and sends messages with CR(UD) REST endpoints

MVP Checklist:
- [] GET    fetch   fetch all user messages truncated / filenames only from database and send them back to user
- [] GET    read    read a specific message id from db and send it to user with its full contents
- [] POST   send    add a message to database

Not important:
- Cronjob to delete messages
- OP Right system
