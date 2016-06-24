using UnityEngine;
using System.Collections;
using RTS;
public class Base : Building
{

    public override void SetSelection(bool selected, Rect playingArea)
    {
        SetSelectionNoRally(selected, playingArea);
    }

    public void OnDestroy()
    {
        HUD[] huds = FindObjectsOfType(typeof(HUD)) as HUD[];
        foreach (HUD hud in huds)
            if (hud.timer>0)
                OpenPauseMenu();
    }

    private void OpenPauseMenu()
    {
        Time.timeScale = 0.0f;
        LossMenu[] menus = FindObjectsOfType(typeof(LossMenu)) as LossMenu[];
        foreach (LossMenu menu in menus)
            menu.enabled = true;
        GetComponentInParent<UserInput>().enabled = false;
        Cursor.visible = true;
        ResourceManager.MenuOpen = true;
    }
}