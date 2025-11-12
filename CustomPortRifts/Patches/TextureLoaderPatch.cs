using HarmonyLib;
using Shared;
using Shared.Utilities;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomPortRifts.Patches;


[HarmonyPatch(typeof(TextureLoader))]
public static class TextureLoaderPatch {
    [HarmonyPatch(nameof(TextureLoader.LoadTextureAsync))]
    [HarmonyPrefix]
    public static bool LoadTextureAsync(string url, TextureLoader __instance, ref Task<TextureLoader.LoadedImage> __result) {
        if(url.StartsWith("address://") || (Application.platform == RuntimePlatform.Switch && url.StartsWith("file://"))) {
            return true;
        }

        __result = Wrapper();
        return false;

        async Task<TextureLoader.LoadedImage> Wrapper() {
            var tcs = new TaskCompletionSource<TextureLoader.LoadedImage>();
            __instance._cache.Add(url, tcs.Task);

            using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url, nonReadable: true);
            UnityWebRequestAsyncOperation response = webRequest.SendWebRequest();
            while(!response.isDone) {
                await GlobalTimer.NextTick();
            }

            if(webRequest.result != UnityWebRequest.Result.Success) {
                var empty = TextureLoader.Empty;
                tcs.SetResult(empty);
                return empty;
            }

            var texture = DownloadHandlerTexture.GetContent(webRequest);
            var sprite = Sprite.Create(texture, new Rect(new Vector2(0f, 0f), new Vector2(texture.width, texture.height)), new Vector2(0.5f, 0.5f));
            var result = new TextureLoader.LoadedImage {
                Texture = texture,
                Sprite = sprite
            };

            tcs.SetResult(result);
            return result;
        }
    }
}
