using System.Text.Json;

namespace Catalog_And_Order_Sys.Extensions
{
    // ISession mặc định chỉ hỗ trợ SetString/GetString, SetInt32/GetInt32.
    // Để lưu được cả danh sách CartItem, ta viết thêm 2 hàm mở rộng
    // tự chuyển đổi qua lại giữa Object <-> chuỗi JSON.
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
