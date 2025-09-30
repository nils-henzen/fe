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

    def fetch(self):
        url = f"{self.base_url}/fetch"
        headers = {"Authorization": f"Bearer {self.config['auth_token']}"}
        response = requests.get(url, headers=headers)
        response.raise_for_status()
        return response.json()

    def read(self, message_id):
        url = f"{self.base_url}/read"
        headers = {"Authorization": f"Bearer {self.config['auth_token']}"}
        data = {"messageId": message_id}
        response = requests.post(url, json=data, headers=headers)
        response.raise_for_status()
        return response.json()

    def send_message(self, recipient, message):
        url = f"{self.base_url}/send_message"
        headers = {"Authorization": f"Bearer {self.config['auth_token']}"}
        data = {"recipient": recipient, "message": message} 
        response = requests.post(url, json=data, headers=headers)
        response.raise_for_status()
        return response.json()

    def send_file(self, recipient, file_path):
        url = f"{self.base_url}/send_file"
        headers = {"Authorization": f"Bearer {self.config['auth_token']}"}
        with open(file_path, "rb") as f:
            files = {"file": f}
            data = {"recipient": recipient}
            response = requests.post(url, files=files, data=data, headers=headers)
            response.raise_for_status()
            return response.json()
