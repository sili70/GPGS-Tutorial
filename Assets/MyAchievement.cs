using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class MyAchievement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_Title;
    [SerializeField] private TextMeshProUGUI m_Description;
    [SerializeField] private RawImage m_Icon;
    [SerializeField] private TextMeshProUGUI m_Progress;

    public void SetUpUI(string title, string description, string imgURL, double percentageCompleted, bool hidden)
    {
        m_Title.text = title;
        m_Description.text = description;

        if (percentageCompleted == 100.0f)
        {
            m_Progress.text = "Completed";
        } 
        else
        {
            if (hidden)
            {
                m_Progress.text = string.Empty;
            }
            else
            {
                m_Progress.text = percentageCompleted.ToString() + " % Completed";
            }
        }

        StartCoroutine(DownloadImageAndShow(imgURL));
    }

    IEnumerator DownloadImageAndShow(string imgURL)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imgURL);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning(request.error);
        }
        else
        {
            m_Icon.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }
}
