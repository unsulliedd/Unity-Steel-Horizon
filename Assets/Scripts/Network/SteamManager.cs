using Steamworks;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }

    public uint appID;
    public bool isSteamRunning;
    private bool isSigningIn = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        try
        {
            SteamClient.Init(appID, true);
            isSteamRunning = true;
            Debug.Log("Steamworks.SteamClient.Init success");
        }
        catch (Exception e)
        {
            isSteamRunning = false;
            Debug.LogError("Steamworks.SteamClient.Init failed: " + e.Message);
        }
    }

    private async void Start()
    {
        if (isSteamRunning)
        {
            await UnityServices.InitializeAsync();
            await SignInWithSteam();
        }
    }

    private void OnApplicationQuit()
    {
        try { SteamClient.Shutdown(); }
        catch (Exception e) { Debug.LogError("Steamworks.SteamClient.Shutdown failed: " + e.Message); }
    }

    void Update()
    {
        if (isSteamRunning)
            SteamClient.RunCallbacks();
    }

    public string GetSteamUserName()
    {
        return SteamClient.Name;
    }

    public SteamId GetSteamUserID()
    {
        return SteamClient.SteamId;
    }

    public async Task<Sprite> GetSteamUserAvatar()
    {
        SteamId steamId = GetSteamUserID();
        var avatar = await SteamFriends.GetLargeAvatarAsync(steamId);

        if (avatar.HasValue)
        {
            var texture = new Texture2D((int)avatar.Value.Width, (int)avatar.Value.Height, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(avatar.Value.Data);
            texture.Apply();

            var pixels = texture.GetPixels();
            var flippedPixels = new Color[pixels.Length];
            int width = texture.width;
            int height = texture.height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    flippedPixels[(height - 1 - y) * width + x] = pixels[y * width + x];
                }
            }

            texture.SetPixels(flippedPixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogWarning("Failed to get Steam user avatar.");
            return null;
        }
    }

    public string GetSteamLevel()
    {
        return SteamUser.SteamLevel.ToString();
    }

    public async Task SignInWithSteam()
    {
        if (isSigningIn)
        {
            Debug.LogWarning("Already signing in.");
            return;
        }

        isSigningIn = true;

        if (!SteamClient.IsValid)
        {
            Debug.LogError("Steam client is not valid.");
            await SignInAnonymously();
            isSigningIn = false;
            return;
        }

        var ticket = await SteamUser.GetAuthSessionTicketAsync();
        Debug.Log(ticket);
        if (ticket == null || ticket.Data == null || ticket.Data.Length == 0)
        {
            Debug.LogError("Failed to get Steam authentication ticket.");
            await SignInAnonymously();
            isSigningIn = false;
            return;
        }
        byte[] authTicket = ticket.Data;
        string base64Ticket = Convert.ToBase64String(authTicket);
        string steamId = SteamClient.SteamId.ToString();

        try
        {
            await AuthenticationService.Instance.SignInWithSteamAsync(base64Ticket, steamId);
            Debug.Log("Signed in with Steam.");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Failed to sign in with Steam: {ex.Message}");
            await SignInAnonymously();
        }
        finally
        {
            isSigningIn = false;
        }
    }

    private async Task SignInAnonymously()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in anonymously.");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Failed to sign in anonymously: {ex.Message}");
        }
    }
}
