using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HyperCasual.Core;
using Immutable.Passport;
using Cysharp.Threading.Tasks;
using TMPro;

namespace HyperCasual.UI
{
    public class MenuView : View
    {
        [SerializeField] private Button m_PlayButton;
        [SerializeField] private Button m_LoginButton;
        [SerializeField] private Button m_LogoutButton;
        [SerializeField] private Text m_LoginButtonText;
        [SerializeField] private TextMeshProUGUI m_StatusText;
        [SerializeField] private TextMeshProUGUI m_UserInfoText;
        [SerializeField] private string m_GameSceneName = "GameScene"; // Name of your game scene

        private Passport m_Passport;
        private bool m_IsLoggedIn;
        private string m_UserEmail;
        private string m_PassportAddress;
        private string[] m_LinkedAddresses;

        private async UniTask<bool> SetIsLoggedIn(bool value)
        {
            if (value)
            {
                try
                {
                    // Verify identity via JWT
                    string idToken = await Passport.Instance.GetIdToken();
                    if (string.IsNullOrEmpty(idToken))
                    {
                        Debug.LogError("Failed to get ID token");
                        return false;
                    }

                    // Get user info using the correct API methods
                    m_UserEmail = await Passport.Instance.GetEmail();
                    
                    // Set up wallet first
                    await Passport.Instance.ConnectEvm();
                    await Passport.Instance.ZkEvmRequestAccounts();
                    
                    // Now get linked addresses
                    var linkedAddressesList = await Passport.Instance.GetLinkedAddresses();
                    m_LinkedAddresses = linkedAddressesList?.ToArray();
                    m_PassportAddress = m_LinkedAddresses != null && m_LinkedAddresses.Length > 0 
                        ? m_LinkedAddresses[0] 
                        : "No Passport address";

                    m_IsLoggedIn = true;
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to verify identity: {e.Message}");
                    return false;
                }
            }
            else
            {
                m_IsLoggedIn = false;
                m_UserEmail = null;
                m_PassportAddress = null;
                m_LinkedAddresses = null;
                return true;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (m_PlayButton != null)
            {
                m_PlayButton.onClick.AddListener(OnPlayButtonClicked);
            }
            if (m_LoginButton != null)
            {
                m_LoginButton.onClick.AddListener(OnLoginButtonClicked);
            }
            if (m_LogoutButton != null)
            {
                m_LogoutButton.onClick.AddListener(OnLogoutButtonClicked);
            }
            
            // Try to find the status text if not set
            if (m_StatusText == null)
            {
                m_StatusText = GetComponentInChildren<TextMeshProUGUI>();
                if (m_StatusText == null)
                {
                    Debug.LogError("Status text not found! Please assign it in the inspector.");
                    return;
                }
            }
            
            InitializePassportAsync().Forget();
            UpdateUI();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_PlayButton != null)
            {
                m_PlayButton.onClick.RemoveListener(OnPlayButtonClicked);
            }
            if (m_LoginButton != null)
            {
                m_LoginButton.onClick.RemoveListener(OnLoginButtonClicked);
            }
            if (m_LogoutButton != null)
            {
                m_LogoutButton.onClick.RemoveListener(OnLogoutButtonClicked);
            }
        }

        private async UniTask InitializePassportAsync()
        {
            try
            {
                string clientId = "WAluhyj5KOamBHWh4FvRbz8S4N8ZDLTZ";
                
                // SANDBOX for testing or PRODUCTION for production
                string environment = Immutable.Passport.Model.Environment.SANDBOX;
        
                // Redirect URLs
                string mobileRedirectUri = "pandasnake://redirect";
                string mobileLogoutUri = "pandasnake://logout";
                string redirectUri = null;
                string logoutUri = null;
                #if (UNITY_ANDROID && !UNITY_EDITOR_WIN) || (UNITY_IPHONE && !UNITY_EDITOR_WIN) || UNITY_STANDALONE_OSX
                        redirectUri = mobileRedirectUri;
                        logoutUri = mobileLogoutUri;
                #endif
                
                // Initialise Passport and store the instance
                m_Passport = await Passport.Init(clientId, environment, redirectUri, logoutUri);
                Debug.Log("Passport initialized successfully");
                await ShouldUserAlreadyBeLoggedIn();
                UpdateUI();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Passport: {e.Message}");
                UpdateUI("Failed to initialize");
            }
        }

        private async UniTask ShouldUserAlreadyBeLoggedIn() {
            // Check if the player is supposed to be logged in and if there are credentials saved
            if (m_IsLoggedIn && await Passport.Instance.HasCredentialsSaved())
            {
                // Try to log in using saved credentials
                bool success = await Passport.Instance.Login(useCachedSession: true);
                // Update the login flag with verification
                if (success)
                {
                    bool verified = await SetIsLoggedIn(true);
                    if (verified)
                    {
                        await Passport.Instance.ConnectEvm();
                        await Passport.Instance.ZkEvmRequestAccounts();
                    }
                    else
                    {
                        await SetIsLoggedIn(false);
                    }
                }
            }
            else
            {
                // No saved credentials to re-login the player, reset the login flag
                await SetIsLoggedIn(false);
            }
        }

        private void OnPlayButtonClicked()
        {
            // Optionally, you can check if user is logged in before allowing play
            if (!m_IsLoggedIn) {
                UpdateUI("Please login first");
                return;
            }
            SceneManager.LoadScene(m_GameSceneName);
        }

        private async void OnLoginButtonClicked()
        {
            if (m_Passport == null)
            {
                Debug.LogError("Passport not initialized");
                UpdateUI("Passport not initialized");
                return;
            }

            try
            {
                m_LoginButton.interactable = false;
                UpdateUI("Connecting to Passport...");

                #if (UNITY_ANDROID && !UNITY_EDITOR_WIN) || (UNITY_IPHONE && !UNITY_EDITOR_WIN) || UNITY_STANDALONE_OSX
                    await m_Passport.LoginPKCE();
                #else
                    await m_Passport.Login();
                #endif
                
                bool verified = await SetIsLoggedIn(true);
                if (verified)
                {
                    Debug.Log("Successfully logged in with Passport");
                    UpdateUI();
                    SetupWallet();
                }
                else
                {
                    UpdateUI("Failed to verify identity");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Login failed: {e.Message}");
                UpdateUI("Login failed");
                await SetIsLoggedIn(false);
            }
            finally
            {
                m_LoginButton.interactable = true;
            }
        }

        private async void SetupWallet()
        {
            try
            {
                UpdateUI("Setting up your wallet...");

                // Set up provider
                await Passport.Instance.ConnectEvm();
                // Set up wallet (includes creating a wallet for new players)
                await Passport.Instance.ZkEvmRequestAccounts();

                UpdateUI("Your wallet has been successfully set up!");
            }
            catch (System.Exception ex)
            {
                // Failed to set up wallet, let the player try again
                Debug.Log($"Failed to set up wallet: {ex.Message}");
                UpdateUI("Failed to set up wallet");
            }
        }

        private async void OnLogoutButtonClicked()
        {
            if (m_Passport == null)
            {
                Debug.LogError("Passport not initialized");
                UpdateUI("Passport not initialized");
                return;
            }

            try
            {
                m_LogoutButton.interactable = false;
                UpdateUI("Logging out...");

                // Logout from Passport
                await m_Passport.LogoutPKCE();
                await SetIsLoggedIn(false);
                
                Debug.Log("Successfully logged out from Passport");
                UpdateUI();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Logout failed: {e.Message}");
                UpdateUI("Logout failed");
            }
            finally
            {
                m_LogoutButton.interactable = true;
            }
        }

        private void UpdateUI(string statusMessage = null)
        {
            if (m_LoginButton != null)
            {
                m_LoginButton.gameObject.SetActive(!m_IsLoggedIn);
            }
            if (m_LogoutButton != null)
            {
                m_LogoutButton.gameObject.SetActive(m_IsLoggedIn);
            }

            if (m_LoginButtonText != null)
            {
                m_LoginButtonText.text = m_IsLoggedIn ? "Logout" : "Login with Passport";
            }

            if (m_StatusText != null)
            {
                if (!string.IsNullOrEmpty(statusMessage))
                {
                    m_StatusText.text = statusMessage;
                }
                else if (m_Passport == null)
                {
                    m_StatusText.text = "Initializing Passport...";
                }
                else
                {
                    m_StatusText.text = m_IsLoggedIn ? "Connected to Passport" : "Not connected";
                }
            }

            if (m_UserInfoText != null)
            {
                if (m_IsLoggedIn && !string.IsNullOrEmpty(m_UserEmail))
                {
                    string linkedAddresses = m_LinkedAddresses != null && m_LinkedAddresses.Length > 0 
                        ? string.Join("\n", m_LinkedAddresses) 
                        : "No linked addresses";
                    
                    m_UserInfoText.text = $"Email: {m_UserEmail}\n" +
                                        $"Passport Address: {m_PassportAddress}\n" +
                                        $"Linked Addresses:\n{linkedAddresses}";
                    m_UserInfoText.gameObject.SetActive(true);
                }
                else
                {
                    m_UserInfoText.text = "";
                    m_UserInfoText.gameObject.SetActive(false);
                }
            }

            // Optionally enable/disable play button based on login status
            if (m_PlayButton != null)
            {
                m_PlayButton.interactable = m_IsLoggedIn;
            }
        }
    }
} 