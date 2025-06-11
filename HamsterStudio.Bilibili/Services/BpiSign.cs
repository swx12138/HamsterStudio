using HamsterStudio.Barefeet.Task;
using HamsterStudio.Bilibili.Services.Restful;
using Refit;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace HamsterStudio.Bilibili.Services;

public static class BpiSign
{
    static readonly byte[] MIXIN_KEY_ENC_TAB = [
        46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49,
        33, 9, 42, 19, 29, 28, 14, 39, 12, 38, 41, 13, 37, 48, 7, 16, 24, 55, 40,
        61, 26, 17, 0, 1, 60, 51, 30, 4, 22, 25, 54, 21, 56, 59, 6, 63, 57, 62, 11,
        36, 20, 34, 44, 52
    ];

    public static Lazy<string> mixin_key = new Lazy<string>(() =>
    {
        var srv = RestService.For<IBilibiliApiService>(new HttpClient() { BaseAddress = new Uri("https://api.bilibili.com") });
        var data = srv.GenWebTicket(BiliTicketSign.CalculateHexSign(out string ts), ts).Result;
        if (data.Code != 0)
        {
            throw new Exception(data.Message);
        }
        string img_key = get_key(data.Data!.Nav.Img);   // "76e91e21c4df4e16af9467fd6f3e1095";
        string sub_key = get_key(data.Data.Nav.Sub);    // "ddfca332d157450784b807c59cd7921e";
        return MapMixinKey(img_key, sub_key);
    });

    public static string MapMixinKey(string img_key, string sub_key)
    {
        return new string([.. Map(img_key + sub_key).Take(32)]);
    }

    private static string get_key(string full)
    {
        return full.Split('/').Last().Split('.').First();
    }

    public static IEnumerable<char> Map(string raw_wbi_key)
    {
        foreach (byte p in MIXIN_KEY_ENC_TAB)
        {
            yield return raw_wbi_key[p];
        }
    }

    public static string GetWrid(NameValueCollection query, string wts)
    {
        var parameters = new Dictionary<string, string>();
        foreach (string key in query.AllKeys)
        {
            parameters[key] = query[key];
        }

        parameters["wts"] = wts;

        // 3. 按键排序
        var sortedParams = parameters.OrderBy(kv => kv.Key);

        // 4. 构建编码后的 query 字符串（注意：编码方式、空格为 %20，字母大写）
        var sb = new StringBuilder();
        foreach (var (key, value) in sortedParams)
        {
            if (sb.Length > 0)
                sb.Append("&");

            string encodedKey = UrlEncodeRfc3986(key);
            string encodedValue = UrlEncodeRfc3986(value);
            sb.AppendFormat("{0}={1}", encodedKey, encodedValue);
        }

        // 5. 拼接 mixin_key
        string rawSignature = sb.ToString() + mixin_key.Value;

        // 6. 计算 MD5
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(rawSignature);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // 将字节数组转换为十六进制字符串（小写或大写可选）
            StringBuilder hashBuilder = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hashBuilder.AppendFormat("{0:x2}", b); // 输出小写
            }
            return hashBuilder.ToString(); // 返回签名值
        }
    }

    public static string Sign(string queryString)
    {
        var query = HttpUtility.ParseQueryString(queryString);

        string wts = Timestamp.Now.ToString();
        query.Add("wts", wts);

        string w_rid = GetWrid(query, wts);
        query.Add("w_rid", w_rid);

        return query.ToString()!;
    }

    public static string UrlEncodeRfc3986(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // 使用 Uri.EscapeDataString，它符合 RFC 3986
        string escaped = Uri.EscapeDataString(value);

        // 替换 '+' 为空格的 %20 形式
        escaped = escaped.Replace("+", "%20");
        // 注意：Uri.EscapeDataString 已经是 UTF-8 编码且字母大写
        return escaped;
    }

}

public static class BiliTicketSign
{
    const string key = "XgwSnGZ1p";

    public static string CalculateHexSign(out string timestamp)
    {
        timestamp = "1749649612" ?? Timestamp.Now.ToString();
        string hmac = CalculateHmacSha256("ts" + timestamp, key);
        return hmac.ToLower(); // 返回小写的十六进制字符串

    }

    public static string CalculateHmacSha256(string message, string secretKey)
    {
        var encoding = new UTF8Encoding();
        byte[] keyBytes = encoding.GetBytes(secretKey);
        byte[] messageBytes = encoding.GetBytes(message);

        using var hmacsha256 = new HMACSHA256(keyBytes);
        byte[] hash = hmacsha256.ComputeHash(messageBytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

}
