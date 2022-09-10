using ImGuiNET;
using ImGuiScene;
using System.Collections.Generic;
using System.Numerics;
using TPie.Models.Elements;

namespace TPie.Config
{
    public class EmoteElementWindow : RingElementWindow
    {
        private EmoteElement? _emoteElement = null;
        public EmoteElement? EmoteElement
        {
            get => _emoteElement;
            set
            {
                _emoteElement = value;
                _inputText = "";
                _searchResult.Clear();

                if (value != null)
                {
                    _inputText = value.Name;
                    _needsSearch = true;
                }
            }
        }

        protected override RingElement? Element
        {
            get => EmoteElement;
            set => EmoteElement = value is EmoteElement o ? o : null;
        }

        private List<EmoteData> _searchResult = new List<EmoteData>();
        private bool _needsSearch = false;

        public EmoteElementWindow(string name) : base(name)
        {
        }

        public override void Draw()
        {
            if (EmoteElement == null) return;

            ImGui.PushItemWidth(240 * _scale);
            if (ImGui.InputText("Name ##Emote", ref _inputText, 100) || _needsSearch)
            {
                SearchEmotes(_inputText);
                _needsSearch = false;
            }

            FocusIfNeeded();

            ImGui.BeginChild("##Items_List", new Vector2(284 * _scale, 170 * _scale), true);
            {
                List<EmoteData> list = _searchResult.Count == 0 ? EmotesData : _searchResult;

                foreach (EmoteData data in list)
                {
                    // name
                    if (ImGui.Selectable($"\t\t\t{data.Name}", false, ImGuiSelectableFlags.None, new Vector2(0, 24)))
                    {
                        EmoteElement.Name = data.Name;
                        EmoteElement.Command = data.Command;
                        EmoteElement.IconID = data.IconID;
                    }

                    // icon
                    TextureWrap? texture = Plugin.TexturesCache.GetTextureFromIconId(data.IconID);
                    if (texture != null)
                    {
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(10 * _scale);
                        ImGui.Image(texture.ImGuiHandle, new Vector2(24 * _scale));
                    }

                }
            }
            ImGui.EndChild();

            // draw text
            ImGui.NewLine();
            ImGui.Checkbox("Draw Text", ref EmoteElement.DrawText);

            if (EmoteElement.DrawText)
            {
                ImGui.SameLine();
                ImGui.Checkbox("Only When Selected", ref EmoteElement.DrawTextOnlyWhenSelected);
            }

            // border
            ImGui.NewLine();
            EmoteElement.Border.Draw();
        }

        private void SearchEmotes(string text)
        {
            if (_inputText.Length == 0)
            {
                _searchResult.Clear();
                return;
            }

            _searchResult = EmotesData.FindAll(o => o.Name.ToUpper().Contains(_inputText.ToUpper()));
        }

        private struct EmoteData
        {
            public string Name;
            public string Command;
            public uint IconID;

            public EmoteData(string name, string command, uint iconID)
            {
                Name = name;
                Command = command;
                IconID = iconID;
            }
        }

        private static List<EmoteData> EmotesData = new List<EmoteData>()
        {
            // general
            new EmoteData("Aback", "/aback", 64360),
            new EmoteData("Air Quotes", "/airquotes", 64063),
            new EmoteData("Angry", "/angry", 64002),
            new EmoteData("Backflip", "/bflip", 64087),
            new EmoteData("Battle Stance", "/bstance", 64085),
            new EmoteData("Beckon", "/", 64008),
            new EmoteData("Blow Kiss", "/blowkiss", 64049),
            new EmoteData("Blush", "/blush", 64004),
            new EmoteData("Bow", "/bow", 64005),
            new EmoteData("Box", "/box", 64348),
            new EmoteData("Cheer", "/cheer", 64006),
            new EmoteData("Chuckle", "/chuckle", 64020),
            new EmoteData("Clap", "/clap", 64007),
            new EmoteData("Comfort", "/comfort", 64009),
            new EmoteData("Congratulate", "/congratulate", 64029),
            new EmoteData("Consider", "/consider", 64392),
            new EmoteData("Converse", "/converse", 64345),
            new EmoteData("Crimson Lotus", "/crimsonlotus", 64353),
            new EmoteData("Cry", "/cry", 64010),
            new EmoteData("Dance", "/dance", 64011),
            new EmoteData("Deny", "/deny", 64419),
            new EmoteData("Disappointed", "/disappointed", 64053),
            new EmoteData("Dote", "/dote", 64334),
            new EmoteData("Doubt", "/doubt", 64012),
            new EmoteData("Doze", "/doze", 64013),
            new EmoteData("Drink Tea", "/tea", 64418),
            new EmoteData("Eastern Bow", "/easternbow", 64340),
            new EmoteData("Eastern Greeting", "/easterngreeting", 64088),
            new EmoteData("Eastern Stretch", "/estretch", 64092),
            new EmoteData("Eat Bread", "/breakfast", 64390),
            new EmoteData("Elucidate", "/elucidate", 64369),
            new EmoteData("Embrace", "/embrace", 64078),
            new EmoteData("Eureka", "/eureka", 64089),
            new EmoteData("Examine Self", "/examineself", 64044),
            new EmoteData("Facepalm", "/facepalm", 64326),
            new EmoteData("Fist Bump", "/fistbump", 64080),
            new EmoteData("Flame Salute", "/flamesalute", 64048),
            new EmoteData("Flex", "/flex", 64328),
            new EmoteData("Flower Shower", "/flowershower", 64395),
            new EmoteData("Fume", "/fume", 64014),
            new EmoteData("Furious", "/furious", 64003),
            new EmoteData("Goodbye", "/goodbye", 64008),
            new EmoteData("Gratuity", "/gratuity", 64378),
            new EmoteData("Greeting", "/greet", 64361),
            new EmoteData("Grovel", "/grovel", 64051),
            new EmoteData("Hand Over", "Hand Over", 64073),
            new EmoteData("Happy", "/happy", 64052),
            new EmoteData("Haurchefant", "/haurchefant", 64091),
            new EmoteData("Headache", "/headache", 64387),
            new EmoteData("High Five", "/highfive", 64397),
            new EmoteData("Hug", "/hug", 64077),
            new EmoteData("Huh", "/huh", 64017),
            new EmoteData("Huzzah", "/huzzah", 64075),
            new EmoteData("Imperial Salute", "/imperialsalute", 64066),
            new EmoteData("Insist", "/insist", 64393),
            new EmoteData("Joy", "/joy", 64018),
            new EmoteData("Kneel", "/kneel", 64019),
            new EmoteData("Lali-ho", "/laliho", 64383),
            new EmoteData("Laugh", "/laugh", 64021),
            new EmoteData("Lookout", "/lookout", 64022),
            new EmoteData("Me", "/me", 64023),
            new EmoteData("Megaflare", "/megaflare", 64352),
            new EmoteData("No", "/no", 64024),
            new EmoteData("Paint It Black", "/paintblack", 64406),
            new EmoteData("Paint It Blue", "/paintblue", 64409),
            new EmoteData("Paint It Red", "/paintred", 64407),
            new EmoteData("Paint It Yellow", "/paintyellow", 64408),
            new EmoteData("Panic", "/panic", 64026),
            new EmoteData("Pantomime", "/pantomime", 64410),
            new EmoteData("Pay Respects", "/respect", 64329),
            new EmoteData("Pet", "Pet", 64067),
            new EmoteData("Point", "/point", 64027),
            new EmoteData("Poke", "Poke", 64028),
            new EmoteData("Pose", "/pose", 64045),
            new EmoteData("Power Up", "/powerup", 64339),
            new EmoteData("Pray", "/pray", 64064),
            new EmoteData("Pretty Please", "/prettyplease", 64330),
            new EmoteData("Psych", "/psych", 64030),
            new EmoteData("Rally", "/rally", 64034),
            new EmoteData("Read A Book", "/read", 64391),
            new EmoteData("Salute", "/salute", 64031),
            new EmoteData("Scheme", "/scheme", 64374),
            new EmoteData("Serpent Salute", "/serpentsalute", 64047),
            new EmoteData("Shiver", "/shiver", 64368),
            new EmoteData("Shocked", "/shocked", 64032),
            new EmoteData("Shrug", "/shrug", 64033),
            new EmoteData("Shush", "/shush", 64412),
            new EmoteData("Slap", "Slap", 64076),
            new EmoteData("Snap", "/snap", 64389),
            new EmoteData("Soothe", "/soothe", 64035),
            new EmoteData("Spectacles", "/spectacles", 64335),
            new EmoteData("Splash", "/splash", 64366),
            new EmoteData("Stagger", "/stagger", 64036),
            new EmoteData("Stand Up", "/standup", 64055),
            new EmoteData("Storm Salute", "/stormsalute", 64046),
            new EmoteData("Stretch", "/stretch", 64037),
            new EmoteData("Sulk", "/sulk", 64038),
            new EmoteData("Surprised", "/surprised", 64360),
            new EmoteData("Sweat", "/sweat", 64367),
            new EmoteData("Think", "/think", 64039),
            new EmoteData("Throw", "Throw", 64050),
            new EmoteData("Thumbs Up", "/thumbsup", 64043),
            new EmoteData("Toast", "/toast", 64386),
            new EmoteData("Tomestone", "/tomestone", 64375),
            new EmoteData("Upset", "/upset", 63040),
            new EmoteData("Vexed", "/vexed", 64411),
            new EmoteData("Victory", "/vpose", 64086),
            new EmoteData("Water Flip", "/waterflip", 64338),
            new EmoteData("Wave", "/wave", 64016),
            new EmoteData("Welcome", "/welcome", 64041),
            new EmoteData("Yes", "/yes", 64042),

            // special
            new EmoteData("At Ease", "/atease", 64347),
            new EmoteData("Attention", "/attention", 64346),
            new EmoteData("Ball Dance", "/balldance", 64071),
            new EmoteData("Bee's Knees", "/beesknees", 64400),
            new EmoteData("Black Ranger Pose A", "/brpa", 64095),
            new EmoteData("Black Ranger Pose B", "/brpb", 64098),
            new EmoteData("Bomb Dance", "/bombdance", 64074),
            new EmoteData("Box Step", "/boxstep", 64382),
            new EmoteData("Breath Control", "/breathcontrol", 64344),
            new EmoteData("Change Pose", "/changepose", 64068),
            new EmoteData("Charmed", "/charmed", 64354),
            new EmoteData("Cheer Jump", "/cheerjump", 64357),
            new EmoteData("Cheer On", "/cheeron", 64355),
            new EmoteData("Cheer Wave", "/cheerwave", 64356),
            new EmoteData("Confirm", "/confirm", 64373),
            new EmoteData("Diamond Dust", "/iceheart", 64332),
            new EmoteData("Eastern Dance", "/edance", 64093),
            new EmoteData("Eat Apple", "/eatapple", 64403),
            new EmoteData("Eat Egg", "/eategg", 64417),
            new EmoteData("Eat Rice Ball", "/eatriceball", 64402),
            new EmoteData("Fist Pump", "/fistpump", 64379),
            new EmoteData("Flame Dance", "/flamedance", 64396),
            new EmoteData("Get Fantasy", "/getfantasy", 64370),
            new EmoteData("Gold Dance", "/golddance", 64083),
            new EmoteData("Goobbue Do", "/goobbuedo", 64377),
            new EmoteData("Guard", "/guard", 64398),
            new EmoteData("Harvest Dance", "/harvestdance", 64070),
            new EmoteData("Heel Toe", "/heeltoe", 64376),
            new EmoteData("Hum", "/hum", 64372),
            new EmoteData("Lali Hop", "/lalihop", 64401),
            new EmoteData("Lean", "/lean", 64388),
            new EmoteData("Malevolence", "/malevolence", 64399),
            new EmoteData("Manderville Dance", "/mandervilledance", 64072),
            new EmoteData("Manderville Mambo", "/mandervillemambo", 64382),
            new EmoteData("Moogle Dance", "/mogdance", 64090),
            new EmoteData("Moonlift Dance", "/moonlift", 64333),
            new EmoteData("Most Gentlemanly", "/hildy", 64073),
            new EmoteData("Play Dead", "/playdead", 64331),
            new EmoteData("Popoto Step", "/popotostep", 64371),
            new EmoteData("Push-ups", "/pushups", 64342),
            new EmoteData("Red Ranger Pose A", "/rrpa", 64094),
            new EmoteData("Red Ranger Pose B", "/rrpb", 64097),
            new EmoteData("Reprimand", "/reprimand", 64380),
            new EmoteData("Ritual Prayer", "/ritualprayer", 64349),
            new EmoteData("Senor Sabotender", "/sabotender", 64381),
            new EmoteData("Show Left", "/showleft", 64421),
            new EmoteData("Show Right", "/showright", 64420),
            new EmoteData("Side Step", "/sidestep", 64363),
            new EmoteData("Simulation F", "/simulationf", 64385),
            new EmoteData("Simulation M", "/simulationm", 64384),
            new EmoteData("Sit", "/sit", 64055),
            new EmoteData("Sit on Ground", "/groundsit", 64054),
            new EmoteData("Sit-ups", "/situps", 64343),
            new EmoteData("Songbird", "/songbird", 64336),
            new EmoteData("Squats", "/squats", 64341),
            new EmoteData("Step Dance", "Step Dance", 64069),
            new EmoteData("Sundrop Dance", "/sundance", 64084),
            new EmoteData("Sweep Up", "/broom", 64405),
            new EmoteData("Thavnairian Dance", "/thavnairiandance", 64082),
            new EmoteData("Tremble", "/tremble", 64358),
            new EmoteData("Ultima", "/ultima", 64364),
            new EmoteData("Visor", "/visor", 64065),
            new EmoteData("Wasshoi", "/wasshoi", 64394),
            new EmoteData("Water Float", "/waterfloat", 64337),
            new EmoteData("Winded", "/winded", 64359),
            new EmoteData("Wring Hands", "/wringhands", 64404),
            new EmoteData("Yellow Ranger Pose A", "/yrpa", 64096),
            new EmoteData("Yellow Ranger Pose B", "/yrpb", 64099),
            new EmoteData("Yol Dance", "/yoldance", 64365),
            new EmoteData("Zantetsuken", "/zantetsuken", 64327),

            // expressions
            new EmoteData("Alert", "/alert", 64112),
            new EmoteData("Amazed", "/amazed", 64109),
            new EmoteData("Annoyed", "/annoyed", 64111),
            new EmoteData("Beam", "/beam", 64124),
            new EmoteData("Big Grin", "/biggrin", 64125),
            new EmoteData("Concentrate", "/concentrate", 64121),
            new EmoteData("Confused", "/disturbed", 64122),
            new EmoteData("Endure", "/endure", 64131),
            new EmoteData("Furrow", "/furrow", 64127),
            new EmoteData("Grin", "/grin", 64103),
            new EmoteData("Ouch", "/ouch", 64110),
            new EmoteData("Ponder", "/ponder", 64129),
            new EmoteData("Pucker Up", "/puckerup", 64120),
            new EmoteData("Reflect", "/reflect", 64126),
            new EmoteData("Sad", "/sad", 64107),
            new EmoteData("Scared", "/scared", 64108),
            new EmoteData("Scoff", "/scoff", 64128),
            new EmoteData("Shut Eyes", "/shuteyes", 64106),
            new EmoteData("Simper", "/simper", 64123),
            new EmoteData("Smile", "/smile", 64102),
            new EmoteData("Smirk", "/smirk", 64104),
            new EmoteData("Sneer", "/sneer", 64119),
            new EmoteData("Straight Face", "/straightface", 64101),
            new EmoteData("Taunt", "/taunt", 64105),
            new EmoteData("Wink (Left)", "/leftwink", 64130),
            new EmoteData("Wink (Right)", "/wink", 64118),
            new EmoteData("Worried", "/worried", 64113)
        };
    }
}
