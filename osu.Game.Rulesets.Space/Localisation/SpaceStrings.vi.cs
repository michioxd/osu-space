using System.Collections.Generic;

namespace osu.Game.Rulesets.Space.Localisation
{
    public static partial class SpaceStrings
    {
        private static readonly Dictionary<string, string> vietnamese = new()
        {
            ["GitHub Repository"] = "Kho mã nguồn GitHub",
            ["Check for Updates"] = "Kiểm tra cập nhật",
            ["Import Sound Space Plus map (.sspm) (WIP)"] =
                "Nhập bản đồ Sound Space Plus (.sspm) (đang phát triển)",
            ["Delete all osu!space beatmaps"] = "Xóa tất cả bản đồ osu!space",
            ["Playfield"] = "Sân chơi",
            ["Enable Grid"] = "Bật lưới",
            ["Playfield Scale"] = "Tỉ lệ sân chơi",
            ["Scale of the playfield (higher values = larger playfield)"] =
                "Tỉ lệ sân chơi (giá trị càng cao, sân chơi càng lớn)",
            ["Show Cursor Trail"] = "Hiện vệt con trỏ",
            ["Touch Input Type"] = "Kiểu điều khiển cảm ứng",
            [
                "Only for touch devices. Relative: Touch input moves the cursor relative to its current position. Absolute: Touch input sets the cursor position directly to the touched position."
            ] =
                "Chỉ dành cho thiết bị cảm ứng. Tương đối: di chuyển con trỏ từ vị trí hiện tại. Tuyệt đối: đặt con trỏ ngay tại vị trí chạm.",
            ["Touch Sensitivity"] = "Độ nhạy cảm ứng",
            [
                "Only for touch devices and Touch Input Type is set to Relative. Sensitivity of touch input (higher values = more sensitive)."
            ] = "Chỉ dành cho thiết bị cảm ứng ở chế độ Tương đối. Giá trị càng cao thì càng nhạy.",
            ["Notes"] = "Nốt",
            ["Note Color Palette"] = "Bảng màu nốt",
            [
                "Changes the colors of the notes. Some colors extracted from Sound Space Plus (Rhythia)"
            ] = "Thay đổi màu nốt. Một số màu lấy từ Sound Space Plus (Rhythia)",
            ["Note Thickness"] = "Độ dày nốt",
            ["Thickness of the notes' borders"] = "Độ dày viền nốt",
            ["Note Corner Radius"] = "Độ bo góc nốt",
            ["Roundness of the notes' corners"] = "Độ tròn của góc nốt",
            ["Note Opacity"] = "Độ mờ nốt",
            ["How opaque/transparent/visible the note appears"] = "Độ hiển thị của nốt",
            ["Note Scale"] = "Tỉ lệ nốt",
            ["The visual size of the notes (doesn't affect hitboxes)"] =
                "Kích thước hiển thị của nốt (không ảnh hưởng vùng bấm)",
            ["Note Glow"] = "Hiệu ứng phát sáng nốt",
            [
                "Enables a glow effect on notes. Best used with 100% background dim and light note colors."
            ] = "Bật hiệu ứng phát sáng nốt. Phù hợp với nền tối 100% và màu nốt sáng.",
            ["Glow Strength"] = "Cường độ phát sáng",
            ["Strength of the glow effect on notes"] = "Cường độ hiệu ứng phát sáng trên nốt",
            ["Gameplay"] = "Lối chơi",
            ["Approach Rate"] = "Tốc độ tiếp cận",
            ["The speed that note move toward the grid (m/s)"] =
                "Tốc độ nốt di chuyển về lưới (m/s)",
            ["Spawn Distance"] = "Khoảng cách xuất hiện",
            ["Distance from the grid that note spawn (m)"] =
                "Khoảng cách từ lưới đến nơi nốt xuất hiện (m)",
            ["Fade Length"] = "Độ dài hiệu ứng xuất hiện",
            [
                "Percentage of the spawn distance that notes take to fade from invisible to fully opaque"
            ] = "Tỉ lệ khoảng cách xuất hiện để nốt chuyển từ vô hình đến rõ hoàn toàn",
            ["Do not push back"] = "Không đẩy lùi",
            [
                "While enabled, notes will go past the grid when you miss, instead of always vanishing 0.2 units past the grid"
            ] = "Khi bật, nốt sẽ đi qua lưới nếu bấm trượt thay vì biến mất sau 0,2 đơn vị",
            ["Half ghost"] = "Nửa bóng ma",
            ["Useful for patterns that fill the whole screen"] =
                "Hữu ích cho các mẫu nốt phủ kín màn hình",
            ["Parallax Strength"] = "Cường độ thị sai",
            [
                "Strength of the parallax effect on the playfield (higher values = stronger effect, 0 = disable)"
            ] = "Cường độ hiệu ứng thị sai (giá trị càng cao càng mạnh, 0 = tắt)",
            ["Hit Window"] = "Khoảng thời gian bấm",
            [
                "The length of time notes can be hit after reaching the grid (default 25ms, rhythia def 55ms)"
            ] = "Thời gian có thể bấm nốt sau khi chạm lưới (mặc định 25 ms, Rhythia 55 ms)",
            ["Checking..."] = "Đang kiểm tra...",
            ["You are running the latest version of osu!space!"] =
                "Bạn đang dùng phiên bản osu!space mới nhất!",
            ["Failed to check for updates. Please check your internet connection."] =
                "Không thể kiểm tra cập nhật. Vui lòng kiểm tra kết nối mạng.",
            ["Failed to check for updates."] = "Không thể kiểm tra cập nhật.",
            ["All osu!space beatmaps added to deletion queue."] =
                "Đã thêm tất cả bản đồ osu!space vào hàng đợi xóa.",
            ["New version of osu!space are available!"] = "Đã có phiên bản osu!space mới!",
            [
                "Your current version is {0} and the latest version is {1}. Do you want to download it or visit the release page of this version?"
            ] =
                "Phiên bản hiện tại: {0}; phiên bản mới nhất: {1}. Bạn muốn tải xuống hay xem trang phát hành?",
            ["View Release"] = "Xem bản phát hành",
            ["Download"] = "Tải xuống",
            ["Cancel"] = "Hủy",
            ["Delete all osu!space beatmaps?"] = "Xóa tất cả bản đồ osu!space?",
            [
                "Are you sure you want to delete all osu!space beatmaps? This action cannot be undone."
            ] = "Bạn có chắc muốn xóa tất cả bản đồ osu!space? Không thể hoàn tác thao tác này.",
            ["Delete All Beatmaps"] = "Xóa tất cả bản đồ",
            ["Lemme think again..."] = "Để tôi nghĩ lại...",
            ["Please select a folder containing .sspm files"] = "Chọn thư mục chứa các tệp .sspm",
            ["Import"] = "Nhập",
            ["Try to locate the Rhythia (SSP) folder"] = "Tìm thư mục Rhythia (SSP)",
            ["Please select a beatmap file to import:"] = "Chọn tệp bản đồ để nhập:",
            ["Please select where to save this beatmap:"] = "Chọn vị trí lưu bản đồ:",
            ["File name"] = "Tên tệp",
            ["Save"] = "Lưu",
            ["Save directly (NOT RECOMMENDED)"] = "Lưu trực tiếp (KHÔNG KHUYẾN KHÍCH)",
            ["Hold to exit"] = "Giữ để thoát",
            ["Where's the cursor?"] = "Con trỏ đâu rồi?",
            ["No objects selected"] = "Chưa chọn đối tượng nào",
            ["Selected object is not a Space hit object"] =
                "Đối tượng đã chọn không phải nốt Space",
            ["Time"] = "Thời gian",
            ["Position"] = "Vị trí",
            ["Index"] = "Chỉ số",
            ["Selected Objects"] = "Các đối tượng đã chọn",
            ["Start Time"] = "Thời gian bắt đầu",
            ["End Time"] = "Thời gian kết thúc",
            [
                "No Sound Space Plus maps folder was detected, or no .sspm files were found inside that. Please select the folder manually."
            ] =
                "Không tìm thấy thư mục bản đồ Sound Space Plus hoặc tệp .sspm. Vui lòng chọn thư mục thủ công.",
            ["No .sspm files found in the selected directory. Please select a different folder."] =
                "Không có tệp .sspm trong thư mục đã chọn. Vui lòng chọn thư mục khác.",
            ["Importing Sound Space Plus map files..."] = "Đang nhập bản đồ Sound Space Plus...",
            ["Import Sound Space Plus map complete!"] = "Đã nhập xong bản đồ Sound Space Plus!",
            ["No .sspm files found to import."] = "Không có tệp .sspm để nhập.",
            ["Importing Sound Space Plus map files ({0}/{1})..."] =
                "Đang nhập bản đồ Sound Space Plus ({0}/{1})...",
            ["An error occurred while importing .sspm files. Please try again."] =
                "Đã xảy ra lỗi khi nhập tệp .sspm. Vui lòng thử lại.",
            ["Sound Space Plus maps folder detected"] =
                "Đã tìm thấy thư mục bản đồ Sound Space Plus",
            ["We found a maps folder at:\n{0}\nDo you want to import from here?"] =
                "Đã tìm thấy thư mục bản đồ tại:\n{0}\nBạn muốn nhập từ đây?",
            ["Yes, import these maps"] = "Có, nhập các bản đồ này",
            ["No, I'll select manually"] = "Không, tôi sẽ chọn thủ công",
            ["osu!space need you to restart the game"] = "osu!space cần khởi động lại trò chơi",
            [
                "Since you have previously installed an earlier version of osu!space (below 2025.1214.0), and this update includes breaking changes. To use this feature, a game restart is required to apply the fixes. Please restart the game to ensure everything works correctly."
            ] =
                "Bạn đã cài phiên bản osu!space cũ (trước 2025.1214.0). Bản cập nhật này có thay đổi không tương thích. Vui lòng khởi động lại trò chơi để áp dụng bản sửa lỗi.",
            ["Restart now (quit the game)"] = "Khởi động lại ngay (thoát trò chơi)",
            ["Later"] = "Để sau",
            ["Beatmap saved to {0}"] = "Đã lưu bản đồ vào {0}",
            ["Failed to save beatmap: {0}"] = "Không thể lưu bản đồ: {0}",
            ["Beatmap was saved directly to local database."] =
                "Đã lưu bản đồ trực tiếp vào cơ sở dữ liệu cục bộ.",
            ["Direct save failed: {0}"] = "Không thể lưu trực tiếp: {0}",
        };
    }
}
