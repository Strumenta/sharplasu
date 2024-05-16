using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Strumenta.Sharplasu.Testing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace Strumenta.Sharplasu.Model
{
    public static class PositionExtensions
    {        
        public static Position Pos(int startLine, int startColumn, int endLine, int endColumn) =>
            new Position(
                    new Point(startLine, startColumn),
                    new Point(endLine, endColumn)
                );

        public static bool IsBefore(this Node node, Node other) => node.Position.Start.IsBefore(other.Position.Start);

        public static int StartLine(this Node node) => node.Position.Start.Line;

        public static int EndLine(this Node node) => node.Position.End.Line;
    }

    [Serializable]
    public class Point : IComparable<Point>, IEquatable<Point>
    {
        public static int START_LINE = 1;
        public static int START_COLUMN = 0;
        public static Point START_POINT = new Point(START_LINE, START_COLUMN);

        private static void CheckLine(int line)
        {
            Asserts.Require(line >= Point.START_LINE, () => $"Line should be equal or greater than 1, was {line}");
        }

        private static void CheckColumn(int column)
        {
            Asserts.Require(column >= Point.START_COLUMN, () => $"Column should be equal or greater than 0, was {column}");
        }

        private Point() {}        

        public Point(int line, int column)
        {
            Line = line;
            Column = column;
            CheckLine(line);
            CheckColumn(column);
        }

        public int Line { get; set; }
        public int Column { get; set; }

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
            return left.CompareTo(right) >= 1;
        }

        public static bool operator <(Point left, Point right)
        {
            return left.CompareTo(right) <= -1;
        }

        public static bool operator >=(Point left, Point right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator <=(Point left, Point right)
        {
            return left.CompareTo(right) <= 0;
        }            

        /**
         * Translate the Point to an offset in the original code stream.
         */
        public int Offset(string code)
        {
            var lines = code.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Asserts.Require(lines.Length >= Line, () =>            
                $"The point does not exist in the given text. It indicates line {Line} but there are only {lines.Length} lines"
            );
            Asserts.Require(lines[Line - 1].Length >= Column, 
                () => $"The column does not exist in the given text. Line {Line} has {lines[Line - 1].Length} columns, " +
                    $"the point indicates column {Column}"
            );
            var newLines = Line - 1;
            return lines.Take(Line - 1).Sum((it) => it.Length) + newLines + Column;
        }

        /**
         * Computes whether this point comes strictly before another point.
         * <param name="other">the other point</param>
         */
        public bool IsBefore(Point other) => this < other;

        /**
         * Computes whether this point is the same as, or comes before, another point.
         * <param name="other">the other point</param>
         */
        public bool IsSameOrBefore(Point other) => this <= other;

        /**
         * Computes whether this point is the same as, or comes after, another point.
         * <param name="other">the other point</param>
         */
        public bool IsSameOrAfter(Point other) => this >= other;

        public static Point operator +(Point point, int length)
        {
            return new Point(point.Line, point.Column + length);
        }

        public static Point operator +(Point point, string text)
        {
            if (string.IsNullOrEmpty(text))
                return point;

            var line = point.Line;
            var column = point.Column;
            var i = 0;
            while(i < text.Length)
            {
                if (text[i] == '\n' || text[i] == '\r')
                {
                    line++;
                    column = 0;
                    if (text[i] == '\r' && i < text.Length - 1 && text[i + 1] == '\n')
                    {
                        i++; // Count the \r\n sequence as 1 line
                    }
                }
                else
                {
                    column++;
                }
                i++;
            }
            return new Point(line, column);           
        }

        public Position PositionWithLength(int length)
        {
            Asserts.Require(length >= 0);
            return new Position(this, this + length);
        }

        public bool Equals(Point other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            int hashCode = -1456208474;
            hashCode = hashCode * -1521134295 + Line.GetHashCode();
            hashCode = hashCode * -1521134295 + Column.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public Position AsPosition
        {
            get
            {
                return new Position(this, this);
            }
        }

        public static bool operator ==(Point one, Point other)
        {
            if (ReferenceEquals(one, other))
                return true;
            if (ReferenceEquals(one, null))
                return false;
            if (ReferenceEquals(other, null))
                return false;
            return one.Line == other.Line && one.Column == other.Column;           
        }

        public static bool operator !=(Point one, Point other)
        {
            return !(one == other);
        }
    }    

    [Serializable]
    public abstract class Source { }

    public class SourceSet
    {
        public string Name { get; private set; }
        public string Root { get; private set; }

        public SourceSet(string name, string root)
        {
            Name = name;
            Root = root;            
        }
    }

    public class SourceSetElement : Source
    {
        public SourceSet SourceSet { get; private set; }
        public string RelativePath { get; private set; }

        public SourceSetElement(SourceSet sourceSet, string relativePath)
        {
            SourceSet = sourceSet;
            RelativePath = relativePath;
        }
    }

    public class FileSource : Source
    {
        public FileInfo File { get; private set; }
        
        public FileSource(FileInfo file)
        {
            File = file;
        }
    }

    public class StringSource : Source
    {
        public string Code { get; private set; }

        public StringSource(string code = null)
        {
            Code = code;
        }
    }

    public class URLSource : Source
    {
        public Uri Url { get; private set; }

        public URLSource(Uri url)
        {
            Url = url;
        }
    }

    /**
     * This source is intended to be used for nodes that are "calculated".
     * For example, nodes representing types that are derived by examining the code
     * but cannot be associated to any specific point in the code.
     *
     * <param name="desciption">this is a description of the source. It is used to describe the process that calculated the node.
     *                    Examples of values could be "type inference".</param>
     */
    public class SyntheticSource : Source
    {
        public string Description { get; private set; }

        public SyntheticSource(string description)
        {
            Description = description;
        }
    }

    /**
     * An area in a source file, from start to end.
     * The start point is the point right before the starting character.
     * The end point is the point right after the last character.
     * An empty position will have coinciding points.
     *
     * Consider a file with one line, containing text "HELLO".
     * The Position of such text will be Position(Point(1, 0), Point(1, 5)).
     */

    [Serializable]
    public class Position : IComparable<Position>
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public Source Source { get; set; } = null;

        private Position() { }

        public Position(Point start, Point end, Source source = null, bool validate = true)
        {
            Start = start;
            End = end;
            Source = source;
            if(validate)
            {
                Asserts.Require(Start.IsBefore(end) || Start == End, () => $"End should follows start or be the same as start (start: {start}, end: {end})");
            }
        }

        public override string ToString()
        {
            return $"Position(start={Start.Line}, {Start.Column}), end=({End.Line}, {End.Column}) {(Source == null ? "" : $", source={Source}")}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Position o = obj as Position;
            return ((Start == null && o.Start == null) || Start.CompareTo(o.Start) == 0) &&
                ((End == null && o.End == null) || End.CompareTo(o.End) == 0);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Start == null ? 0 : Start.GetHashCode());
                hash = hash * 23 + (End == null ? 0 : End.GetHashCode());
                return hash;
            }
        }

        public int CompareTo(Position other)
        {
            var cmp = Start.CompareTo(other.Start);
            if (cmp == 0)
            {
                return End.CompareTo(other.End);
            }
            else
            {
                return cmp;
            }
        }

        /**
         * Given the whole code extract the portion of text corresponding to this position
         */
        public string Text(string wholeText)
        {
            return wholeText.Substring(Start.Offset(wholeText), End.Offset(wholeText));
        }

        /**
         * The length in characters of the text under this position in the provided source.
         * <param name="code">the source text.</param>
         */
        public int Length(string code) => End.Offset(code) - Start.Offset(code);

        public bool IsEmpty => Start == End;

        /**
         * Tests whether the given point is contained in the interval represented by this object.
         * <param name="point">the point.</param>
         */
        public bool Contains(Point point)
        {
            return ((point == Start || Start.IsBefore(point)) && (point == End || point.IsBefore(End)));
        }

        /**
         * Tests whether the given point is contained in the interval represented by this object.
         * <param name="point">the point.</param>
         */
        public bool Contains(Position position)
        {
            return (position != null) &&
                Start.IsSameOrBefore(position.Start) &&
                End.IsSameOrAfter(position.End);
        }

        /**
         * Tests whether the given node is contained in the interval represented by this object.
         * <param name="node">the node.</param>
         */
        public bool Contains(Node node)
        {
            return Contains(node.Position);
        }

        /**
         * Tests whether the given position overlaps the interval represented by this object.
         * <param name="position">the position.</param>
         */
        public bool Overlaps(Position position)
        {
            return (position != null) && (
            (Start.IsSameOrAfter(position.Start) && Start.IsSameOrBefore(position.End)) ||
                (End.IsSameOrAfter(position.Start) && End.IsSameOrBefore(position.End)) ||
                (position.Start.IsSameOrAfter(Start) && position.Start.IsSameOrBefore(End)) ||
                (position.End.IsSameOrAfter(Start) && position.End.IsSameOrBefore(End))
            );
        }
    }
}
