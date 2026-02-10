using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace I18N
{
    public class I18NText : MonoBehaviour
    {
        [Tooltip("本地化文本的Key")] public bool useLocalizationKey = false;
        [ShowIf("useLocalizationKey")] public string localizationKey;
    }
}