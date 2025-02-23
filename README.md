# DesktopSwitcher

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

DesktopSwitcher is an application that allows users to switch between different desktop configurations on Windows. The application saves icon positions for each desktop profile.

## Usage 🚀

- The application will start and display a system tray icon.
- Right-click the tray icon to access the context menu.
- Use the "Desktops" menu to switch between different desktop profiles.
- Use the "Config" menu to reload or edit the configuration file.

## Configuration ⚙️

The application uses a [TOML](https://toml.io) file to store configurations. The file is located in the application's base directory. You can edit this file to add or modify desktop folders.

### Example `config.toml`
```
# Start with Windows
run_at_startup = true

[[desktop_folder]]
name = "Work"
path = "C:\Users\YourUsername\Desktops\Work"

[[desktop_folder]]
name = "Personal"
path = "C:\Users\YourUsername\Desktops\Personal"
```

### Configuration Options
- `run_at_startup` Auto-start with Windows (true/false)
- `desktop_folder` Array of profile configurations
	- `name` Display name in menu
	- `path` Absolute path to desktop subfolder

## Contributing 🤝

Contributions are welcome! Please fork the repository and submit a pull request with your changes.

## Acknowledgements 🙏

- [Avalonia](https://avaloniaui.net/)
- [Tomlyn](https://github.com/xoofx/Tomlyn)
