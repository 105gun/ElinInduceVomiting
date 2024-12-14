using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.Assertions;

namespace InduceVomitingMod;

public enum InduceVomitingLogLevel
{
    None,
    Error,
    Warning,
    Info,
    Debug
};

[BepInPlugin("105gun.inducevomiting.mod", "Human Resource", "1.2.0.0")]
public class Plugin : BaseUnityPlugin
{
    static InduceVomitingLogLevel pluginLogLevel = InduceVomitingLogLevel.Info;

    private void Start()
    {
        ModLog("Initializing");
        var harmony = new Harmony("105gun.inducevomiting.mod");
        harmony.PatchAll();
        LoadData();
        ModLog("Initialization completed");
    }

    private void LoadData()
    {
        var dir = Path.GetDirectoryName(Info.Location);

        ClassCache.caches.Create<AI_InduceVomiting>("InduceVomitingMod.AI_InduceVomiting", "ElinInduceVomiting");

        // Icon of AI_InduceVomiting
        Texture2D tex = IO.LoadPNG(dir + "/Texture/AI_InduceVomiting.png");
        tex.name = "InduceVomitingMod.AI_InduceVomiting";
        Sprite newSprite = Sprite.Create(tex,new Rect(0,0,tex.width, tex.height), Vector2.one * 0.5f);
        newSprite.name = tex.name;
        SpriteSheet.Add(newSprite);
    }

    public static void ModLog(string message, InduceVomitingLogLevel logLevel = InduceVomitingLogLevel.Info)
    {
        if (logLevel > pluginLogLevel)
        {
            return;
        }
        switch (logLevel)
        {
            case InduceVomitingLogLevel.Error:
                message = $"[InduceVomiting][Error] {message}";
                break;
            case InduceVomitingLogLevel.Warning:
                message = $"[InduceVomiting][Warning] {message}";
                break;
            case InduceVomitingLogLevel.Info:
                message = $"[InduceVomiting][Info] {message}";
                break;
            case InduceVomitingLogLevel.Debug:
                message = $"[InduceVomiting][Debug] {message}";
                break;
            default:
                break;
        }
        System.Console.WriteLine(message);
    }
}