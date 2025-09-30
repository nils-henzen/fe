import click
from cli.api.client import FeApiClient
from cli.config.manager import ConfigManager

@click.command()
def fetch():
    """Fetch unread messages from the server."""
    try:
        config = ConfigManager().config
        signature = config.get("auth_token", "unknown")
        sender_id = config.get("auth_token", "unknown")
        receiver_id = config.get("auth_token", "unknown")
        client = FeApiClient()
        result = client.fetch(signature=signature, sender_id=sender_id, receiver_id=receiver_id)
        click.echo(f"Fetched messages successfully!")
        click.echo(result)
    except Exception as e:
        click.echo(f"Fetch failed: {e}", err=True)