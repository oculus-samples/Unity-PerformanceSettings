// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace Meta.PerformanceSettings
{
    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class SetToManifestMetadataValue : MonoBehaviour
    {
        public string Key;

        private void Start()
        {
            GetComponent<TMPro.TMP_Text>().text = GetManifestMetaData(Key);
        }

        private string GetManifestMetaData(string key)
        {
            try
            {
                var pluginClass = new AndroidJavaClass("android.content.pm.PackageManager");
                var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                var flag = pluginClass.GetStatic<int>("GET_META_DATA");
                var pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                var appInfo = pm.Call<AndroidJavaObject>("getApplicationInfo", Application.identifier, flag);
                Debug.Log("key " + Key + " appInfo " + appInfo.ToString());
                var metadata = appInfo.Get<AndroidJavaObject>("metaData");
                Debug.Log("key " + Key + " metadata " + metadata.ToString());
                var stringVal = metadata.Call<AndroidJavaObject>("get", new object[] { key });
                Debug.Log("key " + Key + " stringVal " + (stringVal != null ? stringVal.ToString() : "NULL"));
                var retVal = stringVal.Call<string>("toString");
                return retVal;
            }
            catch (System.Exception e)
            {
                Debug.Log("key " + Key + " exception " + e.ToString());
                return "[Not Set]";
            }
        }
    }
}
