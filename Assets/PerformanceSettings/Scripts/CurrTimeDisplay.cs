// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using UnityEngine;

namespace Meta.PerformanceSettings
{
    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class CurrTimeDisplay : MonoBehaviour
    {
        public string TimestampFormat;
        private TMPro.TMP_Text m_assocText;

        private void Awake()
        {
            m_assocText = GetComponent<TMPro.TMP_Text>();
        }

        private void Update()
        {
            m_assocText.text = DateTime.Now.ToString(TimestampFormat);
        }
    }
}
