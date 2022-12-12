using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteManager",menuName = "ScriptableObject/SpriteManager",order = 1)]
public class CSpriteManager : ScriptableObject
{
    public List<Sprite> sprBoardList;
    public List<Sprite> sprNormalTileList;
    public List<Sprite> sprSpecialTileList;
    public List<Sprite> sprIceBoxList;
}
