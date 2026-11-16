<div align="center">

# Fusonic Git Backup

**Back up all your GitHub, GitLab and Bitbucket repositories in a single run.**

Full mirrors with every branch and tag, incremental updates, parallel execution, error mails and dead man's switch monitoring.

[![License: MIT](https://img.shields.io/github/license/fusonic/git-backup)](LICENSE)
[![Docker Pulls](https://img.shields.io/docker/pulls/fusonic/git-backup?logo=docker)](https://hub.docker.com/r/fusonic/git-backup)
[![.NET](https://img.shields.io/badge/.NET-11-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)

</div>

---

## Table of contents

- [Features](#features)
- [Quick start](#quick-start)
- [Configuration](#configuration)
- [Access tokens](#access-tokens)
- [Running it](#running-it)
- [Backup layout and strategies](#backup-layout-and-strategies)
- [Restoring a backup](#restoring-a-backup)
- [Monitoring](#monitoring)
- [FAQ](#faq)
- [License](#license)

## Features

- **Three providers, one run**: GitHub, GitLab and Bitbucket, with as many accounts per provider as you like.
- **Complete mirrors**: uses `git clone --mirror`, so every branch, tag and ref is included.
- **Incremental by default**: the first run clones, every later run only fetches what changed.
- **Fast**: repositories are processed in parallel (`MaxDegreeOfParallelism`).
- **Docker ready**: ships as `fusonic/git-backup`, no .NET installation needed.
- **Fail loudly**: an email with the full error is sent when a run fails, and an optional [Dead Man's Snitch](https://deadmanssnitch.com) alerts you when it doesn't run at all.
- **Easy restore**: a backup is a normal bare git repository, so a plain `git clone` brings it back.

## Quick start

1. Create an `app-settings.json` (see [Configuration](#configuration)):

   ```json
   {
     "Git": [
       { "Type": "Github", "Username": "your-user", "PersonalAccessToken": "ghp_..." }
     ],
     "Backup": {
       "Local": { "Destination": "/app/GitBackup" }
     }
   }
   ```

2. Run the image:

   ```sh
   docker run --rm \
     -v /absolute/path/to/backups:/app/GitBackup \
     -v /absolute/path/to/app-settings.json:/app/app-settings.json \
     fusonic/git-backup:latest
   ```

3. Find your repositories in `/absolute/path/to/backups`. Schedule the command (see [Running it](#running-it)) and you're done.

## Configuration

The tool reads `app-settings.json` (required), then `app-settings.overwrite.json` (optional) and .NET user secrets, in that order. Later sources win.

Full example:

```json
{
  "Git": [
    { "Type": "Bitbucket", "Username": "bit-user", "PersonalAccessToken": "app-password" },
    { "Type": "Github", "Username": "gh-user", "PersonalAccessToken": "ghp_..." },
    { "Type": "Gitlab", "Username": "gl-user-1", "PersonalAccessToken": "glpat-..." },
    { "Type": "Gitlab", "Username": "gl-user-2", "PersonalAccessToken": "glpat-..." }
  ],
  "Backup": {
    "MaxDegreeOfParallelism": 8,
    "Local": {
      "Destination": "/app/GitBackup",
      "Strategy": "Incremental",
      "DeleteAfterDays": 1
    }
  },
  "Mail": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "backup@example.com",
    "Password": "secret",
    "UseSsl": false,
    "Sender": { "Address": "backup@example.com", "Name": "Git Backup" },
    "Receiver": { "Address": "ops@example.com", "Name": "Ops Team" }
  },
  "DeadmanssnitchUrl": "https://nosnch.in/xxxxxxxxxx"
}
```

### `Git`

One entry per account. Remove providers you don't need.

| Key | Description |
|---|---|
| `Type` | `Github`, `Gitlab` or `Bitbucket` |
| `Username` | Your username on that platform. Not used for GitLab. |
| `PersonalAccessToken` | Token or app password, see [Access tokens](#access-tokens) |

Which repositories are backed up:

| Provider | Scope |
|---|---|
| GitHub | All repositories the user has access to (owned, collaborator, organization) |
| GitLab | All projects the user is a member of. Empty projects are skipped. |
| Bitbucket | All repositories the user is a member of |

### `Backup`

| Key | Default | Description |
|---|---|---|
| `MaxDegreeOfParallelism` | `8` | How many repositories are fetched and cloned at the same time |
| `Local.Destination` | `/app/GitBackup` | Target folder. Inside Docker, keep `/app/GitBackup` and mount your host folder there. |
| `Local.Strategy` | `Incremental` | `Incremental` or `Full`, see [Backup layout and strategies](#backup-layout-and-strategies) |
| `Local.DeleteAfterDays` | `1` | Only used by `Full`: delete backups older than this many days |

### `Mail`

Optional. Used to send a mail with the complete exception when a run fails. Leave `Host` empty to disable.

| Key | Description |
|---|---|
| `Host`, `Port` | SMTP server |
| `UseSsl` | `true` for implicit TLS (typically port 465) |
| `Username`, `Password` | SMTP credentials |
| `Sender` / `Receiver` | `Address` and `Name` of the sender and recipient |

### `DeadmanssnitchUrl`

Optional. Called after every successful run, see [Monitoring](#monitoring).

## Access tokens

Read access to repositories is all the tool needs. Create a dedicated, read-only token per platform.

| Platform | Where | Permissions |
|---|---|---|
| GitHub | Settings → Developer settings → Personal access tokens | `repo` (classic) or read-only *Contents* and *Metadata* (fine-grained, include all repositories you want backed up) |
| GitLab | User settings → Access tokens | `read_api` and `read_repository` |
| Bitbucket | Personal settings → App passwords | Repositories: *Read* |

> [!WARNING]
> GitLab tokens expire. Set a long lifetime and renew the token before it runs out, otherwise the backup starts failing (and the error mail and Snitch will tell you).

## Running it

### Docker

```sh
docker run --rm \
  -v /absolute/path/to/backups:/app/GitBackup \
  -v /absolute/path/to/app-settings.json:/app/app-settings.json \
  fusonic/git-backup:latest
```

| Mount | Purpose |
|---|---|
| `/app/GitBackup` | Backup destination on your host |
| `/app/app-settings.json` | Your configuration |

Schedule it with cron, for example every night at 02:00:

```cron
0 2 * * * docker run --rm -v /srv/backups:/app/GitBackup -v /srv/git-backup/app-settings.json:/app/app-settings.json fusonic/git-backup:latest
```

Or with Docker Compose and your scheduler of choice:

```yaml
services:
  git-backup:
    image: fusonic/git-backup:latest
    volumes:
      - ./backups:/app/GitBackup
      - ./app-settings.json:/app/app-settings.json:ro
```

```sh
docker compose run --rm git-backup
```

### Locally

Requirements: [.NET 11 SDK](https://dotnet.microsoft.com/download) and `git` on your `PATH`.

```sh
git clone https://github.com/fusonic/git-backup.git
cd git-backup
# edit app-settings.json (or add app-settings.overwrite.json)
dotnet run
```

To build a release and schedule it:

```sh
dotnet publish -c Release -o out
```

```cron
@midnight dotnet /path/to/git-backup/out/fusonic-git-backup.dll
```

## Backup layout and strategies

### Incremental (default)

```text
<Destination>/
├── Github/
│   └── <repository>/      # bare mirror
├── Gitlab/
│   └── <group>/<project>/
└── Bitbucket/
    └── <repository>/
```

- First run: `git clone --mirror`
- Later runs: `git remote update` (only new data is transferred)
- Nothing is deleted, so `DeleteAfterDays` has no effect.

This is the fastest option and uses the least bandwidth and storage. Use snapshots (ZFS, restic, borg, cloud volume snapshots, ...) if you also want history of the backup itself.

### Full

```text
<Destination>/
└── 2026-11-16-02-00-00/
    ├── Github/<repository>/
    ├── Gitlab/<group>/<project>/
    └── Bitbucket/<repository>/
```

Every run creates a new timestamped folder with fresh mirror clones. Folders older than `DeleteAfterDays` are removed at the end of a successful run.

## Restoring a backup

A backup is a regular bare git repository, so restoring is a clone:

```sh
git clone /path/to/backups/Github/my-repo my-repo
```

To push it to a new, empty remote with all branches and tags:

```sh
cd /path/to/backups/Github/my-repo
git push --mirror https://example.com/me/my-repo.git
```

## Monitoring

A failing run can tell you about itself. A run that never starts (server down, cron broken) can't. Two mechanisms cover both:

- **Error mail**: configure `Mail` to receive the full exception whenever a run fails.
- **Dead Man's Snitch**: set `DeadmanssnitchUrl` to your snitch URL. The tool calls it after every successful run. If the call stays away for the configured interval, the snitch alerts you.

## FAQ

**How often does it run?**
As often as you schedule it. Daily is a good default.

**How do I know a backup failed?**
You get an email with the full error (if `Mail` is configured) and the process logs it to the console. With a Snitch configured, you are also notified when a scheduled run is missing.

**Can I back up several accounts of the same provider?**
Yes, add one entry per account to `Git`.

**Are my tokens safe?**
Tokens only live in your configuration. They are handed to `git` via `GIT_ASKPASS` and are not written to the cloned repositories' remote URLs. Mount your config read-only and use a read-only token.

## License

[MIT](LICENSE) © Fusonic GmbH
