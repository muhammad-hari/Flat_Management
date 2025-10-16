using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.Entities;

public enum BarcodeType
{
    [Display(Name = "QR Code")]
    QRCode,

    [Display(Name = "Barcode (Code 128)")]
    Barcode128
}
