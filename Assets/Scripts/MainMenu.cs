using Immutable.Passport;
using HyperCasual.Core;

namespace HyperCasual.Runner
{
    public class MainMenu : View
    {
        Passport passport;
    
        async void OnEnable()
        {   
            // Initialise Passport
            string clientId = "WAluhyj5KOamBHWh4FvRbz8S4N8ZDLTZ";
            string environment = Immutable.Passport.Model.Environment.SANDBOX;
            passport = await Passport.Init(clientId, environment);
        }
    }
}