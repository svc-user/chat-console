using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAI.Models
{
    public class Delta
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public class Choice
    {
        public Delta Delta { get; set; }
        public int Index { get; set; }
        public object FinishReason { get; set; }
    }

    public class StreamedChatResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public int Created { get; set; }
        public string Model { get; set; }
        public List<Choice> Choices { get; set; }
    }
}
