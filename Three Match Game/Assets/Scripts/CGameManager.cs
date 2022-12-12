using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum EDirection
{
    UP_L, UP, UP_R,
    DOWN_L, DOWN, DOWN_R,
    END
}
public enum ETileColor
{
    BLUE,
    GREEN,
    PURPLE,
    YELLOW,
    RED,
    ORANGE,
    END
}

public enum EWorkingProcess
{
    CHECK_POSSIBLE_MOVE,
    IMPOSSIBLE_MOVE,
    POSSIBLE_MOVE,
    SWAP,
    FAILE,
    SUCCESS,
    CHAIN_BURST,
    SPECIAL_TILE_MOVEMENT,
    DESTROY_TILE,
    MOVE_DOWN,
    ALL_STOP,
    CLEAR,
    END

}
public enum EMatchType
{
    LINE4 = 100,
    LINE5 = 200,
    LINE3X3 = 300,
    END
}

public class CGameManager : MonoBehaviour
{
    [SerializeField]
    public class CAssist
    {
        public CAssist(CBoard _board, int _direction, int _guideDirection)
        {
            board = _board;
            direction = _direction;
            guideDirection = _guideDirection;
        }
        public CBoard board;
        public int direction;
        public int guideDirection;
    }

    float tileSpeed;
    bool txtOnOff;

    bool isSpecialMovment;
    int rangeCheck;

    public EWorkingProcess eWorkingProcess;
    EWorkingProcess eDebugWP;
    bool isDraging;
    CMapManager cMap;
    CMultyDictionary<int, CBoard> cMatchBoardDict;
    List<CBoard> cRespawnBoardList;

    CBoard first;
    CBoard Second;

    EDirection specialDirection;

    CMultyDictionary<int, CBoard> cAssistMatchBoardDict;
    Vector2[] assistPos = new Vector2[6];

    [SerializeField]
    public List<CAssist> assistList;

    float assistTimer;
    Vector2 assistMovingPos;
    int assistRandIndex;
    int tempGuidDirection;



    public Text txtOrder;
    public GameObject objSpecialBG;
    public CGuideCollector cGuide;
    public Animator clearAnimator;
    public Text txtClear;

    //delegate int AssistMatchDelegate(CBoard board);


    void Start()
    {


        tileSpeed = 20f;
        assistPos[0] = new Vector2(-0.75f, 0.25f);
        assistPos[1] = new Vector2(0, 0.75f);
        assistPos[2] = new Vector2(0.75f, 0.25f);
        assistPos[3] = new Vector2(-0.75f, -0.25f);
        assistPos[4] = new Vector2(0, -0.75f);
        assistPos[5] = new Vector2(0.75f, -0.25f);

        assistList = new List<CAssist>(); ;

        rangeCheck = 0;
        isSpecialMovment = false;
        txtOnOff = false;
        cAssistMatchBoardDict = new CMultyDictionary<int, CBoard>();
        cMatchBoardDict = new CMultyDictionary<int, CBoard>();
        cRespawnBoardList = new List<CBoard>();
        cMap = CMapManager.Instance;
        eWorkingProcess = EWorkingProcess.CHECK_POSSIBLE_MOVE;
        eDebugWP = eWorkingProcess;
        isDraging = false;

        foreach (var item in cMap.cBoardList)
        {
            if (item.eBoardState == EBoardState.RESPAWN)
                cRespawnBoardList.Add(item);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            txtOnOff = !txtOnOff;
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            foreach (CBoard board in cMap.cBoardList)
            {
                if (board.cTile.eObstructionType == EObstructionType.NONE)
                {
                    board.cTile.eTileColor = (ETileColor)Random.Range(0, (int)ETileColor.END);
                    StartCoroutine(board.cTile.TileColorChange());
                }
            }
        }

        if (eDebugWP != eWorkingProcess)
            eDebugWP = eWorkingProcess;

        if (txtOnOff)
        {
            txtOrder.text = eWorkingProcess.ToString();
            foreach (CBoard item in cMap.cBoardList)
                item.transform.GetChild(0).gameObject.SetActive(txtOnOff);
            txtOrder.gameObject.SetActive(txtOnOff);
        }
        else
        {
            foreach (CBoard item in cMap.cBoardList)
                item.transform.GetChild(0).gameObject.SetActive(txtOnOff);
            txtOrder.gameObject.SetActive(txtOnOff);
        }

        if (eWorkingProcess == EWorkingProcess.CHECK_POSSIBLE_MOVE)
        {

            if (assistList.Count > 0)
                assistList.Clear();
            if (cAssistMatchBoardDict.Count > 0)
                cAssistMatchBoardDict.Clear();
            if (AllMatchTile(true))
            {
                eWorkingProcess = EWorkingProcess.IMPOSSIBLE_MOVE;

                if (cMatchBoardDict.Count > 0)
                    cMatchBoardDict.Clear();
                return;
            }

            if (CObjectiveUI.Instance.isClear)
            {
                eWorkingProcess = EWorkingProcess.CLEAR;
                return;
            }


            assistTimer = 0;
            foreach (CBoard board in cMap.cBoardList)
            {
                if (board.eBoardState == EBoardState.CLOSE)
                    continue;

                for (int i = 0; i < 6; i++)
                {
                    if (board.LinkBoardCheck(i) && board.cTile != null && board.LinkTileCheck(i))
                    {
                        if (board.eBoardState != EBoardState.CLOSE && board.cLinkBoard[i].eBoardState != EBoardState.CLOSE &&
                            board.cTile.MovePossible() && board.cLinkBoard[i].cTile.MovePossible())
                        {
                            Swap(ref board.cTile, ref board.cLinkBoard[i].cTile);
                            if (AssistMatch(board))
                            {
                                int direction = 0;
                                switch (i)
                                {
                                    case 0: //EDirection.UP_L
                                        direction = 5;//EDirection.DOWN_R
                                        break;
                                    case 1://EDirection.UP
                                        direction = 4;//EDirection.DOWN
                                        break;
                                    case 2://EDirection.UP_R
                                        direction = 3;//EDirection.DOWN_L
                                        break;
                                    case 3://EDirection.DOWN_L
                                        direction = 2;
                                        break;
                                    case 4://EDirection.DOWN
                                        direction = 1;
                                        break;
                                    case 5://EDirection.DOWN_R
                                        direction = 0;
                                        break;
                                }
                                assistList.Add(new CAssist(board.cLinkBoard[i], direction, tempGuidDirection));

                                Debug.Log(assistList.Count);
                            }
                            Swap(ref board.cTile, ref board.cLinkBoard[i].cTile);
                        }
                    }
                }
            }

            if (assistList.Count > 0)
                eWorkingProcess = EWorkingProcess.POSSIBLE_MOVE;
            else
                eWorkingProcess = EWorkingProcess.IMPOSSIBLE_MOVE;

            if (eWorkingProcess == EWorkingProcess.POSSIBLE_MOVE)
            {
                List<int> tempIndex = new List<int>();

                for (int key = 0; key < cAssistMatchBoardDict.Count; key++)
                {
                    if (cAssistMatchBoardDict[key].Count == 4)
                    {
                        for (int secondKey = 0; secondKey < cAssistMatchBoardDict.Count; secondKey++)
                        {
                            if (key != secondKey)
                            {
                                if (cAssistMatchBoardDict[key][0] == cAssistMatchBoardDict[secondKey][0] ||
                                    cAssistMatchBoardDict[key][1] == cAssistMatchBoardDict[secondKey][0])
                                {
                                    if (cAssistMatchBoardDict[key][2] == cAssistMatchBoardDict[secondKey][2] ||
                                        cAssistMatchBoardDict[key][3] == cAssistMatchBoardDict[secondKey][2])
                                    {
                                        tempIndex.Add(secondKey);
                                    }
                                }
                            }
                        }
                    }
                }

            re:
                assistRandIndex = Random.Range(0, cAssistMatchBoardDict.Count);
                foreach (int item in tempIndex)
                {
                    if (item == assistRandIndex)
                        goto re;
                }
            }
        }
        else if (eWorkingProcess == EWorkingProcess.IMPOSSIBLE_MOVE)
        {

            foreach (CBoard board in cMap.cBoardList)
            {
                if (!board.cTile)
                    continue;
                if (board.cTile.eObstructionType == EObstructionType.NONE)
                {
                    board.cTile.eTileColor = (ETileColor)Random.Range(0, (int)ETileColor.END);
                    StartCoroutine(board.cTile.TileColorChange());
                }
            }

            eWorkingProcess = EWorkingProcess.CHECK_POSSIBLE_MOVE;


        }

        else if (eWorkingProcess == EWorkingProcess.POSSIBLE_MOVE)
        {
            if (AllMatchTile())
            {
                eWorkingProcess = EWorkingProcess.IMPOSSIBLE_MOVE;

                if (cMatchBoardDict.Count > 0)
                    cMatchBoardDict.Clear();
                return;
            }



            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Ray2D ray = new Ray2D(pos, Vector2.zero);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);



            if (assistTimer < 3f)
                assistTimer += Time.deltaTime;
            else
            {
                assistTimer += Time.deltaTime * 3;

                assistMovingPos.x = assistPos[assistList[assistRandIndex].direction].x *
                    Mathf.Cos(assistTimer) + assistPos[assistList[assistRandIndex].direction].x;
                assistMovingPos.y = assistPos[assistList[assistRandIndex].direction].y *
                    Mathf.Cos(assistTimer) + assistPos[assistList[assistRandIndex].direction].y;

                assistList[assistRandIndex].board.cTile.transform.localPosition =
                Vector2.MoveTowards(assistList[assistRandIndex].board.transform.position, assistMovingPos, assistTimer);

                cGuide.ActiveGuide(cAssistMatchBoardDict[assistRandIndex].Count,
                    cAssistMatchBoardDict[assistRandIndex][0].transform.position,
                    assistList[assistRandIndex].guideDirection);
            }

            if (hit.collider != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    assistList[assistRandIndex].board.cTile.transform.localPosition = Vector2.zero;
                    assistTimer = 0;
                    first = hit.collider.gameObject.GetComponent<CBoard>();
                    if (first.eBoardState == EBoardState.CLOSE ||
                        !first.cTile.MovePossible())
                        first = null;
                    isDraging = true;
                }
                else if (Input.GetMouseButton(0))
                {
                    if (isDraging && first != null &&
                        (first.gameObject != hit.collider.gameObject))
                    {
                        Second = hit.collider.gameObject.GetComponent<CBoard>();
                        for (int i = 0; i < (int)EDirection.END; i++)
                        {
                            if (first.cLinkBoard[i] == Second &&
                                Second.eBoardState != EBoardState.CLOSE &&
                                Second.cTile.MovePossible())
                            {
                                Swap(ref first.cTile, ref Second.cTile);
                                first.isMoving = true;
                                Second.isMoving = true;

                                specialDirection = (EDirection)i;

                                eWorkingProcess = EWorkingProcess.SWAP;
                            }
                        }
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                    isDraging = false;
            }

        }

        else if (eWorkingProcess == EWorkingProcess.SWAP)
        {
            assistList[assistRandIndex].board.cTile.transform.localPosition = Vector2.zero;

            if (first != null && Second != null)
            {
                if (!first.isMoving && !Second.isMoving)
                {
                    if (AllMatchTile())
                        eWorkingProcess = EWorkingProcess.SUCCESS;
                    else
                    {
                        Swap(ref first.cTile, ref Second.cTile);
                        first.isMoving = true;
                        Second.isMoving = true;
                        eWorkingProcess = EWorkingProcess.FAILE;
                    }
                }
            }
        }
        else if (eWorkingProcess == EWorkingProcess.FAILE)
        {
            cGuide.Deactiivation();

            if (!first.isMoving && !Second.isMoving)
            {
                first = null;
                Second = null;
                eWorkingProcess = EWorkingProcess.POSSIBLE_MOVE;
            }
        }
        else if (eWorkingProcess == EWorkingProcess.SUCCESS)
        {
            cGuide.Deactiivation();
            //cAssistMatchBoardDict.Clear();
            //assistList.Clear();

            foreach (CBoard item in cMap.cBoardList)
            {
                if (item.cTile != null)
                    item.cTile.cObstruction.ResetHasDamaged();
            }

            if (!isSpecialMovment)
            {
                foreach (int key in cMatchBoardDict.Keys)
                {
                    

                    foreach (CBoard value in cMatchBoardDict[key])
                    {

                        

                        if (value.cTile != null)
                        {

                            


                            objSpecialBG.SetActive(true);
                            value.cTile.isDestroyTile = true;
                            value.DestroyTileBoardState(true);

                            if (value.cTile.eTileType == ETileType.STRAIGHT)
                                value.LineBreak(value.cTile.eDirection);

                        }
                    }

                    if (cMatchBoardDict[key].Count == 4)
                    {
                        foreach (var item in CObjectiveUI.Instance.cObjectiveList)
                        {
                            if (cMatchBoardDict[key][0].cTile != null)
                            {
                                if (item.eTileColor == cMatchBoardDict[key][0].cTile.eTileColor)
                                {
                                    item.count -= 1;
                                    if (item.count <= 0)
                                        item.count = 0;
                                }
                            }
                        }

                        isSpecialMovment = true;
                        break;
                    }
                }
            }
            if (rangeCheck != CObjectPool.Instance.destructionList.Count)
            {
                rangeCheck = CObjectPool.Instance.destructionList.Count;
                eWorkingProcess = EWorkingProcess.CHAIN_BURST;
            }

            if (eWorkingProcess != EWorkingProcess.CHAIN_BURST)
            {
                if (!isSpecialMovment)
                    eWorkingProcess = EWorkingProcess.DESTROY_TILE;

                else
                {
                    eWorkingProcess = EWorkingProcess.SPECIAL_TILE_MOVEMENT;
                    isSpecialMovment = false;
                }
            }
        }

        else if (eWorkingProcess == EWorkingProcess.CHAIN_BURST)
        {
            if (!TileMovingCheck(cMap.cBoardList))
            {
                for (int i = 0; i < CObjectPool.Instance.destructionList.Count; i++)
                {
                    if (CObjectPool.Instance.destructionList[i].cTile.eTileType == ETileType.STRAIGHT)
                        CObjectPool.Instance.destructionList[i].LineBreak(CObjectPool.Instance.destructionList[i].cTile.eDirection);
                }
                eWorkingProcess = EWorkingProcess.SUCCESS;
            }
        }

        else if (eWorkingProcess == EWorkingProcess.SPECIAL_TILE_MOVEMENT)
        {
            foreach (int key in cMatchBoardDict.Keys)
            {
                foreach (CBoard value in cMatchBoardDict[key])
                {
                    if (value.cTile != null)
                    {
                        if (cMatchBoardDict[key].Count == 4)
                        {
                            if (first != null && Second != null)
                            {
                                if (value == first || value == Second)
                                {
                                    value.cTile.isNew = true;
                                    foreach (var item in cMatchBoardDict[key])
                                    {
                                        if (value != item)
                                        {
                                            item.cTile.transform.SetParent(value.transform);
                                            item.cTile.isDestroyTile = true;
                                        }
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                if (cMatchBoardDict[key][2] == value)
                                {
                                    value.cTile.isNew = true;
                                    foreach (var item in cMatchBoardDict[key])
                                    {
                                        if (value != item)
                                        {
                                            item.cTile.transform.SetParent(value.transform);
                                            item.cTile.isDestroyTile = true;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            eWorkingProcess = EWorkingProcess.DESTROY_TILE;

        }

        else if (eWorkingProcess == EWorkingProcess.DESTROY_TILE)
        {
            if (!TileMovingCheck(cMap.cBoardList))
            {
                foreach (int key in cMatchBoardDict.Keys)
                {
                    foreach (CBoard value in cMatchBoardDict[key])
                    {
                        if (value.cTile != null)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                if (value.LinkBoardCheck(i))
                                {
                                    if (value.LinkTileCheck(i))
                                    {
                                        if (value.cLinkBoard[i].cTile.eObstructionType == EObstructionType.ICE_BOX &&
                                            !value.cLinkBoard[i].cTile.cObstruction.hasDamaged)
                                            value.cLinkBoard[i].cTile.cObstruction.Damage();
                                    }
                                }
                            }
                            if (key > 100)
                            {
                                if (first != null && Second != null)
                                {
                                    if (value == first || value == Second)
                                        value.cTile.TileChangeType(ETileType.STRAIGHT, specialDirection);
                                    else
                                        value.DestroyTile();
                                }
                                else
                                {
                                    if (cMatchBoardDict[key][2] == value)
                                    {
                                        value.cTile.TileChangeType(ETileType.STRAIGHT);
                                        Debug.Log(cMatchBoardDict[key].Count);
                                    }
                                    else
                                        value.DestroyTile();
                                }
                            }
                            else
                                value.DestroyTile();
                        }
                    }
                }

                //스페셜 타일에 파괴된 타일들
                foreach (var item in CObjectPool.Instance.destructionList)
                {
                    if (item.cTile != null && item.cTile.MatchPossible())
                    {
                        foreach (var objective in CObjectiveUI.Instance.cObjectiveList)
                        {
                            if (objective.eTileColor == item.cTile.eTileColor)
                            {
                                if (objective.count > 0)
                                    objective.count -= 1;
                            }
                        }
                        item.DestroyTile();
                    }
                }



                CObjectPool.Instance.destructionList.Clear();
                cMatchBoardDict.Clear();
                first = null;
                Second = null;
                eWorkingProcess = EWorkingProcess.MOVE_DOWN;
            }



        }
        else if (eWorkingProcess == EWorkingProcess.MOVE_DOWN)
        {
            foreach (var item in cMap.cBoardList)
                item.DestroyTileBoardState(false);


            objSpecialBG.SetActive(false);

            //if(item.cTile.cObstruction.eObstructionType != EObstructionType.ICE_BOX)
            // 타일 리스폰
            foreach (CBoard item in cRespawnBoardList)
            {
                if (item.cTile == null)
                {
                    item.cTile = CObjectPool.Instance.GetTile();
                    item.cTile.eTileColor = (ETileColor)Random.Range(0, (int)ETileColor.END);
                    item.cTile.transform.SetParent(item.transform);
                    item.cTile.transform.localPosition = new Vector3(0, 5, 0);
                    item.cTile.transform.localScale = new Vector2(2.5f, 2.5f);
                    item.cTile.TileInit();
                    item.isMoving = true;
                }
            }
            //보드에 타일 채우기 DOWN
            if (!TileMovingCheck(cMap.cBoardList))
            {
                foreach (CBoard item in cMap.cBoardList)
                {

                    if (item.cTile != null)
                    {
                        if (item.cTile.isNew)
                            item.cTile.isNew = false;

                        if (item.eBoardState != EBoardState.CLOSE)
                        {
                            if (item.cTile.MovePossible())
                            {
                                if (item.LinkBoardCheck((int)EDirection.DOWN))
                                {
                                    if (!item.LinkTileCheck((int)EDirection.DOWN) &&
                                        !item.isMoving)
                                    {
                                        Swap(ref item.cTile, ref item.cLinkBoard[(int)EDirection.DOWN].cTile);
                                        item.cLinkBoard[(int)EDirection.DOWN].isMoving = true;
                                    }
                                }
                            }
                        }
                    }
                }
                MoveTile(EDirection.DOWN_R);
                MoveTile(EDirection.DOWN_L);

                foreach (CBoard item in cMap.cBoardList)
                {
                    if (item.eBoardState != EBoardState.CLOSE)
                    {
                        if (item.cTile == null)
                        {
                            if (item.LinkBoardCheck((int)EDirection.UP))
                            {
                                if (!item.LinkTileCheck((int)EDirection.UP))  // 1 = EDirection.UP
                                    item.isMoving = false;
                            }
                        }
                    }
                }
            }
            // 타일 움직임 체크
            if (!MovingCheck(cMap.cBoardList))
                eWorkingProcess = EWorkingProcess.ALL_STOP;
        }

        else if (eWorkingProcess == EWorkingProcess.ALL_STOP)
        {
            foreach (var item in cMap.cBoardList)
            {
                if (item.cTile != null)
                    item.cTile.speed = tileSpeed;
            }

            eWorkingProcess = AllMatchTile() ? EWorkingProcess.SUCCESS : EWorkingProcess.CHECK_POSSIBLE_MOVE;


            //if (AllMatchTile())
            //    eWorkingProcess = EWorkingProcess.SUCCESS;
            //else
            //    eWorkingProcess = EWorkingProcess.CHECK_POSSIBLE_MOVE;

        }

        else if (eWorkingProcess == EWorkingProcess.CLEAR)
        {
            clearAnimator.SetTrigger("Game Clear");
            StartCoroutine(ESCGame());
            CGooglePlayManager GPM = FindObjectOfType<CGooglePlayManager>();
            if (GPM != null)
            {
                GPM.AchievementList(SceneManager.GetActiveScene().buildIndex);
                GPM.FullScreenAdShow();
            }

            eWorkingProcess = EWorkingProcess.END;
        }
    }

    IEnumerator ESCGame()
    {
        objSpecialBG.SetActive(true);
        yield return new WaitForSeconds(3f);
        objSpecialBG.GetComponent<Button>().enabled = true;
        txtClear.gameObject.SetActive(true);
    }


    bool AllMatchTile(bool isTest = false)
    {
        foreach (var board in cMap.cBoardList)
        {
            int key = cMatchBoardDict.Count + 1;
            for (int i = 0; i < (int)EDirection.END; i++)
            {
                if (board.CheckTileMatch(i) &&
                        board.cLinkBoard[i].CheckTileMatch(i) &&
                        board.cLinkBoard[i].cLinkBoard[i].CheckTileMatch(i))
                {
                    //if (!cMatchBoardDict.DuplicateValue(board) &&
                    //   !cMatchBoardDict.DuplicateValue(board.cLinkBoard[i]) &&
                    //   !cMatchBoardDict.DuplicateValue(board.cLinkBoard[i].cLinkBoard[i]) &&
                    //   !cMatchBoardDict.DuplicateValue(board.cLinkBoard[i].cLinkBoard[i].cLinkBoard[i]))
                    //{
                    key += (int)EMatchType.LINE4;
                    cMatchBoardDict.Add(key, board);
                    cMatchBoardDict.Add(key, board.cLinkBoard[i]);
                    cMatchBoardDict.Add(key, board.cLinkBoard[i].cLinkBoard[i]);
                    cMatchBoardDict.Add(key, board.cLinkBoard[i].cLinkBoard[i].cLinkBoard[i]);
                    //}
                }
                else if (board.CheckTileMatch(2))
                {
                    /*
                     * EDirection.DOWN_LEFT = 3 
                     * EDirection.UP_RIGHT = 2 
                     */
                    if (board.CheckTileMatch(3) && board.cLinkBoard[3].CheckTileMatch(3))
                    {
                        //if (!cMatchBoardDict.DuplicateValue(board) &&
                        //    !cMatchBoardDict.DuplicateValue(board.cLinkBoard[3]) &&
                        //    !cMatchBoardDict.DuplicateValue(board.cLinkBoard[3].cLinkBoard[3]) &&
                        //    !cMatchBoardDict.DuplicateValue(board.cLinkBoard[2]))
                        //{
                        key += (int)EMatchType.LINE4;
                        cMatchBoardDict.Add(key, board.cLinkBoard[2]);
                        cMatchBoardDict.Add(key, board);
                        cMatchBoardDict.Add(key, board.cLinkBoard[3]);
                        cMatchBoardDict.Add(key, board.cLinkBoard[3].cLinkBoard[3]);
                        //}
                    }
                }
                else if (board.CheckTileMatch(i) &&
                    board.cLinkBoard[i].CheckTileMatch(i))
                {
                    //if (!cMatchBoardDict.DuplicateValue(board) &&
                    //    !cMatchBoardDict.DuplicateValue(board.cLinkBoard[i]) &&
                    //    !cMatchBoardDict.DuplicateValue(board.cLinkBoard[i].cLinkBoard[i]))
                    //{
                    cMatchBoardDict.Add(key, board);
                    cMatchBoardDict.Add(key, board.cLinkBoard[i]);
                    cMatchBoardDict.Add(key, board.cLinkBoard[i].cLinkBoard[i]);
                    //    Debug.Log(board.cTile.eTileColor);
                    //    Debug.Log(board.cLinkBoard[i].cTile.eTileColor);
                    //    Debug.Log(board.cLinkBoard[i].cLinkBoard[i].cTile.eTileColor);

                    //}
                }
            }
        }
        if (cMatchBoardDict.Count > 0)
        {


            return true;
        }

        return false;
    }

    IEnumerator WhaitWorkingProgressSwich(EWorkingProcess _eWorkingProcess, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        eWorkingProcess = _eWorkingProcess;
        if (isSpecialMovment) isSpecialMovment = false;
    }

    bool AssistMatch(CBoard board)
    {
        int key = cAssistMatchBoardDict.Count;

        for (int front = 0; front < 3; front++)
        {
            for (int back = 3; back < 6; back++)
            {
                if (front == 0 && back == 5 || front == 1 && back == 4 || front == 2 && back == 3)
                {
                    if (board.CheckTileMatch(front) && board.CheckTileMatch(back))
                    {

                        if (board.cLinkBoard[back].CheckTileMatch(back))
                        {
                            cAssistMatchBoardDict.Add(key, board.cLinkBoard[front]);
                            cAssistMatchBoardDict.Add(key, board);
                            cAssistMatchBoardDict.Add(key, board.cLinkBoard[back]);
                            cAssistMatchBoardDict.Add(key, board.cLinkBoard[back].cLinkBoard[back]);

                            tempGuidDirection = back;
                            return true;
                        }
                        else if (board.cLinkBoard[front].CheckTileMatch(front))
                        {
                            cAssistMatchBoardDict.Add(key, board.cLinkBoard[front].cLinkBoard[front]);
                            cAssistMatchBoardDict.Add(key, board.cLinkBoard[front]);
                            cAssistMatchBoardDict.Add(key, board);
                            cAssistMatchBoardDict.Add(key, board.cLinkBoard[back]);

                            tempGuidDirection = back;
                            return true;

                        }
                        else
                        {
                            cAssistMatchBoardDict.Add(key, board.cLinkBoard[front]);
                            cAssistMatchBoardDict.Add(key, board);
                            cAssistMatchBoardDict.Add(key, board.cLinkBoard[back]);
                            tempGuidDirection = back;

                            return true;
                        }
                    }
                    else if (board.CheckTileMatch(back) && board.cLinkBoard[back].CheckTileMatch(back))
                    {

                        cAssistMatchBoardDict.Add(key, board);
                        cAssistMatchBoardDict.Add(key, board.cLinkBoard[back]);
                        cAssistMatchBoardDict.Add(key, board.cLinkBoard[back].cLinkBoard[back]);
                        tempGuidDirection = back;

                        return true;
                    }
                    else if (board.CheckTileMatch(front) && board.cLinkBoard[front].CheckTileMatch(front))
                    {
                        cAssistMatchBoardDict.Add(key, board.cLinkBoard[front].cLinkBoard[front]);
                        cAssistMatchBoardDict.Add(key, board.cLinkBoard[front]);
                        cAssistMatchBoardDict.Add(key, board);
                        tempGuidDirection = back;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void MoveTile(EDirection eDirection)
    {
        if (!MovingCheck(cRespawnBoardList))
        {
            foreach (CBoard item in cMap.cBoardList)
            {
                if (item.eBoardState != EBoardState.CLOSE)
                {
                    if (item.LinkBoardCheck((int)eDirection) && item.LinkBoardCheck((int)EDirection.DOWN))
                    {
                        if (!item.LinkTileCheck((int)eDirection) &&
                        item.cTile != null &&
                        item.LinkTileCheck((int)EDirection.DOWN) &&
                        !item.isMoving &&
                        item.cLinkBoard[(int)eDirection].eBoardState != EBoardState.RESPAWN)
                        {
                            if (item.cTile.MovePossible())
                            {
                                if (eDirection == EDirection.DOWN_R)
                                {
                                    if (item.LinkBoardCheck((int)EDirection.UP_R))
                                    {
                                        if (item.LinkTileCheck((int)EDirection.UP_R))
                                        {
                                            break;
                                        }
                                    }
                                }
                                else if (eDirection == EDirection.DOWN_L)
                                {
                                    if (item.LinkBoardCheck((int)EDirection.UP_L))
                                    {
                                        if (item.LinkTileCheck((int)EDirection.UP_L))
                                        {
                                            break;
                                        }
                                    }
                                }

                                Swap(ref item.cTile, ref item.cLinkBoard[(int)eDirection].cTile);
                                item.cLinkBoard[(int)eDirection].isMoving = true;
                                break;
                            }
                        }

                    }
                }
            }
        }
    }

    bool MovingCheck(List<CBoard> list)
    {
        foreach (var item in list)
        {
            if (item.isMoving)
                return true;
        }
        return false;
    }

    bool TileMovingCheck(List<CBoard> list)
    {
        foreach (CBoard item in list)
        {
            if (item.cTile != null)
            {
                if (item.cTile.isDestroyTile)
                    return true;
            }
        }
        return false;
    }
    public void Swap(ref CTile a, ref CTile b)
    {
        CTile temp = a;
        a = b;
        b = temp;
    }


    public void ESCStageButton()
    {
        Debug.Log("Click");
        SceneManager.LoadScene(0);
    }


}


