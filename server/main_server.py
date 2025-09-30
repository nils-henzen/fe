import database
import datatypes
from flask import Flask, request, jsonify

app = Flask(__name__)

def main():
    print("## Server started... ##")
    database.healthcheck()
    database.create_tables()

    print("## Server initializing endpoints ##")
    app.run(host="0.0.0.0", port=26834, debug=True)

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
    receiver_id = data.get("receiver_id",   "unknown")      # default to "unknown" if not provided
    return jsonify({"sender_id": sender_id, "receiver_id": receiver_id, "messages" : ["not implemented", "not implemented"]})

@app.route("/read", methods=["GET"])
def read_message():
    # Get query parameters
    data = request.json  # Expect JSON body
    signature   = data.get("signature",     "unknown")      # default to "unknown" if not provided
    sender_id   = data.get("sender_id",     "unknown")      # default to "unknown" if not provided
    message_id  = data.get("message_id",   "-1")            # default to "-1" if not provided

    return jsonify({"sender_id": sender_id, "message_id": message_id, "message" : "not implemented"})

@app.route("/send_message", methods=["POST"])
def receive_message():
    data = request.json  # Expect JSON body
    signature   = data.get("signature",     "unknown")      # default to "unknown" if not provided
    sender_id   = data.get("sender_id",     "unknown")      # default to "unknown" if not provided
    receiver_id = data.get("receiver_id",   "unknown")      # default to "unknown" if not provided
    message_text  = data.get("message_text","no content")   # default to "no content" if not provided
    return jsonify({"sender_id": sender_id, "receiver_id": receiver_id, "message_text": message_text})

@app.route("/send_file", methods=["POST"])
def receive_file():
    data = request.json  # Expect JSON body
    signature   = data.get("signature",     "unknown")      # default to "unknown" if not provided
    sender_id   = data.get("sender_id",     "unknown")      # default to "unknown" if not provided
    receiver_id = data.get("receiver_id",   "unknown")      # default to "unknown" if not provided
    file_name = data.get("file_name")
    file_type = data.get("file_type")
    file_content = data.get("file_content")
    return jsonify({"sender_id": sender_id, "receiver_id": receiver_id,
    "file_information": {
        "file name" : file_name,
        "file type" : file_type,
        "file contents (truncated)" : file_content[:50]
    }})

if __name__ == "__main__":
    main()
