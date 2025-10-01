import click
from cli.commands.init import init
from cli.commands.ping import ping
from cli.commands.fetch import fetch
from cli.commands.read import read
from cli.commands.send import send

BANNER = r"""
            ______                  ______                                 
           / ____/                 / ____/                                 
          / /_                    / __/                                    
         / __/                   / /___                                    
        /_/__            __     /_____/         __                         
       / ____/___ ______/ /_   / ____/  _______/ /_  ____ _____  ____ ____ 
      / /_  / __ `/ ___/ __/  / __/ | |/_/ ___/ __ \/ __ `/ __ \/ __ `/ _ \
     / __/ / /_/ (__  ) /_   / /____>  </ /__/ / / / /_/ / / / / /_/ /  __/
    /_/    \__,_/____/\__/  /_____/_/|_|\___/_/ /_/\__,_/_/ /_/\__, /\___/ 
                                                              /____/       
    """

@click.group(invoke_without_command=True)
@click.pass_context
def main(ctx):
    """Fast Exchange CLI Tool"""
    if ctx.invoked_subcommand is None:
        click.echo(BANNER)
        click.echo(ctx.get_help())

main.add_command(init)
main.add_command(ping)
main.add_command(fetch)
main.add_command(read)
main.add_command(send)

if __name__ == "__main__":
    main()