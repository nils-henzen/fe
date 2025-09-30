import os
import click
from cli.api.client import FeApiClient
from cli.config.manager import ConfigManager

@click.command()
@click.argument('recipients', required=True)
@click.argument('message', required=True)
def send(recipients, message):
    """Send a message or file to one or more recipients. Usage: fe send [recipient1,recipient2] <message> <message> can be a file path or a quoted string."""
    recipients_list = [r.strip() for r in recipients.split(',') if r.strip()]
    config = ConfigManager().config
    sender_id = config.get("auth_token", "unknown")  # Or use a username if available
    client = FeApiClient()

    if os.path.isfile(message):
        for recipient in recipients_list:
            try:
                result = client.send_file(sender_id, recipient, message)
                click.echo(f"Sent file '{message}' to {recipient}: {result}")
            except Exception as e:
                click.echo(f"Failed to send file to {recipient}: {e}", err=True)
    else:
        for recipient in recipients_list:
            try:
                result = client.send_message(sender_id, recipient, message)
                click.echo(f"Sent message to {recipient}: {result}")
            except Exception as e:
                click.echo(f"Failed to send message to {recipient}: {e}", err=True)