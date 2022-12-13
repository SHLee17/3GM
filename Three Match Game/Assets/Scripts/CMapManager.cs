using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class CMapManager : CSingleton<CMapManager>
{
    float offsetX = 0.62f;
    float offsetHorizontalY = -0.35f;
    float offsetY = -0.72f;

    public Sprite sprBoard;
    public CBoard cBoard;
    public List<CBoard> cBoardList;
    public GameObject boardHolder;

    public int vertical;
    public int horizontal;

    void BoardListSet(int index, int y, int x, int eDirection)
    {
        if (x < horizontal && y < vertical)
            cBoardList[index].cLinkBoard[eDirection] = cBoardList[(y) * horizontal + (x)];
    }

    public void MapSetting(int _vertical, int _horizontal)
    {
        ClearBoardList();

        boardHolder = new GameObject("BoardHolder");
        boardHolder.transform.parent = transform;

        for (int y = 0; y < _vertical; y++)
        {
            for (int x = 0; x < _horizontal; x++)
            {
                cBoardList.Add(Instantiate(cBoard, boardHolder.transform));
                ETileColor RandTileColor = (ETileColor)Random.Range(0, (int)ETileColor.END);
                cBoardList[y * _horizontal + x].Init(RandTileColor, x, y);
                if (cBoardList[y * _horizontal + x] == null)
                    Debug.Log("null");
                if (x % 2 == 0)
                    cBoardList[y * _horizontal + x].transform.position = new Vector2((x * offsetX), (y * offsetY));
                else
                    cBoardList[y * _horizontal + x].transform.position = new Vector2((x * offsetX), (y * offsetY) + offsetHorizontalY);
            }
        }

        Camera.main.transform.position =
            new Vector3(cBoardList[cBoardList.Count - 1].transform.position.x / 2,
            cBoardList[cBoardList.Count - 1].transform.position.y / 2, -10);

        for (int y = 0; y < _vertical; y++)
        {
            for (int x = 0; x < _horizontal; x++)
            {
                int index = y * _horizontal + x;
                for (int i = 0; i < (int)EDirection.END; i++)
                {
                    switch (i)
                    {
                        case 0:
                            if (x % 2 == 0)
                            {
                                if (x > 0 && y > 0)
                                    BoardListSet(index, y - 1, x - 1, i);
                            }
                            else
                            {
                                BoardListSet(index, y, x - 1, i);
                            }
                            break;
                        case 1:
                            if (y > 0)
                                BoardListSet(index, y - 1, x, i);
                            break;
                        case 2:
                            if (x % 2 == 0)
                            {
                                if (y > 0 && x < _horizontal - 1)
                                    BoardListSet(index, y - 1, x + 1, i);
                            }
                            else
                            {
                                BoardListSet(index, y, x + 1, i);
                            }
                            break;
                        case 3:
                            if (x % 2 == 0)
                            {
                                if (x > 0)
                                    BoardListSet(index, y, x - 1, i);
                            }
                            else
                            {
                                if (y < _vertical - 1)
                                    BoardListSet(index, y + 1, x - 1, i);
                            }
                            break;
                        case 4:
                            if (y < _vertical - 1)
                            {
                                BoardListSet(index, y + 1, x, i);
                            }
                            break;
                        case 5:
                            if (x % 2 == 0)
                            {
                                if (x < _horizontal - 1)
                                    BoardListSet(index, y, x + 1, i);
                            }
                            else
                            {
                                if (y < _vertical - 1)
                                    BoardListSet(index, y + 1, x + 1, i);
                            }
                            break;
                    }
                }
            }
        }
    }

    public void ClearBoardList()
    {
        //DestroyImmediate(this.gameObject);
        cBoardList.Clear();
        if (transform.childCount > 0)
        {
            foreach (Transform item in transform)
            {
                DestroyImmediate(item.gameObject);
            }
        }
    }
}
