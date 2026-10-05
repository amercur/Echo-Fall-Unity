using UnityEngine;
using UnityEngine.UI;

namespace EchoFall.Movement
{
    public sealed class MovementLabHUD : MonoBehaviour
    {
        public PlayerMotor player;
        public Text status;

        void Awake()
        {
            if (status != null) return;
            var canvasObject = new GameObject("Movement instructions", typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 540);
            scaler.matchWidthOrHeight = .5f;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            MakeText(canvasObject.transform, font, "ECHO // FALL     MOVEMENT LAB\nA / D or Arrows: Move     Space: Jump (hold for height)     K / Shift: Dash\nDown + Jump: Drop through     R: Restart     Controller: Stick / A / B / View", 12, 15);
            status = MakeText(canvasObject.transform, font, "", 82, 13);
        }

        static Text MakeText(Transform parent, Font font, string words, float top, int size)
        {
            var obj = new GameObject("Instructions", typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(18, -top);
            rect.sizeDelta = new Vector2(925, 76);
            var label = obj.GetComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.text = words;
            label.color = new Color(.8f, .95f, .95f);
            label.raycastTarget = false;
            return label;
        }
        void Update()
        {
            if (player == null || status == null) return;
            string state = player.Dashing ? "DASH" : player.Grounded ? (player.OnPlatform ? "PLATFORM" : "GROUNDED")
                : player.Wall != 0 ? "WALL" : "AIR";
            status.text = $"{state}   |   AIR DASH: {(player.AirDashUsed ? "USED" : "READY")}   |   RESETS: {player.ResetCount}";
        }
    }
}
