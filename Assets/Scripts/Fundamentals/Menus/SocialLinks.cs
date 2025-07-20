using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// Class that handles the social media icons on the main menu
/// </summary>
public class SocialLinks : MonoBehaviour
{
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;

    // Local variables
    private const string YoutubeLink = "https://www.youtube.com/channel/UCnW6bD8t1uPMev8jjitBzag";
    private const string InstagramLink = "https://www.instagram.com/heatwavestudioaz";
    private const string SteamLink = "https://store.steampowered.com/search/?developer=Heatwave%20Studios";
    private const string GitHubLink = "https://github.com/GDCASU";
    private const string SoundCloudLink = "https://soundcloud.com/heatwavestudioaz";


    public void OpenYoutubeLink()
    {
        Application.OpenURL(YoutubeLink);
    }

    public void OpenInstagramLink()
    {
        Application.OpenURL(InstagramLink);
    }

    public void OpenSteamLink()
    {
        Application.OpenURL(SteamLink);
    }

    public void OpenGitHubLink()
    {
        Application.OpenURL(GitHubLink);
    }

    public void OpenSoundCloudLink()
    {
        Application.OpenURL(SoundCloudLink);
    }
}
