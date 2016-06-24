using UnityEngine;
using RTS;

public class PauseMenu : Menu
{

    private Player player;

    protected override void Start()
    {
        base.Start();
        player = transform.root.GetComponent<Player>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Resume();
    }

    protected override void SetButtons()
    {
        buttons = new string[] { "Resume", "Main Menu" };
    }

    protected override void HandleButton(string text)
    {
        switch (text)
        {
            case "Resume": Resume(); break;
            case "Main Menu": ReturnToMainMenu(); break;
            default: break;
        }
    }

    private void ReturnToMainMenu()
    {
        Application.LoadLevel("MainMenu");
        Cursor.visible = true;
    }

    private void Resume()
    {
        Time.timeScale = 1.0f;
        GetComponent<PauseMenu>().enabled = false;
        if (player) player.GetComponent<UserInput>().enabled = true;
        ResourceManager.MenuOpen = false;
    }

}