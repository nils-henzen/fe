import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), '..'))
import database as db
import shared.signature as s
import shared.datatypes as t
import time
from flask import Flask, request, jsonify

app = Flask(__name__)

def main():
    print("## Server started... ##")
    db.healthcheck()
    db.create_tables()

    print("## Server initializing endpoints ##")
    #unit_tests()
    app.run(host="0.0.0.0", port=26834, debug=True)

def get_timestamp():
    return int(time.time())

def get_verified_user(sender_id, key, signature) -> t.User | None:

    if (sender_id == "unknown" or key == "unknown" or signature == "unknown"):
        print(f"VERIFY USER: Invalid arguments. At least one is unknown. Sender ID {sender_id}, Key {key}, Signature {signature}")
        return None

    user: t.User = db.fetch_user(sender_id)
    
    if user == None:
        print("VERIFY USER: Invalid user: " + sender_id)
        return None
    
    secret: str = user.access_key
    user.verified = s.verify_signature(user.name, key, secret, signature)
    return user

### ENDPOINTS ###

##
## HEALTHCHECK for the user to ping
##
@app.route("/healthcheck", methods=["GET"])
def healthcheck():
    return "<h1>Fe is alive, healthy and running 🏃‍♂️</h1>"

##
## Retrieve all messages for a single user
## Signature key is FTCH
##
@app.route("/fetch", methods=["GET"])
def fetch_messages():
    # Get query parameters
    data = request.json  # Expect JSON body
    signature   = data.get("signature",     "unknown")      # default to "unknown" if not provided
    sender_id   = data.get("sender_id",     "unknown")      # default to "unknown" if not provided

    user: t.User = get_verified_user(sender_id, "FTCH", signature)
    messages: t.Messages = db.fetch_messages_for_user(user)
    return messages.serialize()

##
## Retrieve a SINGLE message for a user
## Signature key is message_id
##
@app.route("/read", methods=["GET"])
def read_message():
    # Get query parameters
    data = request.json  # Expect JSON body
    signature   = data.get("signature",     "unknown")      # default to "unknown" if not provided
    sender_id   = data.get("sender_id",     "unknown")      # default to "unknown" if not provided
    message_id  = data.get("message_id",    "unknown")            # default to "-1" if not provided
    
    user: t.User = get_verified_user(sender_id, message_id, signature)

    if (user.verified == False):
        return jsonify({"status" : 403, "message" : "signature dosen't match. user couldn't be verified"})
    
    message: t.Message = db.fetch_message_by_id(int(message_id))
    return message.serialize()

##
## SAVE a MESSAGE to the database for the user
## signature key is message_text[:32]
##
@app.route("/send_message", methods=["POST"])
def send_message():
    data = request.json  # Expect JSON body
    signature   = data.get("signature",     "unknown")      # default to "unknown" if not provided
    sender_id   = data.get("sender_id",     "unknown")      # default to "unknown" if not provided
    receiver_id = data.get("receiver_id",   "unknown")      # default to "unknown" if not provided
    message_text  = data.get("message_text","no content")   # default to "no content" if not provided
    
    print(f"send_message called with: {data}")

    user: t.User = get_verified_user(sender_id, message_text[:32], signature)
    
    if (user.verified == False):
        return jsonify({"status" : 403, "message" : "signature dosen't match. user couldn't be verified"})
    
    message: t.Message = t.Message(0, sender_id, receiver_id, get_timestamp(), message_text, "FETXT", None, False)
    db.save_message(message)
    return jsonify({"status" : 200, "message" : "Message was sent."})


##
## SAVE a FILE to the database for the user
## Signature key is file_name[:16]+file_content[:16]
##
@app.route("/send_file", methods=["POST"])
def send_file():
    data = request.json  # Expect JSON body
    signature   = data.get("signature",     "unknown")      # default to "unknown" if not provided
    sender_id   = data.get("sender_id",     "unknown")      # default to "unknown" if not provided
    receiver_id = data.get("receiver_id",   "unknown")      # default to "unknown" if not provided
    file_name = data.get("file_name")
    file_type = data.get("file_type")
    file_content = data.get("file_content")
        
    user: t.User = get_verified_user(sender_id, file_name[:16]+file_content[:16], signature)
    
    if (user.verified == False):
        return jsonify({"status" : 403, "message" : "signature dosen't match. user couldn't be verified"})
    
    message: t.Message = t.Message(0, sender_id, receiver_id, get_timestamp(), file_name, file_type, file_content, False)
    db.save_message(message)
    return jsonify({"status" : 200, "message" : "File was sent."})


## Call main function
if __name__ == "__main__":
    main()
