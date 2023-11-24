# USTA CLI 🎾

## [![🚀 Release](https://github.com/lineville/usta-cli/actions/workflows/release.yml/badge.svg)](https://github.com/lineville/usta-cli/actions/workflows/release.yml) [![🧪 CI](https://github.com/lineville/usta-cli/actions/workflows/ci.yml/badge.svg)](https://github.com/lineville/usta-cli/actions/workflows/ci.yml) [![🏆 Check Ranking](https://github.com/lineville/usta-cli/actions/workflows/check_rank.yml/badge.svg)](https://github.com/lineville/usta-cli/actions/workflows/check_rank.yml) ![Nuget](https://img.shields.io/nuget/v/usta-cli)

Command Line Tool to view current USTA Tennis rankings for a particular player, or view the current standings by section and NTRP level. This tool is completely free ❤️‍🩹 and distributed via the [nuget package manager](https://www.nuget.org/packages/usta-cli). This tool would not be possible without relying on the work done by [Selenium](https://github.com/SeleniumHQ/selenium) and the [USTA](https://usta.com).

## Requirements

Install [dotnet](https://dotnet.microsoft.com/en-us/download)

## Usage

```console
dotnet tool install --global usta-cli
usta-cli <command> [options]
```

## Commands

### `get [options]`

Gets a player's current ranking

- `-n | --name ['FIRST LAST']`
- `-f | --format [SINGLES|DOUBLES]`
- `-g | --gender [M|F]`
- `-l | --level [3.0|3.5|4.0|4.5|5.0]`
- `-s | --section [Eastern|Florida|Hawaii Pacific|Intermountain|Mid-Atlantic|Middle States|Midwest|Missouri Valley|New England|Northern California|Northern|Pacific NW|Southern|Southern California|Southwest|Texas|Unassigned]`
- `-o | --output [html|json]`

---

### `list [options]`

Lists the top players in a given section and level

- `-f | --format [SINGLES|DOUBLES]`
- `-g | --gender [M|F]`
- `-l | --level [3.0|3.5|4.0|4.5|5.0]`
- `-s | --section [Eastern|Florida|Hawaii Pacific|Intermountain|Mid-Atlantic|Middle States|Midwest|Missouri Valley|New England|Northern California|Northern|Pacific NW|Southern|Southern California|Southwest|Texas|Unassigned]`
- `-o | --output [html|json]`
- `-t | --top [20|50|100]`

---

### `subscribe [options]`

Subscribes to weekly email updates for a player's rankings

- `-n | --name ['FIRST LAST']`
- `-f | --format [SINGLES|DOUBLES]`
- `-g | --gender [M|F]`
- `-l | --level [3.0|3.5|4.0|4.5|5.0]`
- `-s | --section [Eastern|Florida|Hawaii Pacific|Intermountain|Mid-Atlantic|Middle States|Midwest|Missouri Valley|New England|Northern California|Northern|Pacific NW|Southern|Southern California|Southwest|Texas|Unassigned]`
- `-o | --output [html|json]`
- `-e | --email ['youremail@mail.com']`

---

### `unsubscribe [options]`

Unsubscribes to weekly email updates for a player's rankings

- `-n | --name ['FIRST LAST']`
- `-f | --format [SINGLES|DOUBLES]`
- `-g | --gender [M|F]`
- `-l | --level [3.0|3.5|4.0|4.5|5.0]`
- `-s | --section [Eastern|Florida|Hawaii Pacific|Intermountain|Mid-Atlantic|Middle States|Midwest|Missouri Valley|New England|Northern California|Northern|Pacific NW|Southern|Southern California|Southwest|Texas|Unassigned]`
- `-o | --output [html|json]`
- `-e | --email ['youremail@mail.com']`
