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
            @"C:\Users\Sasha Volokh\Misc\AutoExplore\SymexExperiments\Pacman\Library\PackageCache\com.unity.nuget.newtonsoft-json@2.0.0\Runtime"
        };

        public static readonly GameConfiguration GAME_CONFIG_TETRIS =
            new GameConfiguration(
                "symex.tetris.db",
                @"C:\Users\Sasha Volokh\Misc\UnityTetris\Library\ScriptAssemblies\Assembly-CSharp.dll",
                new List<string>()
                {
                    @"C:\Users\Sasha Volokh\Misc\UnityTetris\Library\ScriptAssemblies",
                    @"C:\Users\Sasha Volokh\Misc\UnityTetris\Assets\External\Demigiant\DOTween",
                    @"C:\Users\Sasha Volokh\Misc\UnityTetris\Assets\Scripts\SymexCrawler\Packages\InputSimulator.1.0.4\lib\net20",
                    @"C:\Users\Sasha Volokh\Misc\UnityTetris\Assets\Scripts\SymexCrawler\Packages\Microsoft.Z3.x64.4.8.10\lib\netstandard1.4",
                    @"C:\Users\Sasha Volokh\Misc\UnityTetris\Assets\Scripts\SymexCrawler\Packages\Microsoft.Data.Sqlite.Core.6.0.1\lib\netstandard2.0",
                    @"C:\Users\Sasha Volokh\Misc\UnityTetris\Library\PackageCache\com.unity.nuget.newtonsoft-json@2.0.0\Runtime"
                });

        public static readonly GameConfiguration GAME_CONFIG_PACMAN =
            new GameConfiguration(
                "symex.pacman.db",
                @"C:\Users\Sasha Volokh\Misc\AutoExplore\SymexExperiments\Pacman\Library\ScriptAssemblies\Assembly-CSharp.dll",
                new List<string>() {
                    @"C:\Users\Sasha Volokh\Misc\AutoExplore\SymexExperiments\Pacman\Library\ScriptAssemblies"
                });

        public static readonly GameConfiguration GAME_CONFIG_ASTEROIDS =
            new GameConfiguration(
                "symex.asteroids.db",
                @"C:\Users\Sasha Volokh\Misc\Unity-3D-Asteroids\Library\ScriptAssemblies\Assembly-CSharp.dll",
                new List<string>()
                {
                });

        public static readonly GameConfiguration GAME_CONFIG_SMW =
            new GameConfiguration(
                "symex.smw.db",
                @"C:\Users\Sasha Volokh\Misc\science-mario\Library\ScriptAssemblies\Assembly-CSharp.dll",
                new List<string>()
                {
                });

        public static readonly GameConfiguration GAME_CONFIG_2048 =
            new GameConfiguration(
                "symex.2048.db",
                @"C:\Users\Sasha Volokh\Misc\2048-unity\Library\ScriptAssemblies\Assembly-CSharp.dll",
                new List<string>()
                {
                });

        public static readonly GameConfiguration GAME_CONFIG_SPACEMAN =
            new GameConfiguration("symex.spaceman.db",
                @"C:\Users\Sasha Volokh\Misc\SpaceMan-Game\SpaceMan Platzi\Library\ScriptAssemblies\Assembly-CSharp.dll",
                new List<string>()
                {
                });
    }
}
