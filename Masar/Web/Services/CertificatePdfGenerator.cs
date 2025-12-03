using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Web.ViewModels;

namespace Web.Services;

public class CertificatePdfGenerator
{
    public byte[] GenerateCertificatePdf(CertificateVerificationViewModel model)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(0); // Remove all margins - FULL PAGE
                page.PageColor(Colors.White);

                page.Content().Padding(20).Column(column => // Small padding from edges
                {
                    column.Spacing(3);

                    // Decorative double border - takes full page
                    column.Item().Border(5).BorderColor("#7c3aed").Padding(3)
                        .Border(2).BorderColor("#06b6d4").Padding(20).Column(innerColumn =>
                        {
                            innerColumn.Spacing(4);

                            // ==================== HEADER ====================
                            innerColumn.Item().Row(headerRow =>
                            {
                                headerRow.RelativeItem(2).Column(logoCol =>
                                {
                                    logoCol.Item().Width(40).Height(40).Background("#7c3aed")
                                        .Padding(7).AlignCenter().AlignMiddle()
                                        .Text("M").FontSize(20).FontColor(Colors.White).Bold();
                                });

                                headerRow.RelativeItem(8).AlignCenter().AlignMiddle().Column(titleCol =>
                                {
                                    titleCol.Item().Text("Masar E-Learning Platform")
                                        .FontSize(18)
                                        .FontColor("#7c3aed")
                                        .Bold();
                                    titleCol.Item().Text("Certificate of Achievement")
                                        .FontSize(10)
                                        .FontColor("#64748b")
                                        .Italic();
                                });

                                headerRow.RelativeItem(2);
                            });

                            // Decorative line
                            innerColumn.Item().PaddingVertical(4).LineHorizontal(2).LineColor("#a78bfa");

                            // ==================== CERTIFICATE TITLE ====================
                            innerColumn.Item().PaddingTop(8).AlignCenter().Column(titleBlock =>
                            {
                                titleBlock.Item().Text("CERTIFICATE OF COMPLETION")
                                    .FontSize(26)
                                    .FontColor("#0f172a")
                                    .Bold()
                                    .LetterSpacing(0);

                                titleBlock.Item().PaddingTop(3).Width(150).Height(3).Background("#10b981");
                            });

                            // ==================== CERTIFICATE BODY ====================
                            innerColumn.Item().PaddingTop(10).AlignCenter().Column(bodyCol =>
                            {
                                bodyCol.Spacing(4);

                                bodyCol.Item().Text("This is to certify that")
                                    .FontSize(13)
                                    .FontColor("#64748b")
                                    .Italic();

                                // Student Name with underline
                                bodyCol.Item().PaddingTop(5).Column(nameCol =>
                                {
                                    nameCol.Item().AlignCenter().Text(model.StudentName)
                                        .FontSize(28)
                                        .FontColor("#7c3aed")
                                        .Bold();
                                    nameCol.Item().PaddingTop(3).PaddingHorizontal(40)
                                        .LineHorizontal(2).LineColor("#cbd5e1");
                                });

                                bodyCol.Item().PaddingTop(7).Text("has successfully completed the course")
                                    .FontSize(13)
                                    .FontColor("#64748b");

                                // Course Name in box
                                bodyCol.Item().PaddingTop(6).PaddingHorizontal(50)
                                    .Background("#f8fafc").Padding(10)
                                    .Border(2).BorderColor("#e2e8f0").AlignCenter().Text(model.CourseName)
                                    .FontSize(18)
                                    .FontColor("#0f172a")
                                    .Bold();

                                // Instructor
                                if (!string.IsNullOrEmpty(model.InstructorName))
                                {
                                    bodyCol.Item().PaddingTop(6).AlignCenter().Text(text =>
                                    {
                                        text.Span("Instructed by: ")
                                            .FontSize(11)
                                            .FontColor("#64748b");
                                        text.Span(model.InstructorName)
                                            .FontSize(11)
                                            .FontColor("#7c3aed")
                                            .Bold();
                                    });
                                }

                                bodyCol.Item().PaddingTop(4).Text($"on this {GetDayWithSuffix(model.IssuedDate)} day")
                                    .FontSize(11)
                                    .FontColor("#64748b")
                                    .Italic();
                            });

                            // ==================== FOOTER SECTION ====================
                            innerColumn.Item().PaddingTop(12).Row(footerRow =>
                            {
                                // Date column
                                footerRow.RelativeItem().Column(dateCol =>
                                {
                                    dateCol.Item().AlignCenter().Text("Date of Issue")
                                        .FontSize(9)
                                        .FontColor("#64748b")
                                        .Bold();
                                    dateCol.Item().PaddingTop(4).PaddingHorizontal(10)
                                        .LineHorizontal(2).LineColor("#7c3aed");
                                    dateCol.Item().PaddingTop(3).AlignCenter().Text(model.IssuedDate)
                                        .FontSize(10)
                                        .FontColor("#0f172a")
                                        .Bold();
                                });

                                footerRow.Spacing(20);

                                // Signature column
                                footerRow.RelativeItem().Column(signCol =>
                                {
                                    signCol.Item().AlignCenter().Text("Authorized Signature")
                                        .FontSize(9)
                                        .FontColor("#64748b")
                                        .Bold();
                                    signCol.Item().PaddingTop(4).PaddingHorizontal(10)
                                        .LineHorizontal(2).LineColor("#10b981");
                                    signCol.Item().PaddingTop(3).AlignCenter().Text("Masar Certification Board")
                                        .FontSize(9)
                                        .FontColor("#0f172a");
                                });

                                footerRow.Spacing(20);

                                // Certificate ID column
                                footerRow.RelativeItem().Column(idCol =>
                                {
                                    idCol.Item().AlignCenter().Text("Certificate ID")
                                        .FontSize(9)
                                        .FontColor("#64748b")
                                        .Bold();
                                    idCol.Item().PaddingTop(4).PaddingHorizontal(10)
                                        .LineHorizontal(2).LineColor("#06b6d4");
                                    idCol.Item().PaddingTop(3).AlignCenter().Text(model.VerificationId)
                                        .FontSize(10)
                                        .FontColor("#0f172a")
                                        .Bold();
                                });
                            });

                            // ==================== VERIFICATION INFO ====================
                            innerColumn.Item().PaddingTop(10).AlignCenter().Column(verifyCol =>
                            {
                                verifyCol.Item().Background("#ecfdf5").Padding(5).Row(verifyRow =>
                                {
                                    verifyRow.RelativeItem().AlignCenter().Text("✓")
                                        .FontSize(12)
                                        .FontColor("#10b981")
                                        .Bold();
                                    verifyRow.RelativeItem(8).AlignCenter()
                                        .Text("Digitally Verified and Authenticated")
                                        .FontSize(9)
                                        .FontColor("#059669")
                                        .Bold();
                                });

                                verifyCol.Item().PaddingTop(2).AlignCenter()
                                    .Text($"Verify at: masar.com/verify-certificate/{model.VerificationId}")
                                    .FontSize(7)
                                    .FontColor("#64748b");
                            });
                        });
                });
            });
        });

        return document.GeneratePdf();
    }

    private string GetDayWithSuffix(string dateString)
    {
        if (DateTime.TryParse(dateString, out DateTime date))
        {
            int day = date.Day;
            string suffix = day switch
            {
                1 or 21 or 31 => "st",
                2 or 22 => "nd",
                3 or 23 => "rd",
                _ => "th"
            };
            return $"{day}{suffix} of {date:MMMM yyyy}";
        }
        return dateString;
    }
}