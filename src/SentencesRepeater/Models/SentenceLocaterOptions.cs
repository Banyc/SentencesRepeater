using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SentencesRepeater.Models
{
    public class SentenceLocaterOptions
    {
        // consider as silence when volume is smaller than this
        // range: [-1.0, 1.0]
        public double BackgroundNoiseVolume { get; set; }

        // consider as a pause when the silence takes longer than this interval.
        // range: [10, 6000]
        public int PauseIntervalMs { get; set; }

        // // consider as a sentence only if it takes more time than this.
        // // range: [150, 20000]
        // public int MinimalSentenceLengthMs { get; set; }

        // // range: [0, 20]
        // public int NumberOfLoudNoiseAllowed { get; set; }
    }
}
