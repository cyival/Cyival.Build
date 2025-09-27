# Cyival.Build

[![Test on Commit](https://github.com/cyival/Cyival.Build/actions/workflows/test.yml/badge.svg)](https://github.com/cyival/Cyival.Build/actions/workflows/test.yml)
![NuGet Version](https://img.shields.io/nuget/v/Cyival.Build.Cli)
![GitHub License](https://img.shields.io/github/license/cyival/Cyival.Build)


A build tool for Godot projects.

## Features

- [x] Detecting Godot installations from system, GodotEnv and ...
- [ ] Building multiple projects as applications or packs(.pck)
- [ ] And more!

## Get Started

To start using **Cyival.Build**, you can install the CLI tool via NuGet:

```bash
dotnet tool install --global Cyival.Build.Cli
```

Let's say you have a Godot project located at `./MyGodotProject`. Now create a manifest file named `build.toml` in the upper directory of your project with the following content:

```toml
# The minimal version of Cyival.Build required to build this project.
minimal-version = 0.1

[build.godot]
version = "4.4" # Required, specify the Godot version to use for building.

[targets.project]
type = "godot" # Optional, default is "godot"
path = "./MyGodotProject"
```

You can customize the `build.toml` file further based on your project's needs. For more details, refer to the [documentation](https://github.com/cyival/Cyival.Build).

Now you should have a file structure like this:

```
./
├── build.toml
└── MyGodotProject/
    ├── project.godot
    └── ...
```

And then you can build your project by running the following command:

```bash
cybuild build ./MyGodotProject
```

Or by simply if you are in the project directory:

```bash
cybuild
```

The built application will be located in the `./out` directory by default.

## License

**Cyival.Build** is open-source software licensed under the Apache License, Version 2.0
