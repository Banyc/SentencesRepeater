using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SentencesRepeater.Models;

namespace SentencesRepeater.Helpers.Sampling
{
    public static class SentenceLocater
    {
        public static List<SentenceRange> GetSentenceRanges(List<double> samples, int sampleRate, SentenceLocaterOptions options)
        {
            List<SentenceRange> ranges = new();

            SentenceRange? range = null;

            // state machine
            int lastSilenceIndex = 0;
            int lastSoundIndex = 0;
            int pauseIntervalIndexes = GetNumberOfSampleIndexes(sampleRate, options.PauseIntervalMs);

            // loop through samples
            for (int index = 0; index < samples.Count; index++)
            {
                if (samples[index] < options.BackgroundNoiseVolume)
                {
                    // still in silence

                    // do nothing

                    lastSilenceIndex = index;
                }
                else
                {
                    // got a new sound
                    if (index <= pauseIntervalIndexes)
                    {
                        if (range == null)
                        {
                            range = new()
                            {
                                StartMs = GetTimeMs(sampleRate, lastSilenceIndex),
                            };
                        }
                    }
                    else if (index - lastSoundIndex > pauseIntervalIndexes)
                    {
                        // the pause is long enough to start a new sentence here

                        // end the previous sentence
                        if (range != null)
                        {
                            range.EndMs = GetTimeMs(sampleRate, lastSoundIndex);
                            ranges.Add(range);
                        }

                        // start a new sentence
                        range = new()
                        {
                            StartMs = GetTimeMs(sampleRate, index),
                        };
                    }
                    lastSoundIndex = index;
                }
            }

            // end the last sentence
            if (range != null)
            {
                range.EndMs = GetTimeMs(sampleRate, lastSoundIndex);
                ranges.Add(range);
            }

            return ranges;
        }

        private static int GetTimeMs(int sampleRate, int sampleIndex)
        {
            return sampleIndex * 1000 / sampleRate;
        }

        private static int GetNumberOfSampleIndexes(int sampleRate, int intervalMs)
        {
            return sampleRate * intervalMs / 1000;
        }
    }
}
