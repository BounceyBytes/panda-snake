using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Immutable.Passport;
  
public class InitPassport : MonoBehaviour
{
    private Passport passport;
    
    async void Start()
    {
        // Replace with your actual Passport Client ID
        string clientId = "YOUR_IMMUTABLE_CLIENT_ID";
        
        // Set the environment to SANDBOX for testing or PRODUCTION for production
        string environment = Immutable.Passport.Model.Environment.SANDBOX;
 
        // Your game's redirect URLs
        string redirectUri = "pandasnake://callback";
        string logoutRedirectUri = "pandasnake://logout";
        
        // Initialise Passport
        passport = await Passport.Init(clientId, environment, redirectUri, logoutRedirectUri);
    }
}