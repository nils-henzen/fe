import click
from cli.commands.init import init

@click.group()
def main():
    """Fe CLI tool"""
    pass

main.add_command(init)

if __name__ == "__main__":
    main()