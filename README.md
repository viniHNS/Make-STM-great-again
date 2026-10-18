<div align="center">

# Make STM Great Again!

A server mod for SPT 4.1.6 that buffs the STM-9 Gen.2 and adds a full auto version of it, sold by Mechanic, with fire mode switch animation and a proper full auto sound.

![Version](https://img.shields.io/badge/version-1.1.0-orange?style=flat)
![SPT](https://img.shields.io/badge/SPT-4.1.6-blue?style=flat)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat&logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-green?style=flat)

[Features](#features) · [Install](#install) · [Configuration](#configuration) · [Build](#build-from-source)

</div>

---

## Features

### STM-9 Gen.2

| Change | Game | Mod |
|---|---|---|
| Ergonomics | 50 | 53 |

Everything else stays vanilla (semi-auto only).

### STM-9 Gen.2 Full Auto (new weapon)

Same carbine, same parts and the same weapon mastery as the STM-9, plus:

| Feature | Value |
|---|---|
| Fire modes | Single + full auto |
| Fire rate | 840 RPM |
| Ergonomics | 53 |
| Fire mode switch | M4A1 animation: the right thumb flips the selector and the lever stays in the selected position |
| Firing sound | SIG MPX (full auto loop at 840 RPM, with suppressed variant) |

| Trader | Loyalty | Price | Limit |
|---|---|---|---|
| Mechanic | LL1 | 2× Weapon parts + 1× Gunpowder "Kite" | 2 |
| Mechanic | LL2 | 52,000 ₽ | 3 |

---

## Install

Extract `Make-STM-great-again.zip` into your SPT game folder:

```
<game folder>/
└── SPT_Runtime/user/mods/Make-STM-great-again/
    ├── MakeSTMGreatAgain.dll
    ├── config.json
    ├── bundles.json
    └── bundles/
        └── weapon_stmarms_stm_9_9x19_auto_container.bundle
```

Server only, no client plugin needed.

> Removing the mod breaks profiles that own a STM-9 Full Auto. Sell or discard it first.

**Upgrading from the SPT 4.0 version:** SPT 4.1 uses `SPT_Runtime` instead of `SPT`. Delete the old `SPT/user/mods/Make-STM-great-again/` folder if you copied it into the new install. The regular STM-9 is semi-auto again; full auto moved to the STM-9 Full Auto.

---

## Configuration

`SPT_Runtime/user/mods/Make-STM-great-again/config.json`:

| Option | Default | Description |
|---|---|---|
| `debug` | `false` | Logs what the mod changed at startup. Errors are always logged. |

---

## Build from Source

**Requirements:** .NET 10 SDK.

```sh
dotnet build MakeSTMGreatAgain.sln -c Release
```

The build creates `Make-STM-great-again.zip` in the solution folder.

> The `.csproj` copies the build output into `D:\Jogos\SPT4.1` for testing when that folder exists. Change `SptModsDir` to your own SPT folder. Close the SPT server before building, or the copy fails because the DLL is in use.

### Project Structure

```
Make-STM-great-again/
├── MakeSTMGreatAgain.sln
└── Server/                         .NET 10 server mod
    ├── Mod.cs                      mod metadata
    ├── ModConfig.cs                config.json loader
    ├── config.json                 mod settings
    ├── Stm9Changes.cs              STM-9 ergonomics
    ├── Stm9FullAutoLoader.cs       STM-9 Full Auto item, preset, mastery and Mechanic offers
    ├── bundles.json                bundle manifest read by the SPT server
    └── bundles/                    STM-9 Full Auto container (fire mode switch animation, MPX sound)
```

---

## Resources

| Resource | URL |
|---|---|
| SPT Server C# | https://github.com/SP-Tushonka/server-csharp |
| Server Mod Examples | https://github.com/SP-Tushonka/server-mod-examples |
| SPT Wiki — Modding Resources | https://wiki.sp-tushonka.com/en/modding/Modding_Resources |
| SPT Scaffold | https://github.com/viniHNS/spt-scaffold |

---

## License

This mod is licensed under the [MIT License](LICENSE).
