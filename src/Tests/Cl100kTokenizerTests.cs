using Newtonsoft.Json.Linq;
using OpenAI;
using Xunit;
using Xunit.Sdk;

namespace Tests
{
    [TestClass]
    public class Cl100kTokenizerTests
    {
        private const string TestString = "NUnit vs. xUnit vs. MSTest Code Examples";

        [TestMethod]
        public void TestTokenCount()
        {
            var tokens = Cl100kTokenizer.EncodeNative(TestString, new());
            Assert.AreEqual(12, tokens.Count);
        }

        [TestMethod]
        public void TestTokenizationReconstruction()
        {
            var tokens = Cl100kTokenizer.EncodeNative(TestString, new());
            Assert.AreEqual(TestString, string.Join("", tokens));

        }


        [TestMethod]
        public void TestTokenizationComparedToTiktoken()
        {
            var tokens = Cl100kTokenizer.EncodeNative(TestString, new());


            // From tiktoken 
            // (45, "N"), (4665, "Unit"), (6296, " vs"), (13, "."), (865, " x"), (4665, "Unit"), (6296, " vs"), (13, "."), (85380, " MST"), (478, "est"), (6247, " Code"), (26379, " Examples")
            // [45, 4665, 6296, 13, 865, 4665, 6296, 13, 85380, 478, 6247, 26379]

            var tokenizedArray = tokens.ToArray();
            var tiktoken_encoding = new int[] { 45, 4665, 6296, 13, 865, 4665, 6296, 13, 85380, 478, 6247, 26379 };

            CollectionAssert.AreEqual(tiktoken_encoding, tokenizedArray);
        }

        [TestMethod]
        [DataRow("antidisestablishmentarianism", new int[] { 519, 85342, 34500, 479, 8997, 2191 })]
        [DataRow("2 + 2 = 4", new int[] { 17, 489, 220, 17, 284, 220, 19 })]
        [DataRow("お誕生日おめでとう", new int[] { 33334, 45918, 243, 21990, 9080, 33334, 62004, 16556, 78699 })]
        public void TestEncodeNativeAccordingToPlaybook(string input, int[] expected)
        {
            var tokens = Cl100kTokenizer.EncodeNative(input, new());
            var tokenizedArray = tokens.ToArray();
            CollectionAssert.AreEqual(expected, tokenizedArray);
        }
    }
}