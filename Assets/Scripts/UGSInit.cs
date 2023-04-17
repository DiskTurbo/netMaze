using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class UGSInit : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        GameObject.DontDestroyOnLoad(gameObject);
        try
        {

            await UnityServices.InitializeAsync();

        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var arguments = new Dictionary<string, object> { { "gameVersion", Application.version } };
            var data = await CloudCodeService.Instance.CallEndpointAsync<VersionData>("CloudCurrentVersion", arguments);
            Debug.Log("VERSION MATCH: " + data.match + "\n" + "CURRENT VERSION: " + data.currentVersion + "\n" + "GAME VERSION: " + Application.version);
        }
        catch(System.Exception e)
        {
            Debug.Log("could not check version");
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    class VersionData
    {
        public bool match;
        public string currentVersion;
    };
}
