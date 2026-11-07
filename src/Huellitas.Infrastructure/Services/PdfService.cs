using Huellitas.Infrastructure.Interfaces.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Huellitas.Infrastructure.Services;

public class PdfService : IPdfService
{
    public PdfService()
    {
        // Licencia gratuita de QuestPDF para proyectos open source / académicos
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseSystemFonts = true;
    }

    public byte[] GenerarConstanciaAdopcion(
        string adoptanteNombre,
        string adoptanteCorreo,
        string mascotaNombre,
        string refugioNombre,
        string codigoConstancia,
        DateTime fechaFinalizacion)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));

                // Encabezado
                page.Header().Element(ComposeHeader);

                // Contenido
                page.Content().Element(container => ComposeContent(container, adoptanteNombre, adoptanteCorreo, mascotaNombre, refugioNombre, codigoConstancia, fechaFinalizacion));

                // Pie de página
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("HUELLITAS")
                    .FontSize(24).Bold().FontColor(Colors.Green.Darken2);
                column.Item().Text("Plataforma de Adopción y Trazabilidad Animal")
                    .FontSize(10).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private void ComposeContent(IContainer container, string adoptante, string correo, string mascota, string refugio, string codigo, DateTime fecha)
    {
        container.PaddingVertical(40).Column(column =>
        {
            column.Spacing(20);

            column.Item().Text("CONSTANCIA DE ADOPCIÓN")
                .FontSize(20).Bold().AlignCenter();

            column.Item().PaddingTop(20).Text(text =>
            {
                text.Span("Por medio de la presente, la plataforma Huellitas certifica que el(la) señor(a) ");
                text.Span(adoptante).Bold();
                text.Span(" (");
                text.Span(correo);
                text.Span(") ha adoptado de manera responsable a la mascota ");
                text.Span(mascota).Bold();
                text.Span(", perteneciente al refugio ");
                text.Span(refugio).Bold();
                text.Span(".");
            });

            column.Item().PaddingTop(20).Text(text =>
            {
                text.Span("Fecha de finalización: ");
                text.Span(fecha.ToString("dd/MM/yyyy HH:mm")).Bold();
            });

            column.Item().PaddingTop(40).Background(Colors.Grey.Lighten4).Padding(15).Column(c =>
            {
                c.Item().Text("CÓDIGO ÚNICO DE VERIFICACIÓN").FontSize(10).FontColor(Colors.Grey.Darken1);
                c.Item().Text(codigo).FontSize(18).Bold().FontColor(Colors.Green.Darken3);
            });

            column.Item().PaddingTop(60).Text("Este documento es un comprobante oficial generado automáticamente por el sistema Huellitas y sirve como constancia del compromiso de tenencia responsable adquirido por el adoptante.")
                .FontSize(9).FontColor(Colors.Grey.Medium).Italic();
        });
    }
}