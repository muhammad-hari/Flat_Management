public class DeviceDto
{
    public string LockAlias { get; set; } = "";
    public long LockId { get; set; }
    public int ElectricQuantity { get; set; }
    public string NoKeyPwd { get; set; } = "";
    
}

// DTO untuk IC card
public class ICCardDto
{
    public long CardId { get; set; }
    public long LockId { get; set; }
    public string CardNumber { get; set; } = "";
    public string CardName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreateDate { get; set; }
    public string SenderUsername { get; set; } = "";
    public int CardType { get; set; }
}

public class FingerprintDto
{
    public long FingerprintId { get; set; }       // ID fingerprint
    public long LockId { get; set; }              // ID lock
    public string FingerprintName { get; set; }   // Nama fingerprint
    public string FingerprintNumber { get; set; } // Nomor fingerprint
    public DateTime StartDate { get; set; }       // Mulai berlaku
    public DateTime EndDate { get; set; }         // Berakhir
}
