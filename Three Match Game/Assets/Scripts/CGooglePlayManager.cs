using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GoogleMobileAds.Api;

public class CGooglePlayManager : MonoBehaviour
{
    [Serializable]
    public class CStageClear
    {
        [SerializeField] public bool isClear;
        [SerializeField] public GameObject objClearLabel;
    }
    public class Serialization<T>
    {
        [SerializeField]
        List<T> target;
        public List<T> ToList() { return target; }
        public Serialization(List<T> _target)
        {
            target = _target;
        }
    }

    readonly string bannerID = "ca-app-pub-1733361111976007/5406299412";
    readonly string fullscreenID = "ca-app-pub-1733361111976007/7434092763";

    BannerView banner;
    InterstitialAd screenAd;

    string path;
    public Text txtStatus;
    public List<CStageClear> stageClearList;

    bool isSaving;
    bool hasBeenWarnedLocalSave;

    public bool isLogin
    {
        private set { }
        get { return Social.Active.localUser.authenticated; }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        path = Path.Combine(Application.persistentDataPath, "StageClearData.json");
        FileInfo fileInfo = new FileInfo(path);

        PlayGamesPlatform.InitializeInstance(new PlayGamesClientConfiguration.Builder().EnableSavedGames().Build());
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        if (!fileInfo.Exists)
            LocalSaveJson();
    }
    private void Start()
    {
        InitAd();
    }

    void InitAd()
    {
        banner = new BannerView(bannerID, AdSize.SmartBanner, AdPosition.Bottom);

        screenAd = new InterstitialAd(fullscreenID);

        //var re = gameObject.GetComponent<AdRequest>();


        AdRequest request = new AdRequest.Builder().Build();

        banner.LoadAd(request);

        screenAd.LoadAd(request);

        screenAd.OnAdClosed += (sender, e) => Debug.Log("close");
        screenAd.OnAdLoaded += (sender, e) => Debug.Log("loaded");

    }
    public void FullScreenAdShow()
    {
        StartCoroutine(ShowScreenAd());
    }
    IEnumerator ShowScreenAd()
    {
        while (!screenAd.IsLoaded())
            yield return null;

        screenAd.Show();
    }
    public void ToogleAd(bool active)
    {
        if (active)
            banner.Show();
        else
            banner.Hide();
    }

    public void StageClear(int i, bool isClear)
    {
        stageClearList[i].objClearLabel.SetActive(isClear);
    }

    public void SaveCloud()
    {
        if (isLogin)
        {
            isSaving = true;
            hasBeenWarnedLocalSave = false;
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution
                ("GameSave",
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                SavedGameOpened);
        }
        else
        {
            if (!hasBeenWarnedLocalSave)
                StatusText("Cloud error");
            hasBeenWarnedLocalSave = true;
            //SaveLocal();
        }
    }
    public void LoadCloud()
    {
        isSaving = false;
        ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution
                ("GameSave",
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                SavedGameOpened);
    }
    void SavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            if (isSaving) // Writing data
            {
                byte[] byteData = Encoding.ASCII.GetBytes(LoadJsonReadAllText());
                SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder()
                    .WithUpdatedDescription("Saved Game at" + DateTime.Now);

                SavedGameMetadataUpdate update = builder.Build();
                ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(data, update, byteData, SavedGameWritten);
                StatusText("Save Success");
            }
            else // Reading Data
            {
                ((PlayGamesPlatform)Social.Active).SavedGame.ReadBinaryData(data, SavedGameLoaded);
                StatusText("Load Success");
            }
        }
        else
            StatusText("Error opening game" + status);
    }
    void SavedGameLoaded(SavedGameRequestStatus status, byte[] byteData)
    {
        if (byteData.Length == 0)
        {
            if (status == SavedGameRequestStatus.Success)
                LoadFromString(Encoding.ASCII.GetString(byteData));
            else
            {
                Debug.Log("Error reading game" + status);
                StatusText("Error reading game" + status);
            }
        }
        else
            StatusText("No data found.");
    }
    void SavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        Debug.Log(status);
    }
    void LoadFromString(string saveData)
    {
        stageClearList = JsonUtility.FromJson<Serialization<CStageClear>>(saveData).ToList();
        for (int i = 0; i < stageClearList.Count; i++)
        {
            StageClear(i, stageClearList[i].isClear);
        }
    }


    void StatusText(string str)
    {
        if (txtStatus != null)
            txtStatus.text = str;
    }

    public void OnLogInOut(bool inOut)
    {
        if (inOut)
        {
            if (!Social.localUser.authenticated)
            {
                Social.localUser.Authenticate((bool isSuccess) =>
                {
                    if (isSuccess)
                        StatusText("LogIn Success");
                    else
                        StatusText("LogIn Fail");
                });
            }
        }
        else
        {
            ((PlayGamesPlatform)Social.Active).SignOut();
            StatusText("LogOut");
        }

    }

    public void ShowAchievementUI() => Social.ShowAchievementsUI();

    public void AchievementList(int i)
    {
        switch (i)
        {
            case 1:
                Social.ReportProgress(GPGSIds.achievement_stage_1_clear, 100, isSuccess => { });
                PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement_all_clear, 1, isSuccess => { });
                break;
            case 2:
                Social.ReportProgress(GPGSIds.achievement_stage_2_clear, 100, isSuccess => { });
                PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement_all_clear, 1, isSuccess => { });
                break;
        }
    }

    public void ShowLeaderBoardUI()
    {
        //리더 보드 점수 추가
        Social.ReportScore(100, GPGSIds.leaderboard_leaderboard_1, success =>
        {
            if (success)
            {
                StatusText("Success");
                //전체 리더 보드 보기
                Social.ShowLeaderboardUI();
            }
            else
                StatusText("Fail");

        });
    }

    //특정 리더 보드 보기
    public void ShowLeaderboard() => ((PlayGamesPlatform)Social.Active).ShowLeaderboardUI(GPGSIds.leaderboard_leaderboard_1);

    [ContextMenu("Save Json Data")]
    public void LocalSaveJson()
    {
        string path = Path.Combine(Application.persistentDataPath, "Save", "StageClearData.json");


        string data = JsonUtility.ToJson(new Serialization<CStageClear>(stageClearList), true);
        File.WriteAllText(path, data);
    }

    [ContextMenu("Load Json Data")]
    public string LoadJsonReadAllText()
    {
        string path = Path.Combine(Application.persistentDataPath, "Save", "StageClearData.json");

        //path상에 있는 파일을 문자열로 읽어옴
        string data = File.ReadAllText(path);
        return data;
        //stageClearList = JsonUtility.FromJson<Serialization<CStageClear>>(data).ToList();
    }




}
