using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Chroma.SabreVGA.TextEditor.DataModel
{
    public class Buffer
    {
        private static SHA256 Sha256 = SHA256.Create();

        private int _currentLineIndex;

        private byte[] OriginalHash { get; set; }

        private byte[] CurrentHash
        {
            get
            {
                var text = string.Join('\n', Lines.Select(x => x.Text));
                return Sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            }
        }

        public bool Dirty => !OriginalHash.SequenceEqual(CurrentHash);

        public string FilePath { get; set; }

        public CodeEditor Owner { get; }
        public List<Line> Lines { get; }

        public int Top { get; set; }

        public int CurrentLineIndex
        {
            get => _currentLineIndex;
            set => _currentLineIndex = value >= Lines.Count ? Lines.Count - 1 : value;
        }

        public Line CurrentLine => Lines[CurrentLineIndex];
        public Line PreviousLine => CurrentLineIndex - 1 >= 0 ? Lines[CurrentLineIndex - 1] : null;
        public Line NextLine => CurrentLineIndex + 1 < Lines.Count ? Lines[CurrentLineIndex + 1] : null;

        public Selection Selection = Selection.None;

        public Buffer(CodeEditor owner, IEnumerable<Line> initialLines = null)
        {
            Owner = owner;

            if (initialLines == null)
                Lines = new List<Line>() {new()};
            else
                Lines = new List<Line>(initialLines);

            UpdateHash();
        }

        public void UpdateHash()
        {
            OriginalHash = CurrentHash;
        }

        public void BeginSelection()
        {
            Selection.StartLine = CurrentLineIndex;
            Selection.StartColumn = CurrentLine.CaretIndex;
            Selection.EndLine = Selection.StartLine;
            Selection.EndColumn = Selection.StartColumn;
        }

        public void SelectAll()
        {
            Selection.StartLine = 0;
            Selection.StartColumn = 0;

            LastLine();
            CurrentLine.End();

            Selection.EndLine = CurrentLineIndex;
            Selection.EndColumn = CurrentLine.Text.Length;
        }

        public void UpdateSelection()
        {
            if (Selection.IsNone)
                BeginSelection();

            Selection.EndColumn = CurrentLine.CaretIndex;
            Selection.EndLine = CurrentLineIndex;
        }

        public void ClearSelection()
            => Selection = Selection.None;

        public List<string> Cut()
        {
            if (!Selection.IsNone)
            {
                var lines = GetSelectionText();
                RemoveSelection();

                return lines;
            }
            else
            {
                return new List<string> {CutLine()};
            }
        }

        public string CutLine()
        {
            if (!Selection.IsNone)
                ClearSelection();

            var text = CurrentLine.Text;

            if (Lines.Count > 1)
            {
                Lines.RemoveAt(_currentLineIndex);

                if (_currentLineIndex >= Lines.Count)
                {
                    _currentLineIndex = Lines.Count - 1;
                    MoveWindowUp();
                }
            }
            else
            {
                CurrentLine.Home();
                CurrentLine.Text = string.Empty;
            }

            return text;
        }

        public List<string> GetSelectionText()
        {
            var lines = new string[0];

            if (!Selection.IsNone)
            {
                var sel = Selection.ToAbsolute();
                lines = new string[sel.SelectedLineCount];

                if (sel.SelectedLineCount == 1)
                {
                    lines[0] = Lines[sel.StartLine].TextBetween(sel.StartColumn, sel.EndColumn);
                }
                else
                {
                    lines[0] = Lines[sel.StartLine].TextAfter(sel.StartColumn);
                    lines[sel.SelectedLineCount - 1] = Lines[sel.EndLine].TextBefore(sel.EndColumn);

                    for (var i = 1; i < sel.SelectedLineCount - 1; i++)
                    {
                        lines[i] = Lines[sel.StartLine + i].Text;
                    }
                }
            }
            return lines.ToList();
        }

        public void RemoveSelection()
        {
            if (!Selection.IsNone)
            {
                var sel = Selection.ToAbsolute();

                if (sel.SelectedLineCount == 1)
                {
                    Lines[sel.StartLine].Text = Lines[sel.StartLine].TextOutside(sel.StartColumn, sel.EndColumn);
                }
                else
                {
                    CurrentLineIndex = sel.EndLine;

                    Lines[sel.StartLine].Text = Lines[sel.StartLine].TextBefore(sel.StartColumn);
                    Lines[sel.StartLine].Text += Lines[sel.EndLine].TextAfter(sel.EndColumn);

                    for (var i = 1; i < sel.SelectedLineCount; i++)
                    {
                        Up();
                        Lines.RemoveAt(sel.StartLine + 1);
                    }
                }

                CurrentLineIndex = sel.StartLine;
                CurrentLine.CaretIndex = sel.StartColumn;

                ClearSelection();
            }
        }

        public void Paste()
        {
            if (!Clipboard.HasText)
                return;

            if (!Selection.IsNone)
                RemoveSelection();

            var lines = Clipboard.Text.Split('\n')
                .Select(x => x.TrimEnd()
                    .TrimStart('\r')
                    .Replace("\t", "  ")
                ).ToArray();

            for (var i = 0; i < lines.Length; i++)
            {
                CurrentLine.InsertAtCaret(lines[i]);

                if (i + 1 < lines.Length)
                    NewLineAfterCurrent(true);
            }
        }

        public void MoveLineUp()
        {
            if (CurrentLineIndex == 0)
                return;

            if (!Selection.IsNone)
                Selection = Selection.None;
            
            var tmp = Lines[CurrentLineIndex - 1];
            Lines[CurrentLineIndex - 1] = Lines[CurrentLineIndex];
            Lines[CurrentLineIndex] = tmp;

            CurrentLineIndex--;
        }

        public void MoveLineDown()
        {
            if (CurrentLineIndex == Lines.Count - 1)
                return;
            
            if (!Selection.IsNone)
                Selection = Selection.None;

            var tmp = Lines[CurrentLineIndex + 1];
            Lines[CurrentLineIndex + 1] = Lines[CurrentLineIndex];
            Lines[CurrentLineIndex] = tmp;

            CurrentLineIndex++;
        }

        private void MoveWindowUp()
        {
            if (Owner.Screen.Cursor.Y == Owner.Screen.Margins.Top)
            {
                if (Top - 1 >= 0)
                    Top--;
            }
        }

        public void Up()
        {
            if (CurrentLineIndex == 0)
                return;

            MoveWindowUp();

            if (PreviousLine.Text.Length > CurrentLine.CaretIndex)
            {
                PreviousLine.CaretIndex = CurrentLine.CaretIndex;
                CurrentLineIndex--;
                Owner.Screen.Cursor.Y--;

                if (Owner.ShiftDown)
                {
                    Selection.EndColumn = CurrentLine.CaretIndex;
                    Selection.EndLine = CurrentLineIndex;
                }
            }
            else if (PreviousLine.Text.Length <= CurrentLine.CaretIndex)
            {
                PreviousLine.End();
                CurrentLineIndex--;
                Owner.Screen.Cursor.Y--;
            }

            if (Owner.ShiftDown)
                UpdateSelection();
        }

        public void FirstLine()
        {
            while (CurrentLineIndex != 0)
                Up();

            CurrentLine.Home();

            if (Owner.ShiftDown)
                UpdateSelection();
        }

        public void Down()
        {
            if (CurrentLineIndex >= Lines.Count - 1)
                return;

            if (Owner.Screen.Cursor.Y >= Owner.Screen.WindowRows - 2)
                Top++;

            if (NextLine.Text.Length > CurrentLine.CaretIndex)
            {
                NextLine.CaretIndex = CurrentLine.CaretIndex;
                CurrentLineIndex++;
                Owner.Screen.Cursor.Y++;
            }
            else if (NextLine.Text.Length <= CurrentLine.CaretIndex)
            {
                NextLine.End();
                CurrentLineIndex++;
                Owner.Screen.Cursor.Y++;
            }

            if (Owner.ShiftDown)
                UpdateSelection();
        }

        public void LastLine()
        {
            while (CurrentLineIndex < Lines.Count - 1)
            {
                Down();
            }

            CurrentLine.End();

            if (Owner.ShiftDown)
                UpdateSelection();
        }

        public void Right()
        {
            if (CurrentLine.IsCaretAtEnd)
            {
                if (CurrentLineIndex >= Lines.Count - 1)
                    return;

                Down();
                CurrentLine.Home();
            }
            else
            {
                CurrentLine.Right();
            }

            if (Owner.ShiftDown)
                UpdateSelection();
        }

        public void Left()
        {
            if (CurrentLine.IsCaretAtStart)
            {
                if (CurrentLineIndex == 0)
                    return;

                Up();
                CurrentLine.End();
            }
            else
            {
                CurrentLine.Left();
            }

            if (Owner.ShiftDown)
                UpdateSelection();
        }

        public void Home()
        {
            CurrentLine.Home();

            if (Owner.ShiftDown)
                UpdateSelection();
        }

        public void End()
        {
            CurrentLine.End();

            if (Owner.ShiftDown)
                UpdateSelection();
        }

        public void Clear()
        {
            ClearSelection();

            Lines.Clear();
            Lines.Add(new Line());
        }

        public void Backspace()
        {
            if (!Selection.IsNone)
            {
                RemoveSelection();
                return;
            }

            if (CurrentLine.IsCaretAtStart)
            {
                if (CurrentLineIndex == 0)
                    return;

                var currentText = CurrentLine.Text;
                Up();
                var targetCaretPosition = CurrentLine.Text.Length;
                Lines.RemoveAt(CurrentLineIndex + 1);
                CurrentLine.Text += currentText;
                CurrentLine.CaretIndex = targetCaretPosition;
            }
            else
            {
                CurrentLine.Backspace();
            }
        }

        public void Delete()
        {
            if (!Selection.IsNone)
            {
                RemoveSelection();
                return;
            }

            if (CurrentLine.IsCaretAtEnd)
            {
                if (CurrentLineIndex + 1 >= Lines.Count)
                    return;

                var nextText = Lines[CurrentLineIndex + 1].Text;
                Lines.RemoveAt(CurrentLineIndex + 1);
                CurrentLine.Text += nextText;
            }
            else
            {
                CurrentLine.Delete();
            }
        }

        public void NewLineBeforeCurrent()
        {
            RemoveSelection();

            Lines.Insert(
                CurrentLineIndex,
                new Line()
            );
        }

        public void NewLineAfterCurrent(bool advanceToNew = false)
        {
            RemoveSelection();

            Lines.Insert(
                CurrentLineIndex + 1,
                new Line()
            );

            if (advanceToNew)
            {
                Lines[CurrentLineIndex + 1].Text = CurrentLine.TextAfterCaret;
                CurrentLine.Text = CurrentLine.TextBeforeCaret;

                Down();
                CurrentLine.Home();
            }
        }

        public void TextInput(char c)
        {
            if (CurrentLine.Text.Length + 1 > Owner.Screen.WindowColumns)
                return;
            
            RemoveSelection();

            var pre = CurrentLine.TextBeforeCaret + c;
            CurrentLine.Text = pre + CurrentLine.TextAfterCaret;
            CurrentLine.CaretIndex++;
        }
    }
}