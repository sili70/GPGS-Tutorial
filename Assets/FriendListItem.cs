using GooglePlayGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FriendListItem : MonoBehaviour
{
    public Text m_FriendName;
    public Button m_ShowProfile;
    public RawImage m_ProfileImage;

    private string m_PlayerID;

    public void SetUp(string playerId, string playerName, string avatarUrl)
    {
        m_FriendName.text = playerName;
        m_PlayerID = playerId;

        m_ShowProfile.onClick.RemoveAllListeners();
        m_ShowProfile.onClick.AddListener(ShowProfile);

        StartCoroutine(LoadProfileImage(avatarUrl));
    }

    private void ShowProfile()
    {
        PlayGamesPlatform.Instance.ShowCompareProfileWithAlternativeNameHintsUI(m_PlayerID, null, null, (result) => { 
            //Show UI
        });
    }

    IEnumerator LoadProfileImage(string avatarUrl)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(avatarUrl);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        } else
        {
            m_ProfileImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
}
