// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections;
using UnityEngine;

namespace Meta.PerformanceSettings
{
    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class SetLabelValueTo : MonoBehaviour
    {
        public enum ELabelValueSource
        {
            EyeTexResolutionScale,
            RenderViewportScale,
        }

        public ELabelValueSource Source;
        public string Format;

        private IEnumerator Start()
        {
            yield return null;
            Recalculate();
        }

        private void Update()
        {
            Recalculate();
        }

        private void Recalculate()
        {
            var val = Source switch
            {
                ELabelValueSource.EyeTexResolutionScale => UnityEngine.XR.XRSettings.eyeTextureResolutionScale,
                ELabelValueSource.RenderViewportScale => UnityEngine.XR.XRSettings.renderViewportScale,
                _ => throw new System.Exception(),
            };
            var text = GetComponent<TMPro.TMP_Text>();
            text.text = val.ToString(Format);
        }
    }
}
