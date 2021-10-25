using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using TPie.Config;
using TPie.Helpers;
using TPie.Models;
using TPie.Models.Elements;
using SigScanner = Dalamud.Game.SigScanner;

namespace TPie
{
    public class Plugin : IDalamudPlugin
    {
        public static ClientState ClientState { get; private set; } = null!;
        public static CommandManager CommandManager { get; private set; } = null!;
        public static Condition Condition { get; private set; } = null!;
        public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        public static DataManager DataManager { get; private set; } = null!;
        public static Framework Framework { get; private set; } = null!;
        public static GameGui GameGui { get; private set; } = null!;
        public static JobGauges JobGauges { get; private set; } = null!;
        public static ObjectTable ObjectTable { get; private set; } = null!;
        public static SigScanner SigScanner { get; private set; } = null!;
        public static TargetManager TargetManager { get; private set; } = null!;
        public static UiBuilder UiBuilder { get; private set; } = null!;
        public static PartyList PartyList { get; private set; } = null!;

        public static string AssemblyLocation { get; private set; } = "";
        public string Name => "TPie";

        public static string Version { get; private set; } = "";

        public static Settings Settings { get; private set; } = null!;
        private List<Ring> Rings => Settings.Rings;


        public Plugin(
            ClientState clientState,
            CommandManager commandManager,
            Condition condition,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable,
            PartyList partyList,
            SigScanner sigScanner,
            TargetManager targetManager
        )
        {
            ClientState = clientState;
            CommandManager = commandManager;
            Condition = condition;
            PluginInterface = pluginInterface;
            DataManager = dataManager;
            Framework = framework;
            GameGui = gameGui;
            JobGauges = jobGauges;
            ObjectTable = objectTable;
            PartyList = partyList;
            SigScanner = sigScanner;
            TargetManager = targetManager;
            UiBuilder = PluginInterface.UiBuilder;

            if (pluginInterface.AssemblyLocation.DirectoryName != null)
            {
                AssemblyLocation = pluginInterface.AssemblyLocation.DirectoryName + "\\";
            }
            else
            {
                AssemblyLocation = Assembly.GetExecutingAssembly().Location;
            }

            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.1.0";

            Framework.Update += Update;
            UiBuilder.Draw += Draw;
            UiBuilder.BuildFonts += BuildFont;
            UiBuilder.OpenConfigUi += OpenConfigUi;

            CommandManager.AddHandler(
                "/tpie",
                new CommandInfo(PluginCommand)
                {
                    HelpMessage = "Opens the TPie configuration window.",

                    ShowInHelp = true
                }
            );

            ChatHelper.Initialize();
            KeyboardHelper.Initialize();
            ItemsHelper.Initialize();
            TexturesCache.Initialize();
            TexturesCache.Instance?.LoadPluginTextures();

            Settings = Settings.Load();

            //Settings.Rings.Clear();
            //KeyBind keybind = new KeyBind(72);
            //Ring ring = new Ring("Test", Vector4.One, keybind, 150, new Vector2(40, 40));

            //ActionElement? teleport = new ActionElement(3574);
            //ring.Items.Add(teleport);

            //ItemElement? item = new ItemElement(2001886, false, 25948);
            //ring.Items.Add(item);

            //ItemElement? food = new ItemElement(23187, false, 24414);
            //ring.Items.Add(food);

            //GearSetElement? blm = new GearSetElement(1, JobIDs.BLM);
            //ring.Items.Add(blm);

            //GearSetElement? rdm = new GearSetElement(3, JobIDs.RDM);
            //ring.Items.Add(rdm);

            //Rings.Add(ring);
            //Settings.Save(Settings);

            if (!FontsHelper.DefaultFontBuilt)
            {
                UiBuilder.RebuildFonts();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void BuildFont()
        {
            FontsHelper.LoadFont();
        }

        private void PluginCommand(string command, string arguments)
        {

        }

        private void Update(Framework framework)
        {
            if (Settings == null) return;

            bool needsItems = false;

            foreach (Ring ring in Rings)
            {
                ring.Update();
                needsItems |= (ring.IsActive && ring.HasInventoryItems);
            }

            if (needsItems)
            {
                ItemsHelper.Instance?.CalculateUsableItems();
            }
        }

        private void Draw()
        {
            if (Settings == null) return;

            foreach (Ring ring in Rings)
            {
                ring.Draw();
            }
        }

        private void OpenConfigUi()
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            ChatHelper.Instance.Dispose();
            KeyboardHelper.Instance.Dispose();
            ItemsHelper.Instance.Dispose();
            TexturesCache.Instance.Dispose();

            CommandManager.RemoveHandler("/tpie");

            Framework.Update -= Update;
            UiBuilder.Draw -= Draw;
            UiBuilder.BuildFonts -= BuildFont;
            UiBuilder.OpenConfigUi -= OpenConfigUi;
            UiBuilder.RebuildFonts();
        }
    }
}
