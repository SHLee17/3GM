using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum EBoardState
{
    CLOSE,
    OPEN,
    RESPAWN
}

public class CBoard : MonoBehaviour
{

    public void Init(ETileColor _eTileColor = ETileColor.BLUE,
       int _x = 0, int _y = 0,
       Vector2 _pos = default,
       EBoardState _eBoardState = EBoardState.OPEN,
       EObstructionType _eObstructionType = EObstructionType.NONE)
    {
        cTile.eTileColor = _eTileColor;
        eBoardState = _eBoardState;
        cTile.eObstructionType = _eObstructionType;
        gameObject.transform.position = _pos;
        gameObject.name = _y + " , " + _x;
        txtNumbering.text = _y + " , " + _x;
        x = _x;
        y = _y;
        cLinkBoard = new List<CBoard>();
        for (int i = 0; i < (int)EDirection.END; i++)
        {
            cLinkBoard.Add(null);
        }
    }

    public int x;
    public int y;
    public SpriteRenderer sprRenderer;

    public Text txtNumbering;
    public CTile cTile;

    public List<CBoard> cLinkBoard;
    public EBoardState eBoardState;
    public bool isMoving;

    private void Start()
    {
        isMoving = false;
        if (eBoardState == EBoardState.CLOSE)
        {
            cTile.eTileColor = ETileColor.END;
            cTile.eTileType = ETileType.NOMAL;
            cTile.sprRenderer.sprite = null;
        }

        if (cLinkBoard.TrueForAll(b => b == null))
        {
            throw new Exception();
        }
    }

    private void Update()
    {
        if (isMoving && cTile != null)
        {
            float speed = Time.deltaTime * (cTile.speed);
            cTile.transform.SetParent(transform);
            cTile.transform.localPosition = Vector3.MoveTowards(cTile.transform.localPosition, Vector3.zero, speed);

            if (Vector3.Distance(cTile.transform.localPosition, Vector3.zero) == 0)
            {
                cTile.speed += 10;
                isMoving = false;
            }
        }
    }
    public void DestroyTileBoardState(bool onOff)
    {
        if (eBoardState != EBoardState.CLOSE)
        {
            if (onOff)
            {
                sprRenderer.sprite = CObjectPool.Instance.SOspriteManger.sprBoardList[1];
                sprRenderer.sortingOrder = 99;
                cTile.sprRenderer.sortingOrder = 99;
                cTile.arrowSortingGroup.sortingOrder = 100;

            }
            else
            {
                sprRenderer.sprite = CObjectPool.Instance.SOspriteManger.sprBoardList[0];
                sprRenderer.sortingOrder = 0;
                if (cTile != null)
                {
                    cTile.sprRenderer.sortingOrder = 1;
                    cTile.arrowSortingGroup.sortingOrder = 4;
                }
            }
        }
    }

    public bool CheckTileMatch(int index)
    {
        if (eBoardState == EBoardState.CLOSE)
            return false;
        if (cLinkBoard[index] == null)
            return false;
        if (!cTile)
            return false;
        if (!cLinkBoard[index].cTile)
            return false;

        if (cTile.MatchPossible() && cLinkBoard[index].cTile.MatchPossible())
        {
            if (cTile.eTileColor == cLinkBoard[index].cTile.eTileColor)
                return true;
        }

        return false;
    }
    void RecursiveLink(int index)
    {
        if (LinkBoardCheck(index))
        {
            if (LinkTileCheck(index))
            {
                if (!CObjectPool.Instance.DuplicateList(cLinkBoard[index]))
                {
                    cLinkBoard[index].cTile.isDestroyTile = true;
                    cLinkBoard[index].DestroyTileBoardState(true);
                    CObjectPool.Instance.destructionList.Add(cLinkBoard[index]);
                    if (!cTile.cObstruction.hasDamaged)
                        cTile.cObstruction.Damage();
                }
            }
            //Debug.Log(cLinkBoard[index]);
            cLinkBoard[index].RecursiveLink(index);
        }
    }
    public void LineBreak(EDirection index)
    {
        if (index == EDirection.UP_L || index == EDirection.DOWN_R)
        {
            RecursiveLink((int)EDirection.UP_L);
            RecursiveLink((int)EDirection.DOWN_R);
        }
        else if (index == EDirection.UP_R || index == EDirection.DOWN_L)
        {
            RecursiveLink((int)EDirection.UP_R);
            RecursiveLink((int)EDirection.DOWN_L);
        }
        else
        {
            //if (!CObjectPool.Instance.DuplicateList(this))
            //    CObjectPool.Instance.destructionList.Add(this);
            RecursiveLink((int)EDirection.UP);
            RecursiveLink((int)EDirection.DOWN);
        }
    }
    public bool LinkBoardCheck(int index)
    {
        return cLinkBoard[index] != null;
    }

    public bool LinkTileCheck(int index)
    {
        return cLinkBoard[index].cTile != null;
    }

    public void DestroyTile()
    {
        if (cTile != null && !cTile.isNew)
        {
            foreach (var item in CObjectiveUI.Instance.cObjectiveList)
            {
                if (item.eTileColor == cTile.eTileColor)
                {
                    item.count -= 1;
                    if (item.count <= 0)
                        item.count = 0;
                }
            }
            DestroyTileBoardState(false);
            CObjectPool.Instance.InsertTile(cTile);
            cTile = null;
        }

    }
}

