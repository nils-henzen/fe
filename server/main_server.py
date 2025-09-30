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

@app.route("/fetch/<string:name>", methods=["GET"])
def fetch_messages(name):
    return jsonify({"username": name, "messages" : ["not implemented", "not implemented"]})

@app.route("/read/<int:message_id>", methods=["GET"])
def read_message(msg_id):
    return jsonify({"message_id": msg_id, "message" : "not implemented"})

@app.route("/send_message", methods=["POST"])
def receive_message():
    data = request.json  # Expect JSON body
    username = data.get("username")
    message = data.get("message")
    return jsonify({"username": username, "message": message})

@app.route("/send_file", methods=["POST"])
def receive_file():
    data = request.json  # Expect JSON body
    username = data.get("name")
    file_name = data.get("file_name")
    file_type = data.get("file_type")
    file_content = data.get("file_content")
    return jsonify({"username": username,
    "file_information": {
        "file name" : file_name,
        "file type" : file_type,
        "file contents (truncated)" : file_content[:50]
    }})

if __name__ == "__main__":
    main()
