using System;
using System.Collections.Generic;
using System.Text;

namespace UnitySymexCrawler
{
    public class GameConfigs
    {
        public static readonly List<string> BASE_SEARCH_DIRECTORIES = new List<string>()
        {
            @"C:\Program Files\Unity\Hub\Editor\2020.3.28f1\Editor\Data\Managed\UnityEngine",
            @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\UnityScripts\SymexCrawler\Packages\InputSimulator.1.0.4\lib\net20",
            @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\UnityScripts\SymexCrawler\Packages\Microsoft.Z3.x64.4.8.10\lib\netstandard1.4",
            @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\UnityScripts\SymexCrawler\Packages\Microsoft.Data.Sqlite.Core.6.0.1\lib\netstandard2.0",
            @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\UnityScripts\SymexCrawler\Packages\YamlDotNet.11.2.1\lib\netstandard1.3",
            @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\UnityScripts\SymexCrawler\Packages\vjoyinterface.0.2.1.6\runtimes\win-x64\lib\net20",
            @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\UnityTetris\Library\PackageCache\com.unity.nuget.newtonsoft-json@2.0.0\Runtime"
        };

        public static readonly GameConfiguration GAME_CONFIG_TETRIS =
            new GameConfiguration(
                "Tetris",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\UnityTetris\Library\ScriptAssemblies\Assembly-CSharp.dll",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\UnityTetris\Assets\symex.tetris.db",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\UnityTetris\Assets\Scripts\PreconditionFuncs.cs",
                new List<string>()
                {
                    @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\UnityTetris\Assets\External\Demigiant\DOTween"
                });

        public static readonly GameConfiguration GAME_CONFIG_PACMAN =
            new GameConfiguration(
                "Pacman",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\Pacman\Library\ScriptAssemblies\Assembly-CSharp.dll",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\Pacman\Assets\symex.pacman.db",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\Pacman\Assets\Scripts\PreconditionFuncs.cs",
                new List<string>() 
                {
                });

        public static readonly GameConfiguration GAME_CONFIG_ASTEROIDS =
            new GameConfiguration(
                "Asteroids",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\Unity-3D-Asteroids\Library\ScriptAssemblies\Assembly-CSharp.dll",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\Unity-3D-Asteroids\Assets\symex.asteroids.db",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\Unity-3D-Asteroids\Assets\Asteroids\Scripts\PreconditionFuncs.cs",
                new List<string>()
                {
                });

        public static readonly GameConfiguration GAME_CONFIG_SMB =
            new GameConfiguration(
                "MarioBros",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\SMB-clone\Library\ScriptAssemblies\Assembly-CSharp.dll",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\SMB-clone\Assets\symex.smb.db",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\SMB-clone\Assets\Scripts\PreconditionFuncs.cs",
                new List<string>()
                {
                });

        public static readonly GameConfiguration GAME_CONFIG_SMW =
            new GameConfiguration(
                "MarioWorld",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\science-mario\Library\ScriptAssemblies\Assembly-CSharp.dll",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\science-mario\Assets\symex.smw.db",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\science-mario\Assets\Scripts\PreconditionFuncs.cs",
                new List<string>()
                {
                });

        public static readonly GameConfiguration GAME_CONFIG_RUNNER =
            new GameConfiguration(
                "Runner",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\Unity-Awesome-Runner\Library\ScriptAssemblies\Assembly-CSharp.dll",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\Unity-Awesome-Runner\Assets\symex.runner.db",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\Unity-Awesome-Runner\Assets\Scripts\PreconditionFuncs.cs",
                new List<string>()
                {
                });

        public static readonly GameConfiguration GAME_CONFIG_2048 =
            new GameConfiguration(
                "2048",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\2048-unity\Library\ScriptAssemblies\Assembly-CSharp.dll",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\2048-unity\Assets\symex.2048.db",
                @"C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Subjects\2048-unity\Assets\Scripts\PreconditionFuncs.cs",
                new List<string>()
                {
                });

        public static readonly GameConfiguration[] ALL_CONFIGS = new GameConfiguration[]
        {
            GAME_CONFIG_PACMAN,
            GAME_CONFIG_TETRIS,
            GAME_CONFIG_ASTEROIDS,
            GAME_CONFIG_SMB,
            GAME_CONFIG_SMW,
            GAME_CONFIG_RUNNER,
            GAME_CONFIG_2048
        };
    }
}
