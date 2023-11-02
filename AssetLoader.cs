using BepInEx;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using UnityEngine;


namespace FamilyTreeMod
{
    public class AssetLoader
    {
        public static Dictionary<string, Sprite[]> cached_assets_list = new Dictionary<string, Sprite[]>();

        public static void init()
        {
            loadAssetFolder($"{Paths.PluginPath}/FamilyTreeMod/FamilyTreeModAssets");
        }

        private static void loadAssetFolder(string pPath)
        {
            string[] files = Directory.GetFiles(pPath);
            foreach(string text in files)
            {
                if (text.Contains(".json"))
                {
                    continue;
                }
                loadTexture(text);
            }
            string[] directories = Directory.GetDirectories(pPath);
            foreach(string text in directories)
            {
                loadAssetFolder(text);
            }

            foreach(KeyValuePair<string, Sprite[]> kv in cached_assets_list)
            {
                if (SpriteTextureLoader.cached_sprite_list.ContainsKey(kv.Key))
                {
                    SpriteTextureLoader.cached_sprite_list[kv.Key] = kv.Value;
                    continue;
                }
                SpriteTextureLoader.cached_sprite_list.Add(kv.Key, kv.Value);
            }
        }

        private static void loadTexture(string pPath)
        {
            string[] array = pPath.Split(new char[] { Path.DirectorySeparatorChar });
            string text = array[array.Length - 1];
            byte[] array2 = File.ReadAllBytes(pPath);
            string newPath = pPath.Remove(0, pPath.IndexOf("/FamilyTreeModAssets") + 21).Replace('\\', '/');
            // Might Need To Change This Back In The Future But For Now It Does The Job
            addSpriteList(newPath, text.Remove(text.IndexOf(".png")), array2);
        }

        public static void addSpriteList(string pPathID, string pSpriteName, byte[] pBytes)
        {
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.filterMode = 0;
            if (ImageConversion.LoadImage(texture2D, pBytes))
            {
                Rect rect = new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height);
                Vector2 vector = new Vector2(0.5f, 0.5f);
                Sprite sprite = Sprite.Create(texture2D, rect, vector, 1f);
                sprite.name = pSpriteName;
                if (!cached_assets_list.ContainsKey(pPathID))
                {
                    cached_assets_list.Add(pPathID, new Sprite[]{sprite});
                }
                else
                {
                    cached_assets_list[pPathID] = cached_assets_list[pPathID].Concat(new Sprite[]{sprite}).ToArray();
                }
            }
        }
    }
}
