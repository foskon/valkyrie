﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.UI.Screens;

// Class for quest selection window
public class QuestDownload : MonoBehaviour
{
    public Dictionary<string, QuestData.Quest> questList;
    public WWW download;
    public string serverLocation = "https://raw.githubusercontent.com/NPBruce/valkyrie-store/master/";
    public Game game;
    IniData remoteManifest;
    IniData localManifest;

    void Start()
    {
        game = Game.Get();
        string remoteManifest = serverLocation + game.gameType.TypeName() + "/manifest.ini";
        StartCoroutine(Download(remoteManifest, delegate { ReadManifest(); }));
    }

    public void ReadManifest()
    {
        remoteManifest = IniRead.ReadFromString(download.text);

        DrawList();
    }

    public void DrawList()
    {
        localManifest = IniRead.ReadFromString("");
        if (File.Exists(saveLocation() + "/manifest.ini"))
        {
            localManifest = IniRead.ReadFromIni(saveLocation() + "/manifest.ini");
        }

        // Heading
        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Download " + game.gameType.QuestName());
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        db.SetFont(game.gameType.GetHeaderFont());

        db = new DialogBox(new Vector2(1, 5f), new Vector2(UIScaler.GetWidthUnits()-2f, 21f), "");
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.parent = db.background.transform;
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (UIScaler.GetWidthUnits()-3f) * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);

        scrollRect.content = scrollInnerRect;
        scrollRect.horizontal = false;

        TextButton tb;
        // Start here
        int offset = 5;
        // Loop through all available quests
        foreach (KeyValuePair<string, Dictionary<string, string>> kv in remoteManifest.data)
        {
            string file = kv.Key + ".valkyrie";
            // Size is 1.2 to be clear of characters with tails
            if (File.Exists(saveLocation() + "/" + file))
            {
                int localVersion = 0;
                int remoteVersion = 0;
                int.TryParse(localManifest.Get(kv.Key, "version"), out localVersion);
                int.TryParse(remoteManifest.Get(kv.Key, "version"), out remoteVersion);
                if (localVersion < remoteVersion)
                {
                    tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 5, 1.2f), "  [Update] " + kv.Value["name"], delegate { Selection(file); }, Color.black, offset);
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                    tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                    tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 1f);
                    tb.background.transform.parent = scrollArea.transform;
                }
                else
                {
                    db = new DialogBox(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 5, 1.2f), "  " + kv.Value["name"], Color.black);
                    db.AddBorder();
                    db.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.07f, 0.07f, 0.07f);
                    db.background.transform.parent = scrollArea.transform;
                    db.textObj.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                    db.textObj.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                }
            }
            else
            {
                tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 5, 1.2f), "  " + kv.Value["name"], delegate { Selection(file); }, Color.black, offset);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
                tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
                tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                tb.background.transform.parent = scrollArea.transform;
            }
            offset += 2;
        }

        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (offset - 5) * UIScaler.GetPixelsPerUnit());

        tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Cancel(); }, Color.red);
        tb.SetFont(game.gameType.GetHeaderFont());
    }

    // Return to quest selection
    public void Cancel()
    {
        Destroyer.Dialog();
        // Get a list of available quests
        Dictionary<string, QuestData.Quest> ql = QuestLoader.GetQuests();

        // Pull up the quest selection page
        new QuestSelectionScreen(ql);
    }

    public void Selection(string file)
    {
        string package = serverLocation + game.gameType.TypeName() + "/" + file;
        StartCoroutine(Download(package, delegate { Save(file); }));
    }

    public void Save(string file)
    {
        QuestLoader.mkDir(saveLocation());

        // Write to disk
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveLocation() + "/" + file, FileMode.Create)))
        {
            writer.Write(download.bytes);
            writer.Close();
        }

        string section = file.Substring(0, file.Length - ".valkyrie".Length);
        int localVersion, remoteVersion;
        int.TryParse(localManifest.Get(section, "version"), out localVersion);
        int.TryParse(remoteManifest.Get(section, "version"), out remoteVersion);

        localManifest.Remove(section);
        localManifest.Add(section, remoteManifest.Get(section));


        if (File.Exists(saveLocation() + "/manifest.ini"))
        {
            File.Delete(saveLocation() + "/manifest.ini");
        }
        File.WriteAllText(saveLocation() + "/manifest.ini", localManifest.ToString());

        Destroyer.Dialog();
        DrawList();
    }

    public string saveLocation()
    {
        return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie/Download";
    }

    // Return to main menu
    public IEnumerator Download(string file, UnityEngine.Events.UnityAction call)
    {
        download = new WWW(file);
        yield return download;
        if (!string.IsNullOrEmpty(download.error))
        {
            // fixme not fatal
            ValkyrieDebug.Log(download.error);
            Application.Quit();
        }
        call();
    }
}
