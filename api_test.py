import requests

token = "daSSH-I+CCsRIYxaqtktwf3mb4tYCuhCEmBjp0vpVrvlLfoY="

print(requests.get("http://localhost:8080/api/user", headers={
    "Authorization": f"Bearer {token}"
}).json())

print(requests.post("http://localhost:8080/api/instances", headers={
    "Authorization": f"Bearer {token}"
}, json={
    "name": "API instance",
    "port": 8081,
}).json())

print(requests.get("http://localhost:8080/api/instances", headers={
    "Authorization": f"Bearer {token}"
}).json())
