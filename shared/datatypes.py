import base64
import json
from typing import List, Optional

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

    def serialize(self) -> str:
        """Convert to JSON string"""
        return json.dumps(self.to_dict())

    @staticmethod
    def deserialize(data: str) -> "User":
        """Create User from JSON string"""
        obj = json.loads(data)
        return User(
            verified=obj["verified"],
            name=obj["name"],
            access_key=obj["access_key"],
            op=obj["op"]
        )


class Message:
    def __init__(self, message_id, sender_id, receiver_id, timestamp,
                 file_name, file_type, file_contents, queue_deletion):
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
            "id": self.message_id,
            "sender_id": self.sender_id,
            "receiver_id": self.receiver_id,
            "timestamp": self.timestamp,
            "file_name": self.file_name,
            "file_type": self.file_type,
            "file_contents": (
                base64.b64encode(self.file_contents).decode("utf-8")
                if isinstance(self.file_contents, (bytes, bytearray))
                else self.file_contents
            ),
            "queue_deletion": self.queue_deletion
        }

    def serialize(self) -> str:
        """Convert to JSON string"""
        return json.dumps(self.to_dict())

    @staticmethod
    def deserialize(data: str) -> "Message":
        """Create Message from JSON string"""
        obj = json.loads(data)
        file_contents = obj["file_contents"]
        if isinstance(file_contents, str):
            try:
                # try decode base64 back to bytes
                file_contents = base64.b64decode(file_contents.encode("utf-8"))
            except Exception:
                pass
        return Message(
            message_id=obj["id"],
            sender_id=obj["sender_id"],
            receiver_id=obj["receiver_id"],
            timestamp=obj["timestamp"],
            file_name=obj["file_name"],
            file_type=obj["file_type"],
            file_contents=file_contents,
            queue_deletion=obj["queue_deletion"]
        )


class Messages:
    """Container for multiple Message objects"""

    def __init__(self, messages: Optional[List[Message]] = None):
        self.messages = messages or []

    def to_dict(self) -> list[dict]:
        return [m.to_dict() for m in self.messages]

    def serialize(self) -> str:
        """Convert to JSON string"""
        return json.dumps(self.to_dict())

    @staticmethod
    def deserialize(data: str) -> "Messages":
        """Create Messages container from JSON string"""
        arr = json.loads(data)
        messages = [Message.deserialize(json.dumps(m)) for m in arr]
        return Messages(messages)
