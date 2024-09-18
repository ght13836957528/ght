/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

#if UNITY_2018_2_OR_NEWER
#define HAS_CULL_TRANSPARENT_MESH
#endif

#if UNITY_2017_2_OR_NEWER
#define NEWPLAYMODECALLBACKS
#endif

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor
{
    using Icons = SpineEditorUtilities.Icons;

    [CustomEditor(typeof(SkeletonGraphic))]
    [CanEditMultipleObjects]
    public class SkeletonGraphicInspector : UnityEditor.Editor
    {
        const string SeparatorSlotNamesFieldName = "separatorSlotNames";
        const string ReloadButtonString = "Reload";
        protected GUIContent SkeletonDataAssetLabel;
        static GUILayoutOption reloadButtonWidth;

        static GUILayoutOption ReloadButtonWidth
        {
            get
            {
                return reloadButtonWidth = reloadButtonWidth ??
                                           GUILayout.Width(
                                               GUI.skin.label.CalcSize(new GUIContent(ReloadButtonString)).x + 20);
            }
        }

        static GUIStyle ReloadButtonStyle
        {
            get { return EditorStyles.miniButton; }
        }

        SerializedProperty material, color;
        SerializedProperty additiveMaterial, multiplyMaterial, screenMaterial;
        SerializedProperty skeletonDataAsset, initialSkinName;
        SerializedProperty startingAnimation, startingLoop, timeScale, freeze, updateWhenInvisible, unscaledTime;
        SerializedProperty initialFlipX, initialFlipY;
        SerializedProperty meshGeneratorSettings;

        SerializedProperty useClipping,
            zSpacing,
            pmaVertexColors,
            tintBlack,
            canvasGroupTintBlack,
            addNormals,
            calculateTangents,
            immutableTriangles;

        SerializedProperty allowMultipleCanvasRenderers,
            separatorSlotNames,
            enableSeparatorSlots,
            updateSeparatorPartLocation;

        SerializedProperty raycastTarget;

        readonly GUIContent UseClippingLabel = new GUIContent("Use Clipping",
            "When disabled, clipping attachments are ignored. This may be used to save performance.");

        readonly GUIContent ZSpacingLabel = new GUIContent("Z Spacing",
            "A value other than 0 adds a space between each rendered attachment to prevent Z Fighting when using shaders" +
            " that read or write to the depth buffer. Large values may cause unwanted parallax and spaces depending on " +
            "camera setup.");

        readonly GUIContent TintBlackLabel = new GUIContent("Tint Black (!)",
            "Adds black tint vertex data to the mesh as UV2 and UV3. Black tinting requires that the shader interpret " +
            "UV2 and UV3 as black tint colors for this effect to work. You may then want to use the " +
            "[Spine/SkeletonGraphic Tint Black] shader.");

        readonly GUIContent CanvasGroupTintBlackLabel = new GUIContent("CanvasGroup Tint Black",
            "Enable when using SkeletonGraphic under a CanvasGroup. " +
            "When enabled, PMA Vertex Color alpha value is stored at uv2.g instead of color.a to capture " +
            "CanvasGroup modifying color.a. Also helps to detect correct parameter setting combinations.");

        readonly GUIContent PMAVertexColorsLabel = new GUIContent("PMA Vertex Colors",
            "Use this if you are using the default Spine/Skeleton shader or any premultiply-alpha shader.");

        readonly GUIContent AddNormalsLabel = new GUIContent("Add Normals",
            "Use this if your shader requires vertex normals. A more efficient solution for 2D setups is to modify the " +
            "shader to assume a single normal value for the whole mesh.");

        readonly GUIContent CalculateTangentsLabel = new GUIContent("Calculate Tangents",
            "Calculates the tangents per frame. Use this if you are using lit shaders (usually with normal maps) that " +
            "require vertex tangents.");

        readonly GUIContent ImmutableTrianglesLabel = new GUIContent("Immutable Triangles",
            "Enable to optimize rendering for skeletons that never change attachment visbility");

        readonly GUIContent UnscaledTimeLabel = new GUIContent("Unscaled Time",
            "If enabled, AnimationState uses unscaled game time (Time.unscaledDeltaTime), " +
            "running animations independent of e.g. game pause (Time.timeScale). " +
            "Instance SkeletonAnimation.timeScale will still be applied.");

        SkeletonGraphic thisSkeletonGraphic;
        protected bool isInspectingPrefab;
        protected bool slotsReapplyRequired = false;
        protected bool forceReloadQueued = false;

        protected bool TargetIsValid
        {
            get
            {
                if (serializedObject.isEditingMultipleObjects)
                {
                    foreach (var o in targets)
                    {
                        var component = (SkeletonGraphic)o;
                        if (!component.IsValid)
                            return false;
                    }

                    return true;
                }
                else
                {
                    var component = (SkeletonGraphic)target;
                    return component.IsValid;
                }
            }
        }

        void OnEnable()
        {
#if NEW_PREFAB_SYSTEM
            isInspectingPrefab = false;
#else
			isInspectingPrefab = (PrefabUtility.GetPrefabType(target) == PrefabType.Prefab);
#endif
            SpineEditorUtilities.ConfirmInitialization();

            // Labels
            SkeletonDataAssetLabel = new GUIContent("SkeletonData Asset", Icons.spine);

            var so = this.serializedObject;
            thisSkeletonGraphic = target as SkeletonGraphic;

            // MaskableGraphic
            material = so.FindProperty("m_Material");
            color = so.FindProperty("m_Color");
            raycastTarget = so.FindProperty("m_RaycastTarget");

            // SkeletonRenderer
            additiveMaterial = so.FindProperty("additiveMaterial");
            multiplyMaterial = so.FindProperty("multiplyMaterial");
            screenMaterial = so.FindProperty("screenMaterial");

            skeletonDataAsset = so.FindProperty("skeletonDataAsset");
            initialSkinName = so.FindProperty("initialSkinName");

            initialFlipX = so.FindProperty("initialFlipX");
            initialFlipY = so.FindProperty("initialFlipY");

            // SkeletonAnimation
            startingAnimation = so.FindProperty("startingAnimation");
            startingLoop = so.FindProperty("startingLoop");
            timeScale = so.FindProperty("timeScale");
            unscaledTime = so.FindProperty("unscaledTime");
            freeze = so.FindProperty("freeze");
            updateWhenInvisible = so.FindProperty("updateWhenInvisible");

            meshGeneratorSettings = so.FindProperty("meshGenerator").FindPropertyRelative("settings");
            meshGeneratorSettings.isExpanded = SkeletonRendererInspector.advancedFoldout;

            useClipping = meshGeneratorSettings.FindPropertyRelative("useClipping");
            zSpacing = meshGeneratorSettings.FindPropertyRelative("zSpacing");
            tintBlack = meshGeneratorSettings.FindPropertyRelative("tintBlack");
            canvasGroupTintBlack = meshGeneratorSettings.FindPropertyRelative("canvasGroupTintBlack");
            pmaVertexColors = meshGeneratorSettings.FindPropertyRelative("pmaVertexColors");
            calculateTangents = meshGeneratorSettings.FindPropertyRelative("calculateTangents");
            addNormals = meshGeneratorSettings.FindPropertyRelative("addNormals");
            immutableTriangles = meshGeneratorSettings.FindPropertyRelative("immutableTriangles");

            allowMultipleCanvasRenderers = so.FindProperty("allowMultipleCanvasRenderers");
            updateSeparatorPartLocation = so.FindProperty("updateSeparatorPartLocation");
            enableSeparatorSlots = so.FindProperty("enableSeparatorSlots");

            separatorSlotNames = so.FindProperty("separatorSlotNames");
            separatorSlotNames.isExpanded = true;

#if NEWPLAYMODECALLBACKS
            EditorApplication.playModeStateChanged += OnPlaymodeChanged;
#else
			EditorApplication.playmodeStateChanged += OnPlaymodeChanged;
#endif
        }

        void OnDisable()
        {
#if NEWPLAYMODECALLBACKS
            EditorApplication.playModeStateChanged -= OnPlaymodeChanged;
#else
			EditorApplication.playmodeStateChanged -= OnPlaymodeChanged;
#endif
            DisableEditReferenceRectMode();
        }

#if NEWPLAYMODECALLBACKS
        void OnPlaymodeChanged(PlayModeStateChange mode)
        {
#else
		void OnPlaymodeChanged () {
#endif
            DisableEditReferenceRectMode();
        }

        void DisableEditReferenceRectMode()
        {
            foreach (UnityEngine.Object c in targets)
            {
                SkeletonGraphic component = (SkeletonGraphic)c;
                // component.EditReferenceRect = false;
            }
        }

        public override void OnInspectorGUI()
        {
            if (UnityEngine.Event.current.type == EventType.Layout)
            {
                if (forceReloadQueued)
                {
                    forceReloadQueued = false;
                    foreach (var c in targets)
                    {
                        SpineEditorUtilities.ReloadSkeletonDataAssetAndComponent(c as SkeletonGraphic);
                    }
                }
                else
                {
                    foreach (var c in targets)
                    {
                        var component = c as SkeletonGraphic;
                        if (!component.IsValid)
                        {
                            SpineEditorUtilities.ReinitializeComponent(component);
                            if (!component.IsValid) continue;
                        }
                    }
                }
            }

            bool wasChanged = false;
            EditorGUI.BeginChangeCheck();
            do
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    SpineInspectorUtility.PropertyFieldFitLabel(skeletonDataAsset, SkeletonDataAssetLabel);
                    if (GUILayout.Button(ReloadButtonString, ReloadButtonStyle, ReloadButtonWidth))
                        forceReloadQueued = true;
                }

                if (thisSkeletonGraphic.skeletonDataAsset == null)
                {
                    EditorGUILayout.HelpBox("You need to assign a SkeletonDataAsset first.", MessageType.Info);
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                    break;
                }

                if (!SpineEditorUtilities.SkeletonDataAssetIsValid(thisSkeletonGraphic.skeletonDataAsset))
                {
                    EditorGUILayout.HelpBox("SkeletonData asset error. Please check SkeletonData asset.",
                        MessageType.Error);
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                    break;
                }

                using (new SpineInspectorUtility.LabelWidthScope(100))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(material);
                        if (GUILayout.Button("Detect", EditorStyles.miniButton, GUILayout.Width(67f)))
                        {
                            Undo.RecordObjects(targets, "Detect Material");
                            foreach (UnityEngine.Object skeletonGraphic in targets)
                            {
                                DetectMaterial((SkeletonGraphic)skeletonGraphic);
                            }
                        }
                    }

                    EditorGUILayout.PropertyField(color);
                }

                string errorMessage = null;
                if (SpineEditorUtilities.Preferences.componentMaterialWarning &&
                    MaterialChecks.IsMaterialSetupProblematic(thisSkeletonGraphic, ref errorMessage))
                {
                    EditorGUILayout.HelpBox(errorMessage, MessageType.Error, true);
                }

                bool isSingleRendererOnly = (!allowMultipleCanvasRenderers.hasMultipleDifferentValues &&
                                             allowMultipleCanvasRenderers.boolValue == false);
                bool isSeparationEnabledButNotMultipleRenderers =
                    isSingleRendererOnly && (!enableSeparatorSlots.hasMultipleDifferentValues &&
                                             enableSeparatorSlots.boolValue == true);
                bool meshRendersIncorrectlyWithSingleRenderer =
                    isSingleRendererOnly && SkeletonHasMultipleSubmeshes();

                if (isSeparationEnabledButNotMultipleRenderers || meshRendersIncorrectlyWithSingleRenderer)
                    meshGeneratorSettings.isExpanded = true;

                using (new SpineInspectorUtility.BoxScope())
                {
                    EditorGUILayout.PropertyField(meshGeneratorSettings,
                        SpineInspectorUtility.TempContent("Advanced..."), includeChildren: false);
                    SkeletonRendererInspector.advancedFoldout = meshGeneratorSettings.isExpanded;

                    if (meshGeneratorSettings.isExpanded)
                    {
                        EditorGUILayout.Space();
                        using (new SpineInspectorUtility.IndentScope())
                        {
                            DrawMeshSettings();
                            EditorGUILayout.Space();

                            using (new SpineInspectorUtility.LabelWidthScope())
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(allowMultipleCanvasRenderers,
                                    SpineInspectorUtility.TempContent("Multiple CanvasRenderers"));

                                if (GUILayout.Button(
                                        new GUIContent("Trim Renderers",
                                            "Remove currently unused CanvasRenderer GameObjects. These will be regenerated whenever needed."),
                                        EditorStyles.miniButton, GUILayout.Width(100f)))
                                {
                                    Undo.RecordObjects(targets, "Trim Renderers");
                                    foreach (UnityEngine.Object skeletonGraphic in targets)
                                    {
                                        ((SkeletonGraphic)skeletonGraphic).TrimRenderers();
                                    }
                                }

                                EditorGUILayout.EndHorizontal();

                                BlendModeMaterials blendModeMaterials =
                                    thisSkeletonGraphic.skeletonDataAsset.blendModeMaterials;
                                if (allowMultipleCanvasRenderers.boolValue == true &&
                                    blendModeMaterials.RequiresBlendModeMaterials)
                                {
                                    using (new SpineInspectorUtility.IndentScope())
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField("Blend Mode Materials", EditorStyles.boldLabel);

                                        if (GUILayout.Button(
                                                new GUIContent("Detect",
                                                    "Auto-Assign Blend Mode Materials according to Vertex Data and Texture settings."),
                                                EditorStyles.miniButton, GUILayout.Width(100f)))
                                        {
                                            Undo.RecordObjects(targets, "Detect Blend Mode Materials");
                                            foreach (UnityEngine.Object skeletonGraphic in targets)
                                            {
                                                DetectBlendModeMaterials((SkeletonGraphic)skeletonGraphic);
                                            }
                                        }

                                        EditorGUILayout.EndHorizontal();

                                        bool usesAdditiveMaterial = blendModeMaterials.applyAdditiveMaterial;
                                        bool pmaVertexColors =
                                            thisSkeletonGraphic.MeshGenerator.settings.pmaVertexColors;
                                        if (pmaVertexColors)
                                            using (new EditorGUI.DisabledGroupScope(true))
                                            {
                                                EditorGUILayout.LabelField(
                                                    "Additive Material - Unused with PMA Vertex Colors",
                                                    EditorStyles.label);
                                            }
                                        else if (usesAdditiveMaterial)
                                            EditorGUILayout.PropertyField(additiveMaterial,
                                                SpineInspectorUtility.TempContent("Additive Material", null,
                                                    "SkeletonGraphic Material for 'Additive' blend mode slots. Unused when 'PMA Vertex Colors' is enabled."));
                                        else
                                            using (new EditorGUI.DisabledGroupScope(true))
                                            {
                                                EditorGUILayout.LabelField(
                                                    "No Additive Mat - 'Apply Additive Material' disabled at SkeletonDataAsset",
                                                    EditorStyles.label);
                                            }

                                        EditorGUILayout.PropertyField(multiplyMaterial,
                                            SpineInspectorUtility.TempContent("Multiply Material", null,
                                                "SkeletonGraphic Material for 'Multiply' blend mode slots."));
                                        EditorGUILayout.PropertyField(screenMaterial,
                                            SpineInspectorUtility.TempContent("Screen Material", null,
                                                "SkeletonGraphic Material for 'Screen' blend mode slots."));
                                    }
                                }

                                EditorGUILayout.PropertyField(updateWhenInvisible);
                            }

                            // warning box
                            if (isSeparationEnabledButNotMultipleRenderers)
                            {
                                using (new SpineInspectorUtility.BoxScope())
                                {
                                    meshGeneratorSettings.isExpanded = true;
                                    EditorGUILayout.LabelField(
                                        SpineInspectorUtility.TempContent(
                                            "'Multiple Canvas Renderers' must be enabled\nwhen 'Enable Separation' is enabled.",
                                            Icons.warning), GUILayout.Height(42), GUILayout.Width(340));
                                }
                            }
                            else if (meshRendersIncorrectlyWithSingleRenderer)
                            {
                                using (new SpineInspectorUtility.BoxScope())
                                {
                                    meshGeneratorSettings.isExpanded = true;
                                    EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(
                                            "This mesh uses multiple atlas pages or blend modes.\n" +
                                            "You need to enable 'Multiple Canvas Renderers'\n" +
                                            "for correct rendering. Consider packing\n" +
                                            "attachments to a single atlas page if possible.", Icons.warning),
                                        GUILayout.Height(60), GUILayout.Width(380));
                                }
                            }
                        }

                        EditorGUILayout.Space();
                        SeparatorsField(separatorSlotNames, enableSeparatorSlots, updateSeparatorPartLocation);

                        EditorGUILayout.Space();
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(initialSkinName);
                {
                    var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth,
                        EditorGUIUtility.singleLineHeight);
                    EditorGUI.PrefixLabel(rect, SpineInspectorUtility.TempContent("Initial Flip"));
                    rect.x += EditorGUIUtility.labelWidth;
                    rect.width = 30f;
                    SpineInspectorUtility.ToggleLeft(rect, initialFlipX,
                        SpineInspectorUtility.TempContent("X", tooltip: "initialFlipX"));
                    rect.x += 35f;
                    SpineInspectorUtility.ToggleLeft(rect, initialFlipY,
                        SpineInspectorUtility.TempContent("Y", tooltip: "initialFlipY"));
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(startingAnimation);
                EditorGUILayout.PropertyField(startingLoop);
                EditorGUILayout.PropertyField(timeScale);
                EditorGUILayout.PropertyField(unscaledTime, UnscaledTimeLabel);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(freeze);
                EditorGUILayout.Space();
                SkeletonRendererInspector.SkeletonRootMotionParameter(targets);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(raycastTarget);

                EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 5));
                EditorGUILayout.PrefixLabel("Match RectTransform with Mesh");
                if (GUILayout.Button("Match", EditorStyles.miniButton, GUILayout.Width(65f)))
                {
                    foreach (var skeletonGraphic in targets)
                    {
                        MatchRectTransformWithBounds((SkeletonGraphic)skeletonGraphic);
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (TargetIsValid && !isInspectingPrefab)
                {
                    EditorGUILayout.Space();
                    if (SpineInspectorUtility.CenteredButton(
                            new GUIContent("Add Skeleton Utility", Icons.skeletonUtility), 21, true, 200f))
                        foreach (var t in targets)
                        {
                            var component = t as Component;
                            if (component.GetComponent<SkeletonUtility>() == null)
                            {
                                component.gameObject.AddComponent<SkeletonUtility>();
                            }
                        }
                }
            } while (false);


            wasChanged |= EditorGUI.EndChangeCheck();

            if (wasChanged)
            {
                serializedObject.ApplyModifiedProperties();
                slotsReapplyRequired = true;
            }

            if (slotsReapplyRequired && UnityEngine.Event.current.type == EventType.Repaint)
            {
                foreach (var target in targets)
                {
                    var skeletonGraphic = (SkeletonGraphic)target;
                    skeletonGraphic.ReapplySeparatorSlotNames();
                    skeletonGraphic.LateUpdate();
                    SceneView.RepaintAll();
                }

                slotsReapplyRequired = false;
            }
        }

        protected void DrawMeshSettings()
        {
            EditorGUILayout.PropertyField(useClipping, UseClippingLabel);
            const float MinZSpacing = -0.1f;
            const float MaxZSpacing = 0f;
            EditorGUILayout.Slider(zSpacing, MinZSpacing, MaxZSpacing, ZSpacingLabel);
            EditorGUILayout.Space();

            using (new SpineInspectorUtility.LabelWidthScope())
            {
                // EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Vertex Data", SpineInspectorUtility.UnityIcon<MeshFilter>()), EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(pmaVertexColors, PMAVertexColorsLabel);
                    if (GUILayout.Button("Detect", EditorStyles.miniButton, GUILayout.Width(65f)))
                    {
                        Undo.RecordObjects(targets, "Detect PMA Vertex Colors");
                        foreach (UnityEngine.Object skeletonGraphic in targets)
                        {
                            DetectPMAVertexColors((SkeletonGraphic)skeletonGraphic);
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(tintBlack, TintBlackLabel);
                    if (GUILayout.Button("Detect", EditorStyles.miniButton, GUILayout.Width(65f)))
                    {
                        Undo.RecordObjects(targets, "Detect Tint Black");
                        foreach (UnityEngine.Object skeletonGraphic in targets)
                        {
                            DetectTintBlack((SkeletonGraphic)skeletonGraphic);
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(canvasGroupTintBlack, CanvasGroupTintBlackLabel);
                    if (GUILayout.Button("Detect", EditorStyles.miniButton, GUILayout.Width(65f)))
                    {
                        Undo.RecordObjects(targets, "Detect CanvasGroup Compatible");
                        foreach (UnityEngine.Object skeletonGraphic in targets)
                        {
                            DetectCanvasGroupTintBlack((SkeletonGraphic)skeletonGraphic);
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Detect Settings", EditorStyles.miniButton, GUILayout.Width(100f)))
                    {
                        Undo.RecordObjects(targets, "Detect Settings");
                        foreach (UnityEngine.Object skeletonGraphic in targets)
                        {
                            DetectTintBlack((SkeletonGraphic)skeletonGraphic);
                            DetectCanvasGroupTintBlack((SkeletonGraphic)skeletonGraphic);
                            DetectPMAVertexColors((SkeletonGraphic)skeletonGraphic);
                        }
                    }

                    if (GUILayout.Button("Detect Material", EditorStyles.miniButton, GUILayout.Width(100f)))
                    {
                        Undo.RecordObjects(targets, "Detect Material");
                        foreach (UnityEngine.Object skeletonGraphic in targets)
                        {
                            DetectMaterial((SkeletonGraphic)skeletonGraphic);
                        }
                    }
                }

                EditorGUILayout.PropertyField(addNormals, AddNormalsLabel);
                EditorGUILayout.PropertyField(calculateTangents, CalculateTangentsLabel);
                EditorGUILayout.PropertyField(immutableTriangles, ImmutableTrianglesLabel);
            }
        }

        protected bool SkeletonHasMultipleSubmeshes()
        {
            foreach (var target in targets)
            {
                var skeletonGraphic = (SkeletonGraphic)target;
                if (skeletonGraphic.HasMultipleSubmeshInstructions())
                    return true;
            }

            return false;
        }

        public static void SetSeparatorSlotNames(SkeletonRenderer skeletonRenderer, string[] newSlotNames)
        {
            var field = SpineInspectorUtility.GetNonPublicField(typeof(SkeletonRenderer), SeparatorSlotNamesFieldName);
            field.SetValue(skeletonRenderer, newSlotNames);
        }

        public static string[] GetSeparatorSlotNames(SkeletonRenderer skeletonRenderer)
        {
            var field = SpineInspectorUtility.GetNonPublicField(typeof(SkeletonRenderer), SeparatorSlotNamesFieldName);
            return field.GetValue(skeletonRenderer) as string[];
        }

        public static void SeparatorsField(SerializedProperty separatorSlotNames,
            SerializedProperty enableSeparatorSlots,
            SerializedProperty updateSeparatorPartLocation)
        {
            bool multi = separatorSlotNames.serializedObject.isEditingMultipleObjects;
            bool hasTerminalSlot = false;
            if (!multi)
            {
                var sr = separatorSlotNames.serializedObject.targetObject as ISkeletonComponent;
                var skeleton = sr.Skeleton;
                int lastSlot = skeleton.Slots.Count - 1;
                if (skeleton != null)
                {
                    for (int i = 0, n = separatorSlotNames.arraySize; i < n; i++)
                    {
                        int index = skeleton.FindSlotIndex(separatorSlotNames.GetArrayElementAtIndex(i).stringValue);
                        if (index == 0 || index == lastSlot)
                        {
                            hasTerminalSlot = true;
                            break;
                        }
                    }
                }
            }

            string terminalSlotWarning = hasTerminalSlot ? " (!)" : "";

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                const string SeparatorsDescription =
                    "Stored names of slots where the Skeleton's render will be split into different batches. This is used by separate components that split the render into different MeshRenderers or GameObjects.";
                if (separatorSlotNames.isExpanded)
                {
                    EditorGUILayout.PropertyField(separatorSlotNames,
                        SpineInspectorUtility.TempContent(separatorSlotNames.displayName + terminalSlotWarning,
                            Icons.slotRoot, SeparatorsDescription), true);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+", GUILayout.MaxWidth(28f), GUILayout.MaxHeight(15f)))
                    {
                        separatorSlotNames.arraySize++;
                    }

                    GUILayout.EndHorizontal();
                }
                else
                    EditorGUILayout.PropertyField(separatorSlotNames,
                        new GUIContent(
                            separatorSlotNames.displayName + string.Format("{0} [{1}]", terminalSlotWarning,
                                separatorSlotNames.arraySize), SeparatorsDescription), true);

                using (new SpineInspectorUtility.LabelWidthScope())
                {
                    EditorGUILayout.PropertyField(enableSeparatorSlots,
                        SpineInspectorUtility.TempContent("Enable Separation",
                            tooltip: "Whether to enable separation at the above separator slots."));
                    EditorGUILayout.PropertyField(updateSeparatorPartLocation,
                        SpineInspectorUtility.TempContent("Update Part Location",
                            tooltip:
                            "Update separator part GameObject location to match the position of the SkeletonGraphic. This can be helpful when re-parenting parts to a different GameObject."));
                }
            }
        }

        #region Auto Detect Setting

        static void DetectTintBlack(SkeletonGraphic skeletonGraphic)
        {
            bool requiresTintBlack = HasTintBlackSlot(skeletonGraphic);
            if (requiresTintBlack)
                Debug.Log(string.Format("Found Tint-Black slot at '{0}'", skeletonGraphic));
            else
                Debug.Log(string.Format("No Tint-Black slot found at '{0}'", skeletonGraphic));
            skeletonGraphic.MeshGenerator.settings.tintBlack = requiresTintBlack;
        }

        static bool HasTintBlackSlot(SkeletonGraphic skeletonGraphic)
        {
            SlotData[] slotsItems = skeletonGraphic.SkeletonData.Slots.Items;
            for (int i = 0, count = skeletonGraphic.SkeletonData.Slots.Count; i < count; ++i)
            {
                SlotData slotData = slotsItems[i];
                if (slotData.HasSecondColor)
                    return true;
            }

            return false;
        }

        static void DetectCanvasGroupTintBlack(SkeletonGraphic skeletonGraphic)
        {
            bool requiresCanvasGroupTintBlack = IsBelowCanvasGroup(skeletonGraphic);
            if (requiresCanvasGroupTintBlack)
                Debug.Log(string.Format("Skeleton is a child of CanvasGroup: '{0}'", skeletonGraphic));
            else
                Debug.Log(string.Format("Skeleton is not a child of CanvasGroup: '{0}'", skeletonGraphic));
            skeletonGraphic.MeshGenerator.settings.canvasGroupTintBlack = requiresCanvasGroupTintBlack;
        }

        static bool IsBelowCanvasGroup(SkeletonGraphic skeletonGraphic)
        {
            return skeletonGraphic.gameObject.GetComponentInParent<CanvasGroup>() != null;
        }

        static void DetectPMAVertexColors(SkeletonGraphic skeletonGraphic)
        {
            MeshGenerator.Settings settings = skeletonGraphic.MeshGenerator.settings;
            bool usesSpineShader = MaterialChecks.UsesSpineShader(skeletonGraphic.material);
            if (!usesSpineShader)
            {
                Debug.Log(string.Format("Skeleton is not using a Spine shader, thus the shader is likely " +
                                        "not using PMA vertex color: '{0}'", skeletonGraphic));
                skeletonGraphic.MeshGenerator.settings.pmaVertexColors = false;
                return;
            }

            bool requiresPMAVertexColorsDisabled = settings.canvasGroupTintBlack && !settings.tintBlack;
            if (requiresPMAVertexColorsDisabled)
            {
                Debug.Log(string.Format("Skeleton requires PMA Vertex Colors disabled: '{0}'", skeletonGraphic));
                skeletonGraphic.MeshGenerator.settings.pmaVertexColors = false;
            }
            else
            {
                Debug.Log(string.Format("Skeleton requires or permits PMA Vertex Colors enabled: '{0}'",
                    skeletonGraphic));
                skeletonGraphic.MeshGenerator.settings.pmaVertexColors = true;
            }
        }

        static bool IsSkeletonTexturePMA(SkeletonGraphic skeletonGraphic, out bool detectionSucceeded)
        {
            Texture texture = skeletonGraphic.mainTexture;
            string texturePath = AssetDatabase.GetAssetPath(texture.GetInstanceID());
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texturePath);
            if (importer.alphaIsTransparency != importer.sRGBTexture)
            {
                Debug.LogWarning(string.Format("Texture '{0}' at skeleton '{1}' is neither configured correctly for " +
                                               "PMA nor Straight Alpha.", texture, skeletonGraphic), texture);
                detectionSucceeded = false;
                return false;
            }

            detectionSucceeded = true;
            bool isPMATexture = !importer.alphaIsTransparency && !importer.sRGBTexture;
            return isPMATexture;
        }

        static void DetectMaterial(SkeletonGraphic skeletonGraphic)
        {
            MeshGenerator.Settings settings = skeletonGraphic.MeshGenerator.settings;

            bool detectionSucceeded;
            bool usesPMATexture = IsSkeletonTexturePMA(skeletonGraphic, out detectionSucceeded);
            if (!detectionSucceeded)
            {
                Debug.LogWarning(string.Format("Unable to assign Material for skeleton '{0}'.", skeletonGraphic),
                    skeletonGraphic);
                return;
            }

            Material newMaterial = null;
            if (usesPMATexture)
            {
                if (settings.tintBlack)
                {
                    if (settings.canvasGroupTintBlack)
                        newMaterial = MaterialWithName("SkeletonGraphicTintBlack-CanvasGroup");
                    else
                        newMaterial = MaterialWithName("SkeletonGraphicTintBlack");
                }
                else
                {
                    // not tintBlack
                    if (settings.canvasGroupTintBlack)
                        newMaterial = MaterialWithName("SkeletonGraphicDefault-CanvasGroup");
                    else
                        newMaterial = MaterialWithName("SkeletonGraphicDefault");
                }
            }
            else
            {
                // straight alpha texture
                if (settings.tintBlack)
                {
                    if (settings.canvasGroupTintBlack)
                        newMaterial = MaterialWithName("SkeletonGraphicTintBlack-CanvasGroupStraight");
                    else
                        newMaterial = MaterialWithName("SkeletonGraphicTintBlack-Straight");
                }
                else
                {
                    // not tintBlack
                    if (settings.canvasGroupTintBlack)
                        newMaterial = MaterialWithName("SkeletonGraphicDefault-CanvasGroupStraight");
                    else
                        newMaterial = MaterialWithName("SkeletonGraphicDefault-Straight");
                }
            }

            if (newMaterial != null)
            {
                Debug.Log(string.Format("Assigning material '{0}' at skeleton '{1}'",
                    newMaterial, skeletonGraphic), newMaterial);
                skeletonGraphic.material = newMaterial;
            }
        }

        static void DetectBlendModeMaterials(SkeletonGraphic skeletonGraphic)
        {
            bool detectionSucceeded;
            bool usesPMATexture = IsSkeletonTexturePMA(skeletonGraphic, out detectionSucceeded);
            if (!detectionSucceeded)
            {
                Debug.LogWarning(
                    string.Format("Unable to assign Blend Mode materials for skeleton '{0}'.", skeletonGraphic),
                    skeletonGraphic);
                return;
            }

            DetectBlendModeMaterial(skeletonGraphic, BlendMode.Additive, usesPMATexture);
            DetectBlendModeMaterial(skeletonGraphic, BlendMode.Multiply, usesPMATexture);
            DetectBlendModeMaterial(skeletonGraphic, BlendMode.Screen, usesPMATexture);
        }

        static void DetectBlendModeMaterial(SkeletonGraphic skeletonGraphic, BlendMode blendMode, bool usesPMATexture)
        {
            MeshGenerator.Settings settings = skeletonGraphic.MeshGenerator.settings;

            string optionalTintBlack = settings.tintBlack ? "TintBlack" : "";
            string blendModeString = blendMode.ToString();
            string optionalDash = settings.canvasGroupTintBlack || !usesPMATexture ? "-" : "";
            string optionalCanvasGroup = settings.canvasGroupTintBlack ? "CanvasGroup" : "";
            string optionalStraight = !usesPMATexture ? "Straight" : "";

            string materialName = string.Format("SkeletonGraphic{0}{1}{2}{3}{4}",
                optionalTintBlack, blendModeString, optionalDash, optionalCanvasGroup, optionalStraight);
            Material newMaterial = MaterialWithName(materialName);

            if (newMaterial != null)
            {
                switch (blendMode)
                {
                    case BlendMode.Additive:
                        skeletonGraphic.additiveMaterial = newMaterial;
                        break;
                    case BlendMode.Multiply:
                        skeletonGraphic.multiplyMaterial = newMaterial;
                        break;
                    case BlendMode.Screen:
                        skeletonGraphic.screenMaterial = newMaterial;
                        break;
                }
            }
        }

        #endregion

        #region Menus

        [MenuItem("CONTEXT/SkeletonGraphic/Match RectTransform with Mesh Bounds")]
        static void MatchRectTransformWithBounds(MenuCommand command)
        {
            var skeletonGraphic = (SkeletonGraphic)command.context;
            MatchRectTransformWithBounds(skeletonGraphic);
        }

        static void MatchRectTransformWithBounds(SkeletonGraphic skeletonGraphic)
        {
            if (!skeletonGraphic.MatchRectTransformWithBounds())
                Debug.Log("Mesh was not previously generated.");
        }

        [MenuItem("GameObject/Spine/SkeletonGraphic (UnityUI)", false, 15)]
        static public void SkeletonGraphicCreateMenuItem()
        {
            var parentGameObject = Selection.activeObject as GameObject;
            var parentTransform = parentGameObject == null ? null : parentGameObject.GetComponent<RectTransform>();

            if (parentTransform == null)
                Debug.LogWarning("Your new SkeletonGraphic will not be visible until it is placed under a Canvas");

            var gameObject = NewSkeletonGraphicGameObject("New SkeletonGraphic");
            gameObject.transform.SetParent(parentTransform, false);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = gameObject;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        // SpineEditorUtilities.InstantiateDelegate. Used by drag and drop.
        public static Component SpawnSkeletonGraphicFromDrop(SkeletonDataAsset data)
        {
            return InstantiateSkeletonGraphic(data);
        }

        public static SkeletonGraphic InstantiateSkeletonGraphic(SkeletonDataAsset skeletonDataAsset, string skinName)
        {
            return InstantiateSkeletonGraphic(skeletonDataAsset,
                skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
        }

        public static SkeletonGraphic InstantiateSkeletonGraphic(SkeletonDataAsset skeletonDataAsset, Skin skin = null)
        {
            string spineGameObjectName =
                string.Format("SkeletonGraphic ({0})", skeletonDataAsset.name.Replace("_SkeletonData", ""));
            var go = NewSkeletonGraphicGameObject(spineGameObjectName);
            var graphic = go.GetComponent<SkeletonGraphic>();
            graphic.skeletonDataAsset = skeletonDataAsset;

            SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

            if (data == null)
            {
                for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++)
                {
                    string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
                    skeletonDataAsset.atlasAssets[i] =
                        (AtlasAssetBase)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAssetBase));
                }

                data = skeletonDataAsset.GetSkeletonData(true);
            }

            skin = skin ?? data.DefaultSkin ?? data.Skins.Items[0];
            graphic.MeshGenerator.settings.zSpacing = SpineEditorUtilities.Preferences.defaultZSpacing;

            graphic.startingLoop = SpineEditorUtilities.Preferences.defaultInstantiateLoop;
            graphic.Initialize(false);
            if (skin != null) graphic.Skeleton.SetSkin(skin);
            graphic.initialSkinName = skin.Name;
            graphic.Skeleton.UpdateWorldTransform();
            graphic.UpdateMesh();
            return graphic;
        }

        static GameObject NewSkeletonGraphicGameObject(string gameObjectName)
        {
            var go = EditorInstantiation.NewGameObject(gameObjectName, true, typeof(RectTransform),
                typeof(CanvasRenderer), typeof(SkeletonGraphic));
            var graphic = go.GetComponent<SkeletonGraphic>();
            graphic.material = SkeletonGraphicInspector.DefaultSkeletonGraphicMaterial;
            graphic.additiveMaterial = SkeletonGraphicInspector.DefaultSkeletonGraphicAdditiveMaterial;
            graphic.multiplyMaterial = SkeletonGraphicInspector.DefaultSkeletonGraphicMultiplyMaterial;
            graphic.screenMaterial = SkeletonGraphicInspector.DefaultSkeletonGraphicScreenMaterial;

#if HAS_CULL_TRANSPARENT_MESH
            CanvasRenderer canvasRenderer = go.GetComponent<CanvasRenderer>();
            canvasRenderer.cullTransparentMesh = false;
#endif
            return go;
        }

        public static Material DefaultSkeletonGraphicMaterial
        {
            get { return MaterialWithName("SkeletonGraphicDefault"); }
        }

        public static Material DefaultSkeletonGraphicAdditiveMaterial
        {
            get { return MaterialWithName("SkeletonGraphicAdditive"); }
        }

        public static Material DefaultSkeletonGraphicMultiplyMaterial
        {
            get { return MaterialWithName("SkeletonGraphicMultiply"); }
        }

        public static Material DefaultSkeletonGraphicScreenMaterial
        {
            get { return MaterialWithName("SkeletonGraphicScreen"); }
        }

        protected static Material MaterialWithName(string name)
        {
            string[] guids = AssetDatabase.FindAssets(name + " t:material");
            if (guids.Length <= 0) return null;

            int closestNameDistance = int.MaxValue;
            int closestNameIndex = 0;
            for (int i = 0; i < guids.Length; ++i)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                int distance = string.CompareOrdinal(assetName, name);
                if (distance < closestNameDistance)
                {
                    closestNameDistance = distance;
                    closestNameIndex = i;
                }
            }

            string foundAssetPath = AssetDatabase.GUIDToAssetPath(guids[closestNameIndex]);
            if (string.IsNullOrEmpty(foundAssetPath)) return null;

            Material firstMaterial = AssetDatabase.LoadAssetAtPath<Material>(foundAssetPath);
            return firstMaterial;
        }

        #endregion
    }
}