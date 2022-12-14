using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Text;

namespace Strumenta.Sharplasu.Model
{
    public static class PositionExtensions
    {
        public static Point StartPoint(this IToken token)
        {
            return new Point(token.Line, token.Column);
        }

        public static Point EndPoint(this IToken token)
        {
            return token.StartPoint() + token.Text;
        }
        public static bool HasChildren(this RuleContext rule)
        {
            return rule.ChildCount > 0;
        }

        public static IParseTree FirstChild(this RuleContext rule)
        {
            if (rule.HasChildren())
                return rule.GetChild(0);
            else
                return null;
        }

        public static IParseTree LastChild(this RuleContext rule)
        {
            if (rule.HasChildren())
                return rule.GetChild(rule.ChildCount - 1);
            else
                return null;
        }

        public static Position ToPosition(this ITerminalNode node, bool considerPosition = true)
        {
            if (considerPosition)
                return new Position(node.Symbol.StartPoint(), node.Symbol.EndPoint());
            else
                return null;
        }
    }

    [Serializable]
    public class Point : IComparable<Point>
    {
        public Point(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public int Line { get; private set; }
        public int Column { get; private set; }

        public override string ToString()
        {
            return $"{Line}, Column {Column}";
        }

        public int CompareTo(Point other)
        {
            if (Line == other.Line)
            {
                return Column - other.Column;
            }

            return Line - other.Line;
        }

        public static bool operator >(Point left, Point right)
        {
            return left.CompareTo(right) == 1;
        }

        public static bool operator <(Point left, Point right)
        {
            return left.CompareTo(right) == -1;
        }

        public Position AsPosition
        {
            get
            {
                return new Position(this, this);
            }
        }

        public static Point operator +(Point point, string text)
        {
            if (string.IsNullOrEmpty(text))
                return point;
            else if (text.StartsWith("\r\n"))
                return new Point(point.Line + 1, 0) + text.Substring(2);
            else if (text.StartsWith("\n") || text.StartsWith("\r"))
                return new Point(point.Line + 1, 0) + text.Substring(1);
            else
                return new Point(point.Line, point.Column + 1) + text.Substring(1);
        }
    }

    [Serializable]
    public class Position
    {
        public Point Start { get; private set; }
        public Point End { get; private set; }

        public Position(Point start, Point end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return $"From ({Start.Line}, {Start.Column}) To ({End.Line}, {End.Column})";
        }
    }
}
