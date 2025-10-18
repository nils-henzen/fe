FROM python:3.12-slim

WORKDIR /server
COPY requirements.txt .
RUN pip install -r requirements.txt
COPY . .

CMD ["python", "main_server.py"]
