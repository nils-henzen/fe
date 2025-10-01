import click
import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
from shared import signature as s
from tabulate import tabulate
from datetime import datetime, timezone
from cli.api.client import FeApiClient
from cli.config.manager import ConfigManager

@click.command()
def fetch():
    """Fetch unread messages from the server."""
    try:
        config = ConfigManager().config
        secret = config.get("auth_token", "unknown")
        sender_id = config.get("sender_name", "unknown")
        key = "FTCH"
        signature = s.sign_message(sender_id, key, secret)
        client = FeApiClient()
        result = client.fetch(signature=signature, sender_id=sender_id)
        click.echo(f"Fetched messages successfully!")
        print_messages_table(result)
    except Exception as e:
        click.echo(f"Fetch failed: {e}", err=True)

def print_messages_table(messages):
    """Zeigt Nachrichten als Tabelle im Terminal an."""
    table = [
        [unix_to_iso(msg.get("timestamp")), msg.get("sender_id"), msg.get("file_name")]
        for msg in messages
    ]
    headers = ["Timestamp", "Sender ID", "Message"]
    click.echo(tabulate(table, headers, tablefmt="grid"))

def unix_to_iso(timestamp):
    return datetime.fromtimestamp(timestamp, tz=timezone.utc).isoformat().replace('+00:00', 'Z')