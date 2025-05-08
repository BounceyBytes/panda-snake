using System.Collections;
using System.Collections.Generic;
using HyperCasual.Core;
using UnityEngine;

namespace HyperCasual.Runner
{
    /// <summary>
    /// A simple class used to save a load values
    /// using PlayerPrefs.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        /// <summary>
        /// Returns the SaveManager.
        /// </summary>
        public static SaveManager Instance => s_Instance;
        static SaveManager s_Instance;

        const string k_PassportInitialized = "PassportInitialized";
        const string k_IsLoggedIn = "IsLoggedIn";

        void Awake()
        {
            s_Instance = this;
        }

        public bool IsLoggedIn
        {
            get => PlayerPrefs.GetInt(k_IsLoggedIn) == 1;
            set => PlayerPrefs.SetInt(k_IsLoggedIn, value ? 1 : 0);
        }

        public bool IsPassportInitialized
        {
            get => PlayerPrefs.GetInt(k_PassportInitialized) == 1;
            set => PlayerPrefs.SetInt(k_PassportInitialized, value ? 1 : 0);
        }

        public void Clear()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}