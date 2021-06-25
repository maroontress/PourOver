namespace PourOver.Test
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class CountedTokenSetValidatorTest
    {
        [TestMethod]
        public void OneToken()
        {
            var s0 = "hello {name}";
            var set0 = TokenParser.Parse(s0);

            var s1 = "olah {name}";
            var set1 = TokenParser.Parse(s1);

            var list = CountedTokenSetValidator.Validate(
                "english", set0, "Hilichurlian", set1);
            Assert.AreEqual(0, list.Count());
        }

        [TestMethod]
        public void MismatchedOneToken()
        {
            var s0 = "hello {name}";
            var set0 = TokenParser.Parse(s0);

            var s1 = "olah {nane}";
            var set1 = TokenParser.Parse(s1);

            var list = CountedTokenSetValidator.Validate(
                "English", set0, "Hilichurlian", set1);
            Assert.AreNotEqual(0, list.Count());
            foreach (var m in list)
            {
                Console.WriteLine(m);
            }
        }

        [TestMethod]
        public void MismatchedTokenCount()
        {
            var s0 = "hello {name}";
            var set0 = TokenParser.Parse(s0);

            var s1 = "{name}, olah {name}";
            var set1 = TokenParser.Parse(s1);

            var list = CountedTokenSetValidator.Validate(
                "English", set0, "Hilichurlian", set1);
            Assert.AreNotEqual(0, list.Count());
            foreach (var m in list)
            {
                Console.WriteLine(m);
            }
        }
    }
}
