import requests
from cli.config.manager import ConfigManager

class FeApiClient:
    def __init__(self, config_manager=None):
        if config_manager is None:
            config_manager = ConfigManager()
        self.config = config_manager.config
        self.base_url = f"http://{self.config['server_ip']}:{self.config['server_port']}"

    def healthcheck(self):
        url = f"{self.base_url}/healthcheck"
        response = requests.get(url)
        response.raise_for_status()
        return response.text

    def fetch(self, signature=None, sender_id=None, receiver_id=None):
        url = f"{self.base_url}/fetch"
        headers = {"Authorization": f"Bearer {self.config['auth_token']}"}
        data = {
            "signature": signature or "unknown",
            "sender_id": sender_id or "nils",
            "receiver_id": receiver_id or "unknown"
        }
        response = requests.get(url, headers=headers, json=data)
        response.raise_for_status()
        return response.json()

    def read(self, signature=None, sender_id=None, message_id=None):
        url = f"{self.base_url}/read"
        headers = {"Authorization": f"Bearer {self.config['auth_token']}"}
        data = {
            "signature": signature or "00000",
            "sender_id": sender_id or "nils",
            "message_id": message_id or "-1"
        }
        response = requests.get(url, headers=headers, json=data)
        response.raise_for_status()
        return response.json()

    def send_message(self, sender_id, receiver_id, message_text, signature=None):
        url = f"{self.base_url}/send_message"
        headers = {"Authorization": f"Bearer {self.config['auth_token']}"}
        data = {
            "signature": signature or "0000",
            "sender_id": sender_id or "nils",
            "receiver_id": receiver_id or "unknown",
            "message_text": message_text or "no content"
        }
        response = requests.post(url, json=data, headers=headers)
        response.raise_for_status()
        return response.json()

    def send_file(self, sender_id, receiver_id, file_path, signature=None):
        url = f"{self.base_url}/send_file"
        headers = {"Authorization": f"Bearer {self.config['auth_token']}"}
        import mimetypes, base64
        file_name = file_path.split("/")[-1]
        file_type = mimetypes.guess_type(file_path)[0] or "application/octet-stream"
        with open(file_path, "rb") as f:
            file_content = base64.b64encode(f.read()).decode()
        data = {
            "signature": signature or "00000",
            "sender_id": sender_id or "unknown",
            "receiver_id": receiver_id or "unknown",
            "file_name": file_name,
            "file_type": file_type,
            "file_content": file_content
        }
        response = requests.post(url, json=data, headers=headers)
        response.raise_for_status()
        return response.json()
