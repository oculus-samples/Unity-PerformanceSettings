// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace Meta.PerformanceSettings
{
    [InitializeOnLoad]
    public class IntroEditorWindow : EditorWindow
    {
        private XmlDocument m_manifestDoc;
        private XmlElement m_manifestBody;
        private string m_manifestNamespace;
        private static string s_manifestLocation = "/Plugins/Android/AndroidManifest.xml";
        private static string s_manifestDualCoreModeFlag = "com.oculus.dualcorecpuset";
        private static string s_manifesTradeCpuForGpuFlag = "com.oculus.trade_cpu_for_gpu_amount";

        private static string s_hasWindowDisplayedSessionBool = "com.samples.performancesettings.has_window_displayed";

        // This is called after inspectors update, a good time to display windows. Note: is per-frame, not just on startup.
        static IntroEditorWindow() => EditorApplication.delayCall += ShowOnStartup;

        private static void ShowOnStartup()
        {
            var isAlreadyDisplayed = SessionState.GetBool(s_hasWindowDisplayedSessionBool, false);
            if (!isAlreadyDisplayed)
            {
                ShowWindow();
                SessionState.SetBool(s_hasWindowDisplayedSessionBool, true);
            }
        }

        [MenuItem("Meta/Unity Performance Settings Sample Intro Window")]
        public static void ShowWindow()
        {
            var inspectorType = Type.GetType("UnityEditor.InspectorWindow, UnityEditor.CoreModule");
            _ = GetWindow<IntroEditorWindow>("Meta/Unity Performance Settings Sample", false, inspectorType);
        }

        private void OnEnable()
        {
            LoadManifest();
        }

        private void OnGUI()
        {
            _ = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Meta/Unity Performance Settings Sample", new GUIStyle("SettingsHeader"));
            EditorGUILayout.LabelField("This sample is designed to allow quick testing of performance profiles and features on your Meta Quest device.\n\n" +
                "It is recommended to use a profiler (like OVR Metrics Tool or Perfetto) alongside this sample, to measure performance statistics.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Relevant Documentation", EditorStyles.boldLabel);
            if (EditorGUILayout.LinkButton("OVR Metrics Tool"))
                Application.OpenURL("https://developer.oculus.com/documentation/native/android/ts-ovrmetricstool/");
            if (EditorGUILayout.LinkButton("Dynamic Resolution"))
                Application.OpenURL("https://developer.oculus.com/documentation/unity/dynamic-resolution-unity/");
            if (EditorGUILayout.LinkButton("CPU and GPU Levels"))
                Application.OpenURL("https://developer.oculus.com/resources/os-cpu-gpu-levels/");
            if (EditorGUILayout.LinkButton("CPU and GPU Level 5"))
                Application.OpenURL("https://developer.oculus.com/documentation/unity/po-quest-boost/");

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Build-Time Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("These controls modify your application's AndroidManifest.xml. These settings can change your performance profile, and can only be changed in-editor, before creating a build.", EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);
            _ = EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("[?]"))
                Application.OpenURL("https://developer.oculus.com/documentation/unity/po-quest-boost/#dual-core-mode");

            GUILayout.Label("Dual-Core Mode");
            var isDualCoreEnabled = GetManifestFlagValue(s_manifestDualCoreModeFlag);
            if (GUILayout.Toggle(isDualCoreEnabled, new GUIContent(isDualCoreEnabled ? "Enabled" : "Disabled")) != isDualCoreEnabled)
            {
                SetManifestFlagValue(s_manifestDualCoreModeFlag, !isDualCoreEnabled);
            }
            EditorGUILayout.EndHorizontal();

            _ = EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("[?]"))
                Application.OpenURL("https://developer.oculus.com/documentation/unity/po-quest-boost/#trading-between-cpu-and-gpu-levels");

            GUILayout.Label("Favor CPU/GPU");
            if (GUILayout.Button(new GUIContent("Set value in OVRManager")))
            {
                Selection.activeGameObject = FindObjectOfType<OVRManager>().gameObject;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Additional Notes", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The menus in this sample are rendered into separate compositor layers, and therefore are not affected by many render settings. This is a best practice for UI. "
                + "To see the effect of render settings on perceived quality, spawn objects via the 'gpu utilization' slider.", EditorStyles.wordWrappedLabel);

        }

        private void LoadManifest()
        {
            try
            {
                m_manifestDoc = new XmlDocument();
                m_manifestDoc.Load(Application.dataPath + s_manifestLocation);
                m_manifestNamespace = ((XmlElement)m_manifestDoc.SelectSingleNode("/manifest")).GetAttribute("xmlns:android");
                m_manifestBody = (XmlElement)m_manifestDoc.SelectSingleNode("/manifest/application");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private void SaveManifest()
        {
            try
            {
                m_manifestDoc.Save(Application.dataPath + s_manifestLocation);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private bool GetManifestFlagValue(string key)
        {
            try
            {
                foreach (XmlElement e in m_manifestBody.GetElementsByTagName("meta-data"))
                {
                    if (e.GetAttribute("name", m_manifestNamespace) == key)
                        return e.GetAttribute("value", m_manifestNamespace).ToLower() == "true";
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SetManifestFlagValue(string key, bool value)
        {
            try
            {
                XmlElement manifestFlagElement = null;
                foreach (XmlElement e in m_manifestBody.GetElementsByTagName("meta-data"))
                {
                    if (e.GetAttribute("name", m_manifestNamespace) == key)
                        manifestFlagElement = e;
                }
                if (manifestFlagElement == null)
                {
                    manifestFlagElement = m_manifestDoc.CreateElement("meta-data");
                    _ = manifestFlagElement.SetAttribute("name", m_manifestNamespace, key);
                    _ = m_manifestBody.AppendChild(manifestFlagElement);
                }

                _ = manifestFlagElement.SetAttribute("value", m_manifestNamespace, value ? "true" : "false");
                SaveManifest();
                LoadManifest();
            }
            catch (Exception) { }
        }
    }
}
