# Hangar Improvements Mod for Vanguard Galaxy

This is a BepInEx mod for the game Vanguard Galaxy that overhauls the hangar UI. It replaces the default arrow-based ship navigation with a detailed, scrollable list, making it easier to manage a large fleet of ships.

## How to Compile

This project is a standard .NET project. You can build it using the `dotnet` CLI or a C# IDE like Visual Studio or JetBrains Rider.

### 1. Add Dependencies

Before you can compile, you need to copy the necessary DLL files from your game installation into the `libs/` folder in this project.

1.  Navigate to your Vanguard Galaxy game installation directory.
2.  Find the following two locations:
    *   `VanguardGalaxy_Data/Managed/`
    *   `BepInEx/core/`

3.  Copy the following files into the `libs/` directory:

    **From `VanguardGalaxy_Data/Managed/`:**
    *   `Assembly-CSharp.dll`
    *   `UnityEngine.dll`
    *   `UnityEngine.CoreModule.dll`
    *   `UnityEngine.UI.dll`
    *   `Unity.TextMeshPro.dll`
    *   `UnityEngine.InputLegacyModule.dll`
    *   `UnityEngine.InputModule.dll`

    **From `BepInEx/core/`:**
    *   `BepInEx.dll`
    *   `0Harmony.dll`

    *(Note: The `.gitignore` file is configured to ignore the contents of the `libs` folder, so you don't have to worry about committing them to version control.)*

### 2. Build the Project

Once the dependencies are in place, open a terminal in the root of the `vanguard-galaxy-hangar` directory and run:

```bash
dotnet build --configuration Release
```

This will compile the mod. You will find the output `HangarImprovements.dll` inside the `bin/Release/netstandard2.1/` folder.

## How to Install

1.  Make sure BepInEx is installed in your Vanguard Galaxy game.
2.  Take the compiled `HangarImprovements.dll` from the `bin/` folder.
3.  Place it inside your `Vanguard Galaxy/BepInEx/plugins/` folder.
4.  Launch the game. The mod will be active.

## How to Publish to GitHub

This project is now in a local git repository. To publish it to GitHub:

1.  **Add all files and commit them:**
    ```bash
    git add .
    git commit -m "Initial commit"
    ```

2.  **Link to the GitHub repository:**
    ```bash
    git remote add origin https://github.com/fank/vanguard-galaxy-hangar.git
    ```

3.  **Push the code to the main branch:**
    ```bash
    git push -u origin main
    ```

Enjoy your new and improved hangar!
