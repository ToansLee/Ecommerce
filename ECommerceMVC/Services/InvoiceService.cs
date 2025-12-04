using ECommerceMVC.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;

namespace ECommerceMVC.Services
{
    public class InvoiceService
    {
        // Tạo font hỗ trợ tiếng Việt
        private static BaseFont GetVietnameseFont()
        {
            // Sử dụng font Arial Unicode MS hoặc font mặc định hỗ trợ Unicode
            string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            if (!File.Exists(fontPath))
            {
                fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "times.ttf");
            }
            
            try
            {
                return BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            }
            catch
            {
                // Fallback to default font
                return BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            }
        }

        public static byte[] GenerateInvoice(Order order)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Tạo document PDF
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Setup fonts
                BaseFont baseFont = GetVietnameseFont();
                Font titleFont = new Font(baseFont, 20, Font.BOLD, new BaseColor(255, 149, 0));
                Font headerFont = new Font(baseFont, 14, Font.BOLD, BaseColor.Black);
                Font normalFont = new Font(baseFont, 11, Font.NORMAL, BaseColor.Black);
                Font boldFont = new Font(baseFont, 11, Font.BOLD, BaseColor.Black);
                Font smallFont = new Font(baseFont, 9, Font.NORMAL, BaseColor.DarkGray);

                // Header - Logo và Thông tin công ty
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 1, 1 });

                // Left side - Company info
                PdfPCell leftCell = new PdfPCell();
                leftCell.Border = Rectangle.NO_BORDER;
                Paragraph companyName = new Paragraph("FOOD ORDERING", titleFont);
                Paragraph companyInfo = new Paragraph("Hệ thống đặt món trực tuyến\nĐịa chỉ: 218 Lĩnh Nam, Hoàng Mai, Hà Nội\nHotline: 09 6269 8288", smallFont);
                leftCell.AddElement(companyName);
                leftCell.AddElement(companyInfo);
                headerTable.AddCell(leftCell);

                // Right side - Invoice info
                PdfPCell rightCell = new PdfPCell();
                rightCell.Border = Rectangle.NO_BORDER;
                rightCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                Paragraph invoiceTitle = new Paragraph("HÓA ĐƠN", headerFont);
                invoiceTitle.Alignment = Element.ALIGN_RIGHT;
                Paragraph invoiceNumber = new Paragraph($"Mã đơn: #{order.Id}", normalFont);
                invoiceNumber.Alignment = Element.ALIGN_RIGHT;
                // Convert to Vietnam timezone (GMT+7)
                TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedAt.ToUniversalTime(), vietnamTimeZone);
                Paragraph invoiceDate = new Paragraph($"Ngày: {vietnamTime:dd/MM/yyyy HH:mm} (GMT+7)", normalFont);
                invoiceDate.Alignment = Element.ALIGN_RIGHT;
                rightCell.AddElement(invoiceTitle);
                rightCell.AddElement(invoiceNumber);
                rightCell.AddElement(invoiceDate);
                headerTable.AddCell(rightCell);

                document.Add(headerTable);
                document.Add(new Paragraph(" "));
                // Add a horizontal line
                PdfPTable lineTable = new PdfPTable(1);
                lineTable.WidthPercentage = 100;
                PdfPCell lineCell = new PdfPCell();
                lineCell.BorderWidthTop = 2;
                lineCell.BorderWidthLeft = 0;
                lineCell.BorderWidthRight = 0;
                lineCell.BorderWidthBottom = 0;
                lineCell.BorderColorTop = BaseColor.LightGray;
                lineCell.FixedHeight = 2;
                lineTable.AddCell(lineCell);
                document.Add(lineTable);
                document.Add(new Paragraph(" "));

                // Thông tin khách hàng
                Paragraph customerTitle = new Paragraph("THÔNG TIN KHÁCH HÀNG", headerFont);
                document.Add(customerTitle);
                document.Add(new Paragraph(" "));

                PdfPTable customerTable = new PdfPTable(2);
                customerTable.WidthPercentage = 100;
                customerTable.SetWidths(new float[] { 1, 2 });

                AddCustomerRow(customerTable, "Tên khách hàng:", order.Customer?.FullName ?? "N/A", boldFont, normalFont);
                AddCustomerRow(customerTable, "Email:", order.Customer?.Email ?? "N/A", boldFont, normalFont);
                AddCustomerRow(customerTable, "Số điện thoại:", order.Customer?.Phone ?? "N/A", boldFont, normalFont);
                
                // Địa chỉ từ Customer
                string customerAddress = order.Customer?.Address ?? "N/A";
                AddCustomerRow(customerTable, "Địa chỉ:", customerAddress, boldFont, normalFont);

                document.Add(customerTable);
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));

                // Chi tiết đơn hàng
                Paragraph orderTitle = new Paragraph("CHI TIẾT ĐƠN HÀNG", headerFont);
                document.Add(orderTitle);
                document.Add(new Paragraph(" "));

                // Bảng sản phẩm
                PdfPTable itemsTable = new PdfPTable(5);
                itemsTable.WidthPercentage = 100;
                itemsTable.SetWidths(new float[] { 1, 3, 2, 2, 2 });

                // Header
                AddTableHeader(itemsTable, "STT", boldFont);
                AddTableHeader(itemsTable, "Tên món", boldFont);
                AddTableHeader(itemsTable, "Đơn giá", boldFont);
                AddTableHeader(itemsTable, "Số lượng", boldFont);
                AddTableHeader(itemsTable, "Thành tiền", boldFont);

                // Items
                int index = 1;
                decimal subtotal = 0;
                foreach (var item in order.Items)
                {
                    AddTableCell(itemsTable, index.ToString(), normalFont, Element.ALIGN_CENTER);
                    AddTableCell(itemsTable, item.MenuItem?.Name ?? "N/A", normalFont, Element.ALIGN_LEFT);
                    AddTableCell(itemsTable, $"{item.UnitPrice:N0}đ", normalFont, Element.ALIGN_RIGHT);
                    AddTableCell(itemsTable, item.Quantity.ToString(), normalFont, Element.ALIGN_CENTER);
                    
                    decimal itemTotal = item.Quantity * item.UnitPrice;
                    subtotal += itemTotal;
                    AddTableCell(itemsTable, $"{itemTotal:N0}đ", normalFont, Element.ALIGN_RIGHT);
                    
                    index++;
                }

                document.Add(itemsTable);
                document.Add(new Paragraph(" "));

                // Tổng cộng
                PdfPTable totalTable = new PdfPTable(2);
                totalTable.WidthPercentage = 100;
                totalTable.SetWidths(new float[] { 3, 1 });

                // Tính phí vận chuyển
                decimal shippingFee = 35000;
                if (subtotal >= 500000)
                {
                    shippingFee = 0;
                }
                decimal productTotal = subtotal;
                
                AddTotalRow(totalTable, "Tạm tính:", $"{(double)productTotal:N0}đ", normalFont, normalFont);
                AddTotalRow(totalTable, "Phí vận chuyển:", $"{(double)shippingFee:N0}đ", normalFont, normalFont);
                Font totalPriceFont = new Font(baseFont, 14, Font.BOLD, new BaseColor(255, 149, 0));
                AddTotalRow(totalTable, "TỔNG CỘNG:", $"{order.TotalAmount:N0}đ", boldFont, totalPriceFont);

                document.Add(totalTable);
                document.Add(new Paragraph(" "));

                // Ghi chú
                if (!string.IsNullOrEmpty(order.Notes))
                {
                    document.Add(new Paragraph(" "));
                    Paragraph notesPara = new Paragraph($"Ghi chú: {order.Notes}", smallFont);
                    document.Add(notesPara);
                }

                // Phương thức thanh toán
                document.Add(new Paragraph(" "));
                string paymentMethodText = "Thanh toán khi nhận hàng (COD)";
                
                if (order.Payment != null)
                {
                    if (order.Payment.Method == "VNPay")
                    {
                        paymentMethodText = $"Thanh toán qua VNPay - Đã thanh toán {order.Payment.Amount:N0}đ";
                        if (!string.IsNullOrEmpty(order.Payment.TransactionId))
                        {
                            paymentMethodText += $"\nMã giao dịch: {order.Payment.TransactionId}";
                        }
                    }
                    else if (order.Payment.Method == "Wallet")
                    {
                        paymentMethodText = "Đã thanh toán bằng ví điện tử";
                    }
                    else if (order.Payment.Method == "COD")
                    {
                        paymentMethodText = "Thanh toán khi nhận hàng (COD)";
                    }
                }
                
                Paragraph paymentPara = new Paragraph($"Phương thức thanh toán: {paymentMethodText}", normalFont);
                document.Add(paymentPara);

                // Footer
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));
                // Add footer line
                PdfPTable footerLineTable = new PdfPTable(1);
                footerLineTable.WidthPercentage = 100;
                PdfPCell footerLineCell = new PdfPCell();
                footerLineCell.BorderWidthTop = 2;
                footerLineCell.BorderWidthLeft = 0;
                footerLineCell.BorderWidthRight = 0;
                footerLineCell.BorderWidthBottom = 0;
                footerLineCell.BorderColorTop = BaseColor.LightGray;
                footerLineCell.FixedHeight = 2;
                footerLineTable.AddCell(footerLineCell);
                document.Add(footerLineTable);
                Paragraph footer = new Paragraph("Cảm ơn quý khách đã sử dụng dịch vụ của chúng tôi!", smallFont);
                footer.Alignment = Element.ALIGN_CENTER;
                document.Add(footer);

                document.Close();
                return ms.ToArray();
            }
        }

        private static void AddCustomerRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont));
            labelCell.Border = Rectangle.NO_BORDER;
            labelCell.PaddingBottom = 5;
            table.AddCell(labelCell);

            PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont));
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.PaddingBottom = 5;
            table.AddCell(valueCell);
        }

        private static void AddTableHeader(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = new BaseColor(255, 245, 230); // #FFF5E6
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 8;
            cell.BorderColor = new BaseColor(255, 229, 204); // #FFE5CC
            table.AddCell(cell);
        }

        private static void AddTableCell(PdfPTable table, string text, Font font, int alignment)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = alignment;
            cell.Padding = 8;
            cell.BorderColor = new BaseColor(230, 230, 230);
            table.AddCell(cell);
        }

        private static void AddTotalRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont));
            labelCell.Border = Rectangle.NO_BORDER;
            labelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            labelCell.PaddingTop = 5;
            labelCell.PaddingBottom = 5;
            table.AddCell(labelCell);

            PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont));
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            valueCell.PaddingTop = 5;
            valueCell.PaddingBottom = 5;
            table.AddCell(valueCell);
        }
    }
}
