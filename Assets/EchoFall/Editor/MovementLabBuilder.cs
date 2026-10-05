using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoFall.Movement.Editor
{
    public static class MovementLabBuilder
    {
        public const string Root = "Assets/EchoFall";
        public const string ScenePath = Root + "/Scenes/MovementLab.unity";
        public const string PrefabPath = Root + "/Prefabs/Player.prefab";
        static Sprite block;
        static Material unlit;
        static Font font;
        static int solidLayer, platformLayer, hazardLayer, playerLayer;

        [MenuItem("Echo Fall/Open Movement Lab")]
        public static void Open()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) Create();
            else EditorSceneManager.OpenScene(ScenePath);
        }

        // Batch entry point. Never replaces an existing authored scene or prefab.
        public static void Create()
        {
            if (File.Exists(ScenePath)) { Debug.Log("Movement Lab already exists; preserving it."); return; }
            foreach (string folder in new[] { "Scenes", "Prefabs", "Data", "Input", "Art", "Materials" })
                Directory.CreateDirectory(Root + "/" + folder);
            AssetDatabase.Refresh();
            solidLayer = EnsureLayer("EchoSolid");
            platformLayer = EnsureLayer("EchoPlatform");
            hazardLayer = EnsureLayer("EchoHazard");
            playerLayer = EnsureLayer("EchoPlayer");
            Time.fixedDeltaTime = MovementTuning.Step;
            Time.maximumDeltaTime = .1f;

            var tuning = ScriptableObject.CreateInstance<MovementTuning>();
            AssetDatabase.CreateAsset(tuning, Root + "/Data/MovementTuning.asset");
            var input = CreateInput();
            var frames = SliceWanderer();
            PrepareRendering();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var player = new GameObject("Wanderer");
            player.SetActive(false);
            player.layer = playerLayer;
            var body = player.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            var collider = player.AddComponent<BoxCollider2D>();
            collider.size = tuning.size;
            collider.offset = Vector2.up * .2f;
            var motor = player.AddComponent<PlayerMotor>();
            motor.tuning = tuning;
            motor.solids = 1 << solidLayer;
            motor.oneWayPlatforms = 1 << platformLayer;
            motor.hazards = 1 << hazardLayer;
            motor.roomBounds = new Rect(0, -.88f, 32, 12);
            player.AddComponent<MovementInput>().actions = input;
            var visual = new GameObject("Visual");
            visual.transform.SetParent(player.transform, false);
            visual.transform.localScale = Vector3.one * .34f;
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = frames[0];
            renderer.sharedMaterial = unlit;
            renderer.sortingOrder = 10;
            var animator = visual.AddComponent<PlayerVisual>();
            animator.motor = motor;
            animator.frames = frames;
            player.SetActive(true);
            var prefab = PrefabUtility.SaveAsPrefabAsset(player, PrefabPath);
            UnityEngine.Object.DestroyImmediate(player);
            player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            player.transform.position = new Vector3(1.21f, .02f, 0);
            motor = player.GetComponent<PlayerMotor>();

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(RoomCamera));
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.7f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.035f, .06f, .10f);
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 50;
            cameraObject.transform.position = new Vector3(4.8f, 1.82f, -10);
            var follow = cameraObject.GetComponent<RoomCamera>();
            follow.target = motor;
            follow.bounds = motor.roomBounds;

            var geometry = new GameObject("Collision Course").transform;
            Color solid = new Color(.23f, .32f, .40f), platform = new Color(.32f, .66f, .66f);
            Box("West floor", new Rect(0, -1, 14, 1), solidLayer, solid, geometry);
            Box("East floor", new Rect(15.5f, -1, 16.5f, 1), solidLayer, solid, geometry);
            Box("West boundary", new Rect(-.3f, -1, .3f, 13), solidLayer, solid, geometry);
            Box("East boundary", new Rect(32, -1, .3f, 13), solidLayer, solid, geometry);
            Box("Platform 1", new Rect(2.65f, .7f, 1.4f, .2f), platformLayer, platform, geometry);
            Box("Platform 2", new Rect(4.55f, 1.6f, 1.45f, .2f), platformLayer, platform, geometry);
            Box("Platform 3", new Rect(6.6f, 2.5f, 1.7f, .2f), platformLayer, platform, geometry);
            Box("Dash stop wall", new Rect(8.9f, 0, .28f, 1.4f), solidLayer, solid, geometry);
            Box("Low solid ceiling", new Rect(10.2f, 1.05f, 2, .3f), solidLayer, solid, geometry);
            Box("Hazard reset pit", new Rect(14, -.7f, 1.5f, .25f), hazardLayer, new Color(.94f, .3f, .43f), geometry, true);
            // Browser Belfry geometry: x offset 17 units; y = (452 - browserY) / 100.
            Box("Shaft approach", BrowserRect(210, 355, 140, 20, 17), platformLayer, platform, geometry);
            Box("Shaft entry", BrowserRect(335, 255, 165, 20, 17), platformLayer, platform, geometry);
            Box("Belfry left wall", BrowserRect(345, -360, 28, 590, 17), solidLayer, solid, geometry);
            Box("Belfry right wall", BrowserRect(520, -490, 28, 740, 17), solidLayer, solid, geometry);
            Box("Shaft mid rest", BrowserRect(335, -195, 155, 20, 17), platformLayer, platform, geometry);
            Box("Summit landing", BrowserRect(570, -360, 230, 22, 17), platformLayer, platform, geometry);
            Box("Descent 1", new Rect(25.7f, 6.4f, 1.6f, .2f), platformLayer, platform, geometry);
            Box("Descent 2", new Rect(28.2f, 4.4f, 1.6f, .2f), platformLayer, platform, geometry);
            Box("Descent 3", new Rect(30, 2.4f, 1.6f, .2f), platformLayer, platform, geometry);

            Label("01 / VARIABLE JUMP", new Vector2(.65f, 1.55f));
            Label("02 / ONE-WAY PLATFORMS", new Vector2(3.0f, 3.25f));
            Label("03 / WALL + CEILING", new Vector2(8.8f, 2.05f));
            Label("04 / GAP + SAFE RESET", new Vector2(12.85f, 1.0f));
            Label("05 / BELFRY ASCENT", new Vector2(18.65f, 3.3f));
            Label("WALL JUMP\nHold toward wall; tap Jump", new Vector2(18.4f, 5.2f));
            Label("SUMMIT / AIR DASH ACROSS", new Vector2(22.7f, 8.75f));
            Label("06 / DROP + LAND", new Vector2(27.5f, 5.2f));
            AddHud(motor);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }
                .Concat(EditorBuildSettings.scenes.Where(s => s.path != ScenePath)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("Movement Lab created: " + ScenePath);
        }

        static InputActionAsset CreateInput()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "Movement";
            var map = asset.AddActionMap("Movement");
            var move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");
            move.AddBinding("<Gamepad>/dpad");
            var jump = map.AddAction("Jump", InputActionType.Button, "<Keyboard>/space");
            jump.AddBinding("<Gamepad>/buttonSouth");
            var dash = map.AddAction("Dash", InputActionType.Button, "<Keyboard>/k");
            dash.AddBinding("<Keyboard>/leftShift");
            dash.AddBinding("<Keyboard>/rightShift");
            dash.AddBinding("<Gamepad>/buttonEast");
            var reset = map.AddAction("Reset", InputActionType.Button, "<Keyboard>/r");
            reset.AddBinding("<Gamepad>/select");
            string path = Root + "/Input/Movement.inputactions";
            File.WriteAllText(path, asset.ToJson());
            UnityEngine.Object.DestroyImmediate(asset);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        }

        static Sprite[] SliceWanderer()
        {
            string path = Root + "/Art/Wanderer/wanderer-smooth.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 100;
            importer.maxTextureSize = 2048;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            var rects = Enumerable.Range(0, 40).Select(i => new SpriteRect
            {
                name = $"Wanderer_{i:00}", spriteID = GUID.Generate(),
                rect = new Rect(i % 4 * 192, (9 - i / 4) * 192, 192, 192),
                alignment = SpriteAlignment.Custom, pivot = new Vector2(.5f, 1 - 161.529f / 192)
            }).ToArray();
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()
                .SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        }

        static int EnsureLayer(string name)
        {
            int existing = LayerMask.NameToLayer(name);
            if (existing >= 0) return existing;
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tags.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++)
                if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
                {
                    layers.GetArrayElementAtIndex(i).stringValue = name;
                    tags.ApplyModifiedProperties();
                    return i;
                }
            throw new InvalidOperationException("No free layer for " + name);
        }

        static void PrepareRendering()
        {
            var texture = new Texture2D(4, 4);
            texture.SetPixels(Enumerable.Repeat(Color.white, 16).ToArray());
            texture.Apply();
            string path = Root + "/Art/Block.png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 4;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            block = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) throw new InvalidOperationException("URP sprite unlit shader is missing.");
            unlit = new Material(shader);
            AssetDatabase.CreateAsset(unlit, Root + "/Materials/MovementUnlit.mat");
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        static Rect BrowserRect(float x, float y, float w, float h, float offset) =>
            new Rect(offset + x / 100, (452 - y - h) / 100, w / 100, h / 100);

        static void Box(string name, Rect rect, int layer, Color color, Transform parent, bool trigger = false)
        {
            var obj = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D));
            obj.transform.SetParent(parent);
            obj.transform.position = rect.center;
            obj.transform.localScale = new Vector3(rect.width, rect.height, 1);
            obj.layer = layer;
            var renderer = obj.GetComponent<SpriteRenderer>();
            renderer.sprite = block;
            renderer.sharedMaterial = unlit;
            renderer.color = color;
            obj.GetComponent<BoxCollider2D>().size = Vector2.one;
            obj.GetComponent<BoxCollider2D>().isTrigger = trigger || layer == platformLayer;
        }

        static void Label(string words, Vector2 position)
        {
            var obj = new GameObject(words.Split('\n')[0], typeof(TextMesh));
            obj.transform.position = position;
            var text = obj.GetComponent<TextMesh>();
            text.text = words;
            text.font = font;
            text.fontSize = 48;
            text.characterSize = .025f;
            text.color = new Color(.58f, .79f, .83f);
            obj.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        static void AddHud(PlayerMotor motor)
        {
            var obj = new GameObject("Movement instructions", typeof(Canvas), typeof(CanvasScaler));
            obj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = obj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 540);
            scaler.matchWidthOrHeight = .5f;
            UiText(obj.transform, "Controls", "ECHO // FALL     MOVEMENT LAB\nA / D or Arrows: Move     Space: Jump (hold for height)     K / Shift: Dash\nDown + Jump: Drop through     R: Restart     Controller: Stick / A / B / View", new Vector2(18, -12), 15);
            var status = UiText(obj.transform, "Status", "", new Vector2(18, -82), 13);
            var hud = obj.AddComponent<MovementLabHUD>();
            hud.player = motor;
            hud.status = status;
        }

        static Text UiText(Transform parent, string name, string words, Vector2 position, int size)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(925, 80);
            var text = obj.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = new Color(.8f, .95f, .95f);
            text.text = words;
            text.raycastTarget = false;
            return text;
        }

        public static void BuildWindows()
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = "Builds/MovementLab/EchoFall-MovementLab.exe",
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development
            });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("Movement Lab build failed: " + report.summary.result);
        }
    }
}
