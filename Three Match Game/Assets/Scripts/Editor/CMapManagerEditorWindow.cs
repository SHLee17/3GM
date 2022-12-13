using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CMapManagerEditorWindow : EditorWindow
{
    class CObjectiveActive
    {
        public CObjectiveActive(bool _active, bool _moveLimit, bool _matchLimit)
        {
            active = _active;
            moveLimit = _moveLimit;
            matchLimit = _matchLimit;
        }
        public Texture texture;
        public bool active;
        public bool moveLimit;
        public bool matchLimit;
    }

    static Vector2 windowMinSize = Vector2.one * 300.0f;

    bool showPos = false;

    CMapManager mapManager;
    CObjectiveUI gameKit;
    bool isInit = false;
    Vector2 scrollPos = Vector2.zero;
    List<CObjectiveActive> cObjectiveActiveList;

    float size = 1;
    float offsetX = 80;
    float offsetHorizontalY = 50;
    float offsetY = 95;
    int scrollRange;
    int tabIndex;
    string[] tabName = new string[2];
    string[] obstructionTabName = new string[5];
    int objectiveIndex = -1;
    int movingX;
    int movingY;

    public Texture ActiveBoard;
    public Texture DeactiveBoard;
    public CMapManager cMapManagerPrefab;
    public CObjectiveUI gameKitPrefab;


    [MenuItem("MapManager/Map Maker")]
    static bool MapManagerActive()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0)
            return false;

        CMapManagerEditorWindow window = GetWindow<CMapManagerEditorWindow>(true, "Make Map");
        //window.Show();
        window.minSize = windowMinSize;

        return true;
    }
    
    private void OnDestroy()
    {
        EditorSceneManager.SaveScene(mapManager.gameObject.scene);
        //PrefabUtility.ApplyObjectOverride(gameKit.gameObject,"");
        //EditorSceneManager.SaveScene(gameKit.gameObject.scene);
    }
    private void OnEnable()
    {
        scrollRange = 1500;
        gameKit = (CObjectiveUI)FindObjectOfType(typeof(CObjectiveUI));
        if (gameKit == null)
        {
            gameKit = Instantiate(gameKitPrefab);
            gameKit.gameObject.name = "GameKit";

            Camera.main.transform.position =
            new Vector3(mapManager.cBoardList[mapManager.cBoardList.Count - 1].transform.position.x / 2,
            mapManager.cBoardList[mapManager.cBoardList.Count - 1].transform.position.y / 2, -10);
        }

        cObjectiveActiveList = new List<CObjectiveActive>();

        for (int i = 0; i < (int)EObstructionType.NONE; i++)
            cObjectiveActiveList.Add(new CObjectiveActive(false, false, false));

        cObjectiveActiveList[0].texture = CObjectPool.Instance.SOspriteManger.sprIceBoxList[0].texture;
        obstructionTabName[0] = "ICE_BOX";
        obstructionTabName[1] = "SOME_ONE_ELSE";

        cObjectiveActiveList[(int)EObstructionType.ICE_BOX].texture = CObjectPool.Instance.SOspriteManger.sprIceBoxList[0].texture;

        tabName[0] = "Map Editor";
        tabName[1] = "Game Objectives Editor";


        mapManager = (CMapManager)FindObjectOfType(typeof(CMapManager));
        if (mapManager == null)
        {
            mapManager = Instantiate(cMapManagerPrefab);
            mapManager.name = "MapManager";
        }
        else
        {
            isInit = EditorPrefs.GetBool("Initiate");
            scrollRange = EditorPrefs.GetInt("ScrollRange");
            size = EditorPrefs.GetFloat("Resize");
            movingX = EditorPrefs.GetInt("MovingX");


            if (mapManager.cBoardList == null)
                mapManager.cBoardList = new List<CBoard>();

            offsetX = 80 * size;
            offsetHorizontalY = 50 * size;
            offsetY = 95 * size;
        }
    }
    void OnGUI()
    {
        if (tabIndex == 0)
            scrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height),
                scrollPos, new Rect(0, 0, scrollRange, scrollRange));

        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();
        tabIndex = GUILayout.Toolbar(tabIndex, tabName, GUILayout.Width(420), GUILayout.Height(30));

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();

        GUILayout.Space(10);


        switch (tabIndex)
        {
            case 0:
                MapEditor();
                break;
            case 1:
                GameObjectiveEditor();
                break;
        }

        if (tabIndex == 0)
            GUI.EndScrollView();
    }
    void GameObjectiveEditor()
    {
        if (showPos = EditorGUILayout.Foldout(showPos, "미구현"))
            objectiveIndex = GUI.SelectionGrid(new Rect(10, 70, 100, 50 * obstructionTabName.Length), objectiveIndex, obstructionTabName, 1);
        //objectiveIndex = GUILayout.Toolbar(objectiveIndex, obstructionTabName, GUILayout.Width(200), GUILayout.Height(windowMinSize.y));

        switch (objectiveIndex)
        {
            case 0:
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                for (int i = 0; i < cObjectiveActiveList.Count; i++)
                {
                    //using (EditorGUILayout.VerticalScope v = new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(false), GUILayout.Width(0)))
                    //{
                    EObstructionType temp = (EObstructionType)i;

                    EditorGUILayout.BeginVertical("HelpBox");

                    //GUIStyle uIStyle = new GUIStyle("Button");
                    //uIStyle.alignment = TextAnchor.MiddleCenter;
                    if (GUILayout.Button(cObjectiveActiveList[i].texture, GUILayout.Width(100), GUILayout.Height(100)))
                        cObjectiveActiveList[i].active = !cObjectiveActiveList[i].active;

                    if (cObjectiveActiveList[i].active)
                    {
                        GUILayout.BeginHorizontal("GroupBox");
                        GUILayout.Label("Match Limit");
                        //GUILayout.Space(20);
                        cObjectiveActiveList[i].matchLimit = EditorGUILayout.Toggle(cObjectiveActiveList[i].matchLimit);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal("GroupBox");
                        GUILayout.Label("Move Limit");
                        cObjectiveActiveList[i].moveLimit = EditorGUILayout.Toggle(cObjectiveActiveList[i].moveLimit);
                        GUILayout.EndHorizontal();

                    }
                    EditorGUILayout.EndVertical();
                    //}
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                break;
            default:
                break;
        }



    }
    void MapEditor()
    {

        EditorGUI.BeginChangeCheck();

        if (!isInit)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            //new Rect(10, 5, 200, 20),
            EditorStyles.label.normal.textColor = Color.red;
            mapManager.vertical = EditorGUILayout.IntField("Vertical : ", mapManager.vertical);
            EditorStyles.label.normal.textColor = Color.blue;
            //new Rect(250, 5, 200, 20)
            mapManager.horizontal = EditorGUILayout.IntField("Horizontal : ", mapManager.horizontal);
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        //GUI.Button(new Rect(10, 40, 100, 50), "MapSetting")
        if (GUILayout.Button("MapSetting", GUILayout.Width(120), GUILayout.Height(40)) && !isInit &&
            mapManager.vertical > 0 && mapManager.horizontal > 0)
        {
            isInit = true;
            mapManager.MapSetting(mapManager.vertical, mapManager.horizontal);
        }
        //GUI.Button(new Rect(115, 40, 100, 50), "Remove")
        if (GUILayout.Button("Remove", GUILayout.Width(120), GUILayout.Height(40)) && isInit)
        {
            mapManager.ClearBoardList();
            isInit = false;
            mapManager.vertical = 0;
            mapManager.horizontal = 0;
        }
        //GUI.Button(new Rect(220, 40, 100, 50), "Resize")
        if (GUILayout.Button("Resize", GUILayout.Width(120), GUILayout.Height(40)))
        {
            offsetX = 80 * size;
            offsetHorizontalY = 50 * size;
            offsetY = 95 * size;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (isInit)
        {
            if (GUILayout.Button("Random Tile", GUILayout.Width(120), GUILayout.Height(40)))
            {
                foreach (CBoard item in mapManager.cBoardList)
                    item.cTile.eTileColor = (ETileColor)Random.Range(0, (int)ETileColor.END);
            }
            //if (GUILayout.Button("Overrides Prefab Tile", GUILayout.Width(120), GUILayout.Height(40)))
            //{
            //    for (int i = 0; i < mapManager.cBoardList.Count; i++)
            //    {
            //        CBoard originBoard = mapManager.cBoardList[i];
            //        CBoard temp = Instantiate(mapManager.cBoard, mapManager.boardHolder.transform);
            //        temp.Init(originBoard.cTile.eTileColor,
            //            originBoard.x, originBoard.y,
            //            originBoard.transform.position,
            //            originBoard.eBoardState,
            //            originBoard.cTile.eObstructionType);
            //        DestroyImmediate(mapManager.cBoardList[i].gameObject);
            //        mapManager.cBoardList[i] = temp;
            //    }
            //}
            //if (GUILayout.Button("Overrides GameKit", GUILayout.Width(120), GUILayout.Height(40)))
            //{
            //    DestroyImmediate(gameKit.gameObject);
            //    gameKit = Instantiate(gameKitPrefab);
            //    gameKit.name = "GmaeKit";

            //    Camera.main.transform.position =
            //    new Vector3(mapManager.cBoardList[mapManager.cBoardList.Count - 1].transform.position.x / 2,
            //    mapManager.cBoardList[mapManager.cBoardList.Count - 1].transform.position.y / 2, -10);
            //}
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();



        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        //new Rect(10, 90, 200, 20),
        scrollRange = EditorGUILayout.IntField("Scroll Range : ", scrollRange);
        //new Rect(220, 90, 200, 20),
        size = EditorGUILayout.FloatField("Prefep Size : ", size);

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        if (isInit)
        {
            // 맵타일 Y축
            movingY = 100;
            // 목표 Y축
            int posY = 340;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            movingX = EditorGUILayout.IntSlider("X Pos", movingX, 1, 1000);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("Box");

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("Stage Level : " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex, style);
            //gameKit.stageLevel = EditorGUILayout.IntField("Stage Level : ", gameKit.stageLevel);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            Camera.main.orthographicSize = EditorGUILayout.IntSlider("Camera Size", (int)Camera.main.orthographicSize, 5, 10);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            gameKit.targetCount = EditorGUILayout.IntSlider("TargetCount", gameKit.targetCount, 1, 2);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (gameKit.targetCount > 0)
            {
                for (int i = 0; i < gameKit.targetCount; i++)
                {
                    GUILayout.BeginVertical("Box");

                    gameKit.cObjectiveList[i].eObstructionType =
                        (EObstructionType)EditorGUILayout.EnumPopup(gameKit.cObjectiveList[i].eObstructionType, 
                        GUILayout.Width(100), GUILayout.Height(20));

                    if (gameKit.cObjectiveList[i].eObstructionType == EObstructionType.TILE)
                    {
                        gameKit.cObjectiveList[i].eTileColor =
                        (ETileColor)EditorGUILayout.EnumPopup(gameKit.cObjectiveList[i].eTileColor,
                        GUILayout.Width(100), GUILayout.Height(20));
                    }

                    EditorGUILayout.LabelField("Count", style, GUILayout.Width(100), GUILayout.Height(20));
                    gameKit.cObjectiveList[i].count = EditorGUILayout.IntField(gameKit.cObjectiveList[i].count,
                        GUILayout.Width(100), GUILayout.Height(20));
                    GUILayout.EndVertical();

                    //if (gameKit.cObjectiveList[i].eObstructionType == EObstructionType.ICE_BOX)
                    //{
                    //    EditorGUI.DrawPreviewTexture(new Rect(25, 45, 100, 100),
                    //               CObjectPool.Instance.SOspriteManger.sprIceBoxList[0].texture);

                    //    //if (i == 0)
                    //    //{
                    //    //    if (gameKit.targetCount == 1)
                    //    //    {
                    //    //        EditorGUI.DrawPreviewTexture(new Rect(25, 45, 100, 14),
                    //    //            CObjectPool.Instance.SOspriteManger.sprIceBoxList[0].texture);
                    //    //        //GUI.DrawTexture(new Rect(windowMinSize.x / 2 + 55, posY, 80, 80),
                    //    //        //CObjectPool.Instance.SOspriteManger.sprIceBoxList[0].texture);
                    //    //    }
                    //    //    else if (gameKit.targetCount == 2)
                    //    //    {
                    //    //        GUI.DrawTexture(new Rect(windowMinSize.x / 2 + 10, posY, 80, 80),
                    //    //        CObjectPool.Instance.SOspriteManger.sprIceBoxList[0].texture);
                    //    //    }
                    //    //}
                    //    //else
                    //    //{
                    //    //    GUI.DrawTexture(new Rect(windowMinSize.x / 2 + 55 + 55, posY, 80, 80),
                    //    //        CObjectPool.Instance.SOspriteManger.sprIceBoxList[1].texture);
                    //    //}
                    //}
                    //else if (gameKit.cObjectiveList[i].eObstructionType == EObstructionType.TILE)
                    //{
                    //    EditorGUI.DrawPreviewTexture(new Rect(25, 45, 100, 100),
                    //               CObjectPool.Instance.SOspriteManger.sprNormalTileList[(int)gameKit.cObjectiveList[i].eTileColor].texture);
                    //    //if (i == 0)
                    //    //{
                    //    //    if (gameKit.targetCount == 1)
                    //    //    {
                    //    //        GUI.DrawTexture(new Rect(windowMinSize.x / 2 + 55, posY, 80, 80),
                    //    //            CObjectPool.Instance.SOspriteManger.sprNormalTileList[(int)gameKit.cObjectiveList[i].eTileColor].texture);
                    //    //    }
                    //    //    else if (gameKit.targetCount == 2)
                    //    //    {
                    //    //        GUI.DrawTexture(new Rect(windowMinSize.x / 2 + 10, posY, 80, 80),
                    //    //            CObjectPool.Instance.SOspriteManger.sprNormalTileList[(int)gameKit.cObjectiveList[i].eTileColor].texture);
                    //    //    }
                    //    //}
                    //    //else
                    //    //{

                    //    //    GUI.DrawTexture(new Rect(windowMinSize.x / 2 + 55 + 55, posY, 80, 80),
                    //    //           CObjectPool.Instance.SOspriteManger.sprNormalTileList[(int)gameKit.cObjectiveList[1].eTileColor].texture);
                    //    //}

                    //}
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            gameKit.InitStageOption();

            for (int y = 0; y < mapManager.vertical; y++)
            {
                for (int x = 0; x < mapManager.horizontal; x++)
                {
                    // convert 2d array index to 1d
                    int index = y * mapManager.horizontal + x;

                    //EditorWindow Board DrawTexture 보드
                    if (mapManager.cBoardList[index].eBoardState == EBoardState.CLOSE)
                        GUI.DrawTexture(SetRect(x, y, new Rect(20, 300 + movingY, 100, 100)), DeactiveBoard);
                    else
                        GUI.DrawTexture(SetRect(x, y, new Rect(20, 300 + movingY, 100, 100)), ActiveBoard);

                    //cBoard가 열려 있을 때
                    if (mapManager.cBoardList[index].eBoardState != EBoardState.CLOSE)
                    {
                        //인 게임에 그려지는 Board SetActive(true)
                        mapManager.cBoardList[index].sprRenderer.sprite = mapManager.sprBoard;
                        //에디터에 그려지는 Tile
                        if (mapManager.cBoardList[index].cTile.eTileType == ETileType.NOMAL)
                        {
                            GUI.DrawTexture(SetRect(x, y, new Rect(30, 310 + movingY, 80, 80)),
                            CObjectPool.Instance.SOspriteManger.sprNormalTileList[(int)mapManager.cBoardList[index].cTile.eTileColor].texture);

                            //inGame Tile Sprite
                            mapManager.cBoardList[index].cTile.sprRenderer.sprite =
                          CObjectPool.Instance.SOspriteManger.sprNormalTileList[(int)mapManager.cBoardList[index].cTile.eTileColor];

                        }
                        else if (mapManager.cBoardList[index].cTile.eTileType == ETileType.STRAIGHT)
                        {
                            GUI.DrawTexture(SetRect(x, y, new Rect(30, 310 + movingY, 80, 80)),
                            CObjectPool.Instance.SOspriteManger.sprSpecialTileList[(int)mapManager.cBoardList[index].cTile.eTileColor].texture);

                            //inGame Tile Sprite
                            mapManager.cBoardList[index].cTile.sprRenderer.sprite =
                            CObjectPool.Instance.SOspriteManger.sprSpecialTileList[(int)mapManager.cBoardList[index].cTile.eTileColor];
                        }
                        if (mapManager.cBoardList[index].cTile.cObstruction.eObstructionType == EObstructionType.NONE)
                        {
                            //inGame Tile Obstruction
                            mapManager.cBoardList[index].cTile.cObstruction.Init(EObstructionType.NONE);
                        }
                        else if (mapManager.cBoardList[index].cTile.cObstruction.eObstructionType == EObstructionType.ICE_BOX)
                        {
                            GUI.DrawTexture(SetRect(x, y, new Rect(30, 310 + movingY, 80, 80)),
                            CObjectPool.Instance.SOspriteManger.
                            sprIceBoxList[0].texture);

                            //inGame Tile Obstruction
                            mapManager.cBoardList[index].cTile.cObstruction.Init(EObstructionType.ICE_BOX);
                        }

                        //EditorWindow EnumPopup(Enum) 타일 타입
                        mapManager.cBoardList[index].cTile.eTileType =
                            (ETileType)EditorGUI.EnumPopup(SetRect(x, y, new Rect(30, 323 + movingY, 40, 20)),
                            mapManager.cBoardList[index].cTile.eTileType);

                        //EditorWindow EnumPopup(Enum) 타일색
                        mapManager.cBoardList[index].cTile.eTileColor =
                            (ETileColor)EditorGUI.EnumPopup(SetRect(x, y, new Rect(30, 340 + movingY, 40, 20)),
                            mapManager.cBoardList[index].cTile.eTileColor);

                        //EditorWindow EnumPopup(Enum) 장애물 타입
                        mapManager.cBoardList[index].cTile.cObstruction.eObstructionType =
                            (EObstructionType)EditorGUI.EnumPopup(SetRect(x, y, new Rect(70, 340 + movingY, 40, 20)),
                            mapManager.cBoardList[index].cTile.cObstruction.eObstructionType);
                    }
                    //cBoard가 닫쳐 있을 때
                    else
                    {
                        mapManager.cBoardList[index].cTile.sprRenderer.sprite = null;
                        mapManager.cBoardList[index].sprRenderer.sprite = null;
                    }

                    //보드 상태
                    mapManager.cBoardList[index].eBoardState =
                        (EBoardState)EditorGUI.EnumPopup(SetRect(x, y, new Rect(70, 323 + movingY, 40, 20)), mapManager.cBoardList[index].eBoardState);
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool("Initiate", isInit);
            EditorPrefs.SetInt("ScrollRange", scrollRange);
            EditorPrefs.SetFloat("Resize", size);
            EditorPrefs.SetInt("MovingX", movingX);
        }

    }
    Rect SetRect(int _x, int _y, Rect _RectPos)
    {

        _RectPos.x = (_RectPos.x * size) + movingX;
        _RectPos.y = _RectPos.y * size;
        _RectPos.width = _RectPos.width * size;
        _RectPos.height = _RectPos.height * size;


        if (_x % 2 == 0)
            return new Rect(_RectPos.x + (offsetX * _x), _RectPos.y + (offsetY * _y), _RectPos.width, _RectPos.height);


        else
            return new Rect(_RectPos.x + (offsetX * _x), _RectPos.y + (offsetY * _y) + (offsetHorizontalY), _RectPos.width, _RectPos.height);
    }
}
