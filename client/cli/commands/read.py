import click
from cli.api.client import FeApiClient

@click.command()
@click.argument('message_id', type=int, required=True)
def read(message_id):
    """Read a message by its ID. Usage: fe read <message_id>"""
    try:
        client = FeApiClient()
        result = client.read(message_id)
        click.echo(f"Read message successfully!")
        click.echo(result)
    except Exception as e:
        click.echo(f"Read failed: {e}", err=True)