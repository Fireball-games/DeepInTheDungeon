using Scripts.Localization;
using Scripts.ScenesManagement;
using Scripts.System;
using Scripts.System.MonoBases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayEscapeMenu : DialogBase
{
    [SerializeField] private Button toEditorButton;
    [SerializeField] private Button toMainSceneButton;

    private bool isOpened;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isOpened)
            {
                isOpened = true;
                Open();
                return;
            }

            isOpened = false;
            CloseDialog();
        }
    }

    private void Open()
    {
        base.Open();
        if (GameManager.Instance.IsPlayingFromEditor)
        {
            toEditorButton.gameObject.SetActive(true);
            toEditorButton.GetComponentInChildren<TMP_Text>().text = T.Get(LocalizationKeys.ReturnToEditor);
            toEditorButton.onClick.AddListener(LeaveToEditor); 
        }
        else
        {
            toEditorButton.gameObject.SetActive(false);
        }
        
        toMainSceneButton.GetComponentInChildren<TMP_Text>().text = T.Get(LocalizationKeys.ReturnToMainScene);
        toMainSceneButton.onClick.AddListener(LeaveToMainScene);
    }

    private void LeaveToMainScene()
    {
        SceneLoader.Instance.LoadMainScene();
    }

    private void LeaveToEditor()
    {
        SceneLoader.Instance.LoadEditorScene();
    }
}
