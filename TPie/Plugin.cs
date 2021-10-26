using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
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
        public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        public static DataManager DataManager { get; private set; } = null!;
        public static Framework Framework { get; private set; } = null!;
        public static GameGui GameGui { get; private set; } = null!;
        public static SigScanner SigScanner { get; private set; } = null!;
        public static UiBuilder UiBuilder { get; private set; } = null!;

        public static string AssemblyLocation { get; private set; } = "";
        public string Name => "TPie";

        public static string Version { get; private set; } = "";

        public static Settings Settings { get; private set; } = null!;
        private List<Ring> Rings => Settings.Rings;

        private static WindowSystem _windowSystem = null!;
        private static SettingsWindow _settingsWindow = null!;
        private static RingSettingsWindow _ringSettingsWindow = null!;
        private static ActionElementWindow _actionElementWindow = null!;
        private static ItemElementWindow _itemElementWindow = null!;

        public Plugin(
            ClientState clientState,
            CommandManager commandManager,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            SigScanner sigScanner
        )
        {
            ClientState = clientState;
            CommandManager = commandManager;
            PluginInterface = pluginInterface;
            DataManager = dataManager;
            Framework = framework;
            GameGui = gameGui;
            SigScanner = sigScanner;
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
            JobsHelper.Initialize();
            ItemsHelper.Initialize();
            TexturesCache.Initialize();
            TexturesCache.Instance?.LoadPluginTextures();

            Settings = Settings.Load();

            //Settings.Rings.Clear();
            //KeyBind keybind = new KeyBind(72);
            //Ring ring = new Ring("Test", Vector4.One, keybind, 150, new Vector2(40, 40));

            //ActionElement? teleport = new ActionElement(3574);
            //ring.Items.Add(teleport);

            //ItemElement? item = new ItemElement(2001886, false, "Aether Compass", 25948);
            //ring.Items.Add(item);

            //ItemElement? food = new ItemElement(23187, false, "Matcha", 24414);
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

            CreateWindows();
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
            _settingsWindow.IsOpen = !_settingsWindow.IsOpen;
        }

        private void CreateWindows()
        {
            _settingsWindow = new SettingsWindow("TPie Settings");
            _ringSettingsWindow = new RingSettingsWindow("Ring Settings");
            _actionElementWindow = new ActionElementWindow("Add Action");
            _itemElementWindow = new ItemElementWindow("Add Item");

            _windowSystem = new WindowSystem("TPie_Windows");
            _windowSystem.AddWindow(_settingsWindow);
            _windowSystem.AddWindow(_ringSettingsWindow);
            _windowSystem.AddWindow(_actionElementWindow);
            _windowSystem.AddWindow(_itemElementWindow);
        }

        public static void ShowRingSettingsWindow(Vector2 position, Ring ring)
        {
            _ringSettingsWindow.Position = position;
            _ringSettingsWindow.Ring = ring;
            _ringSettingsWindow.IsOpen = true;
        }

        public static void ShowActionElementWindow(Vector2 position, ActionElement? actionElement, Action<RingElement?>? callback)
        {
            _actionElementWindow.Position = position;
            _actionElementWindow.WindowName = actionElement != null ? "Edit Action" : "Add Action";
            _actionElementWindow.ActionElement = actionElement;
            _actionElementWindow.Callback = callback;

            _actionElementWindow.IsOpen = true;
        }

        public static void ShowItemElementWindow(Vector2 position, ItemElement? itemElement, Action<RingElement?>? callback)
        {
            _itemElementWindow.Position = position;
            _itemElementWindow.WindowName = itemElement != null ? "Edit Item" : "Add Item";
            _itemElementWindow.ItemElement = itemElement;
            _itemElementWindow.Callback = callback;

            _itemElementWindow.IsOpen = true;
        }

        private void Update(Framework framework)
        {
            if (Settings == null || ClientState.LocalPlayer == null) return;

            ItemsHelper.Instance?.CalculateUsableItems();

            foreach (Ring ring in Rings)
            {
                ring.Update();
            }
        }

        private void Draw()
        {
            if (Settings == null || ClientState.LocalPlayer == null) return;

            _windowSystem.Draw();

            foreach (Ring ring in Rings)
            {
                ring.Draw();
            }
        }

        private void OpenConfigUi()
        {
            _settingsWindow.IsOpen = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            ChatHelper.Instance.Dispose();
            KeyboardHelper.Instance.Dispose();
            JobsHelper.Instance.Dispose();
            ItemsHelper.Instance.Dispose();
            TexturesCache.Instance.Dispose();

            _windowSystem.RemoveAllWindows();

            CommandManager.RemoveHandler("/tpie");

            Framework.Update -= Update;
            UiBuilder.Draw -= Draw;
            UiBuilder.BuildFonts -= BuildFont;
            UiBuilder.OpenConfigUi -= OpenConfigUi;
            UiBuilder.RebuildFonts();
        }
    }
}
