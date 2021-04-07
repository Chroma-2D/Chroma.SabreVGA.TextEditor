using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using Chroma.Graphics;
using Chroma.Graphics.TextRendering;
using Chroma.Input;
using Chroma.SabreVGA.TextEditor.DataModel;
using Chroma.SabreVGA.TextEditor.SyntaxHighlighting;
using Buffer = Chroma.SabreVGA.TextEditor.DataModel.Buffer;
using Color = Chroma.Graphics.Color;

namespace Chroma.SabreVGA.TextEditor
{
    public class CodeEditor
    {
        private int _currentBufferIndex;
        private Color _lineHighlightColor = new(22, 22, 22, 255);

        internal VgaScreen Screen;

        public List<Buffer> Buffers { get; } = new();
        public Options Options { get; set; } = new();

        public Action<string, string> FileSaveAction { get; set; }
        public Func<string, bool> FileExistsFunc { get; set; }
        public Action QuitRequest { get; set; }

        public HotkeyManager HotkeyManager { get; }
        public SyntaxHighlighter Highlighter { get; }

        public StatusLine StatusLine { get; }
        
        public bool ShiftDown
            => Keyboard.IsKeyDown(KeyCode.LeftShift)
               || Keyboard.IsKeyDown(KeyCode.RightShift);

        public string FileName
        {
            get
            {
                if (CurrentBuffer != null)
                    return Path.GetFileName(CurrentBuffer.FilePath);

                return "no file open";
            }
        }

        public Buffer CurrentBuffer => _currentBufferIndex < Buffers.Count ? Buffers[_currentBufferIndex] : null;

        public CodeEditor(Vector2 position, Size size, IFontProvider font, int cellWidth, int cellHeight)
        {
            Screen = new VgaScreen(position, size, font, cellWidth, cellHeight);
            Screen.Margins = new(0);
            Screen.ActiveBackgroundColor = Color.Black;

            HotkeyManager = new HotkeyManager(this);
            Highlighter = new SyntaxHighlighter(this);
            StatusLine = new StatusLine(this);

            Buffers.Add(new Buffer(this));
            _currentBufferIndex = 0;
        }

        public void Initialize(string filePath, string fileContent = null)
        {
            _currentBufferIndex = 0;
            Buffers.Clear();

            Buffer buf;
            if (string.IsNullOrEmpty(fileContent))
            {
                buf = new Buffer(this);
            }
            else
            {
                var lines = fileContent.Split('\n').Select(s => new Line(s));
                buf = new Buffer(this, lines);
            }

            buf.FilePath = filePath;
            Buffers.Add(buf);
        }

        public void Close()
        {
            _currentBufferIndex = 0;
            Buffers.Clear();
            
            QuitRequest?.Invoke();
        }

        public void Update(float delta)
        {
            ClearScreen();
            Screen.Update(delta);

            if (Buffers.Count > 0 && CurrentBuffer != null)
            {
                if (StatusLine.TakingInput)
                {
                    Screen.Cursor.X = StatusLine.Prompt.Length + StatusLine.CaretIndex + 1;
                    Screen.Cursor.Y = Screen.TotalRows - 1;
                }
                else
                {
                    Screen.Cursor.X = Screen.Margins.Left + CurrentBuffer.CurrentLine.CaretIndex;
                    Screen.Cursor.Y = Screen.Margins.Top + CurrentBuffer.CurrentLineIndex - CurrentBuffer.Top;
                }

                for (var i = CurrentBuffer.Top; i - CurrentBuffer.Top < Screen.WindowRows; i++)
                {
                    if (i >= CurrentBuffer.Lines.Count)
                    {
                        CurrentBuffer.Top = 0;
                        continue;
                    }

                    if (!CurrentBuffer.Selection.IsNone)
                    {
                        SetSelectionAppearance(i);
                    }
                    else
                    {
                        var line = CurrentBuffer.Lines[i];
                        for (var j = 0; j < line.Text.Length; j++)
                        {
                            Screen.SetColorAt(
                                Screen.Margins.Left + j,
                                Screen.Margins.Top + (i - CurrentBuffer.Top),
                                Screen.ActiveForegroundColor,
                                Screen.ActiveBackgroundColor
                            );
                        }
                    }

                    if (Options.HighlightSyntax)
                    {
                        Highlighter.Colorize(
                            CurrentBuffer.Lines[i].Text,
                            Screen.Margins.Left,
                            Screen.Margins.Top + (i - CurrentBuffer.Top)
                        );
                    }

                    PutStringAtWithLimitWindowWidth(
                        Screen.Margins.Left,
                        Screen.Margins.Top + (i - CurrentBuffer.Top),
                        CurrentBuffer.Lines[i].Text
                    );

                    if (i + 1 >= CurrentBuffer.Lines.Count)
                        break;
                }

                if (Options.HighlightCurrentLine)
                    HighlightCurrentLine();
            }

            Screen.Cursor.Shape = Options.CursorShape;
            StatusLine.Render();
        }

        public void Draw(RenderContext context)
        {
            Screen.Draw(context);
        }

        public void KeyPressed(KeyEventArgs e)
        {
            if (StatusLine.TakingInput)
            {
                StatusLine.KeyPressed(e);
            }
            else
            {
                HotkeyManager.KeyPressed(e, CurrentBuffer);
            }
        }

        public void TextInput(TextInputEventArgs e)
        {
            if (e == null || e.Text == null || e.Text.Length == 0)
                return;

            var c = e.Text[0];

            if (StatusLine.TakingInput)
            {
                StatusLine.TextInput(c);
            }
            else
            {
                CurrentBuffer?.TextInput(c);
            }
        }

        private void HighlightCurrentLine()
        {
            var y = Screen.Margins.Top + CurrentBuffer.CurrentLineIndex - CurrentBuffer.Top;

            for (var x = Screen.Margins.Left; x < Screen.WindowColumns; x++)
            {
                Screen[x, y].Background = _lineHighlightColor;
            }
        }

        private void SetSelectionAppearance(int i)
        {
            var abs = CurrentBuffer.Selection.ToAbsolute();

            if (i >= abs.StartLine && i <= abs.EndLine)
            {
                if (abs.StartLine - i == 0)
                {
                    if (abs.StartLine == abs.EndLine)
                    {
                        for (var j = abs.StartColumn; j < abs.EndColumn; j++)
                        {
                            Screen.SetColorAt(
                                Screen.Margins.Left + j,
                                Screen.Margins.Top + (i - CurrentBuffer.Top),
                                Screen.ActiveForegroundColor,
                                ColorUtilities.CalculateContrastingColor(Screen.ActiveForegroundColor)
                            );
                        }
                    }
                    else
                    {
                        for (var j = abs.StartColumn; j < CurrentBuffer.Lines[i].Text.Length; j++)
                        {
                            Screen.SetColorAt(
                                Screen.Margins.Left + j,
                                Screen.Margins.Top + (i - CurrentBuffer.Top),
                                ColorUtilities.CalculateContrastingColor(Screen.ActiveForegroundColor),
                                Screen.ActiveForegroundColor
                            );
                        }
                    }
                }
                else if (i - abs.StartLine == abs.EndLine - abs.StartLine)
                {
                    for (var j = 0; j < abs.EndColumn; j++)
                    {
                        Screen.SetColorAt(
                            Screen.Margins.Left + j,
                            Screen.Margins.Top + (i - CurrentBuffer.Top),
                            ColorUtilities.CalculateContrastingColor(Screen.ActiveForegroundColor),
                            Screen.ActiveForegroundColor
                        );
                    }
                }
                else
                {
                    for (var j = 0; j < CurrentBuffer.Lines[i].Text.Length; j++)
                    {
                        Screen.SetColorAt(
                            Screen.Margins.Left + j,
                            Screen.Margins.Top + (i - CurrentBuffer.Top),
                            ColorUtilities.CalculateContrastingColor(Screen.ActiveForegroundColor),
                            Screen.ActiveForegroundColor
                        );
                    }
                }
            }
        }

        private void PutStringAtWithLimitWindowWidth(int x, int y, string s)
        {
            while (x < Screen.Margins.Left)
                x++;

            while (y < Screen.Margins.Top)
                y++;

            while (x > Screen.WindowColumns)
                x--;

            while (y > Screen.WindowRows)
                y--;

            for (var i = 0; i < s.Length; i++)
            {
                if (x + i > Screen.WindowColumns)
                    break;

                Screen[x + i, y].Character = s[i];
            }
        }

        private void ClearScreen()
        {
            for (var x = Screen.Margins.Left; x < Screen.WindowColumns; x++)
            {
                for (var y = Screen.Margins.Top; y < Screen.WindowRows; y++)
                {
                    Screen[x, y].Background = Screen.ActiveBackgroundColor;
                    Screen[x, y].Foreground = Screen.ActiveForegroundColor;
                    Screen[x, y].Character = ' ';
                }
            }
        }
    }
}