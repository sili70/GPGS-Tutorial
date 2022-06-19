using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using GooglePlayGames.BasicApi;
using GooglePlayGames;
using System;
using UnityEngine.SocialPlatforms;

public class GPGSManager : MonoBehaviour
{
    public Text m_Message;
    public Button m_SignIn;

    [SerializeField] private GameObject m_FriendsPrefab;
    [SerializeField] private GameObject m_FriedPrefabToIntiateAt;
    [SerializeField] private GameObject m_friendsScrollRect;


    [SerializeField] private GameObject m_AchievementPrefab;
    [SerializeField] private GameObject m_AchievementToInstantiateAt;

    [SerializeField] private Button m_ShowCustomAchievementButton;
    [SerializeField] private GameObject m_AchievementListScollView;

    private FriendsListVisibilityStatus m_FriendsListVisibilityStatus = FriendsListVisibilityStatus.Unknown;


    private void Start()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
    .RequestIdToken()
    .RequestServerAuthCode(false)
    .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        m_SignIn.onClick.RemoveAllListeners();

        m_SignIn.onClick.AddListener(SignInGooglePlayGames);

        SignInGooglePlayGames();
    }


    private void SignInGooglePlayGames()
    {
        string playerName = string.Empty;

        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (result) =>
        {
            if (result == SignInStatus.Success)
            {
                playerName = PlayGamesPlatform.Instance.GetUserDisplayName();
            }

            m_Message.text = "Login " + result.ToString() + " " + playerName;
            m_SignIn.onClick.AddListener(SignoutGooglePlay);
        });
    }

    private void SignoutGooglePlay()
    {
        PlayGamesPlatform.Instance.SignOut();
        m_Message.text = "User Signed Out";
        SignInGooglePlayGames();
    }

    public void WinRace()
    {
        PlayGamesPlatform.Instance.ReportProgress(GPGSIds.achievement_won_a_race, 100.0f, (result) => 
        {
            if (result)
            {
                Debug.Log("Progress Reported");
            } else
            {
                Debug.LogWarning("Failed to report progress !");
            }
        });
    }

    public void RevealHiddenReward()
    {
        PlayGamesPlatform.Instance.ReportProgress(GPGSIds.achievement_secret_reward, 100.0f, (result) =>
        {
            if (result)
            {
                Debug.Log("Progress Reported");
            }
            else
            {
                Debug.LogWarning("Failed to report progress !");
            }
        });
    }

    public void IncrementProgress()
    {
        PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement_win_20_race, 2, (result) =>
        {
            if (result)
            {
                Debug.Log("Progress Increased");
            }
            else
            {
                Debug.LogWarning("Failed to increase progress !");
            }
        });
    }

    public void ShowDefaultAchievementUI()
    {
        PlayGamesPlatform.Instance.ShowAchievementsUI();
    }

    public void ShowCustomAchievementUI()
    {
        m_AchievementListScollView.SetActive(!m_AchievementListScollView.activeSelf);

        if (m_AchievementListScollView.activeSelf)
        {
            m_ShowCustomAchievementButton.GetComponentInChildren<Text>().text = "Hide Custom Achievement UI";
        }
        else
        {
            m_ShowCustomAchievementButton.GetComponentInChildren<Text>().text = "Show Custom Achievement UI";
        }

        foreach (Transform child in m_AchievementToInstantiateAt.transform)
        {
            Destroy(child.gameObject);
        }

        PlayGamesPlatform.Instance.LoadAchievements(achievements => {
        
            if (achievements.Length > 0)
            {
                foreach(IAchievement achievement in achievements)
                {
                    PlayGamesAchievement playGamesAchievement = (PlayGamesAchievement)achievement;

                    GameObject achievementObject = Instantiate(m_AchievementPrefab, m_AchievementToInstantiateAt.transform, false);
                    MyAchievement myAchievement = achievementObject.GetComponent<MyAchievement>();

                    myAchievement.SetUpUI(playGamesAchievement.title, playGamesAchievement.unachievedDescription,
                        playGamesAchievement.GetImageURL(), playGamesAchievement.percentCompleted, playGamesAchievement.hidden);
                   
                }
            }
        });
    }

    public void GetFriends()
    {
        PlayGamesPlatform.Instance.GetFriendsListVisibility( /* forceReload = */ true, friendListVisibilityStatus =>
        {
            m_FriendsListVisibilityStatus = friendListVisibilityStatus;
        });

        switch(m_FriendsListVisibilityStatus)
        {
            case FriendsListVisibilityStatus.ResolutionRequired:
                PlayGamesPlatform.Instance.AskForLoadFriendsResolution((result) =>
                {
                    if (result == UIStatus.Valid)
                    {
                        //Agree
                        m_Message.text = "Agree";
                    } else
                    {
                        m_Message.text = "Not valid";
                    }
                });
                break;

            case FriendsListVisibilityStatus.Unknown:
                m_Message.text = "Unknow, Try again";
                break;
            case FriendsListVisibilityStatus.Visible:

                //Show and Hide Scroll Rect
                m_friendsScrollRect.SetActive(!m_friendsScrollRect.activeSelf);

                //Clear List
                foreach(Transform child in m_FriedPrefabToIntiateAt.transform)
                {
                    Destroy(child.gameObject);
                }

                PlayGamesPlatform.Instance.LoadFriends(2, false, (result) => { 
                
                    foreach(IUserProfile friends in Social.localUser.friends)
                    {

                        PlayGamesUserProfile friendUserProfile = (PlayGamesUserProfile)friends;

                        //TODO: We will display it in a List
                        GameObject friendObject = Instantiate(m_FriendsPrefab, m_FriedPrefabToIntiateAt.transform, false);
                        FriendListItem friendListItem = friendObject.GetComponent<FriendListItem>();
                        //friendListItem.SetUp(friends.id, friends.userName, friends.image);
                        friendListItem.SetUp(friendUserProfile.id, friendUserProfile.userName, friendUserProfile.AvatarURL);
                    }

                    if (Social.localUser.friends.Length == 0)
                    {
                        m_Message.text = "No friends found !. Please add some";
                    }
                
                });
                break;
        }
    }

    public void GetMoreFriends()
    {
        PlayGamesPlatform.Instance.GetFriendsListVisibility( /* forceReload = */ true, friendListVisibilityStatus =>
        {
            m_FriendsListVisibilityStatus = friendListVisibilityStatus;
        });

        switch (m_FriendsListVisibilityStatus)
        {
            case FriendsListVisibilityStatus.ResolutionRequired:
                PlayGamesPlatform.Instance.AskForLoadFriendsResolution((result) =>
                {
                    if (result == UIStatus.Valid)
                    {
                        //Agree
                        m_Message.text = "Agree";
                    }
                    else
                    {
                        m_Message.text = "Not valid";
                    }
                });
                break;

            case FriendsListVisibilityStatus.Unknown:
                m_Message.text = "Unknow, Try again";
                break;
            case FriendsListVisibilityStatus.Visible:

                PlayGamesPlatform.Instance.LoadMoreFriends(2, (result) => {

                    foreach (IUserProfile friends in Social.localUser.friends)
                    {
                        PlayGamesUserProfile friendUserProfile = (PlayGamesUserProfile)friends;

                        GameObject friendObject = Instantiate(m_FriendsPrefab, m_FriedPrefabToIntiateAt.transform, false);
                        FriendListItem friendListItem = friendObject.GetComponent<FriendListItem>();
                        friendListItem.SetUp(friendUserProfile.id, friendUserProfile.userName, friendUserProfile.AvatarURL);
                    }

                    if (Social.localUser.friends.Length == 0)
                    {
                        m_Message.text = "No friends found !. Please add some";
                    }

                });
                break;
        }
    }
}
