# Overview
This repository provides the implementation of our paper "Static Analysis for Automated Identification of Valid Game Actions During Exploration" (FDG 2022).

In this repository is the implementation of our approach for the Unity game engine. We've provided:

1. The offline analysis tool for computing the possible actions in terms of execution paths through the input-handling code.
2. The Unity scripts that use the offline information to (a) compute the set of valid actions in the current state and (b) simulate a chosen action.
3. An example random exploration agent that uses our action analysis to automatically perform relevant random actions on the game.

# Limitations
- This is a prototype implementation with limited scope, which is as follows:
    - It supports key, button, or axis Input APIs (i.e. no mouse/touch support), and does not support the new Input System APIs
    - Inputs handled inside of co-routines are not supported
- The provided code relies on Windows-specific APIs and therefore will only run on the Windows operating system (either Windows 10 or 11)

# Games
The following open-source Unity games were used in our paper's experiments and give an idea of what kinds of input-handling code our prototype implementation supports:
- [Pacman](https://github.com/vilbeyli/Pacman)
- [UnityTetris](https://github.com/Mukarillo/UnityTetris)
- [SMB-clone](https://github.com/linhdvu14/SMB-clone)
- [science-mario](https://github.com/lucasnfe/science-mario)
- [Unity-Awesome-Runner](https://github.com/rajandeepsingh13/Unity-Awesome-Runner)
- [2048-unity](https://github.com/jamiltron/2048-unity)

# Usage
In this section we provide instructions on how to get the random exploration agent running for a Unity game (along with the associated code for determining/simulating valid game actions).

Requirements:
- Unity (tested on 2020.3.28f1, but will likely work with other versions as well)
- Visual Studio 2022

## 1. Build the offline analysis tool

Clone the repository with `git clone`. Open `UnityActionAnalysis.sln` with Visual Studio. Go to Build -> Build Solution, then go to Build -> Rebuild Solution (two rounds of build are necessary to copy over all the libraries correctly). 

## 2. Create a configuration file for the offline tool

Create a JSON configuration file to specify where the compiled game assembly is located and where to place the output files. Refer to the following example:
```
{
    "assemblyPath": "C:\\Users\\svolokh\\repos\\UnityTetris\\Library\\ScriptAssemblies\\Assembly-CSharp.dll",
    "databaseOutputDirectory": "C:\\Users\\svolokh\\repos\\UnityTetris",
    "scriptOutputDirectory": "C:\\Users\\svolokh\\repos\\UnityTetris\\Assets\\Scripts",
    "assemblySearchDirectories": [
        "C:\\Program Files\\Unity\\Hub\\Editor\\2020.3.28f1\\Editor\\Data\\Managed\\UnityEngine"
    ],
    "ignoreNamespaces": []
}
```

a) Specify the path to the compiled game assembly in `assemblyPath`, this will be `<root>\Library\ScriptAssemblies\Assembly-CSharp.dll`. If you do not see this file then you need to run your game at least once in Unity.

b) Specify the directory where to output the analysis database in `databaseOutputDirectory`, this can just be the root directory of your Unity project.

c) Specify the folder into which to generate the helper script in `scriptOutputDirectory`, this will usually be `<root>\Assets\Scripts`.

d) Indicate additional directories to search for assemblies in an array for `assemblySearchDirectories`. This will usually just be a single path to the Unity's libraries. If your game is split across multiple assemblies (usually not the case) then indicate the directories of these assemblies as well.

e) Indicate any namespaces to exclude from the analysis in the `ignoreNamespaces` field.

## 3. Run the offline tool 

Run the offline tool via command-line: `OfflineAnalysis.exe <config>`, which will produce output similar to the following:

```
Performing offline analysis of Assembly-CSharp.dll
Processing TetrisEngine.GameLogic.Update()
        Analyzing branches
        Running symbolic execution
        Writing path information to database
        Generating code
Generating file C:\Users\svolokh\repos\UnityTetris\Assets\Scripts\PreconditionFuncs.cs
Wrote database C:\Users\svolokh\repos\UnityTetris\paths.db
```

The tool should be re-run if there are any changes made to the game code.

## 4. Copy scripts for identifying and performing actions

Copy (or symlink) all three of the folders in the `UnityScripts` directory of the repository into your Unity game's Assets/Scripts folder.

## 5. Create exploration agent game object

In the scene from which you want to begin the automated exploration, create a new game object. In the Inspector, add the component "Random Agent":

<p align="center">
<img width="763" height="273" src="https://user-images.githubusercontent.com/61521182/183319497-ca4567a2-3da6-4e90-8485-73a077993ee4.png">
</p>

a) Specify the absolute path to the `paths.db` file created by the offline tool in `Analysis Database Path`.

b) Specify the absolute path to your project's InputManager.asset in `Input Manager Settings Path`, this is located in `<root>\ProjectSettings\InputManager.asset`. This is required for looking up the necessary key codes / joystick buttons for simulating the relevant inputs.

c) If your project is not already configured as such, make sure Asset Serialization Mode is set to `Force Text`.

## 6. Run the game

You will now see the random agent playing the game by simulating the appropriate key-presses (determined automatically by our analysis). The log below details which actions are being chosen.

<p align="center">
<img width="754" height="309" src="https://user-images.githubusercontent.com/61521182/183319964-5d362e03-d9ab-4e2f-9c9e-2504390eb05b.png">
</p>

## 7. (Optional) Instrument the game assembly to simulate inputs directly

The default configuration of the agent is to simulate actual key-presses through the Windows device APIs, which means the game focus cannot be lost or else the key events will be sent to other applications. Our implementation can solve this problem by instrumenting the `Input` APIs such that they can be simulated directly. 

Pull up the offline tool, and this time run `OfflineAnalysis.exe --instrument <config>`, which should produce an output such as:
```
Instrumenting input API calls of C:\Users\svolokh\UnityTetris\Library\ScriptAssemblies\Assembly-CSharp.dll
```
Now the game assembly is modified so that inputs can be simulated directly. Go back to the Unity project, and enable the `Use Instrumentation Input Simulator` option for the Random Agent. Now start the game and the agent will run the same as before, except now you can freely tab out and do other activities and the exploration will continue.
**Important note:** since this is modifying the compiled assembly directly, if any of the game code changes (such that Assembly-CSharp.dll is re-compiled), then the instrumentation will be lost and you will need to re-run the above command.
