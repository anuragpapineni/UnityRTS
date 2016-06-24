using UnityEngine;
using RTS;

public class LossMenu : Menu
{

    private Player player;

    protected override void Start()
    {
        base.Start();
        player = transform.root.GetComponent<Player>();
    }

    protected override void SetButtons()
    {
        buttons = new string[] { "Main Menu" };
    }

    protected override void HandleButton(string text)
    {
        switch (text)
        {
            case "Main Menu": ReturnToMainMenu(); break;
            default: break;
        }
    }

    protected override void SetText(float topPos, float leftPos)
    {
        float padding = ResourceManager.Padding;
        float itemHeight = ResourceManager.ButtonHeight;
        float buttonWidth = ResourceManager.ButtonWidth;
        HUD hud = GetComponent<HUD>();
        float time = hud.timer;
        string message;
        if (time < 0)
            message = "Time's Up! You Lost!";
        else
            message = "You win! Score: "+(int)time;
        GUI.Label(new Rect(leftPos, topPos, Screen.width - 2 * padding, itemHeight), message);

    }

    private void ReturnToMainMenu()
    {
        Application.LoadLevel("MainMenu");
        Cursor.visible = true;
    }

}