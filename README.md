# daSSH

AGH UST C# project.

## Features

- [Discord](https://discord.com/) OAuth2 authentication
- Multiple instances of SSH servers
- SSH port forwarding *(WIP)*
- SSH file transfer *(WIP)*
- Instance sharing
- REST API with Bearer token authentication

## How to run locally

- Prerequisites:
    - [Docker](https://www.docker.com/get-started)
    - [Discord App](https://discord.com/developers/applications) with OAuth2

- Build the container:
```sh
$ docker build . -t dassh
```

- Create a `.env` file with at least the following content:
```env
DISCORD_CLIENT_ID=<your_client_id>
DISCORD_SECRET=<your_client_secret>
```

- Run with exposed ports:
```sh
$ docker run -it --rm --env-file .env -p 8080:8080 dassh
```

- Go to http://localhost:8080/ in your browser to try the app out.

- You can also configure a Docker volume at `/app/storage` to make user data persistent.
