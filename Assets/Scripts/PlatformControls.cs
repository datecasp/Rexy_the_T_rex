using UnityEngine;

public class PlatformControls : MonoBehaviour
{
    [SerializeField] private bool forceTouchControls;

    private void Awake()
    {
        ConfigureControls();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            ConfigureControls();
    }

    public void ConfigureControls()
    {
        bool isAndroid = Application.platform == RuntimePlatform.Android;
        bool showTouchControls = forceTouchControls || isAndroid;

#if UNITY_EDITOR
        if (!forceTouchControls)
            showTouchControls = false;
#endif

        string[] buttonNames =
        {
            "btn_left",
            "btn_right",
            "btn_jump",
            "btn_run"
        };

        for (int i = 0; i < buttonNames.Length; i++)
        {
            Transform button = FindChild(transform, buttonNames[i]);
            if (button != null)
                button.gameObject.SetActive(showTouchControls);
        }
    }

    private static Transform FindChild(Transform root, string objectName)
    {
        if (root.name == objectName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindChild(root.GetChild(i), objectName);
            if (result != null)
                return result;
        }

        return null;
    }
}
