using System.Text.Json;

public class DeviceService
{
    private readonly TTLockClient _ttLock;

    public DeviceService(TTLockClient ttLock)
    {
        _ttLock = ttLock;
    }

    public async Task AuthAsync()
    {
        await _ttLock.AuthAsync();
    }

    public async Task<List<DeviceDto>> GetDevicesAsync(int pageNo = 1, int pageSize = 20)
    {
        var doc = await _ttLock.GetLockListAsync(pageNo, pageSize);
        var list = new List<DeviceDto>();

        if (doc.RootElement.TryGetProperty("list", out var devices))
        {
            foreach (var item in devices.EnumerateArray())
            {
                list.Add(new DeviceDto
                {
                    LockAlias = item.GetProperty("lockAlias").GetString() ?? "",
                    LockId = item.GetProperty("lockId").GetInt64(),
                    ElectricQuantity = item.GetProperty("electricQuantity").GetInt32(),
                    NoKeyPwd = item.GetProperty("noKeyPwd").GetString() ?? ""
                });
            }
        }

        return list;
    }

    // ===============================================
    // Method baru: Get all IC cards of a lock
    // ===============================================
    public async Task<List<ICCardDto>> GetCardsAsync(long lockId, int pageNo = 1, int pageSize = 20)
    {
        var doc = await _ttLock.GetCardsAsync(lockId);
        var list = new List<ICCardDto>();

        if (doc.RootElement.TryGetProperty("list", out var cards))
        {
            foreach (var item in cards.EnumerateArray())
            {
                list.Add(new ICCardDto
                {
                    CardId = item.GetProperty("cardId").GetInt64(),
                    LockId = item.GetProperty("lockId").GetInt64(),
                    CardNumber = item.GetProperty("cardNumber").GetString() ?? "",
                    CardName = item.GetProperty("cardName").GetString() ?? "",
                    StartDate = DateTimeOffset.FromUnixTimeMilliseconds(item.GetProperty("startDate").GetInt64()).DateTime,
                    EndDate = DateTimeOffset.FromUnixTimeMilliseconds(item.GetProperty("endDate").GetInt64()).DateTime,
                    CreateDate = DateTimeOffset.FromUnixTimeMilliseconds(item.GetProperty("createDate").GetInt64()).DateTime,
                    SenderUsername = item.GetProperty("senderUsername").GetString() ?? "",
                    CardType = item.GetProperty("cardType").GetInt32()
                });
            }
        }

        return list;
    }

    // Get Fingerprints
    public async Task<List<FingerprintDto>> GetFingerprintsAsync(long lockId)
    {
        var doc = await _ttLock.GetFingerprintsAsync(lockId);
        var list = new List<FingerprintDto>();

        if (doc.RootElement.TryGetProperty("list", out var fps))
        {
            foreach (var item in fps.EnumerateArray())
            {
                list.Add(new FingerprintDto
                {
                    FingerprintId = item.GetProperty("fingerprintId").GetInt64(),
                    LockId = item.GetProperty("lockId").GetInt64(),
                    FingerprintName = item.GetProperty("fingerprintName").GetString() ?? "",
                    FingerprintNumber = item.GetProperty("fingerprintNumber").GetString() ?? "",
                    StartDate = DateTimeOffset.FromUnixTimeMilliseconds(item.GetProperty("startDate").GetInt64()).DateTime,
                    EndDate = DateTimeOffset.FromUnixTimeMilliseconds(item.GetProperty("endDate").GetInt64()).DateTime
                });
            }
        }

        return list;
    }

    // ==========================
    // Add Card wrapper
    // ==========================
    public async Task AddCardAsync(long lockId, string cardName, string cardNumber, int validDays)
    {
        await _ttLock.AddCardAsync(lockId, cardName, cardNumber, validDays);
    }

    // ==========================
    // Add Fingerprint wrapper
    // ==========================
    public async Task AddFingerprintAsync(long lockId, string fingerprintNumber, int fingerprintType,
        string fingerprintName, long startDate, long endDate)
    {
        await _ttLock.AddFingerprintAsync(lockId, fingerprintNumber, fingerprintType, fingerprintName, startDate, endDate);
    }

    public async Task<JsonDocument> QueryLockSettingAsync(long lockId, int type) =>
        await _ttLock.QueryLockSettingAsync(lockId, type);

    public async Task LockDeviceAsync(long lockId) => await _ttLock.LockAsync(lockId);
    public async Task UnlockDeviceAsync(long lockId) => await _ttLock.UnlockAsync(lockId);
}
