using System;
using Scripts.UI;
using Scripts.UI.EditorUI;
using UnityEngine;

public class EditorUIManager : MonoBehaviour
{
    [SerializeField] private GameObject body;
    [SerializeField] private FileOperations fileOperations;
    [SerializeField] private StatusBar statusBar;

    private void Awake()
    {
        StatusBar = statusBar ??= FindObjectOfType<StatusBar>();
    }

    public static StatusBar StatusBar;

}
