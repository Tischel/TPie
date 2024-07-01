using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Textures.TextureWraps;
using TPie.Config;
using TPie.Helpers;
using TPie.Models;
using TPie.Models.Elements;

namespace TPie
{
    public class Plugin : IDalamudPlugin
    {
        public static IClientState ClientState { get; private set; } = null!;
        public static ICommandManager CommandManager { get; private set; } = null!;
        public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        public static IDataManager DataManager { get; private set; } = null!;
        public static IFramework Framework { get; private set; } = null!;
        public static IGameGui GameGui { get; private set; } = null!;
        public static ISigScanner SigScanner { get; private set; } = null!;
        public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
        public static UiBuilder UiBuilder { get; private set; } = null!;
        public static IKeyState KeyState { get; private set; } = null!;
        public static IPluginLog Logger { get; private set; } = null!;
        public static ITextureProvider TextureProvider { get; private set; } = null!;

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
        private static EmoteElementWindow _emoteElementWindow = null!;
        private static CommandElementWindow _commandElementWindow = null!;
        private static GameMacroElementWindow _gameMacroElementWindow = null!;
        private static NestedRingElementWindow _nestedRingElementWindow = null!;
        private static IconBrowserWindow _iconBrowserWindow = null!;
        private static KeyBindWindow _keyBindWindow = null!;

        public static RingsManager RingsManager = null!;

        public static UldWrapper? RingBackground;

        public Plugin(
            IClientState clientState,
            ICommandManager commandManager,
            IDalamudPluginInterface pluginInterface,
            IDataManager dataManager,
            IFramework framework,
            IGameGui gameGui,
            ISigScanner sigScanner,
            IGameInteropProvider gameInteropProvider,
            IKeyState keyState,
            IPluginLog logger,
            ITextureProvider textureProvider
        )
        {
            ClientState = clientState;
            CommandManager = commandManager;
            PluginInterface = pluginInterface;
            DataManager = dataManager;
            Framework = framework;
            GameGui = gameGui;
            SigScanner = sigScanner;
            GameInteropProvider = gameInteropProvider;
            UiBuilder = (UiBuilder)PluginInterface.UiBuilder;
            KeyState = keyState;
            Logger = logger;
            TextureProvider = textureProvider;

            if (pluginInterface.AssemblyLocation.DirectoryName != null)
            {
                AssemblyLocation = pluginInterface.AssemblyLocation.DirectoryName + "\\";
            }
            else
            {
                AssemblyLocation = Assembly.GetExecutingAssembly().Location;
            }

            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.8.2.0";

            Framework.Update += Update;
            UiBuilder.Draw += Draw;
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
            WotsitHelper.Initialize();

            LoadPluginTextures();

            Settings = Settings.Load();

            FontsHelper.LoadFont();

            CreateWindows();

            RingsManager = new RingsManager();
            WotsitHelper.Instance?.Update();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void LoadPluginTextures()
        {
            try
            {
                string ringBgPath = Path.Combine(Path.GetDirectoryName(AssemblyLocation) ?? "", "Media", "ring_bg.png");
                if (File.Exists(ringBgPath))
                {
                    RingBackground = UiBuilder.LoadUld(ringBgPath);
                }
            }
            catch { }
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
            _emoteElementWindow = new EmoteElementWindow("Edit Emote");
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
            _windowSystem.AddWindow(_emoteElementWindow);
            _windowSystem.AddWindow(_commandElementWindow);
            _windowSystem.AddWindow(_gameMacroElementWindow);
            _windowSystem.AddWindow(_nestedRingElementWindow);
            _windowSystem.AddWindow(_iconBrowserWindow);
            _windowSystem.AddWindow(_keyBindWindow);
        }

        public static void ShowRingSettingsWindowInCursor(Ring ring)
        {
            Vector2 margin = new Vector2(20 * ImGuiHelpers.GlobalScale);
            Vector2 windowSize = (_ringSettingsWindow.Size ?? Vector2.Zero) * ImGuiHelpers.GlobalScale;
            Vector2 cursor = ImGui.GetMousePos();
            Vector2 windowPos = new Vector2(cursor.X - ring.Radius - margin.X - windowSize.X, cursor.Y - windowSize.Y / 2f);

            ShowRingSettingsWindow(windowPos, ring);
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

            _actionElementWindow.IsOpen = false;
            _itemElementWindow.IsOpen = false;
            _gearSetElementWindow.IsOpen = false;
            _commandElementWindow.IsOpen = false;
            _gameMacroElementWindow.IsOpen = false;
            _emoteElementWindow.IsOpen = false;
            _nestedRingElementWindow.IsOpen = false;

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

            else if (element is EmoteElement emoteElement)
            {
                window = _emoteElementWindow;
                _emoteElementWindow.EmoteElement = emoteElement;
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

        private void Update(IFramework framework)
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
            WotsitHelper.Instance?.Dispose();

            _windowSystem.RemoveAllWindows();

            CommandManager.RemoveHandler("/tpie");

            Framework.Update -= Update;
            UiBuilder.Draw -= Draw;
            UiBuilder.OpenConfigUi -= OpenConfigUi;

            FontsHelper.ClearFont();
            UiBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.Async, false, null);
        }
    }
}
