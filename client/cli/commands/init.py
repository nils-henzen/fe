import os
import json
import click

@click.command()
def init():
    """Initialize Fe CLI config or environment."""
    config_dir = os.path.expanduser("~/.fe")
    os.makedirs(config_dir, exist_ok=True)
    config_path = os.path.join(config_dir, "config.json")
    default_config = {
        "server_ip": "127.0.0.1",
        "server_port": 26834,
        "auth_token": "",
        "storage_path": os.path.join(config_dir, "messages")
    }
    with open(config_path, "w") as f:
        json.dump(default_config, f, indent=4)
    click.echo(f"Initialized config at {config_path}")