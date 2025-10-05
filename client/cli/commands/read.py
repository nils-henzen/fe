import click
from api.client import FeApiClient
from config.manager import ConfigManager
import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
from shared import signature as s

@click.command()
@click.argument('message_id', required=True)
def read(message_id):
    """Read a message by its ID. Usage: fe read <message_id>"""
    try:
        config = ConfigManager().config
        sender_id = config.get("sender_name", "unknown")
        key = message_id
        secret = config.get("auth_token", "unknown")
        signature = s.sign_message(sender_id, key, secret)
        client = FeApiClient()
        result = client.read(signature=signature, sender_id=sender_id, message_id=message_id)
        click.echo(f"Read message successfully!")
        click.echo(result)
    except Exception as e:
        click.echo(f"Read failed: {e}", err=True)