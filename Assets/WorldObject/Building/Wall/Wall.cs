using UnityEngine;
using System.Collections;

public class Wall : Building
{

    public override void SetSelection(bool selected, Rect playingArea)
    {
        SetSelectionNoRally(selected, playingArea);
    }

}