using System.Collections;
using System.IO;
using UnityEngine;

public class CaptureBedPreview : MonoBehaviour
{
    [SerializeField] private Renderer ceiling;
    [SerializeField] private Material loopTwoCeiling;

#if UNITY_EDITOR
    private IEnumerator Start()
    {
        if (ceiling == null || loopTwoCeiling == null)
        {
            Debug.LogError("CaptureBedPreview: Assign Ceiling and Loop Two Ceiling.", this);
            yield break;
        }

        Material originalMaterial = ceiling.sharedMaterial;
        ceiling.sharedMaterial = loopTwoCeiling;

        yield return new WaitForEndOfFrame();

        Texture2D frame = ScreenCapture.CaptureScreenshotAsTexture();
        ceiling.sharedMaterial = originalMaterial;

        string folder = Path.Combine(
            Application.dataPath, "CollegeStudentSimulator", "UI");
        Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, "Loop02_BedPreview.png");
        File.WriteAllBytes(path, frame.EncodeToPNG());
        Destroy(frame);

        UnityEditor.AssetDatabase.Refresh();
        Debug.Log("Saved Loop02_BedPreview.png", this);
    }
#endif
}