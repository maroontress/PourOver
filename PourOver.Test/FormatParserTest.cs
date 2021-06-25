namespace PourOver.Test
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class FormatParserTest
    {
        [TestMethod]
        public void NoBrace()
        {
            var array = new[]
            {
                "",
                "hello",
            };
            foreach (var s in array)
            {
                var set = TokenParser.Parse(s);
                Assert.AreEqual(0, set.Count);
            }
        }

        [TestMethod]
        public void OneToken()
        {
            var s = "hello {name}";
            var set = TokenParser.Parse(s);
            Assert.AreEqual(1, set.Count);
            var all = set.ToArray();
            var token0 = all[0];
            Assert.AreEqual(1, token0.Count);
            Assert.AreEqual("name", token0.Token);
        }

        [TestMethod]
        public void TwoToken()
        {
            var s = "{greeting} {name}";
            var set = TokenParser.Parse(s);
            Assert.AreEqual(2, set.Count);
            var all = set.ToArray();
            Array.Sort(all);
            var token0 = all[0];
            Assert.AreEqual(1, token0.Count);
            Assert.AreEqual("greeting", token0.Token);
            var token1 = all[1];
            Assert.AreEqual(1, token1.Count);
            Assert.AreEqual("name", token1.Token);
        }

        [TestMethod]
        public void TwoToken12()
        {
            var s = "dear {name}, {greeting} {name}";
            var set = TokenParser.Parse(s);
            Assert.AreEqual(2, set.Count);
            var all = set.ToArray();
            Array.Sort(all);
            var token0 = all[0];
            Assert.AreEqual(1, token0.Count);
            Assert.AreEqual("greeting", token0.Token);
            var token1 = all[1];
            Assert.AreEqual(2, token1.Count);
            Assert.AreEqual("name", token1.Token);
        }

        [TestMethod]
        public void FormatException()
        {
            var s = "dear {name}, {greeting} {name";
            try
            {
                TokenParser.Parse(s);
                Assert.Fail("exception not thrown");
            }
            catch (FormatException)
            {
            }
        }

        [TestMethod]
        public void FormatExceptionLastChar()
        {
            var s = "dear {name}, {greeting} {";
            try
            {
                TokenParser.Parse(s);
                Assert.Fail("exception not thrown");
            }
            catch (FormatException)
            {
            }
        }

        [TestMethod]
        public void FormatExceptionMissingOpeningBrace()
        {
            var s = "dear name}, {greeting} {name}";
            try
            {
                TokenParser.Parse(s);
                Assert.Fail("exception not thrown");
            }
            catch (FormatException)
            {
            }
        }
    }
}
