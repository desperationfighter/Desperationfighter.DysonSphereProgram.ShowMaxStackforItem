using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace ShowMaxStackforItem
{
    /*
    Original Author: Desperationfigher 15.01.2024
    */

    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public new static ManualLogSource Logger { get; private set; }
        public const string GUID = "Desperationfighter.DysonSphereProgram.ShowMaxStackforItem";
        public const string Name = "Show Max Stack for Item";
        public const string Version = "1.0.0.0"; //Remmber to Update Assembly Version too !

        public static ConfigEntry<int> Modus;
        public static ConfigEntry<int> Modus_1_Setting;
        public static ConfigEntry<int> Modus_2_Setting;

        private void Awake()
        {
            Modus = Config.Bind("General", "Modus", 2, "(Game restart recommend to apply changes) Change Modus: 1. Show Max Stack Size in Item Name, 2. Show Max Stack Size as Item Description, 3. Show Both");
            Modus_1_Setting = Config.Bind("Modus 1 Settings", "Modus1Setting", 2, "Change Config: 1. Show as (Stack Size: 100), 2. Show as (Stack: 100), 3. Show as (100)");
            Modus_2_Setting = Config.Bind("Modus 2 Settings", "Modus1Setting", 1, "Change Config: 1. Show Stack Size on Start of list, 2. Show Stack Size on End of list");

            //Validate Values
            if (Modus.Value <= 0 | Modus.Value >= 4) 
            {
                Modus.Value = 2;
            }
            if (Modus_1_Setting.Value <= 0 | Modus_1_Setting.Value >= 4)
            {
                Modus_1_Setting.Value = 2;
            }
            if (Modus_2_Setting.Value <= 0 | Modus_2_Setting.Value >= 3)
            {
                Modus_2_Setting.Value = 1;
            }

            Logger = base.Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);
            Logger.LogInfo($"Plugin {GUID} is loaded!");
        }
    }
}
