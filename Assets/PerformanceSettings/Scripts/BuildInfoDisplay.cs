// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Reflection;
using UnityEngine;

//If this is a build, displays the date/time and version info of the build.
//Thanks to https://answers.unity.com/questions/8449/how-can-a-game-automatically-display-the-datetime.html 

[assembly: AssemblyVersion("1.0.*")]
namespace Meta.PerformanceSettings
{
    public class BuildInfoDisplay : MonoBehaviour
    {
        private TMPro.TextMeshProUGUI m_assocText;
        [Header("{0} is build date; {1} is version number; {2} is version name")]
        public string TextFormat;

        private void Awake()
        {
            m_assocText = GetComponent<TMPro.TextMeshProUGUI>();
        }

        private void Start()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var startDate = new DateTime(2000, 1, 1, 0, 0, 0);
            var span = new TimeSpan(version.Build, 0, 0, version.Revision * 2);
            var buildDate = startDate.Add(span);

            m_assocText.text = string.Format(TextFormat, buildDate.ToString(@"dddd, dd MMMM yyyy hh:mm tt"), GetVersionCode(), GetVersionName());
        }

        public static string GetVersionName()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var packageMngr = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            var packageName = currentActivity.Call<string>("getPackageName");
            var packageInfo = packageMngr.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
            return packageInfo.Get<string>("versionName");
        }

        public static int GetVersionCode()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var packageMngr = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            var packageName = currentActivity.Call<string>("getPackageName");
            var packageInfo = packageMngr.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
            return packageInfo.Get<int>("versionCode");
        }
    }
}
