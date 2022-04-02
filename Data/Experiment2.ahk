ListStatFiles(DataDir)
{
	files := ""
	Loop Files, %DataDir%\*.json
	{
		files := files . A_LoopFileName . " "
	}
	return files
}

^+PgUp::
ExitApp
return

^+PgDn::
NumRunIters := 3

; assumes the assets are in list view, Unity maximized on 1920x1080 display
Subjects := []

Subjects.Push({ Name: "Pacman"
              , WindowTitlePrefix: "Pacman - game"
			  , AutomatedRuns: [ { Name: "Blind"
			                     , AssetCoord:     {X: 256, Y: 661}
								 , RunButtonCoord: {X: 1652, Y: 327}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\Pacman\blind" }
							   , { Name: "Null"
							     , AssetCoord:     {X: 256, Y: 678}
								 , RunButtonCoord: {X: 1659, Y: 305}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\Pacman\null" }
							   , { Name: "Smart"
							     , AssetCoord:     {X: 256, Y: 693}
								 , RunButtonCoord: {X: 1652, Y: 327}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\Pacman\smart" }
							   , { Name: "Symex"
							     , AssetCoord:     {X: 256, Y: 710}
								 , RunButtonCoord: {X: 1652, Y: 327}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\Pacman\symex"} ] })

Subjects.Push({ Name: "Tetris"
              , WindowTitlePrefix: "UnityTetris - Main"
			  , AutomatedRuns: [ { Name: "Blind"
			                     , AssetCoord:     {X: 256, Y: 661}
								 , RunButtonCoord: {X: 1714, Y: 325}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\Tetris\blind" }
							   , { Name: "Null"
							     , AssetCoord:     {X: 256, Y: 678}
								 , RunButtonCoord: {X: 1714, Y: 308}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\Tetris\null" }
							   , { Name: "Smart"
							     , AssetCoord:     {X: 256, Y: 693}
								 , RunButtonCoord: {X: 1714, Y: 325}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\Tetris\smart" }
							   , { Name: "Symex"
							     , AssetCoord:     {X: 256, Y: 710}
								 , RunButtonCoord: {X: 1714, Y: 325}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\Tetris\symex"} ] })


Subjects.Push({ Name: "Mario Bros"
              , WindowTitlePrefix: "SMB-clone - Main Menu"
			  , AutomatedRuns: [ { Name: "Blind"
			                     , AssetCoord:     {X: 256, Y: 661}
								 , RunButtonCoord: {X: 1699, Y: 386}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\MarioBros\blind" }
							   , { Name: "Null"
							     , AssetCoord:     {X: 256, Y: 678}
								 , RunButtonCoord: {X: 1715, Y: 347}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\MarioBros\null" }
							   , { Name: "Smart"
							     , AssetCoord:     {X: 256, Y: 693}
								 , RunButtonCoord: {X: 1699, Y: 386}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\MarioBros\smart" }
							   , { Name: "Symex"
							     , AssetCoord:     {X: 256, Y: 710}
								 , RunButtonCoord: {X: 1699, Y: 386}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\MarioBros\symex"} ] })

Subjects.Push({ Name: "Mario World"
              , WindowTitlePrefix: "science-mario - GameWorldScene"
			  , AutomatedRuns: [ { Name: "Blind"
			                     , AssetCoord:     {X: 256, Y: 661}
								 , RunButtonCoord: {X: 1710, Y: 348}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\MarioWorld\blind" }
							   , { Name: "Null"
							     , AssetCoord:     {X: 256, Y: 678}
								 , RunButtonCoord: {X: 1707, Y: 307}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\MarioWorld\null" }
							   , { Name: "Smart"
							     , AssetCoord:     {X: 256, Y: 693}
								 , RunButtonCoord: {X: 1710, Y: 348}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\MarioWorld\smart" }
							   , { Name: "Symex"
							     , AssetCoord:     {X: 256, Y: 710}
								 , RunButtonCoord: {X: 1710, Y: 348}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\MarioWorld\symex"} ] })

Subjects.Push({ Name: "Runner"
              , WindowTitlePrefix: "Unity-Awesome-Runner - MainMenu"
			  , AutomatedRuns: [ { Name: "Blind"
			                     , AssetCoord:     {X: 256, Y: 661}
								 , RunButtonCoord: {X: 1715, Y: 365}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\Runner\blind" }
							   , { Name: "Null"
							     , AssetCoord:     {X: 256, Y: 678}
								 , RunButtonCoord: {X: 1724, Y: 346}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\Runner\null" }
							   , { Name: "Smart"
							     , AssetCoord:     {X: 256, Y: 693}
								 , RunButtonCoord: {X: 1715, Y: 365}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\Runner\smart" }
							   , { Name: "Symex"
							     , AssetCoord:     {X: 256, Y: 710}
								 , RunButtonCoord: {X: 1715, Y: 365}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\Runner\symex"} ] })

Subjects.Push({ Name: "2048"
              , WindowTitlePrefix: "2048-unity - GameScene"
			  , AutomatedRuns: [ { Name: "Blind"
			                     , AssetCoord:     {X: 256, Y: 661}
								 , RunButtonCoord: {X: 1710, Y: 348}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\2048\blind" }
							   , { Name: "Null"
							     , AssetCoord:     {X: 256, Y: 678}
								 , RunButtonCoord: {X: 1711, Y: 307}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\2048\null" }
							   , { Name: "Smart"
							     , AssetCoord:     {X: 256, Y: 693}
								 , RunButtonCoord: {X: 1710, Y: 348}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\2048\smart" }
							   , { Name: "Symex"
							     , AssetCoord:     {X: 256, Y: 710}
								 , RunButtonCoord: {X: 1710, Y: 348}
								 , DataDir: "C:\Users\Sasha Volokh\Misc\UnitySymexCrawler\Data\Exp2Results\2048\symex"} ] })

CoordMode, Mouse, Client
SetTitleMatchMode, 1
for index, subject in Subjects
{
    WinActivate % subject.WindowTitlePrefix
	for index2, autoRun in subject.AutomatedRuns
	{
		Loop %NumRunIters%
		{
			Click, % autoRun.AssetCoord.X . " " . autoRun.AssetCoord.Y
			Sleep 1000
			
			Click, % autoRun.RunButtonCoord.X . " " . autoRun.RunButtonCoord.Y
			Sleep 1000
			
			initFiles := ListStatFiles(autoRun.DataDir)
			while ListStatFiles(autoRun.DataDir) == initFiles
			{
				Sleep 500
			}
			
			Sleep 1000
		}
	}
}

return