import database
import datatypes as t
import time
from flask import Flask, request, jsonify

app = Flask(__name__)

def unit_tests():
    ## UNIT TESTS
    
    print("##### RUNNING UNIT TESTS #####")

    # Example user
    nico_user: t.User = t.User(True, "nico", "12345", False)
    nils_user: t.User = t.User(True, "nils", "00000", False)

    # Register User
    database.register_user(nico_user)
    database.register_user(nils_user)

    # nils sends a message to nico
    #message: t.Message = t.Message(0, "nils", "nico", 45678946, "welcome", "txt", "Moin meister", False)
    #database.save_message(message)

    # fetch message contents
    message = database.fetch_message_by_id(1)
    print(t.message_to_dict(message))

    # Example fetch
    user_messages = database.fetch_messages_for_user(nico_user)
    for msg in user_messages:
        print(f"{msg.timestamp}: {msg.sender_id} -> {msg.receiver_id}, file: {msg.file_name}")



def main():
    print("## Server started... ##")
    database.healthcheck()
    database.create_tables()

    print("## Server initializing endpoints ##")
    #unit_tests()
    app.run(host="0.0.0.0", port=26834, debug=True)

def get_timestamp():
    return int(time.time())

def get_verified_user(signature, sender_id) -> t.User | None:
    user: t.User = database.fetch_user(sender_id)
    user.verified = user.access_key == signature # just use access key for now
    #user.verified = True # DEBUG REMOVE THIS ON PROD
    return user

### ENDPOINTS ###

@app.route("/healthcheck", methods=["GET"])
def healthcheck():
    return "<h1>Fe is alive, healthy and running 🏃‍♂️</h1>"

@app.route("/fetch", methods=["GET"])
def fetch_messages():
    # Get query parameters
    data = request.json  # Expect JSON body
    signature   = data.get("signature",     "unknown")      # default to "unknown" if not provided
    sender_id   = data.get("sender_id",     "unknown")      # default to "unknown" if not provided

    user: t.User = get_verified_user(signature, sender_id)
    messages: list[t.Message] = database.fetch_messages_for_user(user)
    return jsonify(t.messages_to_dict(messages))

@app.route("/read", methods=["GET"])
def read_message():
    # Get query parameters
    data = request.json  # Expect JSON body
    signature   = data.get("signature",     "unknown")      # default to "unknown" if not provided
    sender_id   = data.get("sender_id",     "unknown")      # default to "unknown" if not provided
    message_id  = data.get("message_id",   "-1")            # default to "-1" if not provided
    
    user: t.User = get_verified_user(signature, sender_id)

    if (user.verified == False):
        return jsonify({"status" : 403, "message" : "signature dosen't match. user couldn't be verified"})
    
    message: t.Message = database.fetch_message_by_id(int(message_id))
    return jsonify(t.message_to_dict(message))

@app.route("/send_message", methods=["POST"])
def send_message():
    data = request.json  # Expect JSON body
    signature   = data.get("signature",     "unknown")      # default to "unknown" if not provided
    sender_id   = data.get("sender_id",     "unknown")      # default to "unknown" if not provided
    receiver_id = data.get("receiver_id",   "unknown")      # default to "unknown" if not provided
    message_text  = data.get("message_text","no content")   # default to "no content" if not provided
    
    print(f"send_message called with: {data}")

    user: t.User = get_verified_user(signature, sender_id)
    
    if (user.verified == False):
        return jsonify({"status" : 403, "message" : "signature dosen't match. user couldn't be verified"})
    
    message: t.Message = t.Message(0, sender_id, receiver_id, get_timestamp(), "Message", "TXT", message_text, False)
    database.save_message(message)
    return jsonify({"status" : 200, "message" : "Message was sent."})


@app.route("/send_file", methods=["POST"])
def send_file():
    data = request.json  # Expect JSON body
    signature   = data.get("signature",     "unknown")      # default to "unknown" if not provided
    sender_id   = data.get("sender_id",     "unknown")      # default to "unknown" if not provided
    receiver_id = data.get("receiver_id",   "unknown")      # default to "unknown" if not provided
    file_name = data.get("file_name")
    file_type = data.get("file_type")
    file_content = data.get("file_content")
        
    user: t.User = get_verified_user(signature, sender_id)
    
    if (user.verified == False):
        return jsonify({"status" : 403, "message" : "signature dosen't match. user couldn't be verified"})
    
    message: t.Message = t.Message(0, sender_id, receiver_id, get_timestamp(), file_name, file_type, file_content, False)
    database.save_message(message)
    return jsonify({"status" : 200, "message" : "File was sent."})

if __name__ == "__main__":
    main()
