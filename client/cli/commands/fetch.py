import click
from cli.api.client import FeApiClient

@click.command()
def fetch():
    """Fetch unread messages from the server."""
    try:
        client = FeApiClient()
        result = client.fetch()
        click.echo(f"Fetched messages successfully!")
        click.echo(result)
    except Exception as e:
        click.echo(f"Fetch failed: {e}", err=True)