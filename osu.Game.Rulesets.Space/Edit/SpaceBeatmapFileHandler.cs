using System;
using System.IO;
using System.Linq;
using System.Text;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Models;
using osu.Game.Screens.Edit;
using osu.Rulesets.Space.Beatmaps.Formats;

namespace osu.Game.Rulesets.Space.Edit
{
    public static class SpaceBeatmapFileHandler
    {
        public static string GetDefaultSaveFileName(EditorBeatmap editorBeatmap)
        {
            if (editorBeatmap == null)
                return string.Empty;

            string artist = sanitiseFileNamePart(editorBeatmap.Metadata?.Artist);
            string title = sanitiseFileNamePart(editorBeatmap.Metadata?.Title);
            string difficultyName = sanitiseFileNamePart(editorBeatmap.BeatmapInfo?.DifficultyName);
            string mapper = sanitiseFileNamePart(editorBeatmap.Metadata?.Author?.Username);

            if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(difficultyName) || string.IsNullOrWhiteSpace(mapper))
                return "beatmap.osu";

            return $"{artist} - {title} ({mapper}) [{difficultyName}].osu";
        }

        public static void SaveBeatmapToPath(EditorBeatmap editorBeatmap, string path)
        {
            if (editorBeatmap == null || string.IsNullOrWhiteSpace(path))
                return;

            string directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            using StreamWriter writer = new StreamWriter(path, false);
            new SpaceLegacyBeatmapEncoder(editorBeatmap).Encode(writer);
        }

        public static void DirectSaveToRealm(EditorBeatmap editorBeatmap, RealmAccess realm, Storage storage)
        {
            if (editorBeatmap == null || realm == null || storage == null)
                throw new InvalidOperationException("Required dependencies are unavailable for direct realm save.");

            using var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                new SpaceLegacyBeatmapEncoder(editorBeatmap).Encode(writer);

            stream.Seek(0, SeekOrigin.Begin);

            string targetFilename = createBeatmapFilenameFromMetadata(editorBeatmap.BeatmapInfo);
            var realmFileStore = new RealmFileStore(realm, storage);

            realm.Write(r =>
            {
                var targetBeatmap = r.Find<BeatmapInfo>(editorBeatmap.BeatmapInfo.ID)
                                    ?? throw new InvalidOperationException("Current beatmap could not be found in realm.");

                var setInfo = targetBeatmap.BeatmapSet
                              ?? throw new InvalidOperationException("Current beatmap set could not be found in realm.");

                if (setInfo.Beatmaps.Any(b => b.ID != targetBeatmap.ID && string.Equals(b.Path, targetFilename, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"{setInfo.GetDisplayString()} already has a difficulty with the name '{targetBeatmap.DifficultyName}'.");

                var existingFileInfo = targetBeatmap.Path != null ? setInfo.GetFile(targetBeatmap.Path) : null;

                if (existingFileInfo != null)
                    setInfo.Files.Remove(existingFileInfo);

                string oldMd5Hash = targetBeatmap.MD5Hash;

                targetBeatmap.MD5Hash = stream.ComputeMD5Hash();
                targetBeatmap.Hash = stream.ComputeSHA2Hash();
                targetBeatmap.LastLocalUpdate = DateTimeOffset.Now;
                targetBeatmap.Status = BeatmapOnlineStatus.LocallyModified;

                stream.Seek(0, SeekOrigin.Begin);

                var realmFile = realmFileStore.Add(stream, r);
                setInfo.Files.Add(new RealmNamedFileUsage(realmFile, targetFilename.ToStandardisedPath()));

                setInfo.Hash = computeBeatmapSetHash(setInfo, realmFileStore);
                setInfo.Status = BeatmapOnlineStatus.LocallyModified;

                targetBeatmap.TransferCollectionReferences(r, oldMd5Hash);
                targetBeatmap.UpdateLocalScores(r);
            });
        }

        private static string createBeatmapFilenameFromMetadata(BeatmapInfo beatmapInfo)
        {
            var metadata = beatmapInfo.Metadata;
            return $"{metadata.Artist} - {metadata.Title} ({metadata.Author?.Username}) [{beatmapInfo.DifficultyName}].osu".GetValidFilename();
        }

        private static string computeBeatmapSetHash(BeatmapSetInfo setInfo, RealmFileStore fileStore)
        {
            using var hashable = new MemoryStream();

            foreach (var file in setInfo.Files.Where(f => f.Filename.EndsWith(".osu", StringComparison.OrdinalIgnoreCase)).OrderBy(f => f.Filename))
            {
                using Stream fileStream = fileStore.Store.GetStream(file.File.GetStoragePath());
                fileStream?.CopyTo(hashable);
            }

            if (hashable.Length > 0)
                return hashable.ComputeSHA2Hash();

            return setInfo.Hash;
        }

        private static string sanitiseFileNamePart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string sanitised = value.Trim();

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                sanitised = sanitised.Replace(invalidChar.ToString(), string.Empty);

            return sanitised.Trim();
        }
    }
}
