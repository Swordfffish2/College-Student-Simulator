using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DesktopUI : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private CameraSequence cameraSequence;
    [SerializeField] private Canvas monitorCanvas;

    [Header("Desktop")]
    [SerializeField] private GameObject desktopPanel;
    [SerializeField] private Button projectButton;

    [Header("Editor")]
    [SerializeField] private GameObject editorPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private TMP_Text playLabel;

    [Header("Second loop")]
    [SerializeField] private Renderer ceiling;
    [SerializeField] private Material loopTwoCeiling;

    public struct CursorSample
    {
        public float time;
        public Vector2 position;
    }

    private readonly List<CursorSample> cursorSamples = new List<CursorSample>();
    public IReadOnlyList<CursorSample> CursorSamples => cursorSamples;

    private RectTransform screenRect;
    private bool recording;
    private bool inSecondLoop;
    private float recordingStart;
    private float nextSampleTime;

    private void Awake()
    {
        if (!cameraSequence || !monitorCanvas || !monitorCanvas.worldCamera ||
            !desktopPanel || !projectButton || !editorPanel || !playButton || !playLabel)
        {
            Debug.LogError("DesktopUI: Check the desktop references and the Canvas Event Camera.", this);
            enabled = false;
            return;
        }

        screenRect = (RectTransform)monitorCanvas.transform;
        desktopPanel.SetActive(true);
        editorPanel.SetActive(false);
        projectButton.onClick.AddListener(OpenProject);
        playButton.onClick.AddListener(LaunchProject);
    }

    private void OpenProject()
    {
        if (!cameraSequence.IsAtDesk || !desktopPanel.activeSelf)
            return;

        desktopPanel.SetActive(false);
        editorPanel.SetActive(true);

        if (inSecondLoop)
        {
            playLabel.text = "Loop 02";
            playButton.interactable = false;
            return;
        }

        cursorSamples.Clear();
        recordingStart = Time.unscaledTime;
        CaptureCursor();
        recording = true;
        nextSampleTime = recordingStart + 0.1f;
    }

    private void Update()
    {
        if (!recording || Time.unscaledTime < nextSampleTime)
            return;

        CaptureCursor();
        nextSampleTime = Time.unscaledTime + 0.1f;
    }

    private void CaptureCursor()
    {
        Vector2 pointer;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null)
            return;
        pointer = Mouse.current.position.ReadValue();
#else
        pointer = Input.mousePosition;
#endif
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                screenRect, pointer, monitorCanvas.worldCamera, out Vector2 local))
            return;

        Rect rect = screenRect.rect;
        if (!rect.Contains(local))
            return;

        cursorSamples.Add(new CursorSample
        {
            time = Time.unscaledTime - recordingStart,
            position = Rect.PointToNormalized(rect, local)
        });
    }

    private void LaunchProject()
    {
        if (!recording)
            return;

        if (!ceiling || !loopTwoCeiling)
        {
            Debug.LogError("DesktopUI: Assign Ceiling and Loop Two Ceiling.", this);
            return;
        }

        CaptureCursor();
        recording = false;
        playButton.interactable = false;
        playLabel.text = "Launching...";
        Debug.Log($"DesktopUI: Play clicked. Captured {cursorSamples.Count} cursor samples.", this);
        StartCoroutine(EnterSecondLoop());
    }

    private IEnumerator EnterSecondLoop()
    {
        yield return cameraSequence.EnterMonitor();

        // Match the room to its preview before the camera returns to the bed.
        ceiling.sharedMaterial = loopTwoCeiling;
        inSecondLoop = true;
        editorPanel.SetActive(false);
        desktopPanel.SetActive(true);
        yield return cameraSequence.WakeFromBed();
    }
}
