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
