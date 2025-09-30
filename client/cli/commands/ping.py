import click
from cli.api.client import FeApiClient

@click.command()
def ping():
    """Ping the Fe server's healthcheck endpoint."""
    try:
        client = FeApiClient()
        result = client.healthcheck()
        click.echo(f"Server health: {result}")
    except Exception as e:
        click.echo(f"Ping failed: {e}", err=True)