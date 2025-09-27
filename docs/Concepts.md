# Concepts

Here are some concepts you should know before using Cyival.Build.

## Manifest

The manifest is a TOML file named `build.toml` that describes how to build your application. It contains information about the project, build targets, and other configurations.

## How does it build?

There are three main things to process with in Cyival.Build:

1. **Environment**: This includes all the environment variables and paths that are needed for the build process.
2. **Configuration**: This includes the settings and options specified in the `build.toml` file.
3. **Builder**: This is the component responsible for executing the build process, using the environment and configuration information.

The three components work together to ensure that your application is built correctly and efficiently. They're provided by the plugin system, allowing for flexibility and customization.

Next, let's check out the build process.

1. Create a `BuildApp` instance.
2. Load the manifest file (`build.toml`).
3. Initialize the environment based on the manifest and system settings.
4. Read the configuration options from the manifest.
5. Create the builder instance with the environment and configuration.
6. Execute the build process by using `BuildTargetApp`.

## Plugin System

The plugin system in Cyival.Build allows you to extend and customize the build process. You can create your own plugins to add new features or modify existing behavior. Plugins can be written in C# and must inherit from the `BuildPlugin` class.

To write a plugin, you need to:

1. Create a new class that inherits from the `BuildPlugin` class.
2. Add a `Plugin` attribute with your custom plugin id.
3. The plugin will be automatically discovered and loaded once if it's loaded to CLR.

Plugins can be used to add custom build steps, integrate with external tools, or modify the build environment.

The default plugins is under the `Cyival.Build.Plugin.Default` namespace. You can refer to the source code for examples of how to create your own plugins.