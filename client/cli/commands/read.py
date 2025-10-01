import click
from api.client import FeApiClient
from config.manager import ConfigManager

@click.command()
@click.argument('message_id', required=True)
def read(message_id):
    """Read a message by its ID. Usage: fe read <message_id>"""
    try:
        config = ConfigManager().config
        signature = config.get("auth_token", "unknown")
        sender_id = config.get("auth_token", "unknown")
        client = FeApiClient()
        result = client.read(signature=signature, sender_id=sender_id, message_id=message_id)
        click.echo(f"Read message successfully!")
        click.echo(result)
    except Exception as e:
        click.echo(f"Read failed: {e}", err=True)