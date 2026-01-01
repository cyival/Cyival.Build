# Working With Multiple Projects

Cyival.Build can manage multiple projects in a single repository. This is particularly useful for monorepos or when you have several related projects that share common configurations.

## Project Structure

A typical structure for multiple projects might look like this:

```
/repo
  build.toml
    /project1
        /src
        /tests
    /project2
        /src
        /tests
    /project3
        /src
        /tests
    ...
```

In this structure, the root `build.toml` file contains the configuration for all projects. Each project has its own directory with source code and tests.

## Configuring Multiple Projects

Here's an example manifest file (`build.toml`) that defines multiple projects:

```toml
minimal-version = 0.1

[build.godot]
version = "4.5"

[target.project1]
default = true
path = "project1"
godot = {version = "4.4"}

[target.project2]
path = "project2"
out = "utils/module"
godot = {export-pack = true}

[target.project3]
path = "project3"
out = "utils/tool"
```

Now, let's break down the configuration:

- The `[build.godot]` section specifies the default Godot version for all projects.
- The `[target.project1]` section configures the first project, setting its path and Godot version.
- The `[target.project2]` section configures the second project, setting its path and enabling export packing.
- The `[target.project3]` section configures the third project, setting its path without any additional options.

Obviously, you can set different options for each project as needed. But notice that the options provided per-project will merge with the global options defined in the `[build.godot]` section. This means that if a project does not specify a particular option, it will inherit the value from the global configuration.

The `out` option specifies the output directory for the built project.

The `default = true` option in the first project indicates that this project will be built by default when no specific target is specified.

## Requirements between projects
Simply add `requirements = ["project2", ..]` to section `target.project1`, and then when the `project1` being built, `project2` (and ..) will be built too.


```toml

[target.project1]
default = true
path = "project1"
godot = {version = "4.4"}
requirements = ["project2"] # <--- Here it is.

[target.project2]
path = "project2"
out = "utils/module"
godot = {export-pack = true}
```

Note that the output path of targets is always based to the output directory (usually `out/`).
