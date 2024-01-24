// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.PerformanceSettings
{
    [RequireComponent(typeof(Slider))]
    public class SetSliderValueTo : MonoBehaviour
    {
        public enum ESliderValueSource
        {
            RenderScale,
            MinDynamicResolution,
            MaxDynamicResolution
        }

        public ESliderValueSource Source;

        private IEnumerator Start()
        {
            yield return null;

            var slider = GetComponent<Slider>();
            slider.value = Source switch
            {
                ESliderValueSource.RenderScale => UnityEngine.XR.XRSettings.renderViewportScale / UnityEngine.XR.XRSettings.eyeTextureResolutionScale,
                ESliderValueSource.MinDynamicResolution => OVRManager.instance.minDynamicResolutionScale,
                ESliderValueSource.MaxDynamicResolution => OVRManager.instance.maxDynamicResolutionScale,
                _ => throw new System.Exception(),
            };
        }
    }
}
