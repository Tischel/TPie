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
        private static MacroElementWindow _macroElementWindow = null!;
        private static IconBrowserWindow _iconBrowserWindow = null!;

        private Ring? _activeRing = null;
        private int _activeRingIndex = 0;

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

            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.2.0.1";

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
            _macroElementWindow = new MacroElementWindow("Edit Macro");
            _iconBrowserWindow = new IconBrowserWindow("Icon Picker");

            _windowSystem = new WindowSystem("TPie_Windows");
            _windowSystem.AddWindow(_settingsWindow);
            _windowSystem.AddWindow(_ringSettingsWindow);
            _windowSystem.AddWindow(_actionElementWindow);
            _windowSystem.AddWindow(_itemElementWindow);
            _windowSystem.AddWindow(_gearSetElementWindow);
            _windowSystem.AddWindow(_macroElementWindow);
            _windowSystem.AddWindow(_iconBrowserWindow);
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

            else if (element is MacroElement macroElement)
            {
                window = _macroElementWindow;
                _macroElementWindow.MacroElement = macroElement;
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

        private void Update(Framework framework)
        {
            if (Settings == null || ClientState.LocalPlayer == null) return;

            KeyboardHelper.Instance?.Update();
            ItemsHelper.Instance?.CalculateUsableItems();

            if (_activeRing?.IsClosed() == true)
            {
                _activeRing = null;
            }

            _activeRing?.Update();

            for (int i = 0; i < Rings.Count; i++)
            {
                if (_activeRing == null && Rings[i].Update())
                {
                    _activeRing = Rings[i];
                    _activeRingIndex = i;
                    break;
                }
                else if (Rings[i] != _activeRing)
                {
                    Rings[i].ForceClose();
                }
            }
        }

        private void Draw()
        {
            if (Settings == null || ClientState.LocalPlayer == null) return;

            _windowSystem?.Draw();

            _activeRing?.Draw($"ring_{_activeRingIndex}");

            for (int i = 0; i < Rings.Count; i++)
            {
                if (_activeRingIndex == i) continue;
                if (!Rings[i].Previewing) continue;

                Rings[i].Draw($"ring_{i}");
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
