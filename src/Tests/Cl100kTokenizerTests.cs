using Newtonsoft.Json.Linq;
using OpenAI;
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
            var tokens = Cl100kTokenizer.Tokenize(TestString);
            Assert.AreEqual(12, tokens.Count);
        }

        [TestMethod]
        public void TestTokenizationReconstruction()
        {
            var tokens = Cl100kTokenizer.Tokenize(TestString);
            Assert.AreEqual(TestString, string.Join("", tokens.Select(t => t.Value)));

        }

        [TestMethod]
        public void TestTokenizationComparedToTiktoken()
        {
            var tokens = Cl100kTokenizer.Tokenize(TestString);


            // From tiktoken 
            // (45, "N"), (4665, "Unit"), (6296, " vs"), (13, "."), (865, " x"), (4665, "Unit"), (6296, " vs"), (13, "."), (85380, " MST"), (478, "est"), (6247, " Code"), (26379, " Examples")
            // [45, 4665, 6296, 13, 865, 4665, 6296, 13, 85380, 478, 6247, 26379]

            var tokenizedArray = tokens.Select(t => t.Id).ToArray();
            uint[] tiktoken_encoding = new uint[] { 45, 4665, 6296, 13, 865, 4665, 6296, 13, 85380, 478, 6247, 26379 };

            CollectionAssert.AreEqual(tiktoken_encoding, tokenizedArray);
        }
    }
}