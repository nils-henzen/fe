import click
from cli.commands.init import init
from cli.commands.ping import ping
from cli.commands.fetch import fetch
from cli.commands.read import read
from cli.commands.send import send

@click.group()
def main():
    """Fe CLI tool"""
    pass

main.add_command(init)
main.add_command(ping)
main.add_command(fetch)
main.add_command(read)
main.add_command(send)

if __name__ == "__main__":
    main()