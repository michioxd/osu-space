#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using osu.Framework.Logging;
using osu.Game.Database;
using System.Threading;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Space.Extension.SSPM
{
    public class SSPMConverter
    {
        private readonly BeatmapManager beatmapManager;
        private readonly RulesetStore rulesetStore;

        public SSPMConverter(BeatmapManager beatmapManager, RulesetStore rulesetStore)
        {
            this.beatmapManager = beatmapManager;
            this.rulesetStore = rulesetStore;
        }

        public void ImportFromDirectory(string directory, CancellationToken cancellationToken, Action<int, int, int, bool, bool>? onProgress = null)
        {
            if (!Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch
                {
                    Logger.Log($"Could not create import directory: {directory}");
                    return;
                }
            }

            string[] files = Directory.GetFiles(directory, "*.sspm");
            int total = files.Length, failed = 0;
            if (total == 0)
            {
                onProgress?.Invoke(0, 0, 0, false, true);
                Logger.Log("No .sspm files found to import.");
                return;
            }
            bool isDone;
            for (int i = 0; i < total; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Log("SSPM Import cancelled by user.");
                    break;
                }

                string file = files[i];
                try
                {
                    string oszPath = ConvertSSPMToOSZ(file);
                    if (oszPath != null)
                    {
                        var importTask = new ImportTask(oszPath);
                        var importedSet = beatmapManager.Import(importTask).Result;

                        if (importedSet != null)
                        {
                            var spaceRuleset = rulesetStore.GetRuleset("osuspaceruleset");
                            if (spaceRuleset != null)
                            {
                                importedSet.PerformWrite(s =>
                                {
                                    foreach (var b in s.Beatmaps)
                                    {
                                        b.Ruleset = spaceRuleset;
                                    }
                                });
                            }
                            else
                            {
                                Logger.Log("Could not find osuspaceruleset in ruleset store.");
                            }
                        }

                        Logger.Log($"Successfully converted and imported: {file}");
                    }
                }
                catch (Exception e)
                {
                    failed++;
                    Logger.Log($"Failed to convert {file}: {e.Message}");
                }
                finally
                {
                    isDone = i == total - 1;
                    onProgress?.Invoke(i + 1, total, failed, isDone, false);
                }
            }
        }

        public string ConvertSSPMToOSZ(string sspmPath)
        {
            using (var stream = File.OpenRead(sspmPath))
            using (var reader = new BinaryReader(stream))
            {
                // header
                byte[] signature = reader.ReadBytes(4);
                if (Encoding.ASCII.GetString(signature) != "SS+m")
                    throw new Exception("Invalid SSPM signature");

                short version = reader.ReadInt16();

                if (version == 1)
                    return parseV1(reader, sspmPath);
                else if (version == 2)
                    return parseV2(reader, sspmPath);
                else
                    throw new Exception($"Unknown SSPM version: {version}");
            }
        }

        private string parseV2(BinaryReader reader, string originalPath)
        {
            reader.ReadBytes(4); // reserved

            reader.ReadBytes(20);
            reader.ReadInt32();
            _ = reader.ReadInt32();
            int markerCount = reader.ReadInt32();
            byte difficulty = reader.ReadByte();
            _ = reader.ReadInt16();
            bool hasAudio = reader.ReadByte() == 1;
            bool hasCover = reader.ReadByte() == 1;
            _ = reader.ReadByte() == 1;

            _ = reader.ReadInt64();
            _ = reader.ReadInt64();
            long audioOffset = reader.ReadInt64();
            long audioLength = reader.ReadInt64();
            long coverOffset = reader.ReadInt64();
            long coverLength = reader.ReadInt64();
            long markerDefOffset = reader.ReadInt64();
            _ = reader.ReadInt64();
            long markerOffset = reader.ReadInt64();
            _ = reader.ReadInt64();

            string id = readString16(reader);
            string rawName = readString16(reader);
            _ = readString16(reader);
            string[] parts = rawName.Split(new[] { " - " }, 2, StringSplitOptions.None);
            string artist = parts.Length > 1 ? parts[0].Trim() : "Unknown";
            string name = parts.Length > 1 ? parts[1].Trim() : rawName;
            int mapperCount = reader.ReadInt16();
            List<string> mappers = [];
            for (int i = 0; i < mapperCount; i++)
                mappers.Add(readString16(reader));
            string creator = string.Join(" & ", mappers);

            byte[]? musicData = null;
            if (hasAudio && audioLength > 0)
            {
                reader.BaseStream.Seek(audioOffset, SeekOrigin.Begin);
                musicData = reader.ReadBytes((int)audioLength);
            }

            byte[]? coverData = null;
            if (hasCover && coverLength > 0)
            {
                reader.BaseStream.Seek(coverOffset, SeekOrigin.Begin);
                coverData = reader.ReadBytes((int)coverLength);
            }

            reader.BaseStream.Seek(markerDefOffset, SeekOrigin.Begin);
            byte defCount = reader.ReadByte();
            var markerDefs = new List<(string id, List<byte> types)>();

            for (int i = 0; i < defCount; i++)
            {
                string defId = readString16(reader);
                byte typeCount = reader.ReadByte();
                var types = new List<byte>();
                for (int j = 0; j < typeCount; j++)
                    types.Add(reader.ReadByte());
                reader.ReadByte(); // 0x00 terminator
                markerDefs.Add((defId, types));
            }

            reader.BaseStream.Seek(markerOffset, SeekOrigin.Begin);
            var notes = new List<(int time, float x, float y)>();

            for (int i = 0; i < markerCount; i++)
            {
                int time = reader.ReadInt32();
                byte markerTypeIndex = reader.ReadByte();

                if (markerTypeIndex >= markerDefs.Count)
                    throw new Exception($"Invalid marker type index: {markerTypeIndex}");

                var def = markerDefs[markerTypeIndex];

                float? noteX = null;
                float? noteY = null;

                foreach (byte type in def.types)
                {
                    switch (type)
                    {
                        case 0x01: reader.ReadByte(); break;
                        case 0x02: reader.ReadInt16(); break;
                        case 0x03: reader.ReadInt32(); break;
                        case 0x04: reader.ReadInt64(); break;
                        case 0x05: reader.ReadSingle(); break;
                        case 0x06: reader.ReadDouble(); break;
                        case 0x07:
                            byte posType = reader.ReadByte();
                            if (posType == 0)
                            {
                                byte x = reader.ReadByte();
                                byte y = reader.ReadByte();
                                if (def.id == "ssp_note")
                                {
                                    noteX = x;
                                    noteY = y;
                                }
                            }
                            else
                            {
                                float x = reader.ReadSingle();
                                float y = reader.ReadSingle();
                                if (def.id == "ssp_note")
                                {
                                    noteX = x;
                                    noteY = y;
                                }
                            }
                            break;
                        case 0x08:
                        case 0x09:
                            int len16 = reader.ReadUInt16();
                            reader.ReadBytes(len16);
                            break;
                        case 0x0a:
                        case 0x0b:
                            int len32 = reader.ReadInt32();
                            reader.ReadBytes(len32);
                            break;
                        default:
                            throw new Exception($"Unknown data type in marker: {type}");
                    }
                }

                if (def.id == "ssp_note" && noteX.HasValue && noteY.HasValue)
                {
                    notes.Add((time, noteX.Value, noteY.Value));
                }
            }

            return SSPMHelper.CreateOSZ(originalPath, id, name, artist, creator, difficulty, musicData, coverData, notes);
        }

        private string readString16(BinaryReader reader)
        {
            ushort len = reader.ReadUInt16();
            byte[] bytes = reader.ReadBytes(len);
            return Encoding.UTF8.GetString(bytes);
        }

        private string parseV1(BinaryReader reader, string originalPath)
        {
            reader.ReadInt16();

            string id = SSPMHelper.ReadLine(reader);
            string rawName = SSPMHelper.ReadLine(reader);
            string creator = SSPMHelper.ReadLine(reader);
            string[] parts = rawName.Split(new[] { " - " }, 2, StringSplitOptions.None);
            string artist = parts.Length > 1 ? parts[0].Trim() : "Unknown";
            string name = parts.Length > 1 ? parts[1].Trim() : rawName;
            _ = reader.ReadInt32();
            int noteCount = reader.ReadInt32();
            int difficulty = reader.ReadByte() - 1;

            byte coverType = reader.ReadByte();
            byte[]? coverData = null;

            if (coverType == 1)
            {
                // short h = reader.ReadInt16();
                // short w = reader.ReadInt16();
                // bool mip = reader.ReadBoolean();
                // byte format = reader.ReadByte();
                // long len = reader.ReadInt64();
                // coverData = reader.ReadBytes((int)len);
                // TODO: godot raw img data implement later
                Logger.Log("SSPM v1 Raw Cover format not fully supported, skipping cover.");
                coverData = null;
            }
            else if (coverType == 2)
            {
                long len = reader.ReadInt64();
                coverData = reader.ReadBytes((int)len);
            }

            byte musicType = reader.ReadByte();
            byte[]? musicData = null;
            if (musicType == 1)
            {
                long len = reader.ReadInt64();
                musicData = reader.ReadBytes((int)len);
            }

            var notes = new List<(int time, float x, float y)>();
            for (int i = 0; i < noteCount; i++)
            {
                int time = reader.ReadInt32();
                byte type = reader.ReadByte();
                float x, y;
                if (type == 1)
                {
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                }
                else
                {
                    x = reader.ReadByte();
                    y = reader.ReadByte();
                }
                notes.Add((time, x, y));
            }

            return SSPMHelper.CreateOSZ(originalPath, id, name, artist, creator, difficulty, musicData, coverData, notes);
        }
    }

    public static class StringExtensions
    {
        public static string ReplaceAny(this string s, char[] chars, char replacement)
        {
            string res = s;
            foreach (char c in chars)
                res = res.Replace(c, replacement);
            return res;
        }
    }
}
