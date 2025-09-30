import os
import json

class ConfigManager:
    def __init__(self, config_path=None):
        if config_path is None:
            config_path = os.path.expanduser("~/.fe/config.json")
        self.config_path = config_path
        self._config = None
        self.load()

    def load(self):
        with open(self.config_path, "r") as f:
            self._config = json.load(f)

    @property
    def config(self):
        return self._config

    def get(self, key, default=None):
        return self._config.get(key, default)
