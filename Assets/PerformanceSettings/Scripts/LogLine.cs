// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace Meta.PerformanceSettings
{
    public class LogLine : MonoBehaviour
    {
        public TMPro.TMP_Text AssocLogText;
        public TMPro.TMP_Text AssocDetailsText;
        public GameObject[] ShowWhenExpanded;
        private bool m_expanded = false;

        public void Set(string logText, string details, Color color)
        {
            AssocLogText.text = logText;
            AssocLogText.color = color;
            AssocDetailsText.text = details;
            Recalculate();
        }

        public void ToggleExpanded()
        {
            if (!m_expanded)
            {
                m_expanded = true;
                AssocLogText.enableWordWrapping = true;
                AssocLogText.overflowMode = TMPro.TextOverflowModes.Overflow;
            }
            else
            {
                m_expanded = false;
                AssocLogText.enableWordWrapping = false;
                AssocLogText.overflowMode = TMPro.TextOverflowModes.Truncate;
            }
            Recalculate();
        }

        private void Recalculate()
        {
            foreach (var toShow in ShowWhenExpanded)
                toShow.SetActive(m_expanded);
        }
    }
}
