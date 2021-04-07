using System;

namespace Chroma.SabreVGA.TextEditor.DataModel
{
    public struct Selection : IEquatable<Selection>
    {
        public static readonly Selection None = new Selection
        {
            StartColumn = -1,
            StartLine = -1,
            EndColumn = -1,
            EndLine = -1
        };

        public bool IsNone => None.Equals(this);

        public int StartColumn;
        public int StartLine;
        public int EndColumn;
        public int EndLine;

        public int SelectedLineCount
        {
            get
            {
                var abs = ToAbsolute();
                return abs.EndLine - abs.StartLine + 1;
            }
        }

        public bool IsInSelectionRange(int x, int y)
        {
            if (IsNone)
                return false;

            var abs = ToAbsolute();

            if (abs.SelectedLineCount == 1)
            {
                return y == StartLine && x >= abs.StartColumn && x < abs.EndColumn;
            }
            else
            {
                for (var i = y; i <= abs.EndLine; i++)
                {
                    if (i == abs.StartLine)
                    {
                        return y == abs.StartLine && x >= abs.StartColumn;
                    }
                    else if (i == abs.EndLine)
                    {
                        return y == abs.EndLine && x < abs.EndColumn;
                    }
                    else if (i > abs.StartLine && i < abs.EndLine)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Selection ToAbsolute()
        {
            int startCol, startLine;
            int endCol, endLine;

            if (StartLine < EndLine)
            {
                startCol = StartColumn;
                startLine = StartLine;
                endCol = EndColumn;
                endLine = EndLine;
            }
            else if (StartLine > EndLine)
            {
                startCol = EndColumn;
                startLine = EndLine;
                endCol = StartColumn;
                endLine = StartLine;
            }
            else
            {
                startLine = StartLine;
                endLine = EndLine;

                if (StartColumn > EndColumn)
                {
                    startCol = EndColumn;
                    endCol = StartColumn;
                }
                else
                {
                    startCol = StartColumn;
                    endCol = EndColumn;
                }
            }

            return new Selection
            {
                StartColumn = startCol,
                StartLine = startLine,
                EndColumn = endCol,
                EndLine = endLine
            };
        }

        public override bool Equals(object o)
        {
            return o is Selection sel &&
                   sel.StartColumn == StartColumn &&
                   sel.EndColumn == EndColumn &&
                   sel.StartLine == StartLine &&
                   sel.EndLine == EndLine;
        }

        public bool Equals(Selection other)
        {
            return StartColumn == other.StartColumn &&
                   StartLine == other.StartLine &&
                   EndColumn == other.EndColumn &&
                   EndLine == other.EndLine;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                StartColumn,
                StartLine,
                EndColumn,
                EndLine
            );
        }
    }
}