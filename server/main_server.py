import database
from flask import Flask, request, jsonify

app = Flask(__name__)

def main():
    print("## Server started... ##")
    database.healthcheck()
    database.create_tables()

    print("## Server initializing endpoints ##")
    app.run(debug=True)

### ENDPOINTS ###

@app.route("/healthcheck", methods=["GET"])
def healthcheck():
    return "<h1>Fe is alive, healthy and running 🏃‍♂️</h1>"



if __name__ == "__main__":
    main()
