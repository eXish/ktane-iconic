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
    private List<string> ModuleList = new List<string>{ "Wires", "The Button", "Keypad", "Simon Says", "Who's on First", "Memory", "Morse Code", "Complicated Wires", "Wire Sequence", "Maze", "Password"};
    //private static string[][] DataList = new string[][] { iconicData._Wires, iconicData._BigButton, iconicData._Keypad, iconicData._Simon, iconicData._WhosOnFirst, iconicData._Memory, iconicData._Morse, iconicData._Venn, iconicData._WireSequence, iconicData._Maze, iconicData._Password};
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
                Debug.LogFormat("[Iconic #{0}] Autosolving as there are no non-boss Modules on the bomb, boss Modules will have their own special support at a later date.", ModuleId);
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
                    Debug.LogFormat("[Iconic #{0}] Adding blank because I can't recognize the following module: {1}", ModuleId, MostRecent);
                    TheIcon.GetComponent<MeshRenderer>().material = IconMats[1];
                    CurrentData = iconicData.BlankModule.ToArray();
                    CurrentModule = "(Blank)";
                    FoundAModule = true;
                }
                QueuedUp = true;

                NumberOfOptions = CurrentData.Count();

                SelectedOption = UnityEngine.Random.Range(1, NumberOfOptions);
                ModulePart = CurrentData[SelectedOption];
                Phrase.text = ModulePart;

                if (ModulePart.Length > 15) {
                    Phrase.transform.localScale = new Vector3(0.00025f, 0.001f, 0.01f);
                }   else if (ModulePart.Length > 7) {
                    Phrase.transform.localScale = new Vector3(0.0005f, 0.001f, 0.01f);
                }

            }
            if (SolveList.Count() - IgnoredSolved == NonBosses && Queue.Count() == 0) {
                Phrase.text = "GG!";
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
            case "Wires":                       return iconicData._Wires; break;
            case "The Button":                  return iconicData._TheButton; break;
            case "Keypad":                      return iconicData._Keypad; break;
            case "Simon Says":                  return iconicData._SimonSays; break;
            case "Who's on First":              return iconicData._WhosOnFirst; break;
            case "Memory":                      return iconicData._Memory; break;
            case "Morse Code":                  return iconicData._MorseCode; break;
            case "Complicated Wires":           return iconicData._ComplicatedWires; break;
            case "Wire Sequence":               return iconicData._WireSequence; break;
            case "Maze":                        return iconicData._Maze; break;
            case "Password":                    return iconicData._Password; break;
            default:                            return iconicData.BlankModule; break;
        }
    }
}
