using UnityEngine;
using RTS;

public class LMenu : Menu
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
        string message = "You won!";
        GUI.Label(new Rect(leftPos, topPos, Screen.width - 2 * padding, itemHeight), message);

    }

    private void ReturnToMainMenu()
    {
        Application.LoadLevel("MainMenu");
        Cursor.visible = true;
    }

}