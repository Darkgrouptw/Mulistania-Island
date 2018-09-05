using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuButtonEvent : MonoBehaviour 
{
    public GameObject Menu;
    public GameObject ProgressBar;
    public GameObject LoadingBar;
    public Text LoadingText;                                    // Loading 的字
    private int LoadingState = -1;                              // Loading 的 State
    private float timeCount = 0;                                // 計數器
    
    public void StartButtonClick()
    {
        Menu.SetActive(false);
        ProgressBar.SetActive(true);
        LoadingState = 0;
    }

    public void SettingButtonClick()
    {
    }

    public void ExitButtonClick()
    {
        Application.Quit();
    }

    void Update()
    {
        switch(LoadingState)
        {
            case 0:
                if (timeCount <= 2)
                {
                    LoadingText.text = "初始化中 !!";
                    timeCount += Time.deltaTime;
                    ProgressBarText(timeCount / 2 * 20);
                    break;
                }
                else
                {
                    LoadingState++;
                    timeCount = 0;
                }
                break;
            case 1:
                if(timeCount < 1)
                {
                    LoadingText.text = "檢查目錄中 !!";
                    timeCount += Time.deltaTime;
                    ProgressBarText(timeCount * 30 + 20);
                    break;
                }
                if (!Directory.Exists("Save"))
                    Directory.CreateDirectory("Save");

                LoadingState++;
                timeCount = 0;
                break;
            case 2:
                SceneManager.LoadSceneAsync(1);
                break;
        }
    }

    private void ProgressBarText(float value)
    {
        LoadingBar.transform.localScale = new Vector3(value / 100, 1, 1);
    }
}
