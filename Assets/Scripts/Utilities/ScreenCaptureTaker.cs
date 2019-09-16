using UnityEngine;

public class ScreenCaptureTaker : KeptMonoSingleton<ScreenCaptureTaker>
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            ScreenCapture.CaptureScreenshot("Capture_" + Time.frameCount + ".jpg");
        }
    }
}
