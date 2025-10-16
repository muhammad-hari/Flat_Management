using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class TTLockClient
{
    private readonly HttpClient _http;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _username;
    private readonly string _password;
    private string _accessToken = "";

    private const string BaseUrl = "https://api.sciener.com/v3";

    public TTLockClient(string clientId, string clientSecret, string username, string password)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _username = username;
        _password = password;
        _http = new HttpClient();
    }

    private static string Md5Lower(string input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder();
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    public async Task<string> AuthAsync()
    {
        var pwdMd5 = Md5Lower(_password);
        var data = new Dictionary<string, string>
        {
            {"grant_type", "password"},
            {"client_id", _clientId},
            {"client_secret", _clientSecret},
            {"username", _username},
            {"password", pwdMd5}
        };

        var doc = await PostAsync("https://api.sciener.com/oauth2/token", data);
        _accessToken = doc.RootElement.GetProperty("access_token").GetString()!;
        return _accessToken;
    }

    private async Task<JsonDocument> PostAsync(string url, Dictionary<string, string> data)
    {
        var form = new FormUrlEncodedContent(data);
        var res = await _http.PostAsync(url, form);
        var content = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"HTTP {res.StatusCode}: {content}");
        }

        return JsonDocument.Parse(content);
    }

    private async Task<JsonDocument> PostV3Async(string path, Dictionary<string, string>? extra = null)
    {
        if (string.IsNullOrEmpty(_accessToken))
            throw new Exception("Belum ada access token! Jalankan AuthAsync dulu.");

        var data = new Dictionary<string, string>
        {
            {"clientId", _clientId},
            {"accessToken", _accessToken},
            {"date", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()}
        };

        if (extra != null)
        {
            foreach (var kv in extra)
                data[kv.Key] = kv.Value;
        }

        return await PostAsync($"{BaseUrl}/{path}", data);
    }

    public async Task<JsonDocument> GetGatewayListAsync(int pageNo = 1, int pageSize = 20) =>
        await PostV3Async("gateway/list", new() { { "pageNo", pageNo.ToString() }, { "pageSize", pageSize.ToString() } });

    public async Task<JsonDocument> GetLockListAsync(int pageNo = 1, int pageSize = 20)
    {
        var url = $"{BaseUrl}/lock/list?clientId={_clientId}&accessToken={_accessToken}&pageNo={pageNo}&pageSize={pageSize}&date={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var res = await _http.GetAsync(url);
        var content = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"HTTP {res.StatusCode}: {content}");

        return JsonDocument.Parse(content);
    }

    public async Task<JsonDocument> LockAsync(long lockId) =>
        await PostV3Async("lock/lock", new() { { "lockId", lockId.ToString() } });

    public async Task<JsonDocument> UnlockAsync(long lockId) =>
        await PostV3Async("lock/unlock", new() { { "lockId", lockId.ToString() } });

    public async Task<JsonDocument> GetCardsAsync(long lockId) =>
        await PostV3Async("identityCard/list", new() { { "lockId", lockId.ToString() }, { "pageNo", "1" }, { "pageSize", "20" } });

   public async Task<JsonDocument> AddCardAsync(long lockId, string cardName, string cardNumber, int validDays)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var end = DateTimeOffset.UtcNow.AddDays(validDays).ToUnixTimeMilliseconds();

        // Gunakan endpoint untuk cloud/gateway method
        string endpoint = "identityCard/addForReversedCardNumber";

        return await PostV3Async(endpoint, new()
        {
            {"lockId", lockId.ToString()},
            {"cardNumber", cardNumber},
            {"cardName", cardName},
            {"startDate", now.ToString()},
            {"endDate", end.ToString()},
            {"addType", "2"},
            {"date", now.ToString()}
        });
    }


   public async Task<JsonDocument> DeleteCardAsync(long lockId, long cardId)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return await PostV3Async("identityCard/delete", new()
        {
            { "lockId", lockId.ToString() },
            { "cardId", cardId.ToString() },
            { "deleteType", "2" }, // ⚠️ Penting: 1=Bluetooth, 2=Gateway/Cloud
            { "date", now.ToString() }
        });
    }

    public async Task<JsonDocument> GetFingerprintsAsync(long lockId) =>
        await PostV3Async("fingerprint/list", new() { { "lockId", lockId.ToString() } });

    
    public async Task<JsonDocument> AddFingerprintAsync(long lockId, string fingerprintNumber, int fingerprintType,
    string fingerprintName, long startDate, long endDate)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return await PostV3Async("fingerprint/add", new Dictionary<string, string>
        {
            {"lockId", lockId.ToString()},
            {"fingerprintNumber", fingerprintNumber},
            {"fingerprintType", fingerprintType.ToString()},
            {"fingerprintName", fingerprintName},
            {"startDate", startDate.ToString()},
            {"endDate", endDate.ToString()},
            {"date", now.ToString()}
        });
    }

    /// <summary>
    /// Query lock settings via gateway.
    /// Type options:
    /// 2 = Privacy Lock
    /// 3 = Tamper Alert
    /// 4 = Reset Button
    /// 7 = Open Direction
    /// </summary>
    public async Task<JsonDocument> QueryLockSettingAsync(long lockId, int type)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var data = new Dictionary<string, string>
        {
            {"lockId", lockId.ToString()},
            {"type", type.ToString()},
            {"date", now.ToString()}
        };

        return await PostV3Async("lock/querySetting", data);
    }
    public static void Print(JsonDocument doc)
    {
        Console.WriteLine(JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true }));
    }
}
