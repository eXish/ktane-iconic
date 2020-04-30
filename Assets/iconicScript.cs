using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class iconicScript : MonoBehaviour {

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBossModule Boss;

    public GameObject TheIcon;
    public Material[] IconMats; //0 = empty; 1 = blank, 2 = banana
    public TextMesh Phrase;
    public KMSelectable[] Pixels;

    private string[] IgnoredModules;

    private int NonBosses = 1;
    private int Solves;
    private string MostRecent;
    private List<string> SolveList = new List<string>{};
    private List<string> Queue = new List<string>{};
    private bool QueuedUp = false;
    private bool FoundAModule = false;
    private int NumberOfOptions = 0;
    private int SelectedOption = 0;
    private int IgnoredSolved = 0;
    private string ModulePart = "";
    private string CurrentModule = "";
    private string CharacterList = ".0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private List<string> ModuleList = new List<string>{ "Wires", "The Button", "Keypad", "Simon Says", "Who's on First", "Memory", "Morse Code", "Complicated Wires", "Wire Sequence", "Maze", "Password", "Venting Gas", "Capacitor Discharge", "Knob", "Colour Flash", "Piano Keys", "Semaphore", "Math", "Emoji Math", "Lights Out", "Switches", "Two Bits", "Word Scramble", "Anagrams", "Combination Lock", "Filibuster", "Motion Sense", "Square Button", "Simon States", "Round Keypad", "Listening", "Foreign Exchange Rates", "Answering Questions", "Orientation Cube", "Morsematics", "Connection Check", "Letter Keys", "Forget Me Not", "Rotary Phone", "Astrology", "Logic", "Crazy Talk", "Adventure Game", "Turn The Key", "Mystic Square", "Plumbing", "Cruel Piano Keys", "Safety Safe", "Tetris", "Cryptography", "Chess", "Turn The Keys", "Mouse In The Maze", "3D Maze", "Silly Slots", "Number Pad", "Laundry", "Probing", "Resistors", "Skewed Slots", "Caesar Cipher", "Perspective Pegs", "Microcontroller", "Murder", "The Gamepad", "Tic-Tac-Toe", "Who's That Monsplode", "Monsplode, Fight!", "Shape Shift", "Follow the Leader", "Friendship", "The Bulb", "Alphabet", "Blind Alley", "Sea Shells", "English Test", "Rock-Paper-Scissors-L.-Sp.", "Hexamaze", "Bitmaps", "Colored Squares", "Adjacent Letters", "Third Base", "Souvenir", "Word Search", "Broken Buttons", "Simon Screams", "Modules Against Humanity", "Complicated Buttons", "Battleship", "Text Field", "Symbolic Password", "Wire Placement", "Double-Oh", "Cheap Checkout", "Coordinates", "Light Cycle", "HTTP Response", "Rhythms", "Color Math", "Only Connect", "Neutralization", "Web Design", "Chord Qualities", "Creation", "Rubik's Cube", "FizzBuzz", "The Clock", "LED Encryption", "Edgework", "Bitwise Operations", "Fast Math", "Minesweeper", "Zoo", "Binary LEDs", "Boolean Venn Diagram", "Point of Order", "Ice Cream", "Hex To Decimal", "The Screw", "Yahtzee", "X-Ray"};
    private string[] CurrentData = { };

    //Logging
    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake () {
        ModuleId = ModuleIdCounter++;
        foreach (KMSelectable ThePixel in Pixels) {
            ThePixel.OnInteract += delegate () { PixelPress(ThePixel); return false; };
        }
    }

    // Use this for initialization
    void Start () {
        if (IgnoredModules == null) {
            IgnoredModules = Boss.GetIgnoredModules("Iconic", new string[]{
                "14",
                "Bamboozling Time Keeper",
                "Brainf---",
                "Forget Enigma",
                "Forget Everything",
                "Forget It Not",
                "Forget Me Later",
                "Forget Me Not",
                "Forget Perspective",
                "Forget The Colors",
                "Forget Them All",
                "Forget This",
                "Forget Us Not",
                "Iconic",
                "Organization",
                "Purgatory",
                "RPS Judging",
                "Simon Forgets",
                "Simon's Stages",
                "Souvenir",
                "Tallordered Keys",
                "The Time Keeper",
                "The Troll",
                "The Twin",
                "The Very Annoying Button",
                "Timing is Everything",
                "Turn The Key",
                "Ultimate Custom Night",
                "Übermodule"
            });
        }

        Module.OnActivate += delegate () {
            NonBosses = Bomb.GetSolvableModuleNames().Where(a => !IgnoredModules.Contains(a)).ToList().Count;
            if (NonBosses == 0)
            {
                Debug.LogFormat("[Iconic #{0}] Autosolving as there are no non-boss modules on the bomb, boss Modules will have their own special support at a later date.", ModuleId);
                GetComponent<KMBombModule>().HandlePass();
            }
        };

        //AddNeedies();
        //AddBosses();
	}

	// Update is called once per frame
	void Update () {
        if (ModuleSolved == false) {
            Solves = Bomb.GetSolvedModuleNames().Count();
            if (Solves > SolveList.Count()) {
                MostRecent = GetLatestSolve(Bomb.GetSolvedModuleNames(), SolveList);
                if (!(IgnoredModules.Contains(MostRecent)))
                {
                    Queue.Add(MostRecent);
                    SolveList.Add(MostRecent);
                } else {
                    Debug.LogFormat("[Iconic #{0}] The following ignored module has solved: {1}", ModuleId, MostRecent);
                    SolveList.Add(MostRecent);
                    IgnoredSolved += 1;
                }
            }
            if (QueuedUp == false && Queue.Count() > 0) {
                FoundAModule = false;
                Array.Clear(CurrentData, 0, CurrentData.Count());
                for (int i = 0; i < ModuleList.Count(); i++) {
                    if (ModuleList[i] == Queue[0] && FoundAModule == false) {
                        TheIcon.GetComponent<MeshRenderer>().material = IconMats[i + 3];
                        CurrentData = NameToData(ModuleList[i]).ToArray();
                        CurrentModule = ModuleList[i];
                        FoundAModule = true;
                    }
                }
                if (FoundAModule == false) {
                    Debug.LogFormat("[Iconic #{0}] Adding blank because I can't recognize the following module: {1}", ModuleId, Queue[0]);
                    TheIcon.GetComponent<MeshRenderer>().material = IconMats[1];
                    CurrentData = iconicData.BlankModule.ToArray();
                    CurrentModule = "(Blank)";
                    FoundAModule = true;
                }
                QueuedUp = true;

                NumberOfOptions = CurrentData.Count();

                SelectedOption = UnityEngine.Random.Range(1, NumberOfOptions);
                ModulePart = CurrentData[SelectedOption];

                if (ModulePart == null) {
                    Debug.LogFormat("[Iconic #{0}] A part of {1} cannot be found. Autosolving the module.", ModuleId, MostRecent);
                    GetComponent<KMBombModule>().HandlePass();
                }

                Phrase.text = ModulePart;

                if (ModulePart.Length > 15) {
                    Phrase.transform.localScale = new Vector3(0.00025f, 0.001f, 0.01f);
                }   else if (ModulePart.Length > 7) {
                    Phrase.transform.localScale = new Vector3(0.0005f, 0.001f, 0.01f);
                }

            }
            if (SolveList.Count() - IgnoredSolved == NonBosses && Queue.Count() == 0) {
                Phrase.text = "GG!";
                Audio.PlaySoundAtTransform("GoodGame", transform);
                Debug.LogFormat("[Iconic #{0}] All icons shown, Module solved.", ModuleId);
                TheIcon.GetComponent<MeshRenderer>().material = IconMats[2];
                GetComponent<KMBombModule>().HandlePass();
                ModuleSolved = true;
            }
        }
	}

    /*
    void AddNeedies () {

	}

    void AddBosses () {

	}
    */

    void PixelPress (KMSelectable ThePixel) {
        if (QueuedUp == true) {
            Audio.PlaySoundAtTransform("Blip", transform);
            for (int p = 0; p < 1024; p++) {
                if (ThePixel == Pixels[p]) {
                    if (CharacterList[SelectedOption] == CurrentData[0][p]) {
                        Debug.LogFormat("[Iconic #{0}] Correct part of {1} selected, \"{2}\" at {3}.", ModuleId, CurrentModule, ModulePart, ConvertToCoordinate(p));
                        Queue.RemoveAt(0);
                        TheIcon.GetComponent<MeshRenderer>().material = IconMats[0];
                        QueuedUp = false;
                        Phrase.transform.localScale = new Vector3(0.001f, 0.001f, 0.01f);
                        Phrase.text = "Iconic";
                    } else {
                        Debug.LogFormat("[Iconic #{0}] Incorrect part of {1} selected, \"{2}\" at {3}. Strike!", ModuleId, CurrentModule, ModulePart, ConvertToCoordinate(p));
                        GetComponent<KMBombModule>().HandleStrike();
                        Queue.RemoveAt(0);
                        TheIcon.GetComponent<MeshRenderer>().material = IconMats[0];
                        QueuedUp = false;
                        Phrase.transform.localScale = new Vector3(0.001f, 0.001f, 0.01f);
                        Phrase.text = "Iconic";
                    }
                }
            }
        }
    }

    private string GetLatestSolve(List<string> a, List<string> b)
    {
        string z = "";
        for(int i = 0; i < b.Count; i++)
        {
            a.Remove(b.ElementAt(i));
        }
        z = a.ElementAt(0);
        return z;
    }

    private string ConvertToCoordinate(int p) {
        int c = p % 32;
        int d = p / 32;
        if (c > 25) {
            return "A" + CharacterList[11+(c-26)] + (d+1).ToString();
        } else {
            return CharacterList[11+c] + (d+1).ToString();
        }
    }

    private string[] NameToData(string s) {
        switch (s) {
            case "Wires":	return iconicData._Wires; break;
            case "The Button":	return iconicData._TheButton; break;
            case "Keypad":	return iconicData._Keypad; break;
            case "Simon Says":	return iconicData._SimonSays; break;
            case "Who's on First":	return iconicData._WhosOnFirst; break;
            case "Memory":	return iconicData._Memory; break;
            case "Morse Code":	return iconicData._MorseCode; break;
            case "Complicated Wires":	return iconicData._ComplicatedWires; break;
            case "Wire Sequence":	return iconicData._WireSequence; break;
            case "Maze":	return iconicData._Maze; break;
            case "Password":	return iconicData._Password; break;
            case "Venting Gas":	return iconicData._VentingGas; break;
            case "Capacitor Discharge":	return iconicData._CapacitorDischarge; break;
            case "Knob":	return iconicData._Knob; break;
            case "Colour Flash":	return iconicData._ColourFlash; break;
            case "Piano Keys":	return iconicData._PianoKeys; break;
            case "Semaphore":	return iconicData._Semaphore; break;
            case "Math":	return iconicData._Math; break;
            case "Emoji Math":	return iconicData._EmojiMath; break;
            case "Lights Out":	return iconicData._LightsOut; break;
            case "Switches":	return iconicData._Switches; break;
            case "Two Bits":	return iconicData._TwoBits; break;
            case "Word Scramble":	return iconicData._WordScramble; break;
            case "Anagrams":	return iconicData._Anagrams; break;
            case "Combination Lock":	return iconicData._CombinationLock; break;
            case "Filibuster":	return iconicData._Filibuster; break;
            case "Motion Sense":	return iconicData._MotionSense; break;
            case "Square Button":	return iconicData._SquareButton; break;
            case "Simon States":	return iconicData._SimonStates; break;
            case "Round Keypad":	return iconicData._RoundKeypad; break;
            case "Listening":	return iconicData._Listening; break;
            case "Foreign Exchange Rates":	return iconicData._ForeignExchangeRates; break;
            case "Answering Questions":	return iconicData._AnsweringQuestions; break;
            case "Orientation Cube":	return iconicData._OrientationCube; break;
            case "Morsematics":	return iconicData._Morsematics; break;
            case "Connection Check":	return iconicData._ConnectionCheck; break;
            case "Letter Keys":	return iconicData._LetterKeys; break;
            case "Forget Me Not":	return iconicData._ForgetMeNot; break;
            case "Rotary Phone":	return iconicData._RotaryPhone; break;
            case "Astrology":	return iconicData._Astrology; break;
            case "Logic":	return iconicData._Logic; break;
            case "Crazy Talk":	return iconicData._CrazyTalk; break;
            case "Adventure Game":	return iconicData._AdventureGame; break;
            case "Turn The Key":	return iconicData._TurnTheKey; break;
            case "Mystic Square":	return iconicData._MysticSquare; break;
            case "Plumbing":	return iconicData._Plumbing; break;
            case "Cruel Piano Keys":	return iconicData._CruelPianoKeys; break;
            case "Safety Safe":	return iconicData._SafetySafe; break;
            case "Tetris":	return iconicData._Tetris; break;
            case "Cryptography":	return iconicData._Cryptography; break;
            case "Chess":	return iconicData._Chess; break;
            case "Turn The Keys":	return iconicData._TurnTheKeys; break;
            case "Mouse In The Maze":	return iconicData._MouseInTheMaze; break;
            case "3D Maze":	return iconicData._3DMaze; break;
            case "Silly Slots":	return iconicData._SillySlots; break;
            case "Number Pad":	return iconicData._NumberPad; break;
            case "Laundry":	return iconicData._Laundry; break;
            case "Probing":	return iconicData._Probing; break;
            case "Resistors":	return iconicData._Resistors; break;
            case "Skewed Slots":	return iconicData._SkewedSlots; break;
            case "Caesar Cipher":	return iconicData._CaesarCipher; break;
            case "Perspective Pegs":	return iconicData._PerspectivePegs; break;
            case "Microcontroller":	return iconicData._Microcontroller; break;
            case "Murder":	return iconicData._Murder; break;
            case "The Gamepad":	return iconicData._TheGamepad; break;
            case "Tic-Tac-Toe":	return iconicData._TicTacToe; break;
            case "Who's That Monsplode":	return iconicData._WhosThatMonsplode; break;
            case "Monsplode, Fight!":	return iconicData._MonsplodeFight; break;
            case "Shape Shift":	return iconicData._ShapeShift; break;
            case "Follow the Leader":	return iconicData._FollowTheLeader; break;
            case "Friendship":	return iconicData._Friendship; break;
            case "The Bulb":	return iconicData._TheBulb; break;
            case "Alphabet":	return iconicData._Alphabet; break;
            case "Blind Alley":	return iconicData._BlindAlley; break;
            case "Sea Shells":	return iconicData._SeaShells; break;
            case "English Test":	return iconicData._EnglishTest; break;
            case "Rock-Paper-Scissors-L.-Sp.":	return iconicData._RockPaperScissorsLizardSpock; break;
            case "Hexamaze":	return iconicData._Hexamaze; break;
            case "Bitmaps":	return iconicData._Bitmaps; break;
            case "Colored Squares":	return iconicData._ColoredSquares; break;
            case "Adjacent Letters":	return iconicData._AdjacentLetters; break;
            case "Third Base":	return iconicData._ThirdBase; break;
            case "Souvenir":	return iconicData._Souvenir; break;
            case "Word Search":	return iconicData._WordSearch; break;
            case "Broken Buttons":	return iconicData._BrokenButtons; break;
            case "Simon Screams":	return iconicData._SimonScreams; break;
            case "Modules Against Humanity":	return iconicData._ModulesAgainstHumanity; break;
            case "Complicated Buttons":	return iconicData._ComplicatedButtons; break;
            case "Battleship":	return iconicData._Battleship; break;
            case "Text Field":	return iconicData._TextField; break;
            case "Symbolic Password":	return iconicData._SymbolicPassword; break;
            case "Wire Placement":	return iconicData._WirePlacement; break;
            case "Double-Oh":	return iconicData._DoubleOh; break;
            case "Cheap Checkout":	return iconicData._CheapCheckout; break;
            case "Coordinates":	return iconicData._Coordinates; break;
            case "Light Cycle":	return iconicData._LightCycle; break;
            case "HTTP Response":	return iconicData._HTTPResponse; break;
            case "Rhythms":	return iconicData._Rhythms; break;
            case "Color Math":	return iconicData._ColorMath; break;
            case "Only Connect":	return iconicData._OnlyConnect; break;
            case "Neutralization":	return iconicData._Neutralization; break;
            case "Web Design":	return iconicData._WebDesign; break;
            case "Chord Qualities":	return iconicData._ChordQualities; break;
            case "Creation":	return iconicData._Creation; break;
            case "Rubik's Cube":	return iconicData._RubiksCube; break;
            case "FizzBuzz":	return iconicData._FizzBuzz; break;
            case "The Clock":	return iconicData._TheClock; break;
            case "LED Encryption":	return iconicData._LEDEncryption; break;
            case "Edgework":	return iconicData._Edgework; break;
            case "Bitwise Operations":	return iconicData._BitwiseOperations; break;
            case "Fast Math":	return iconicData._FastMath; break;
            case "Minesweeper":	return iconicData._Minesweeper; break;
            case "Zoo":	return iconicData._Zoo; break;
            case "Binary LEDs":	return iconicData._BinaryLEDs; break;
            case "Boolean Venn Diagram":	return iconicData._BooleanVennDiagram; break;
            case "Point of Order":	return iconicData._PointOfOrder; break;
            case "Ice Cream":	return iconicData._IceCream; break;
            case "Hex To Decimal":	return iconicData._HexToDecimal; break;
            case "The Screw":	return iconicData._TheScrew; break;
            case "Yahtzee":	return iconicData._Yahtzee; break;
            case "X-Ray":	return iconicData._XRay; break;
            /*
            case "QR Code":	return iconicData._QRCode; break;
            case "Button Masher":	return iconicData._ButtonMasher; break;
            case "Random Number Generator":	return iconicData._RandomNumberGenerator; break;
            case "Color Morse":	return iconicData._ColorMorse; break;
            case "Mastermind Simple":	return iconicData._MastermindSimple; break;
            case "Mastermind Cruel":	return iconicData._MastermindCruel; break;
            case "Gridlock":	return iconicData._Gridlock; break;
            case "Big Circle":	return iconicData._BigCircle; break;
            case "Morse-A-Maze":	return iconicData._MorseAMaze; break;
            case "Colored Switches":	return iconicData._ColoredSwitches; break;
            case "Perplexing Wires":	return iconicData._PerplexingWires; break;
            case "Monsplode Trading Cards":	return iconicData._MonsplodeTradingCards; break;
            case "Game of Life Simple":	return iconicData._GameofLifeSimple; break;
            case "Game of Life Cruel":	return iconicData._GameofLifeCruel; break;
            case "Nonogram":	return iconicData._Nonogram; break;
            case "S.E.T.":	return iconicData._SET; break;
            case "Refill that Beer!":	return iconicData._RefillthatBeer; break;
            case "Painting":	return iconicData._Painting; break;
            case "Color Generator":	return iconicData._ColorGenerator; break;
            case "Shape Memory":	return iconicData._ShapeMemory; break;
            case "Symbol Cycle":	return iconicData._SymbolCycle; break;
            case "Hunting":	return iconicData._Hunting; break;
            case "Extended Password":	return iconicData._ExtendedPassword; break;
            case "Curriculum":	return iconicData._Curriculum; break;
            case "Braille":	return iconicData._Braille; break;
            case "Mafia":	return iconicData._Mafia; break;
            case "Festive Piano Keys":	return iconicData._FestivePianoKeys; break;
            case "Flags":	return iconicData._Flags; break;
            case "Timezone":	return iconicData._Timezone; break;
            case "Polyhedral Maze":	return iconicData._PolyhedralMaze; break;
            case "Symbolic Coordinates":	return iconicData._SymbolicCoordinates; break;
            case "Poker":	return iconicData._Poker; break;
            case "Sonic the Hedgehog":	return iconicData._SonictheHedgehog; break;
            case "Poetry":	return iconicData._Poetry; break;
            case "Button Sequence":	return iconicData._ButtonSequence; break;
            case "Algebra":	return iconicData._Algebra; break;
            case "Visual Impairment":	return iconicData._VisualImpairment; break;
            case "The Jukebox":	return iconicData._TheJukebox; break;
            case "Identity Parade":	return iconicData._IdentityParade; break;
            case "Maintenance":	return iconicData._Maintenance; break;
            case "Blind Maze":	return iconicData._BlindMaze; break;
            case "Backgrounds":	return iconicData._Backgrounds; break;
            case "Mortal Kombat":	return iconicData._MortalKombat; break;
            case "Mashematics":	return iconicData._Mashematics; break;
            case "Faulty Backgrounds":	return iconicData._FaultyBackgrounds; break;
            case "Radiator":	return iconicData._Radiator; break;
            case "Modern Cipher":	return iconicData._ModernCipher; break;
            case "LED Grid":	return iconicData._LEDGrid; break;
            case "Sink":	return iconicData._Sink; break;
            case "The iPhone":	return iconicData._TheiPhone; break;
            case "The Swan":	return iconicData._TheSwan; break;
            case "Waste Management":	return iconicData._WasteManagement; break;
            case "Human Resources":	return iconicData._HumanResources; break;
            case "Skyrim":	return iconicData._Skyrim; break;
            case "Burglar Alarm":	return iconicData._BurglarAlarm; break;
            case "Press X":	return iconicData._PressX; break;
            case "European Travel":	return iconicData._EuropeanTravel; break;
            case "Error Codes":	return iconicData._ErrorCodes; break;
            case "Rapid Buttons":	return iconicData._RapidButtons; break;
            case "LEGOs":	return iconicData._LEGOs; break;
            case "Rubik's Clock":	return iconicData._RubiksClock; break;
            case "Font Select":	return iconicData._FontSelect; break;
            case "The Stopwatch":	return iconicData._TheStopwatch; break;
            case "Pie":	return iconicData._Pie; break;
            case "The Wire":	return iconicData._TheWire; break;
            case "The London Underground":	return iconicData._TheLondonUnderground; break;
            case "Logic Gates":	return iconicData._LogicGates; break;
            case "Forget Everything":	return iconicData._ForgetEverything; break;
            case "Grid Matching":	return iconicData._GridMatching; break;
            case "Color Decoding":	return iconicData._ColorDecoding; break;
            case "The Sun":	return iconicData._TheSun; break;
            case "Playfair Cipher":	return iconicData._PlayfairCipher; break;
            case "Tangrams":	return iconicData._Tangrams; break;
            case "The Number":	return iconicData._TheNumber; break;
            case "Cooking":	return iconicData._Cooking; break;
            case "Superlogic":	return iconicData._Superlogic; break;
            case "The Moon":	return iconicData._TheMoon; break;
            case "The Cube":	return iconicData._TheCube; break;
            case "Dr. Doctor":	return iconicData._DrDoctor; break;
            case "Tax 	returns":	return iconicData._Tax	returns; break;
            case "The Jewel Vault":	return iconicData._TheJewelVault; break;
            case "Digital Root":	return iconicData._DigitalRoot; break;
            case "Graffiti Numbers":	return iconicData._GraffitiNumbers; break;
            case "Marble Tumble":	return iconicData._MarbleTumble; break;
            case "X01":	return iconicData._X01; break;
            case "Logical Buttons":	return iconicData._LogicalButtons; break;
            case "The Code":	return iconicData._TheCode; break;
            case "Tap Code":	return iconicData._TapCode; break;
            case "Simon Sings":	return iconicData._SimonSings; break;
            case "Simon Sends":	return iconicData._SimonSends; break;
            case "Synonyms":	return iconicData._Synonyms; break;
            case "Greek Calculus":	return iconicData._GreekCalculus; break;
            case "Simon Shrieks":	return iconicData._SimonShrieks; break;
            case "Complex Keypad":	return iconicData._ComplexKeypad; break;
            case "Subways":	return iconicData._Subways; break;
            case "Lasers":	return iconicData._Lasers; break;
            case "Turtle Robot":	return iconicData._TurtleRobot; break;
            case "Guitar Chords":	return iconicData._GuitarChords; break;
            case "Calendar":	return iconicData._Calendar; break;
            case "USA Maze":	return iconicData._USAMaze; break;
            case "Binary Tree":	return iconicData._BinaryTree; break;
            case "The Time Keeper":	return iconicData._TheTimeKeeper; break;
            case "Lightspeed":	return iconicData._Lightspeed; break;
            case "Black Hole":	return iconicData._BlackHole; break;
            case "Simon's Star":	return iconicData._SimonsStar; break;
            case "Morse War":	return iconicData._MorseWar; break;
            case "The Stock Market":	return iconicData._TheStockMarket; break;
            case "Mineseeker":	return iconicData._Mineseeker; break;
            case "Maze Scrambler":	return iconicData._MazeScrambler; break;
            case "The Number Cipher":	return iconicData._TheNumberCipher; break;
            case "Alphabet Numbers":	return iconicData._AlphabetNumbers; break;
            case "British Slang":	return iconicData._BritishSlang; break;
            case "Double Color":	return iconicData._DoubleColor; break;
            case "Maritime Flags":	return iconicData._MaritimeFlags; break;
            case "Equations":	return iconicData._Equations; break;
            case "Determinants":	return iconicData._Determinants; break;
            case "Pattern Cube":	return iconicData._PatternCube; break;
            case "Know Your Way":	return iconicData._KnowYourWay; break;
            case "Splitting The Loot":	return iconicData._SplittingTheLoot; break;
            case "Simon Samples":	return iconicData._SimonSamples; break;
            case "Character Shift":	return iconicData._CharacterShift; break;
            case "Uncolored Squares":	return iconicData._UncoloredSquares; break;
            case "Dragon Energy":	return iconicData._DragonEnergy; break;
            case "Flashing Lights":	return iconicData._FlashingLights; break;
            case "3D Tunnels":	return iconicData._3DTunnels; break;
            case "Synchronization":	return iconicData._Synchronization; break;
            case "The Switch":	return iconicData._TheSwitch; break;
            case "Reverse Morse":	return iconicData._ReverseMorse; break;
            case "Manometers":	return iconicData._Manometers; break;
            case "Shikaku":	return iconicData._Shikaku; break;
            case "Wire Spaghetti":	return iconicData._WireSpaghetti; break;
            case "Tennis":	return iconicData._Tennis; break;
            case "Module Homework":	return iconicData._ModuleHomework; break;
            case "Benedict Cumberbatch":	return iconicData._BenedictCumberbatch; break;
            case "Signals":	return iconicData._Signals; break;
            case "Horrible Memory":	return iconicData._HorribleMemory; break;
            case "Boggle":	return iconicData._Boggle; break;
            case "Command Prompt":	return iconicData._CommandPrompt; break;
            case "Boolean Maze":	return iconicData._BooleanMaze; break;
            case "Sonic & Knuckles":	return iconicData._SonicKnuckles; break;
            case "Quintuples":	return iconicData._Quintuples; break;
            case "The Sphere":	return iconicData._TheSphere; break;
            case "Coffeebucks":	return iconicData._Coffeebucks; break;
            case "Colorful Madness":	return iconicData._ColorfulMadness; break;
            case "Bases":	return iconicData._Bases; break;
            case "Lion's Share":	return iconicData._LionsShare; break;
            case "Snooker":	return iconicData._Snooker; break;
            case "Blackjack":	return iconicData._Blackjack; break;
            case "Party Time":	return iconicData._PartyTime; break;
            case "Accumulation":	return iconicData._Accumulation; break;
            case "The Plunger Button":	return iconicData._ThePlungerButton; break;
            case "The Digit":	return iconicData._TheDigit; break;
            case "The Jack-O'-Lantern":	return iconicData._TheJackOLantern; break;
            case "T-Words":	return iconicData._TWords; break;
            case "Divided Squares":	return iconicData._DividedSquares; break;
            case "Connection Device":	return iconicData._ConnectionDevice; break;
            case "Instructions":	return iconicData._Instructions; break;
            case "Valves":	return iconicData._Valves; break;
            case "Encrypted Morse":	return iconicData._EncryptedMorse; break;
            case "The Crystal Maze":	return iconicData._TheCrystalMaze; break;
            case "Cruel Countdown":	return iconicData._CruelCountdown; break;
            case "Countdown":	return iconicData._Countdown; break;
            case "Catchphrase":	return iconicData._Catchphrase; break;
            case "Blockbusters":	return iconicData._Blockbusters; break;
            case "IKEA":	return iconicData._IKEA; break;
            case "Retirement":	return iconicData._Retirement; break;
            case "Periodic Table":	return iconicData._PeriodicTable; break;
            case "101 Dalmatians":	return iconicData._101Dalmatians; break;
            case "Schlag den Bomb":	return iconicData._SchlagdenBomb; break;
            case "Mahjong":	return iconicData._Mahjong; break;
            case "Kudosudoku":	return iconicData._Kudosudoku; break;
            case "The Radio":	return iconicData._TheRadio; break;
            case "Modulo":	return iconicData._Modulo; break;
            case "Number Nimbleness":	return iconicData._NumberNimbleness; break;
            case "Pay Respects":	return iconicData._PayRespects; break;
            case "Challenge & Contact":	return iconicData._ChallengeContact; break;
            case "The Triangle":	return iconicData._TheTriangle; break;
            case "Sueet Wall":	return iconicData._SueetWall; break;
            case "Hot Potato":	return iconicData._HotPotato; break;
            case "Christmas Presents":	return iconicData._ChristmasPresents; break;
            case "Hieroglyphics":	return iconicData._Hieroglyphics; break;
            case "Functions":	return iconicData._Functions; break;
            case "Scripting":	return iconicData._Scripting; break;
            case "Needy Mrs Bob":	return iconicData._NeedyMrsBob; break;
            case "Simon Spins":	return iconicData._SimonSpins; break;
            case "Ten-Button Color Code":	return iconicData._TenButtonColorCode; break;
            case "Cursed Double-Oh":	return iconicData._CursedDoubleOh; break;
            case "Crackbox":	return iconicData._Crackbox; break;
            case "Street Fighter":	return iconicData._StreetFighter; break;
            case "The Labyrinth":	return iconicData._TheLabyrinth; break;
            case "Spinning Buttons":	return iconicData._SpinningButtons; break;
            case "Color Match":	return iconicData._ColorMatch; break;
            case "The Festive Jukebox":	return iconicData._TheFestiveJukebox; break;
            case "Skinny Wires":	return iconicData._SkinnyWires; break;
            case "The Hangover":	return iconicData._TheHangover; break;
            case "Factory Maze":	return iconicData._FactoryMaze; break;
            case "Binary Puzzle":	return iconicData._BinaryPuzzle; break;
            case "Broken Guitar Chords":	return iconicData._BrokenGuitarChords; break;
            case "Regular Crazy Talk":	return iconicData._RegularCrazyTalk; break;
            case "Hogwarts":	return iconicData._Hogwarts; break;
            case "Dominoes":	return iconicData._Dominoes; break;
            case "Simon Speaks":	return iconicData._SimonSpeaks; break;
            case "Discolored Squares":	return iconicData._DiscoloredSquares; break;
            case "Krazy Talk":	return iconicData._KrazyTalk; break;
            case "Numbers":	return iconicData._Numbers; break;
            case "Flip The Coin":	return iconicData._FlipTheCoin; break;
            case "Varicolored Squares":	return iconicData._VaricoloredSquares; break;
            case "Simon's Stages":	return iconicData._SimonsStages; break;
            case "Free Parking":	return iconicData._FreeParking; break;
            case "Cookie Jars":	return iconicData._CookieJars; break;
            case "Alchemy":	return iconicData._Alchemy; break;
            case "Zoni":	return iconicData._Zoni; break;
            case "Simon Squawks":	return iconicData._SimonSquawks; break;
            case "Unrelated Anagrams":	return iconicData._UnrelatedAnagrams; break;
            case "Mad Memory":	return iconicData._MadMemory; break;
            case "Bartending":	return iconicData._Bartending; break;
            case "Question Mark":	return iconicData._QuestionMark; break;
            case "Shapes And Bombs":	return iconicData._ShapesAndBombs; break;
            case "Flavor Text EX":	return iconicData._FlavorTextEX; break;
            case "Flavor Text":	return iconicData._FlavorText; break;
            case "Decolored Squares":	return iconicData._DecoloredSquares; break;
            case "Homophones":	return iconicData._Homophones; break;
            case "DetoNATO":	return iconicData._DetoNATO; break;
            case "Air Traffic Controller":	return iconicData._AirTrafficController; break;
            case "SYNC-125 [3]":	return iconicData._SYNC1253; break;
            case "Westeros":	return iconicData._Westeros; break;
            case "Morse Identification":	return iconicData._MorseIdentification; break;
            case "Pigpen Rotations":	return iconicData._PigpenRotations; break;
            case "LED Math":	return iconicData._LEDMath; break;
            case "Alphabetical Order":	return iconicData._AlphabeticalOrder; break;
            case "Simon Sounds":	return iconicData._SimonSounds; break;
            case "The Fidget Spinner":	return iconicData._TheFidgetSpinner; break;
            case "Simon's Sequence":	return iconicData._SimonsSequence; break;
            case "Simon Scrambles":	return iconicData._SimonScrambles; break;
            case "Harmony Sequence":	return iconicData._HarmonySequence; break;
            case "Unfair Cipher":	return iconicData._UnfairCipher; break;
            case "Melody Sequencer":	return iconicData._MelodySequencer; break;
            case "Colorful Insanity":	return iconicData._ColorfulInsanity; break;
            case "Passport Control":	return iconicData._PassportControl; break;
            case "Left and Right":	return iconicData._LeftandRight; break;
            case "Gadgetron Vendor":	return iconicData._GadgetronVendor; break;
            case "Wingdings":	return iconicData._Wingdings; break;
            case "The Hexabutton":	return iconicData._TheHexabutton; break;
            case "Genetic Sequence":	return iconicData._GeneticSequence; break;
            case "Micro-Modules":	return iconicData._MicroModules; break;
            case "Module Maze":	return iconicData._ModuleMaze; break;
            case "Elder Futhark":	return iconicData._ElderFuthark; break;
            case "Tasha Squeals":	return iconicData._TashaSqueals; break;
            case "Forget This":	return iconicData._ForgetThis; break;
            case "Digital Cipher":	return iconicData._DigitalCipher; break;
            case "Subscribe to Pewdiepie":	return iconicData._SubscribetoPewdiepie; break;
            case "Grocery Store":	return iconicData._GroceryStore; break;
            case "Draw":	return iconicData._Draw; break;
            case "Burger Alarm":	return iconicData._BurgerAlarm; break;
            case "Purgatory":	return iconicData._Purgatory; break;
            case "Mega Man 2":	return iconicData._MegaMan2; break;
            case "Lombax Cubes":	return iconicData._LombaxCubes; break;
            case "The Stare":	return iconicData._TheStare; break;
            case "Graphic Memory":	return iconicData._GraphicMemory; break;
            case "Quiz Buzz":	return iconicData._QuizBuzz; break;
            case "Wavetapping":	return iconicData._Wavetapping; break;
            case "The Hypercube":	return iconicData._TheHypercube; break;
            case "Speak English":	return iconicData._SpeakEnglish; break;
            case "Stack'em":	return iconicData._Stackem; break;
            case "Seven Wires":	return iconicData._SevenWires; break;
            case "Colored Keys":	return iconicData._ColoredKeys; break;
            case "The Troll":	return iconicData._TheTroll; break;
            case "Planets":	return iconicData._Planets; break;
            case "The Necronomicon":	return iconicData._TheNecronomicon; break;
            case "Four-Card Monte":	return iconicData._FourCardMonte; break;
            case "Aa":	return iconicData._Aa; break;
            case "The Giant's Drink":	return iconicData._TheGiantsDrink; break;
            case "Digit String":	return iconicData._DigitString; break;
            case "Alpha":	return iconicData._Alpha; break;
            case "Snap!":	return iconicData._Snap; break;
            case "Hidden Colors":	return iconicData._HiddenColors; break;
            case "Colour Code":	return iconicData._ColourCode; break;
            case "Vexillology":	return iconicData._Vexillology; break;
            case "Brush Strokes":	return iconicData._BrushStrokes; break;
            case "Odd One Out":	return iconicData._OddOneOut; break;
            case "The Triangle Button":	return iconicData._TheTriangleButton; break;
            case "Mazematics":	return iconicData._Mazematics; break;
            case "Equations X":	return iconicData._EquationsX; break;
            case "Maze³":	return iconicData._Maze3; break;
            case "Gryphons":	return iconicData._Gryphons; break;
            case "Arithmelogic":	return iconicData._Arithmelogic; break;
            case "Roman Art":	return iconicData._RomanArt; break;
            case "Faulty Sink":	return iconicData._FaultySink; break;
            case "Simon Stops":	return iconicData._SimonStops; break;
            case "Morse Buttons":	return iconicData._MorseButtons; break;
            case "Terraria Quiz":	return iconicData._TerrariaQuiz; break;
            case "Baba Is Who":	return iconicData._BabaIsWho; break;
            case "Triangle Buttons":	return iconicData._TriangleButtons; break;
            case "Simon Stores":	return iconicData._SimonStores; break;
            case "Risky Wires":	return iconicData._RiskyWires; break;
            case "Modulus Manipulation":	return iconicData._ModulusManipulation; break;
            case "Daylight Directions":	return iconicData._DaylightDirections; break;
            case "Cryptic Password":	return iconicData._CrypticPassword; break;
            case "Stained Glass":	return iconicData._StainedGlass; break;
            case "The Block":	return iconicData._TheBlock; break;
            case "Bamboozling Button":	return iconicData._BamboozlingButton; break;
            case "Insane Talk":	return iconicData._InsaneTalk; break;
            case "Transmitted Morse":	return iconicData._TransmittedMorse; break;
            case "A Mistake":	return iconicData._AMistake; break;
            case "Red Arrows":	return iconicData._RedArrows; break;
            case "Green Arrows":	return iconicData._GreenArrows; break;
            case "Yellow Arrows":	return iconicData._YellowArrows; break;
            case "Encrypted Values":	return iconicData._EncryptedValues; break;
            case "Encrypted Equations":	return iconicData._EncryptedEquations; break;
            case "Forget Them All":	return iconicData._ForgetThemAll; break;
            case "Ordered Keys":	return iconicData._OrderedKeys; break;
            case "Blue Arrows":	return iconicData._BlueArrows; break;
            case "Sticky Notes":	return iconicData._StickyNotes; break;
            case "Unordered Keys":	return iconicData._UnorderedKeys; break;
            case "Orange Arrows":	return iconicData._OrangeArrows; break;
            case "Hyperactive Numbers":	return iconicData._HyperactiveNumbers; break;
            case "Reordered Keys":	return iconicData._ReorderedKeys; break;
            case "Button Grid":	return iconicData._ButtonGrid; break;
            case "Find The Date":	return iconicData._FindTheDate; break;
            case "Misordered Keys":	return iconicData._MisorderedKeys; break;
            case "The Matrix":	return iconicData._TheMatrix; break;
            case "Purple Arrows":	return iconicData._PurpleArrows; break;
            case "Bordered Keys":	return iconicData._BorderedKeys; break;
            case "The Dealmaker":	return iconicData._TheDealmaker; break;
            case "Seven Deadly Sins":	return iconicData._SevenDeadlySins; break;
            case "The Ultracube":	return iconicData._TheUltracube; break;
            case "Symbolic Colouring":	return iconicData._SymbolicColouring; break;
            case "Recorded Keys":	return iconicData._RecordedKeys; break;
            case "The Deck of Many Things":	return iconicData._TheDeckofManyThings; break;
            case "Disordered Keys":	return iconicData._DisorderedKeys; break;
            case "Character Codes":	return iconicData._CharacterCodes; break;
            case "Raiding Temples":	return iconicData._RaidingTemples; break;
            case "Bomb Diffusal":	return iconicData._BombDiffusal; break;
            case "Tallordered Keys":	return iconicData._TallorderedKeys; break;
            case "Pong":	return iconicData._Pong; break;
            case "Ten Seconds":	return iconicData._TenSeconds; break;
            case "Cruel Ten Seconds":	return iconicData._CruelTenSeconds; break;
            case "Double Expert":	return iconicData._DoubleExpert; break;
            case "Calculus":	return iconicData._Calculus; break;
            case "Boolean Keypad":	return iconicData._BooleanKeypad; break;
            case "Toon Enough":	return iconicData._ToonEnough; break;
            case "Pictionary":	return iconicData._Pictionary; break;
            case "Qwirkle":	return iconicData._Qwirkle; break;
            case "Antichamber":	return iconicData._Antichamber; break;
            case "Simon Simons":	return iconicData._SimonSimons; break;
            case "Lucky Dice":	return iconicData._LuckyDice; break;
            case "Forget Enigma":	return iconicData._ForgetEnigma; break;
            case "Constellations":	return iconicData._Constellations; break;
            case "Prime Checker":	return iconicData._PrimeChecker; break;
            case "Cruel Digital Root":	return iconicData._CruelDigitalRoot; break;
            case "Faulty Digital Root":	return iconicData._FaultyDigitalRoot; break;
            case "The Crafting Table":	return iconicData._TheCraftingTable; break;
            case "Boot Too Big":	return iconicData._BootTooBig; break;
            case "Vigenère Cipher":	return iconicData._VigenereCipher; break;
            case "Langton's Ant":	return iconicData._LangtonsAnt; break;
            case "Old Fogey":	return iconicData._OldFogey; break;
            case "Insanagrams":	return iconicData._Insanagrams; break;
            case "Treasure Hunt":	return iconicData._TreasureHunt; break;
            case "Snakes and Ladders":	return iconicData._SnakesandLadders; break;
            case "Module Movements":	return iconicData._ModuleMovements; break;
            case "Bamboozled Again":	return iconicData._BamboozledAgain; break;
            case "Safety Square":	return iconicData._SafetySquare; break;
            case "Roman Numerals":	return iconicData._RomanNumerals; break;
            case "Colo(u)r Talk":	return iconicData._ColourTalk; break;
            case "Annoying Arrows":	return iconicData._AnnoyingArrows; break;
            case "Double Arrows":	return iconicData._DoubleArrows; break;
            case "Boolean Wires":	return iconicData._BooleanWires; break;
            case "Block Stacks":	return iconicData._BlockStacks; break;
            case "Vectors":	return iconicData._Vectors; break;
            case "Partial Derivatives":	return iconicData._PartialDerivatives; break;
            case "Caesar Cycle":	return iconicData._CaesarCycle; break;
            case "Needy Piano":	return iconicData._NeedyPiano; break;
            case "Forget Us Not":	return iconicData._ForgetUsNot; break;
            case "Affine Cycle":	return iconicData._AffineCycle; break;
            case "Pigpen Cycle":	return iconicData._PigpenCycle; break;
            case "Flower Patch":	return iconicData._FlowerPatch; break;
            case "Playfair Cycle":	return iconicData._PlayfairCycle; break;
            case "Jumble Cycle":	return iconicData._JumbleCycle; break;
            case "Organization":	return iconicData._Organization; break;
            case "Forget Perspective":	return iconicData._ForgetPerspective; break;
            case "Alpha-Bits":	return iconicData._AlphaBits; break;
            case "Jack Attack":	return iconicData._JackAttack; break;
            case "Ultimate Cycle":	return iconicData._UltimateCycle; break;
            case "Needlessly Complicated Button":	return iconicData._NeedlesslyComplicatedButton; break;
            case "Hill Cycle":	return iconicData._HillCycle; break;
            case "Binary":	return iconicData._Binary; break;
            case "Chord Progressions":	return iconicData._ChordProgressions; break;
            case "Matchematics":	return iconicData._Matchematics; break;
            case "Bob Barks":	return iconicData._BobBarks; break;
            case "Simon's On First":	return iconicData._SimonsOnFirst; break;
            case "Weird Al Yankovic":	return iconicData._WeirdAlYankovic; break;
            case "Forget Me Now":	return iconicData._ForgetMeNow; break;
            case "Simon Selects":	return iconicData._SimonSelects; break;
            case "The Witness":	return iconicData._TheWitness; break;
            case "Simon Literally Says":	return iconicData._SimonLiterallySays; break;
            case "Cryptic Cycle":	return iconicData._CrypticCycle; break;
            case "Bone Apple Tea":	return iconicData._BoneAppleTea; break;
            case "Robot Programming":	return iconicData._RobotProgramming; break;
            case "Masyu":	return iconicData._Masyu; break;
            case "Hold Ups":	return iconicData._HoldUps; break;
            case "Red Cipher":	return iconicData._RedCipher; break;
            case "Flash Memory":	return iconicData._FlashMemory; break;
            case "A-maze-ing Buttons":	return iconicData._AmazeingButtons; break;
            case "Desert Bus":	return iconicData._DesertBus; break;
            case "Orange Cipher":	return iconicData._OrangeCipher; break;
            case "Common Sense":	return iconicData._CommonSense; break;
            case "The Very Annoying Button":	return iconicData._TheVeryAnnoyingButton; break;
            case "Unown Cipher":	return iconicData._UnownCipher; break;
            case "Needy Flower Mash":	return iconicData._NeedyFlowerMash; break;
            case "TetraVex":	return iconicData._TetraVex; break;
            case "Meter":	return iconicData._Meter; break;
            case "Timing is Everything":	return iconicData._TimingisEverything; break;
            case "The Modkit":	return iconicData._TheModkit; break;
            case "Red Buttons":	return iconicData._RedButtons; break;
            case "The Rule":	return iconicData._TheRule; break;
            case "Fruits":	return iconicData._Fruits; break;
            case "Bamboozling Button Grid":	return iconicData._BamboozlingButtonGrid; break;
            case "Footnotes":	return iconicData._Footnotes; break;
            case "Lousy Chess":	return iconicData._LousyChess; break;
            case "Module Listening":	return iconicData._ModuleListening; break;
            case "Garfield Kart":	return iconicData._GarfieldKart; break;
            case "Yellow Cipher":	return iconicData._YellowCipher; break;
            case "Kooky Keypad":	return iconicData._KookyKeypad; break;
            case "Green Cipher":	return iconicData._GreenCipher; break;
            case "RGB Maze":	return iconicData._RGBMaze; break;
            case "Blue Cipher":	return iconicData._BlueCipher; break;
            case "The Legendre Symbol":	return iconicData._TheLegendreSymbol; break;
            case "Keypad Lock":	return iconicData._KeypadLock; break;
            case "Forget Me Later":	return iconicData._ForgetMeLater; break;
            case "Übermodule":	return iconicData._Ubermodule; break;
            case "Heraldry":	return iconicData._Heraldry; break;
            case "Faulty RGB Maze":	return iconicData._FaultyRGBMaze; break;
            case "Indigo Cipher":	return iconicData._IndigoCipher; break;
            case "Violet Cipher":	return iconicData._VioletCipher; break;
            case "Encryption Bingo":	return iconicData._EncryptionBingo; break;
            case "Color Addition":	return iconicData._ColorAddition; break;
            case "Chinese Counting":	return iconicData._ChineseCounting; break;
            case "Tower of Hanoi":	return iconicData._TowerofHanoi; break;
            case "Keypad Combinations":	return iconicData._KeypadCombinations; break;
            case "UltraStores":	return iconicData._UltraStores; break;
            case "Kanji":	return iconicData._Kanji; break;
            case "Geometry Dash":	return iconicData._GeometryDash; break;
            case "Ternary Converter":	return iconicData._TernaryConverter; break;
            case "N&Ms":	return iconicData._NMs; break;
            case "Eight Pages":	return iconicData._EightPages; break;
            case "The Colored Maze":	return iconicData._TheColoredMaze; break;
            case "White Cipher":	return iconicData._WhiteCipher; break;
            case "Gray Cipher":	return iconicData._GrayCipher; break;
            case "The Hyperlink":	return iconicData._TheHyperlink; break;
            case "Black Cipher":	return iconicData._BlackCipher; break;
            case "Loopover":	return iconicData._Loopover; break;
            case "Divisible Numbers":	return iconicData._DivisibleNumbers; break;
            case "Corners":	return iconicData._Corners; break;
            case "The High Score":	return iconicData._TheHighScore; break;
            case "Ingredients":	return iconicData._Ingredients; break;
            case "Jenga":	return iconicData._Jenga; break;
            case "Intervals":	return iconicData._Intervals; break;
            case "Cruel Boolean Maze":	return iconicData._CruelBooleanMaze; break;
            case "Cheep Checkout":	return iconicData._CheepCheckout; break;
            case "Spelling Bee":	return iconicData._SpellingBee; break;
            case "Memorable Buttons":	return iconicData._MemorableButtons; break;
            case "Thinking Wires":	return iconicData._ThinkingWires; break;
            case "Seven Choose Four":	return iconicData._SevenChooseFour; break;
            case "Object Shows":	return iconicData._ObjectShows; break;
            case "Lunchtime":	return iconicData._Lunchtime; break;
            case "Natures":	return iconicData._Natures; break;
            case "Neutrinos":	return iconicData._Neutrinos; break;
            case "Musical Transposition":	return iconicData._MusicalTransposition; break;
            case "Scavenger Hunt":	return iconicData._ScavengerHunt; break;
            case "Polygons":	return iconicData._Polygons; break;
            case "Ultimate Cipher":	return iconicData._UltimateCipher; break;
            case "Codenames":	return iconicData._Codenames; break;
            case "Odd Mod Out":	return iconicData._OddModOut; break;
            case "Logic Statement":	return iconicData._LogicStatement; break;
            case "Blinkstop":	return iconicData._Blinkstop; break;
            case "Ultimate Custom Night":	return iconicData._UltimateCustomNight; break;
            case "Hinges":	return iconicData._Hinges; break;
            case "Time Accumulation":	return iconicData._TimeAccumulation; break;
            case "❖":	return iconicData._nonverbalSimon; break;
            case "Forget It Not":	return iconicData._ForgetItNot; break;
            case "egg":	return iconicData._egg; break;
            case "BuzzFizz":	return iconicData._BuzzFizz; break;
            case "Answering Can Be Fun":	return iconicData._AnsweringCanBeFun; break;
            case "3x3 Grid":	return iconicData._3x3Grid; break;
            case "15 Mystic Lights":	return iconicData._15MysticLights; break;
            case "14":	return iconicData._14; break;
            case "Rainbow Arrows":	return iconicData._RainbowArrows; break;
            case "Time Signatures":	return iconicData._TimeSignatures; break;
            case "Multi-Colored Switches":	return iconicData._MultiColoredSwitches; break;
            case "Digital Dials":	return iconicData._DigitalDials; break;
            case "Passcodes":	return iconicData._Passcodes; break;
            case "Hereditary Base Notation":	return iconicData._HereditaryBaseNotation; break;
            case "Lines of Code":	return iconicData._LinesofCode; break;
            case "The cRule":	return iconicData._ThecRule; break;
            case "Prime Encryption":	return iconicData._PrimeEncryption; break;
            case "Encrypted Dice":	return iconicData._EncryptedDice; break;
            case "Colorful Dials":	return iconicData._ColorfulDials; break;
            case "Naughty or Nice":	return iconicData._NaughtyorNice; break;
            case "Following Orders":	return iconicData._FollowingOrders; break;
            case "Atbash Cipher":	return iconicData._AtbashCipher; break;
            case "Addition":	return iconicData._Addition; break;
            case "Binary Grid":	return iconicData._BinaryGrid; break;
            case "Matrices":	return iconicData._Matrices; break;
            case "Cruel Keypads":	return iconicData._CruelKeypads; break;
            case "The Black Page":	return iconicData._TheBlackPage; break;
            case "64":	return iconicData._64; break;
            case "% Grey":	return iconicData._Grey; break;
            case "Simon Forgets":	return iconicData._SimonForgets; break;
            case "Greek Letter Grid":	return iconicData._GreekLetterGrid; break;
            case "Bamboozling Time Keeper":	return iconicData._BamboozlingTimeKeeper; break;
            case "Going Backwards":	return iconicData._GoingBackwards; break;
            case "Scalar Dials":	return iconicData._ScalarDials; break;
            case "The World's Largest Button":	return iconicData._TheWorldsLargestButton; break;
            case "Keywords":	return iconicData._Keywords; break;
            case "State of Aggregation":	return iconicData._StateofAggregation; break;
            case "Dreamcipher":	return iconicData._Dreamcipher; break;
            case "Brainf---":	return iconicData._Brainf; break;
            case "Rotating Squares":	return iconicData._RotatingSquares; break;
            case "Red Light Green Light":	return iconicData._RedLightGreenLight; break;
            case "Marco Polo":	return iconicData._MarcoPolo; break;
            case "Hyperneedy":	return iconicData._Hyperneedy; break;
            case "Echolocation":	return iconicData._Echolocation; break;
            case "Boozleglyph Identification":	return iconicData._BoozleglyphIdentification; break;
            case "Boxing":	return iconicData._Boxing; break;
            case "Topsy Turvy":	return iconicData._TopsyTurvy; break;
            case "Railway Cargo Loading":	return iconicData._RailwayCargoLoading; break;
            case "Conditional Buttons":	return iconicData._ConditionalButtons; break;
            case "ASCII Art":	return iconicData._ASCIIArt; break;
            case "Semamorse":	return iconicData._Semamorse; break;
            case "Hide and Seek":	return iconicData._HideandSeek; break;
            case "Symbolic Tasha":	return iconicData._SymbolicTasha; break;
            case "Alphabetical Ruling":	return iconicData._AlphabeticalRuling; break;
            case "Microphone":	return iconicData._Microphone; break;
            case "Widdershins":	return iconicData._Widdershins; break;
            case "Lockpick Maze":	return iconicData._LockpickMaze; break;
            case "Dimension Disruption":	return iconicData._DimensionDisruption; break;
            case "V":	return iconicData._V; break;
            case "Silhouettes":	return iconicData._Silhouettes; break;
            case "A Message":	return iconicData._AMessage; break;
            case "Alliances":	return iconicData._Alliances; break;
            case "Dungeon":	return iconicData._Dungeon; break;
            case "Unicode":	return iconicData._Unicode; break;
            case "Password Generator":	return iconicData._PasswordGenerator; break;
            case "Baccarat":	return iconicData._Baccarat; break;
            case "Guess Who?":	return iconicData._GuessWho; break;
            case "Reverse Alphabetize":	return iconicData._ReverseAlphabetize; break;
            case "Alphabetize":	return iconicData._Alphabetize; break;
            case "Gatekeeper":	return iconicData._Gatekeeper; break;
            case "Light Bulbs":	return iconicData._LightBulbs; break;
            case "1000 Words":	return iconicData._1000Words; break;
            case "Five Letter Words":	return iconicData._FiveLetterWords; break;
            case "Settlers of KTaNE":	return iconicData._SettlersofKTaNE; break;
            case "The Hidden Value":	return iconicData._TheHiddenValue; break;
            case "Red":	return iconicData._Red; break;
            case "Blue":	return iconicData._Blue; break;
            case "Directional Button":	return iconicData._DirectionalButton; break;
            case "...?":	return iconicData._dotDotDotQuestionMark; break;
            case "The Simpleton":	return iconicData._TheSimpleton; break;
            case "Misery Squares":	return iconicData._MiserySquares; break;
            case "Not Wiresword":	return iconicData._NotWiresword; break;
            case "Not Wire Sequence":	return iconicData._NotWireSequence; break;
            case "Not Who's on First":	return iconicData._NotWhosonFirst; break;
            case "Not Simaze":	return iconicData._NotSimaze; break;
            case "Not Password":	return iconicData._NotPassword; break;
            case "Not Morse Code":	return iconicData._NotMorseCode; break;
            case "Not Memory":	return iconicData._NotMemory; break;
            case "Not Maze":	return iconicData._NotMaze; break;
            case "Not Keypad":	return iconicData._NotKeypad; break;
            case "Not Complicated Wires":	return iconicData._NotComplicatedWires; break;
            case "Not Capacitor Discharge":	return iconicData._NotCapacitorDischarge; break;
            case "Not the Button":	return iconicData._NottheButton; break;
            case "Sequences":	return iconicData._Sequences; break;
            case "Dungeon 2nd Floor":	return iconicData._Dungeon2ndFloor; break;
            case "Wire Ordering":	return iconicData._WireOrdering; break;
            case "Vcrcs":	return iconicData._Vcrcs; break;
            case "Quaternions":	return iconicData._Quaternions; break;
            case "Abstract Sequences":	return iconicData._AbstractSequences; break;
            case "osu!":	return iconicData._osu; break;
            case "Shifting Maze":	return iconicData._ShiftingMaze; break;
            case "Banana":	return iconicData._Banana; break;
            case "Sorting":	return iconicData._Sorting; break;
            case "Role Reversal":	return iconicData._RoleReversal; break;
            case "Placeholder Talk":	return iconicData._PlaceholderTalk; break;
            case "Art Appreciation":	return iconicData._ArtAppreciation; break;
            case "Answer to...":	return iconicData._Answerto; break;
            case "Cruel Boolean Math":	return iconicData._CruelBooleanMath; break;
            case "Boolean Math":	return iconicData._BooleanMath; break;
            case "Shell Game":	return iconicData._ShellGame; break;
            case "Pattern Lock":	return iconicData._PatternLock; break;
            case "Quick Arithmetic":	return iconicData._QuickArithmetic; break;
            case "Minecraft Cipher":	return iconicData._MinecraftCipher; break;
            case "Cheat Checkout":	return iconicData._CheatCheckout; break;
            case "The Samsung":	return iconicData._TheSamsung; break;
            case "Forget The Colors":	return iconicData._ForgetTheColors; break;
            case "Etterna":	return iconicData._Etterna; break;
            case "Recolored Switches":	return iconicData._RecoloredSwitches; break;
            case "Cruel Garfield Kart":	return iconicData._CruelGarfieldKart; break;
            case "1D Maze":	return iconicData._1DMaze; break;
            case "Reverse Polish Notation":	return iconicData._ReversePolishNotation; break;
            case "Snowflakes":	return iconicData._Snowflakes; break;
            case "Funny Numbers":	return iconicData._FunnyNumbers; break;
            case "Label Priorities":	return iconicData._LabelPriorities; break;
            case "Numbered Buttons":	return iconicData._NumberedButtons; break;
            case "Exoplanets":	return iconicData._Exoplanets; break;
            case "Simon Stages":	return iconicData._SimonStages; break;
            case "Not Venting Gas":	return iconicData._NotVentingGas; break;
            case "Forget Infinity":	return iconicData._ForgetInfinity; break;
            case "Faulty Seven Segment Displays":	return iconicData._FaultySevenSegmentDisplays; break;
            case "Stock Images":	return iconicData._StockImages; break;
            case "Roger":	return iconicData._Roger; break;
            case "Malfunctions":	return iconicData._Malfunctions; break;
            case "Minecraft Parody":	return iconicData._MinecraftParody; break;
            case "Shuffled Strings":	return iconicData._ShuffledStrings; break;
            case "NumberWang":	return iconicData._NumberWang; break;
            case "Minecraft Survival":	return iconicData._MinecraftSurvival; break;
            case "RPS Judging":	return iconicData._RPSJudging; break;
            case "Fencing":	return iconicData._Fencing; break;
            case "Strike/Solve":	return iconicData._Strike/Solve; break;
            case "Uncolored Switches":	return iconicData._UncoloredSwitches; break;
            case "The Twin":	return iconicData._TheTwin; break;
            case "Name Changer":	return iconicData._NameChanger; break;
            case "Just Numbers":	return iconicData._JustNumbers; break;
            case "Lying Indicators":	return iconicData._LyingIndicators; break;
            case "Flag Identification":	return iconicData._FlagIdentification; break;
            case "Training Text":	return iconicData._TrainingText; break;
            case "Wonder Cipher":	return iconicData._WonderCipher; break;
            case "Caesar's Maths":	return iconicData._CaesarsMaths; break;
            case "Random Access Memory":	return iconicData._RandomAccessMemory; break;
            case "Triamonds":	return iconicData._Triamonds; break;
            case "Stars":	return iconicData._Stars; break;
            case "Button Order":	return iconicData._ButtonOrder; break;
            case "Jukebox.WAV":	return iconicData._JukeboxWAV; break;
            case "Elder Password":	return iconicData._ElderPassword; break;
            case "Switching Maze":	return iconicData._SwitchingMaze; break;
            case "Iconic":	return iconicData._Iconic; break;
            case "Mystery Module":	return iconicData._MysteryModule; break;
            case "Ladder Lottery":	return iconicData._LadderLottery; break;
            case "Co-op Harmony Sequence":	return iconicData._CoopHarmonySequence; break;
            case "Standard Crazy Talk":	return iconicData._StandardCrazyTalk; break;
            case "Quote Crazy Talk End Quote":	return iconicData._QuoteCrazyTalkEndQuote; break;
            case "Kilo Talk":	return iconicData._KiloTalk; break;
            case "Kay-Mazey Talk":	return iconicData._KayMazeyTalk; break;
            case "Jaden Smith Talk":	return iconicData._JadenSmithTalk; break;
            case "Deck Creating":	return iconicData._DeckCreating; break;
            case "Crazy Talk With A K":	return iconicData._CrazyTalkWithAK; break;
            case "BoozleTalk":	return iconicData._BoozleTalk; break;
            case "Arrow Talk":	return iconicData._ArrowTalk; break;
            case "Siffron":	return iconicData._Siffron; break;
            case "Red Herring":	return iconicData._RedHerring; break;
            */
            default:                            return iconicData.BlankModule; break;
        }
    }
}
