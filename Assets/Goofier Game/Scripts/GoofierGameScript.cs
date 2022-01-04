using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using rnd = UnityEngine.Random;

public class GoofierGameScript : MonoBehaviour
{
    public KMBombInfo BombInfo;
    public KMBombModule Module;
    public KMAudio Audio;
    public KMSelectable[] Ups;
    public KMSelectable[] Downs;
    public KMSelectable[] Lefts;
    public KMSelectable[] Rights;
    public KMSelectable StatusLight;
    public TextMesh[] Screens;

    private List<string> initialMoves = new List<string>();
    private List<string> moves = new List<string>();
    private bool solveActive = false;
    private int[][] values = new[]
    {
        new int[] { 0 , 0 , 0 , 0 , 0 },
        new int[] { 0 , 0 , 0 , 0 , 0 },
        new int[] { 0 , 0 , 0 , 0 , 0 },
        new int[] { 0 , 0 , 0 , 0 , 0 },
        new int[] { 0 , 0 , 0 , 0 , 0 }
    };

    int moduleId;
    static int moduleIdCounter = 1;
    bool moduleSolved;

    void Start()
    {
        moduleId = moduleIdCounter++;

        for (int i = 0; i < Ups.Length; i++)
            Ups[i].OnInteract += UpPressed(i);
        for (int i = 0; i < Downs.Length; i++)
            Downs[i].OnInteract += DownPressed(i);
        for (int i = 0; i < Lefts.Length; i++)
            Lefts[i].OnInteract += LeftPressed(i);
        for (int i = 0; i < Rights.Length; i++)
            Rights[i].OnInteract += RightPressed(i);
        StatusLight.OnInteract += Reset();

        GeneratePuzzle();
        UpdateScreens();
    }

    private KMSelectable.OnInteractHandler Reset()
    {
        return delegate
        {
            if (moduleSolved) return false;
            Debug.LogFormat(@"[Goofier Game #{0}] Module Reset.", moduleId);
            StartCoroutine(MoveBackInTime());
            return false;
        };
    }

    private KMSelectable.OnInteractHandler UpPressed(int btn)
    {
        return delegate
        {
            if (moduleSolved) return false;
            for (int i = 0; i < 5; i++)
                values[i][btn] = values[i][btn] == 9 ? 0 : values[i][btn] + 1;

            var a = values[0][btn];
            var b = values[1][btn];
            var c = values[2][btn];
            var d = values[3][btn];
            var e = values[4][btn];

            values[0][btn] = b;
            values[1][btn] = c;
            values[2][btn] = d;
            values[3][btn] = e;
            values[4][btn] = a;

            moves.Add(string.Format("Up {0}", btn + 1));

            UpdateScreens();
            CheckSolution();
            return false;
        };
    }

    private KMSelectable.OnInteractHandler DownPressed(int btn)
    {
        return delegate
        {
            if (moduleSolved) return false;
            for (int i = 0; i < 5; i++)
                values[i][btn] = values[i][btn] == 0 ? 9 : values[i][btn] - 1;

            var a = values[0][btn];
            var b = values[1][btn];
            var c = values[2][btn];
            var d = values[3][btn];
            var e = values[4][btn];

            values[0][btn] = e;
            values[1][btn] = a;
            values[2][btn] = b;
            values[3][btn] = c;
            values[4][btn] = d;

            moves.Add(string.Format("Down {0}", btn + 1));

            UpdateScreens();
            CheckSolution();
            return false;
        };
    }

    private KMSelectable.OnInteractHandler LeftPressed(int btn)
    {
        return delegate
        {
            if (moduleSolved) return false;
            for (int i = 0; i < 5; i++)
                values[btn][i] = values[btn][i] == 0 ? 9 : values[btn][i] - 1;

            var a = values[btn][0];
            var b = values[btn][1];
            var c = values[btn][2];
            var d = values[btn][3];
            var e = values[btn][4];

            values[btn][0] = b;
            values[btn][1] = c;
            values[btn][2] = d;
            values[btn][3] = e;
            values[btn][4] = a;

            moves.Add(string.Format("Left {0}", btn + 1));

            UpdateScreens();
            CheckSolution();
            return false;
        };
    }

    private KMSelectable.OnInteractHandler RightPressed(int btn)
    {
        return delegate
        {
            if (moduleSolved) return false;
            for (int i = 0; i < 5; i++)
                values[btn][i] = values[btn][i] == 9 ? 0 : values[btn][i] + 1;

            var a = values[btn][0];
            var b = values[btn][1];
            var c = values[btn][2];
            var d = values[btn][3];
            var e = values[btn][4];

            values[btn][0] = e;
            values[btn][1] = a;
            values[btn][2] = b;
            values[btn][3] = c;
            values[btn][4] = d;

            moves.Add(string.Format("Right {0}", btn + 1));

            UpdateScreens();
            CheckSolution();
            return false;
        };
    }

    private void GeneratePuzzle()
    {
        var value = rnd.Range(0, 10);
        for (int row = 0; row < 5; row++)
            for (int col = 0; col < 5; col++)
                values[row][col] = value;

        for (int j = 0; j < 2; j++)
        {
            var order = Enumerable.Range(0, 10).ToList().Shuffle();
            for (int i = 0; i < order.Count; i++)
            {
                if (order[i] < 5)
                {
                    if (rnd.Range(0, 2) == 0)
                        Ups[order[i]].OnInteract();
                    else
                        Downs[order[i]].OnInteract();
                }
                else
                {
                    if (rnd.Range(0, 2) == 0)
                        Lefts[order[i] - 5].OnInteract();
                    else
                        Rights[order[i] - 5].OnInteract();
                }
            }
        }

        initialMoves.AddRange(moves);
        moves.Clear();

        Debug.LogFormat(@"[Goofier Game #{0}] Initial Grid is {1}. Generated moves are: {2}", moduleId, value, initialMoves.Join(", "));
    }

    private void UpdateScreens()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        for (int row = 0; row < 5; row++)
            for (int col = 0; col < 5; col++)
                Screens[row * 5 + col].text = values[row][col].ToString();
    }

    private void CheckSolution()
    {
        if (Enumerable.Range(0, 25).Select(i => values[i / 5][i % 5]).ToArray().Distinct().Count() == 1)
        {
            Module.HandlePass();
            moduleSolved = true;
        }
    }

    private IEnumerator MoveBackInTime()
    {
        var movesToUse = new List<string>();

        if (solveActive)
            movesToUse.AddRange(initialMoves);

        movesToUse.AddRange(moves);

        movesToUse.Reverse();

        foreach (var move in movesToUse)
        {
            var temp = move.Split(' ');
            if (temp[0] == "Up")
                Downs[int.Parse(temp[1]) - 1].OnInteract();
            if (temp[0] == "Down")
                Ups[int.Parse(temp[1]) - 1].OnInteract();
            if (temp[0] == "Left")
                Rights[int.Parse(temp[1]) - 1].OnInteract();
            if (temp[0] == "Right")
                Lefts[int.Parse(temp[1]) - 1].OnInteract();
            if (solveActive)
                yield return new WaitForSeconds(.2f);
        }
        moves.Clear();
        yield return null;
    }

#pragma warning disable 0414
    readonly string TwitchHelpMessage = "!{0} r1l, r4r, c2d, c5u [Press Row 1 Left, Row 4 Right, Column 2 Down and Column 5 Up in that order] - !{0} reset [Reset the module]";
#pragma warning restore 0414

    IEnumerator ProcessTwitchCommand(string command)
    {
        Match m;
        if (moduleSolved || solveActive)
        {
            yield return "sendtochaterror The module is already solved or is being force-solved.";
            yield break;
        }
        else if ((m = Regex.Match(command, @"^\s*([rc12345udl, ]+\s*)\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            var match = m.Groups[1].Value.Split(',').Select(x => x.Trim(' ')).ToList();
            Debug.Log(match.Join(", "));

            for (int i = 0; i < match.Count; i++)
            {
                if (match[i].Length != 3)
                {
                    Debug.Log(match[i].Length);
                    Debug.Log(i);
                    yield return "sendtochaterror Incorrect Length.";
                }
                if (match[i][0] == 'c' && (match[i][2] == 'u' || match[i][2] == 'd'))
                {
                    if (match[i][2] == 'u')
                        Ups[int.Parse(match[i][1].ToString()) - 1].OnInteract();
                    else
                        Downs[int.Parse(match[i][1].ToString()) - 1].OnInteract();
                }
                else if (match[i][0] == 'r' && (match[i][2] == 'l' || match[i][2] == 'r'))
                {
                    if (match[i][2] == 'l')
                        Lefts[int.Parse(match[i][1].ToString()) - 1].OnInteract();
                    else
                        Rights[int.Parse(match[i][1].ToString()) - 1].OnInteract();
                }
                else
                    yield return "sendtochaterror Incorrect Input.";

                yield return new WaitForSeconds(.2f);
            }
            yield return null;
            yield break;
        }
        else if (Regex.IsMatch(command, @"^\s*(reset)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            StatusLight.OnInteract();
            yield return null;
            yield break;
        }
        else
        {
            yield return "sendtochaterror Invalid Command.";
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        Debug.LogFormat(@"[Mindlock #{0}] Module was force-solved by TP.", moduleId);
        solveActive = true;
        yield return MoveBackInTime();
        yield return null;
    }

}