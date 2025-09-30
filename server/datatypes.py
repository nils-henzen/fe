class User:
    def __init__(self, name, access_key, op):
        self.name = name
        self.access_key = access_key
        self.op = op

class Message:
    def __init__(self, sender_id, receiver_id, timestamp, file_name, file_type, file_contents, queue_deletion):
        self.sender_id = sender_id
        self.receiver_id = receiver_id
        self.timestamp = timestamp
        self.file_name = file_name
        self.file_type = file_type
        self.file_contents = file_contents
        self.queue_deletion = queue_deletion