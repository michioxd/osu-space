using System.Collections.Generic;

namespace osu.Game.Rulesets.Space.Localisation
{
    public static partial class SpaceStrings
    {
        private static readonly Dictionary<string, string> japanese = new()
        {
            ["GitHub Repository"] = "GitHubリポジトリ",
            ["Check for Updates"] = "アップデートを確認",
            ["Import Sound Space Plus map (.sspm) (WIP)"] =
                "Sound Space Plusのマップ（.sspm）をインポート（開発中）",
            ["Delete all osu!space beatmaps"] = "すべてのosu!spaceビートマップを削除",
            ["Playfield"] = "プレイフィールド",
            ["Enable Grid"] = "グリッドを有効にする",
            ["Playfield Scale"] = "プレイフィールドの拡大率",
            ["Scale of the playfield (higher values = larger playfield)"] =
                "プレイフィールドの大きさ（値が大きいほど拡大）",
            ["Show Cursor Trail"] = "カーソルの軌跡を表示",
            ["Touch Input Type"] = "タッチ入力方式",
            [
                "Only for touch devices. Relative: Touch input moves the cursor relative to its current position. Absolute: Touch input sets the cursor position directly to the touched position."
            ] =
                "タッチデバイス専用。相対：現在位置からカーソルを移動します。絶対：タッチした位置にカーソルを直接移動します。",
            ["Touch Sensitivity"] = "タッチ感度",
            [
                "Only for touch devices and Touch Input Type is set to Relative. Sensitivity of touch input (higher values = more sensitive)."
            ] = "相対方式のタッチデバイス専用。値が大きいほど感度が高くなります。",
            ["Notes"] = "ノーツ",
            ["Note Color Palette"] = "ノーツのカラーパレット",
            [
                "Changes the colors of the notes. Some colors extracted from Sound Space Plus (Rhythia)"
            ] = "ノーツの色を変更します。一部の色はSound Space Plus（Rhythia）から取得しています。",
            ["Note Thickness"] = "ノーツの枠線の太さ",
            ["Thickness of the notes' borders"] = "ノーツの枠線の太さを変更します。",
            ["Note Corner Radius"] = "ノーツの角の丸み",
            ["Roundness of the notes' corners"] = "ノーツの角を丸くします。",
            ["Note Opacity"] = "ノーツの不透明度",
            ["How opaque/transparent/visible the note appears"] =
                "ノーツの見えやすさを変更します。",
            ["Note Scale"] = "ノーツの大きさ",
            ["The visual size of the notes (doesn't affect hitboxes)"] =
                "ノーツの見た目の大きさ（判定範囲には影響しません）",
            ["Note Glow"] = "ノーツの発光",
            [
                "Enables a glow effect on notes. Best used with 100% background dim and light note colors."
            ] = "ノーツの発光を有効にします。背景を100%暗くし、明るいノーツ色を使うと効果的です。",
            ["Glow Strength"] = "発光の強さ",
            ["Strength of the glow effect on notes"] = "ノーツの発光効果の強さを変更します。",
            ["Gameplay"] = "ゲームプレイ",
            ["Approach Rate"] = "接近速度",
            ["The speed that note move toward the grid (m/s)"] =
                "ノーツがグリッドに向かう速度（m/s）",
            ["Spawn Distance"] = "出現距離",
            ["Distance from the grid that note spawn (m)"] =
                "グリッドからノーツの出現位置までの距離（m）",
            ["Fade Length"] = "フェードイン距離",
            [
                "Percentage of the spawn distance that notes take to fade from invisible to fully opaque"
            ] = "ノーツが透明から完全に表示されるまでの、出現距離に対する割合",
            ["Do not push back"] = "押し戻さない",
            [
                "While enabled, notes will go past the grid when you miss, instead of always vanishing 0.2 units past the grid"
            ] = "有効にすると、ミスしたノーツはグリッドの0.2単位先で消えず、そのまま通過します。",
            ["Half ghost"] = "ハーフゴースト",
            ["Useful for patterns that fill the whole screen"] = "画面全体を覆う配置に便利です。",
            ["Parallax Strength"] = "視差効果の強さ",
            [
                "Strength of the parallax effect on the playfield (higher values = stronger effect, 0 = disable)"
            ] = "プレイフィールドの視差効果の強さ（値が大きいほど強く、0で無効）",
            ["Hit Window"] = "判定時間",
            [
                "The length of time notes can be hit after reaching the grid (default 25ms, rhythia def 55ms)"
            ] = "ノーツがグリッドに到達してから叩ける時間（既定値25ms、Rhythiaの既定値55ms）",
            ["Checking..."] = "確認中...",
            ["You are running the latest version of osu!space!"] = "osu!spaceは最新版です！",
            ["Failed to check for updates. Please check your internet connection."] =
                "アップデートを確認できませんでした。インターネット接続を確認してください。",
            ["Failed to check for updates."] = "アップデートを確認できませんでした。",
            ["All osu!space beatmaps added to deletion queue."] =
                "すべてのosu!spaceビートマップを削除キューに追加しました。",
            ["New version of osu!space are available!"] = "osu!spaceの新しいバージョンがあります！",
            [
                "Your current version is {0} and the latest version is {1}. Do you want to download it or visit the release page of this version?"
            ] =
                "現在のバージョンは{0}、最新バージョンは{1}です。ダウンロードしますか？それともリリースページを開きますか？",
            ["View Release"] = "リリースページを見る",
            ["Download"] = "ダウンロード",
            ["Cancel"] = "キャンセル",
            ["Delete all osu!space beatmaps?"] = "すべてのosu!spaceビートマップを削除しますか？",
            [
                "Are you sure you want to delete all osu!space beatmaps? This action cannot be undone."
            ] = "すべてのosu!spaceビートマップを削除しますか？この操作は元に戻せません。",
            ["Delete All Beatmaps"] = "すべてのビートマップを削除",
            ["Lemme think again..."] = "もう一度考える...",
            ["Please select a folder containing .sspm files"] =
                ".sspmファイルを含むフォルダーを選択してください",
            ["Import"] = "インポート",
            ["Try to locate the Rhythia (SSP) folder"] = "Rhythia（SSP）フォルダーを探す",
            ["Please select a beatmap file to import:"] =
                "インポートするビートマップファイルを選択してください：",
            ["Please select where to save this beatmap:"] =
                "ビートマップの保存先を選択してください：",
            ["File name"] = "ファイル名",
            ["Save"] = "保存",
            ["Save directly (NOT RECOMMENDED)"] = "直接保存（非推奨）",
            ["Hold to exit"] = "長押しで終了",
            ["Where's the cursor?"] = "カーソルはどこ？",
            ["No objects selected"] = "オブジェクトが選択されていません",
            ["Selected object is not a Space hit object"] =
                "選択したオブジェクトはSpaceのノーツではありません",
            ["Time"] = "時間",
            ["Position"] = "位置",
            ["Index"] = "インデックス",
            ["Selected Objects"] = "選択中のオブジェクト",
            ["Start Time"] = "開始時間",
            ["End Time"] = "終了時間",
            [
                "No Sound Space Plus maps folder was detected, or no .sspm files were found inside that. Please select the folder manually."
            ] =
                "Sound Space Plusのマップフォルダー、またはその中の.sspmファイルが見つかりませんでした。フォルダーを手動で選択してください。",
            ["No .sspm files found in the selected directory. Please select a different folder."] =
                "選択したフォルダーに.sspmファイルがありません。別のフォルダーを選択してください。",
            ["Importing Sound Space Plus map files..."] =
                "Sound Space Plusのマップをインポート中...",
            ["Import Sound Space Plus map complete!"] =
                "Sound Space Plusのマップのインポートが完了しました！",
            ["No .sspm files found to import."] = "インポートする.sspmファイルがありません。",
            ["Importing Sound Space Plus map files ({0}/{1})..."] =
                "Sound Space Plusのマップをインポート中（{0}/{1}）...",
            ["An error occurred while importing .sspm files. Please try again."] =
                ".sspmファイルのインポート中にエラーが発生しました。もう一度お試しください。",
            ["Sound Space Plus maps folder detected"] =
                "Sound Space Plusのマップフォルダーが見つかりました",
            ["We found a maps folder at:\n{0}\nDo you want to import from here?"] =
                "マップフォルダーが見つかりました：\n{0}\nここからインポートしますか？",
            ["Yes, import these maps"] = "はい、マップをインポート",
            ["No, I'll select manually"] = "いいえ、手動で選択します",
            ["osu!space need you to restart the game"] =
                "osu!spaceを使用するにはゲームの再起動が必要です",
            [
                "Since you have previously installed an earlier version of osu!space (below 2025.1214.0), and this update includes breaking changes. To use this feature, a game restart is required to apply the fixes. Please restart the game to ensure everything works correctly."
            ] =
                "以前のosu!space（2025.1214.0より前）をインストールしていたため、この更新の変更を適用するにはゲームの再起動が必要です。正常に動作させるために再起動してください。",
            ["Restart now (quit the game)"] = "今すぐ再起動（ゲームを終了）",
            ["Later"] = "後で",
            ["Beatmap saved to {0}"] = "ビートマップを{0}に保存しました",
            ["Failed to save beatmap: {0}"] = "ビートマップを保存できませんでした：{0}",
            ["Beatmap was saved directly to local database."] =
                "ビートマップをローカルデータベースに直接保存しました。",
            ["Direct save failed: {0}"] = "直接保存できませんでした：{0}",
        };
    }
}
