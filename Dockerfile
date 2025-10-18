FROM python:3.12-slim

WORKDIR /server
COPY server/requirements.txt .
RUN pip install -r requirements.txt
COPY server .
COPY shared /shared

CMD ["python", "main_server.py"]
