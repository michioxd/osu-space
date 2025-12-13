using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace osu.Game.Rulesets.Space.Extension.SSPM
{
    public static class SSPMHelper
    {
        private static readonly string[] diff_name =
        [
            "Unknown",
            "Easy",
            "Medium",
            "Hard",
            "Logic",
            "Blast off"
        ];
        private static string getDiffName(int diff)
        {
            if (diff < 0 || diff >= diff_name.Length)
                return "Unknown";
            return diff_name[diff];
        }
        public static string ReadLine(BinaryReader reader)
        {
            List<byte> bytes = new List<byte>();
            while (true)
            {
                byte b = reader.ReadByte();
                if (b == 0x0A) break;
                bytes.Add(b);
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public static string GenerateOsuFile(
            string id,
            string title,
            string artist,
            string creator,
            int difficulty,
            string audioFilename,
            string bgFilename,
            List<(int time, float x, float y)> notes
        )
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("osuspaceruleset file format v1");
            sb.AppendLine();
            sb.AppendLine("[General]");
            sb.AppendLine($"AudioFilename: {audioFilename}");
            sb.AppendLine("AudioLeadIn: 0");
            sb.AppendLine("PreviewTime: -1");
            sb.AppendLine("Countdown: 0");
            sb.AppendLine("SampleSet: Normal");
            sb.AppendLine("StackLeniency: 0.7");
            sb.AppendLine("Mode: 727");
            sb.AppendLine("LetterboxInBreaks: 0");
            sb.AppendLine("WidescreenStoryboard: 1");
            sb.AppendLine();
            sb.AppendLine("[Metadata]");
            sb.AppendLine($"Title:{title}");
            sb.AppendLine($"TitleUnicode:{title}");
            sb.AppendLine($"Artist:{artist}");
            sb.AppendLine($"ArtistUnicode:{artist}");
            sb.AppendLine($"Creator:{creator}");
            sb.AppendLine($"Version:{getDiffName(difficulty)}");
            sb.AppendLine($"Source:");
            sb.AppendLine($"Tags:sspm {id} osuspaceruleset");
            sb.AppendLine($"BeatmapID:0");
            sb.AppendLine($"BeatmapSetID:-1");
            sb.AppendLine();
            sb.AppendLine("[Events]");
            sb.AppendLine("//Background and Video events");
            if (!string.IsNullOrEmpty(bgFilename))
            {
                sb.AppendLine($"0,0,\"{bgFilename}\",0,0");
            }
            sb.AppendLine();
            sb.AppendLine("[TimingPoints]");
            sb.AppendLine("0,500,4,1,0,100,1,0"); // dummy 120 bpm since it does not have bpm lol
            sb.AppendLine();
            sb.AppendLine("[HitObjects]");

            foreach (var note in notes)
            {
                float ox = (note.x + 0.5f) * (512f / 3f);
                float oy = (note.y + 0.5f) * (512f / 3f);

                sb.AppendLine($"{Math.Round(ox)},{Math.Round(oy)},{note.time},1,0,0:0:0:0:");
            }

            return sb.ToString();
        }

        public static string CreateOSZ(
            string originalPath,
            string id,
            string name,
            string artist,
            string creator,
            int difficulty,
            byte[] musicData,
            byte[] coverData,
            List<(int time, float x, float y)> notes
        )
        {
            string filename = $"{id} {name}".ReplaceAny(Path.GetInvalidFileNameChars(), '_');
            string oszPath = Path.Combine(Path.GetDirectoryName(originalPath), $"{filename}.osz");

            using (var zip = ZipFile.Open(oszPath, ZipArchiveMode.Create))
            {
                string audioFilename = "audio.mp3";
                if (musicData != null)
                {
                    var entry = zip.CreateEntry(audioFilename);
                    using (var entryStream = entry.Open())
                        entryStream.Write(musicData, 0, musicData.Length);
                }

                string bgFilename = "bg.png";
                if (coverData != null)
                {
                    var entry = zip.CreateEntry(bgFilename);
                    using (var entryStream = entry.Open())
                        entryStream.Write(coverData, 0, coverData.Length);
                }

                string osuContent = GenerateOsuFile(id, name, artist, creator, difficulty, audioFilename, bgFilename, notes);
                var osuEntry = zip.CreateEntry($"{creator} - {name} ({creator}) [{difficulty}].osu");
                using (var writer = new StreamWriter(osuEntry.Open()))
                {
                    writer.Write(osuContent);
                }
            }

            return oszPath;
        }
    }
}
