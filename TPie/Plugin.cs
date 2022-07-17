using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Keys;
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
        public static KeyState KeyState { get; private set; } = null!;

        public static TexturesCache TexturesCache { get; private set; } = null!;

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
        private static GearSetElementWindow _gearSetElementWindow = null!;
        private static CommandElementWindow _commandElementWindow = null!;
        private static GameMacroElementWindow _gameMacroElementWindow = null!;
        private static NestedRingElementWindow _nestedRingElementWindow = null!;
        private static IconBrowserWindow _iconBrowserWindow = null!;
        private static KeyBindWindow _keyBindWindow = null!;

        public static RingsManager RingsManager = null!;

        public Plugin(
            ClientState clientState,
            CommandManager commandManager,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            SigScanner sigScanner,
            KeyState keyState
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
            KeyState = keyState;

            if (pluginInterface.AssemblyLocation.DirectoryName != null)
            {
                AssemblyLocation = pluginInterface.AssemblyLocation.DirectoryName + "\\";
            }
            else
            {
                AssemblyLocation = Assembly.GetExecutingAssembly().Location;
            }

            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.3.2.0";

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

            TexturesCache = new TexturesCache();
            TexturesCache.LoadPluginTextures();

            Settings = Settings.Load();

            if (!FontsHelper.DefaultFontBuilt)
            {
                UiBuilder.RebuildFonts();
            }

            CreateWindows();

            RingsManager = new RingsManager();
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
            _actionElementWindow = new ActionElementWindow("Edit Action");
            _itemElementWindow = new ItemElementWindow("Edit Item");
            _gearSetElementWindow = new GearSetElementWindow("Edit Gear Set");
            _commandElementWindow = new CommandElementWindow("Edit Command");
            _gameMacroElementWindow = new GameMacroElementWindow("Edit Game Macro");
            _nestedRingElementWindow = new NestedRingElementWindow("Edit Nested Ring");
            _iconBrowserWindow = new IconBrowserWindow("Icon Picker");
            _keyBindWindow = new KeyBindWindow("Edit Keybind");

            _windowSystem = new WindowSystem("TPie_Windows");
            _windowSystem.AddWindow(_settingsWindow);
            _windowSystem.AddWindow(_ringSettingsWindow);
            _windowSystem.AddWindow(_actionElementWindow);
            _windowSystem.AddWindow(_itemElementWindow);
            _windowSystem.AddWindow(_gearSetElementWindow);
            _windowSystem.AddWindow(_commandElementWindow);
            _windowSystem.AddWindow(_gameMacroElementWindow);
            _windowSystem.AddWindow(_nestedRingElementWindow);
            _windowSystem.AddWindow(_iconBrowserWindow);
            _windowSystem.AddWindow(_keyBindWindow);
        }

        public static void ShowRingSettingsWindow(Vector2 position, Ring ring)
        {
            _ringSettingsWindow.Position = position;
            _ringSettingsWindow.Ring = ring;
            _ringSettingsWindow.IsOpen = true;
        }

        public static void ShowElementWindow(Vector2 position, Ring ring, RingElement element)
        {
            RingElementWindow? window = null;

            // action
            if (element is ActionElement actionElement)
            {
                window = _actionElementWindow;
                _actionElementWindow.ActionElement = actionElement;
            }

            else if (element is ItemElement itemElement)
            {
                window = _itemElementWindow;
                _itemElementWindow.ItemElement = itemElement;
            }

            else if (element is GearSetElement gearSetElement)
            {
                window = _gearSetElementWindow;
                _gearSetElementWindow.GearSetElement = gearSetElement;
            }

            else if (element is CommandElement commandElement)
            {
                window = _commandElementWindow;
                _commandElementWindow.CommandElement = commandElement;
            }

            else if (element is GameMacroElement gameMacroElement)
            {
                window = _gameMacroElementWindow;
                _gameMacroElementWindow.GameMacroElement = gameMacroElement;
            }

            else if (element is NestedRingElement nestedRingElement)
            {
                window = _nestedRingElementWindow;
                _nestedRingElementWindow.NestedRingElement = nestedRingElement;
            }

            if (window != null)
            {
                window.Ring = ring;
                window.Position = position;
                window.IsOpen = true;
            }
        }

        public static void ShowIconBrowserWindow(uint selectedIconId, Action<uint> onSelect)
        {
            _iconBrowserWindow._selectedId = selectedIconId;
            _iconBrowserWindow.OnSelect = onSelect;
            _iconBrowserWindow.IsOpen = true;
        }

        public static void ShowKeyBindWindow(Vector2 position, Ring ring)
        {
            _keyBindWindow.Ring = ring;
            _keyBindWindow.Position = position;
            _keyBindWindow.IsOpen = true;
        }

        private void Update(Framework framework)
        {
            if (Settings == null || ClientState.LocalPlayer == null) return;

            KeyboardHelper.Instance?.Update();
            ItemsHelper.Instance?.CalculateUsableItems();

            RingsManager?.Update();
        }

        private void Draw()
        {
            if (Settings == null || ClientState.LocalPlayer == null) return;

            _windowSystem?.Draw();

            RingsManager?.Draw();
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

            Settings.Save(Settings);

            ChatHelper.Instance?.Dispose();
            KeyboardHelper.Instance?.Dispose();
            JobsHelper.Instance?.Dispose();
            ItemsHelper.Instance?.Dispose();
            TexturesCache.Dispose();

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
