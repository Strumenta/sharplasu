using Antlr4.Runtime;
using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strumenta.Sharplasu.Parsing;
using System.ComponentModel.DataAnnotations;

namespace Strumenta.Sharplasu.Tests.Model
{
    [TestClass]
    public class PositionTest
    {
        private class MySetStatement : Node
        {
            public new Position SpecifiedPosition { get; set; }

            public MySetStatement(Position specifiedPosition = null) : base(specifiedPosition)
            {
                SpecifiedPosition = specifiedPosition;
            }
        }

        [TestMethod]
        public void OffsetTests()
        {
            var code = @"this is some code
second line
third line";
            Assert.AreEqual(0, Point.START_POINT.Offset(code));
            Assert.AreEqual(5, new Point(1, 5).Offset(code));
            Assert.AreEqual(17, new Point(1, 17).Offset(code));
            Assert.AreEqual(18, new Point(2, 0).Offset(code));
            Assert.ThrowsException<InvalidOperationException>(() => new Point(1, 18).Offset(code));
            Assert.ThrowsException<InvalidOperationException>(() => new Point(4, 0).Offset(code));
        }

        [TestMethod]
        public void PointCompare()
        {
            var p0 = Point.START_POINT;
            var p1 = new Point(1, 1);
            var p2 = new Point(1, 100);
            var p3 = new Point(2, 90);
            
            Assert.AreEqual(false, p0 < p0);
            Assert.AreEqual(true, p0 <= p0);
            Assert.AreEqual(true, p0 >= p0);
            Assert.AreEqual(false, p0 > p0);
                   
            Assert.AreEqual(true, p0 < p1);
            Assert.AreEqual(true, p0 <= p1);
            Assert.AreEqual(false, p0 >= p1);
            Assert.AreEqual(false, p0 > p1);
                   
            Assert.AreEqual(true, p0 < p2);
            Assert.AreEqual(true, p0 <= p2);
            Assert.AreEqual(false, p0 >= p2);
            Assert.AreEqual(false, p0 > p2);
                   
            Assert.AreEqual(true, p0 < p3);
            Assert.AreEqual(true, p0 <= p3);
            Assert.AreEqual(false, p0 >= p3);
            Assert.AreEqual(false, p0 > p3);
                   
            Assert.AreEqual(true, p1 < p2);
            Assert.AreEqual(true, p1 <= p2);
            Assert.AreEqual(false, p1 >= p2);
            Assert.AreEqual(false, p1 > p2);
                   
            Assert.AreEqual(true, p1 < p3);
            Assert.AreEqual(true, p1 <= p3);
            Assert.AreEqual(false, p1 >= p3);
            Assert.AreEqual(false, p1 > p3);
        }

        [TestMethod]
        public void IsBefore()
        {
            var p0 = Point.START_POINT;
            var p1 = new Point(1, 1);
            var p2 = new Point(1, 100);
            var p3 = new Point(2, 90);

            Assert.AreEqual(false, p0.IsBefore(p0));
            Assert.AreEqual(true, p0.IsBefore(p1));
            Assert.AreEqual(true, p0.IsBefore(p2));
            Assert.AreEqual(true, p0.IsBefore(p3));

            Assert.AreEqual(false, p1.IsBefore(p0));
            Assert.AreEqual(false, p1.IsBefore(p1));
            Assert.AreEqual(true, p1.IsBefore(p2));
            Assert.AreEqual(true, p1.IsBefore(p3));

            Assert.AreEqual(false, p2.IsBefore(p0));
            Assert.AreEqual(false, p2.IsBefore(p1));
            Assert.AreEqual(false, p2.IsBefore(p2));
            Assert.AreEqual(true, p2.IsBefore(p3));

            Assert.AreEqual(false, p3.IsBefore(p0));
            Assert.AreEqual(false, p3.IsBefore(p1));
            Assert.AreEqual(false, p3.IsBefore(p2));
            Assert.AreEqual(false, p3.IsBefore(p3));
        }

        [TestMethod]
        public void Text()
        {
            var code = @"this is some code
second line
third line".ReplaceLineEndings("\n");
            
            Assert.AreEqual("", new Position(Point.START_POINT, Point.START_POINT).Text(code));
            Assert.AreEqual("t", new Position(Point.START_POINT, new Point(1, 1)).Text(code));
            Assert.AreEqual("this is some cod", new Position(Point.START_POINT, new Point(1, 16)).Text(code));
            Assert.AreEqual("this is some code", new Position(Point.START_POINT, new Point(1, 17)).Text(code));
            var e = new Position(Point.START_POINT, new Point(2, 0)).Text(code);
            Assert.AreEqual($"this is some code\n", new Position(Point.START_POINT, new Point(2, 0)).Text(code));
            Assert.AreEqual($"this is some code\ns", new Position(Point.START_POINT, new Point(2, 1)).Text(code));
        }

        [TestMethod]
        public void ContainsPoint()
        {
            var before = new Point(1, 0);
            var start = new Point(1, 1);
            var middle = new Point(1, 2);
            var end = new Point(1, 3);
            var after = new Point(1, 4);
            var position = new Position(start, end);

            Assert.IsFalse(position.Contains(before), "contains should return false with point before");
            Assert.IsTrue(position.Contains(start), "contains should return true with point at the beginning");
            Assert.IsTrue(position.Contains(middle), "contains should return true with point in the middle");
            Assert.IsTrue(position.Contains(end), "contains should return true with point at the end");
            Assert.IsFalse(position.Contains(after), "contains should return false with point after");
        }

        [TestMethod]
        public void ContainsPosition()
        {
            var before = new Position(new Point(1, 0), new Point(1, 10));
            var inside = new Position(new Point(2, 3), new Point(2, 8));
            var after = new Position(new Point(3, 0), new Point(3, 10));
            var position = new Position(new Point(2, 0), new Point(2, 10));

            Assert.IsFalse(position.Contains(before), "contains should return false with position before");
            Assert.IsTrue(position.Contains(position), "contains should return true with same position");
            Assert.IsTrue(position.Contains(inside), "contains should return true with position inside");
            Assert.IsFalse(position.Contains(after), "contains should return false with position after");
        }

        [TestMethod]
        public void ContainsNode()
        {
            var before = new Position(new Point(1, 0), new Point(1, 10));
            var inside = new Position(new Point(2, 3), new Point(2, 8));
            var after = new Position(new Point(3, 0), new Point(3, 10));
            var position = new Position(new Point(2, 0), new Point(2, 10));

            Assert.IsFalse(position.Contains(before), "contains should return false with node before");
            Assert.IsTrue(position.Contains(inside), "contains should return true with node inside");            
            Assert.IsFalse(position.Contains(after), "contains should return false with node after");
        }

        [TestMethod]
        public void ParserRuleContextPosition()
        {
            var code = "set foo = 123";
            var lexer = new SimpleLangLexer(CharStreams.fromString(code));
            var parser = new SimpleLangParser(new CommonTokenStream(lexer));
            var cu = parser.compilationUnit();
            var setStmt = cu.statement(0) as SimpleLangParser.SetStmtContext;
            var pos = setStmt.ToPosition();
            Assert.AreEqual(new Position(new Point(1, 0), new Point(1, 13)), pos);
        }

        [TestMethod]
        public void PositionDerivedFromParseTreeNode()
        {
            var code = "set foo = 123";
            var lexer = new SimpleLangLexer(CharStreams.fromString(code));
            var parser = new SimpleLangParser(new CommonTokenStream(lexer));
            var cu = parser.compilationUnit();
            var setStmt = cu.statement(0) as SimpleLangParser.SetStmtContext;
            var mySetStatement = new MySetStatement();
            mySetStatement.Origin = new ParseTreeOrigin(setStmt);

            var pos = setStmt.ToPosition();
            Assert.AreEqual(new Position(new Point(1, 0), new Point(1, 13)), pos);
        }

        [TestMethod]
        public void IllegalPositionAccepted()
        {
            var position = new Position(new Point(10, 1), new Point(5, 2), validate: false);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IllegalPositionNotAccepted()
        {
            var position = new Position(new Point(10, 1), new Point(5, 2), validate: true);
        }

        [TestMethod]
        public void ParserTreePosition()
        {
            var code = "set foo = 123";
            var lexer = new SimpleLangLexer(CharStreams.fromString(code));
            var parser = new SimpleLangParser(new CommonTokenStream(lexer));
            var cu = parser.compilationUnit();
            var pos = cu.ToPosition();
            Assert.AreEqual(new Position(new Point(1, 0), new Point(1, 13)), pos);
        }
    }
}
