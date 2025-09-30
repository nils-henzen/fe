from setuptools import setup, find_packages

setup(
    name="fe_cli",
    version="0.1",
    packages=find_packages(),
    entry_points={
        "console_scripts": [
            "fe=cli.main:main",
            "fe init=cli.commands.init:init",
            "fe ping=cli.commands.ping:ping",
            "fe fetch=cli.commands.fetch:fetch",
            "fe read=cli.commands.read:read",
            "fe send=cli.commands.send:send",
        ]
    },
)
