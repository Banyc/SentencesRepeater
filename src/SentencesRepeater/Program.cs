
using FFmpeg.AutoGen.Example;
using FFmpegFirstTry;
using LibVLCSharp.Shared;
using SentencesRepeater.Helpers.Sampling;

Console.WriteLine("Hello, World!");

FFmpegBinariesHelper.RegisterFFmpegBinaries();

const string audioFilePath = "../../Resources/file_example_MP3_1MG.mp3";
// const int sampleRate = 44100;
const int sampleRate = 60;  // so as not to explode the memory
List<double> sampleData = Sampling.DecodeAudioFile(audioFilePath, sampleRate);

var sentenceRanges = SentenceLocater.GetSentenceRanges(sampleData, sampleRate, new()
{
    BackgroundNoiseVolume = 0.005,
    PauseIntervalMs = 700,
});
int sentenceIndex = 0;
CancellationTokenSource tokenSource = new();

bool isPlaying = false;

Core.Initialize();

using var libvlc = new LibVLC();
using var media = new Media(libvlc, audioFilePath);
using var mediaPlayer = new MediaPlayer(media);

media.AddOption(":no-screen");

while (true)
{
    Console.Write("> ");
    switch (Console.ReadLine())
    {
        case "":
            if (isPlaying)
            {
                isPlaying = false;
                tokenSource.Cancel();
                mediaPlayer.Stop();
            }
            else
            {
                PlayAsyncFacade();
            }
            break;
        case "+":
        case "n":
            sentenceIndex = Math.Min(sentenceIndex + 1, sentenceRanges.Count - 1);
            PlayAsyncFacade();
            break;
        case "-":
        case "p":
            sentenceIndex = Math.Max(sentenceIndex - 1, 0);
            PlayAsyncFacade();
            break;
    }
    Console.WriteLine($"Sentence index: {sentenceIndex}");
}

async Task PlayAsyncFacade()
{
    isPlaying = true;
    tokenSource.Cancel();
    tokenSource = new();
    int startMs = sentenceRanges[sentenceIndex].StartMs;
    startMs = Math.Max(0, startMs - 500);
    int endMs = sentenceRanges[sentenceIndex].EndMs;
    endMs += 500;
    await PlayAsync(startMs, endMs - startMs, tokenSource.Token).ConfigureAwait(false);
    isPlaying = false;
}

async Task PlayAsync(int startMs, int intervalMs, CancellationToken cancellationToken)
{
    mediaPlayer.Stop(); // stop playing audio
    mediaPlayer.Play(); // start playing audio
    mediaPlayer.SeekTo(TimeSpan.FromMilliseconds(startMs));

    await Task.Delay(intervalMs, cancellationToken).ConfigureAwait(false);
    if (cancellationToken.IsCancellationRequested)
    {
        return;
    }

    mediaPlayer.Stop(); // stop playing audio
}
