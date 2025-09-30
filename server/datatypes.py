import base64

class User:
    def __init__(self, verified: bool, name: str, access_key: str, op: bool):
        self.verified = verified
        self.name = name
        self.access_key = access_key
        self.op = op

    def to_dict(self) -> dict:
        return {
            "verified": self.verified,
            "name": self.name,
            "access_key": self.access_key,
            "op": self.op
        }


class Message:
    def __init__(self, message_id, sender_id, receiver_id, timestamp, file_name, file_type, file_contents, queue_deletion):
        self.message_id = message_id
        self.sender_id = sender_id
        self.receiver_id = receiver_id
        self.timestamp = timestamp
        self.file_name = file_name
        self.file_type = file_type
        self.file_contents = file_contents
        self.queue_deletion = queue_deletion

    def to_dict(self) -> dict:
        return {
            "message_id": self.message_id,
            "sender_id": self.sender_id,
            "receiver_id": self.receiver_id,
            "timestamp": self.timestamp,
            "file_name": self.file_name,
            "file_type": self.file_type,
            # Encode bytes safely if needed
            "file_contents": (
                base64.b64encode(self.file_contents).decode("utf-8")
                if isinstance(self.file_contents, (bytes, bytearray))
                else self.file_contents
            ),
            "queue_deletion": self.queue_deletion
        }


def users_to_dict(users: list[User]) -> list[dict]:
    if users == None:
        return None
    return [u.to_dict() for u in users]

def messages_to_dict(messages: list[Message]) -> list[dict]:
    if messages == None:
        return None
    return [m.to_dict() for m in messages]


def message_to_dict(message: Message) -> list[dict]:
    if message == None:
        return None
    return message.to_dict()