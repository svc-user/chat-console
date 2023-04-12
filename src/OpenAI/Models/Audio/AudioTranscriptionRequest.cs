using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAI.Models.Audio
{
    public class AudioTranscriptionRequest
    {
        [Required]
        public byte[] File { get; set; } = new byte[] { };

        [Required]
        public string Model { get; set; } = "whisper-1";

        public string Prompt { get; set; } = "Jeg har i dag været ude og spise. Lige nu ser jeg fjernsyn. Synes du jeg skal have klippet mit hår?";

        public string ResponseFormat { get; set; } = "json";

        public float Temperature { get; set; } = 0;
        public string Language { get; set; } = "da";
    }
}
