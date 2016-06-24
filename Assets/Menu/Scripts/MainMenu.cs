using UnityEngine;
using System.Collections;
using RTS;

public class MainMenu : Menu
{

    protected override void SetButtons()
    {
        buttons = new string[] { "Start Game", "Quit Game" };
    }

    protected override void HandleButton(string text)
    {
        switch (text)
        {
            case "Start Game": StartGame(); break;
            case "Quit Game": ExitGame(); break;
            default: break;
        }
    }

    private void StartGame()
    {
        ResourceManager.MenuOpen = false;
        Application.LoadLevel("Game");
        //makes sure that the loaded level runs at normal speed
        Time.timeScale = 1.0f;
    }
}