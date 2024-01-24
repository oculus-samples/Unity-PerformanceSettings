// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace Meta.PerformanceSettings
{
    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class SliderValueDisplay : MonoBehaviour
    {
        private TMPro.TMP_Text m_text;
        public string Format;

        private void Awake()
        {
            m_text = GetComponent<TMPro.TMP_Text>();
        }

        public void OnSliderValueChanged(float newVal)
        {
            m_text.text = newVal.ToString(Format);
        }
    }
}
