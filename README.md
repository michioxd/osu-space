# osu!space (osu! ruleset)

[![Join our Discord server](https://img.shields.io/discord/1286351304171585607?style=plastic&logo=discord&logoColor=fff&label=Join%20our%20Discord%20server&labelColor=5764f1
)](https://discord.gg/stpVy4GkVJ) [![GitHub Downloads (all assets, latest release)](https://img.shields.io/github/downloads/michioxd/osu-space/latest/total?sort=date&style=plastic&logo=github)](https://github.com/michioxd/osu-space/releases/latest)

A custom game mode for [osu!(lazer)](https://github.com/ppy/osu) based on [Sound Space (Roblox)](https://www.roblox.com/games/2677609345/Sound-Space-Rhythm-Game) and [Sound Space Plus (a.k.a Rhythia)](https://github.com/Rhythia/sound-space-plus).

This project is currently **in early development**. While some features have been implemented, you may encounter many bugs. Please feel free to open an issue if you find any.

Please note that breaking changes may occur in the future as I continue to learn how osu! and the osu!framework work. Expect instability and bugs.

| Main gameplay | Customization |
|---|---|
| ![](https://github.com/user-attachments/assets/22e24ad0-c870-432e-883a-8e2e844c6bc1) ![](https://github.com/user-attachments/assets/12fe79b0-092a-4e37-ab2b-d55c3907c79b) | ![](https://github.com/user-attachments/assets/6a384060-5290-4219-b712-d041ddfff09b) |

https://github.com/user-attachments/assets/7eb79bfb-90f1-409a-8aa0-f08481fa6481

## Features

- Unique hit objects and gameplay mechanics inspired by Sound Space.
- Map star rating calculation. (WIP)
- Customizable playfield:
    - Cursor size and trail.
    - Playfield settings (grid, border, scale, parallax).
    - Note color palettes (some colors extracted from SSP / Rhythia).
    - Note properties (thickness, corner radius, opacity, scale, approach rate, spawn distance, fade, glow).
- Online check for updates.
- Import `.sspm` files (Sound Space Plus maps, v1 and v2) from Sound Space Plus (Rhythia) (WIP).
- Editor (WIP / Not able to save maps yet).

## Download

Visit the [Releases](https://github.com/michioxd/osu-space/releases) page to download the latest version of osu!space.

[Click here to download osu!space directly](https://github.com/michioxd/osu-space/releases/latest/download/osu.Game.Rulesets.Space.dll)

## Installation

0. Make sure your osu!(lazer) is up to date.
1. Download the [`osu.Game.Rulesets.Space.dll`](https://github.com/michioxd/osu-space/releases/latest/download/osu.Game.Rulesets.Space.dll) from the [Releases page](https://github.com/michioxd/osu-space/releases).
2. Copy the downloaded DLL file to your osu! data directory. This is usually located at:
    - Windows: `C:\Users\<YourUsername>\AppData\Roaming\osu\rulesets` or `%APPDATA%\osu\rulesets`.
    - Linux: `~/.local/share/osu/rulesets`
    - Android: `/storage/emulated/0/Android/data/sh.ppy.osulazer/files/rulesets`.
    - iOS: `who knows lol`.
    - macOS: `~/Library/Application Support/osu/rulesets`.
3. Restart osu!(lazer) if it was running.

## Todos

- [x] Basic converter from osu!standard
- [x] Basic gameplay
- [x] SSPM v1/v2 converter/importer
- [ ] Note speed change event
- [x] Quantum note
- [ ] Editor (in progress)

## A Note About the Editor

The editor is still in early development and is not fully functional yet. Currently, only basic features are implemented, such as placing hit objects and a basic timeline. You cannot save your maps yet (ask peppy). Stay tuned for more updates on the editor!

![](https://github.com/user-attachments/assets/08a52111-b6f7-48bf-8d84-962aa466273f)

## Contributing

Contributions are welcome! If you'd like to contribute, please fork the repository and create a pull request.

You can also contribute new color palettes for the game by opening an issue if you prefer not to code.

## Have fun

Interested? Support meeee!!

[![BuyMeACoffee](https://raw.githubusercontent.com/pachadotdev/buymeacoffee-badges/main/bmc-yellow.svg)](https://www.buymeacoffee.com/michioxd)
