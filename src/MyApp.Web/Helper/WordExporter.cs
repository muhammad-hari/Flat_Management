using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MyApp.Core.Entities;

namespace MyApp.Web.Helper
{
    public static class WordExporter
    {
        public static async Task<byte[]> ExportBarcodesToWordAsync(List<Inventory> inventories)
        {
            using var ms = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());

                var body = mainPart.Document.Body;

                // Tambahkan judul
                body.Append(new Paragraph(new Run(new Text("Inventory Barcodes Report")))
                {
                    ParagraphProperties = new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Center })
                });
                body.Append(new Paragraph(new Run(new Text(" "))));

                // Buat tabel
                var table = new Table();

                // Style tabel
                TableProperties props = new TableProperties(
                    new TableBorders(
                        new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                    )
                );
                table.AppendChild(props);

                // Header
                var header = new TableRow();
                header.Append(CreateCell("Name"));
                header.Append(CreateCell("Code"));
                header.Append(CreateCell("Type"));
                header.Append(CreateCell("Repository"));
                header.Append(CreateCell("Barcode"));
                table.Append(header);

                // Isi tabel
                foreach (var inv in inventories)
                {
                    var row = new TableRow();
                    row.Append(CreateCell(inv.Name ?? "-"));
                    row.Append(CreateCell(inv.Code ?? "-"));
                    row.Append(CreateCell(inv.InventoryType?.TypeName ?? "-"));
                    row.Append(CreateCell(inv.Repository?.Name ?? "-"));

                    var barcodeCell = new TableCell();
                    if (!string.IsNullOrEmpty(inv.GeneratedBarcodeValue))
                    {
                        var base64 = inv.GeneratedBarcodeValue.Replace("data:image/png;base64,", "");
                        var bytes = Convert.FromBase64String(base64);
                        AddImageToCell(wordDoc, barcodeCell, bytes, $"{inv.Code}.png");
                    }
                    else
                    {
                        barcodeCell.Append(new Paragraph(new Run(new Text("-"))));
                    }

                    row.Append(barcodeCell);
                    table.Append(row);
                }

                body.Append(table);
                mainPart.Document.Save();
            }

            return ms.ToArray();
        }

        private static TableCell CreateCell(string text)
        {
            return new TableCell(new Paragraph(new Run(new Text(text))));
        }

        private static void AddImageToCell(WordprocessingDocument doc, TableCell cell, byte[] imageBytes, string name)
        {
            var mainPart = doc.MainDocumentPart;
            var imagePart = mainPart.AddImagePart(ImagePartType.Png);

            using (var stream = new MemoryStream(imageBytes))
            {
                imagePart.FeedData(stream);
            }

            var imageId = mainPart.GetIdOfPart(imagePart);

            var element = new Drawing(
                new DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline(
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent() { Cx = 2500000L, Cy = 1000000L }, // ukuran
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties()
                    {
                        Id = (UInt32Value)1U,
                        Name = name
                    },
                    new DocumentFormat.OpenXml.Drawing.Graphic(
                        new DocumentFormat.OpenXml.Drawing.GraphicData(
                            new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
                                new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties(
                                    new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties()
                                    {
                                        Id = (UInt32Value)0U,
                                        Name = name
                                    },
                                    new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()
                                ),
                                new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                                    new DocumentFormat.OpenXml.Drawing.Blip() { Embed = imageId },
                                    new DocumentFormat.OpenXml.Drawing.Stretch(new DocumentFormat.OpenXml.Drawing.FillRectangle())
                                ),
                                new DocumentFormat.OpenXml.Drawing.Pictures.ShapeProperties(
                                    new DocumentFormat.OpenXml.Drawing.Transform2D(
                                        new DocumentFormat.OpenXml.Drawing.Offset() { X = 0L, Y = 0L },
                                        new DocumentFormat.OpenXml.Drawing.Extents() { Cx = 2500000L, Cy = 1000000L }
                                    ),
                                    new DocumentFormat.OpenXml.Drawing.PresetGeometry(new DocumentFormat.OpenXml.Drawing.AdjustValueList())
                                    { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle }
                                )
                            )
                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                    )
                )
            );

            cell.Append(new Paragraph(new Run(element)));
        }
    }
}
