import click
from cli.commands.init import init
from cli.commands.ping import ping

@click.group()
def main():
    """Fe CLI tool"""
    pass

main.add_command(init)
main.add_command(ping)

if __name__ == "__main__":
    main()