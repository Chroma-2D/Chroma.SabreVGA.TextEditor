using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Chroma.Graphics;

namespace Chroma.SabreVGA.TextEditor.SyntaxHighlighting
{
    public class SyntaxHighlighter
    {
        private CodeEditor _owner;
        private List<SyntaxRule> _highlightingRules = new();

        public SyntaxHighlighter(CodeEditor owner)
        {
            _owner = owner;
        }

        public void Clear()
        {
            _highlightingRules.Clear();
        }

        public void ApplyRule(SyntaxRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule), "Cannot apply a null highlighting rule.");
            
            _highlightingRules.Add(rule);
        }

        public void ApplyRule<T>() where T : SyntaxRule, new()
        {
            ApplyRule(new T());
        }

        public void Colorize(string s, int tx, int ty)
        {
            for (var i = 0; i < _highlightingRules.Count; i++)
            {
                var rule = _highlightingRules[i];
                var matches = rule.Regex.Matches(s);

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        SetColorRange(
                            tx + match.Index,
                            ty, match.Length,
                            rule.Foreground,
                            rule.Background
                        );
                    }
                }
            }
        }

        private void SetColorRange(int tx, int ty, int length, Color foreground, Color background)
        {
            for (var x = tx; x < tx + length; x++)
            {
                if (x > _owner.Screen.WindowColumns)
                    break;

                if (_owner.CurrentBuffer.Selection.IsInSelectionRange(x - _owner.Screen.Margins.Left,
                    ty - _owner.Screen.Margins.Top + _owner.CurrentBuffer.Top))
                {
                    ref var cell = ref _owner.Screen[x, ty];

                    cell.Foreground = ColorUtilities.CalculateContrastingColor(
                        cell.Background
                    );
                }
                else
                {
                    _owner.Screen[x, ty].Foreground = foreground;
                    _owner.Screen[x, ty].Background = background;
                }
            }
        }
    }
}