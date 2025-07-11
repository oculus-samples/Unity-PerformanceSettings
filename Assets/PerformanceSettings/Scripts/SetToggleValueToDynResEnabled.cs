// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.UI;

namespace Meta.PerformanceSettings
{
    [RequireComponent(typeof(Toggle))]
    public class SetToggleValueToDynResEnabled : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Toggle>().isOn = OVRManager.instance.enableDynamicResolution;
        }
    }
}
