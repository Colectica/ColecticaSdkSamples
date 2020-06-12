using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Utility;
using Colectica.Reporting;
using ColecticaAddinSamples.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColecticaSdkSamples.SampleTasks
{
    public class DisseminationTasks
    {
        public static async Task MainAsync()
        {
            VersionableBase.DefaultAgencyId = "int.example";

            var documentedSAS = ProcessingTasks.ImportSASDataset("nyts2014_dataset.sas7bdat", "nyts2014_formats.sas7bcat");

            // Test ID 002, 003, 004, 005
            // Render PDF and HTML
            CreateHtmlAndPdf(documentedSAS);
        }

        private static void CreateHtmlAndPdf(PhysicalInstance dataset)
        {
            string resourcePath = Path.Combine("Resources", "icons", "types");
            string baseFileName = "codebook";
            string addinsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Algenta",
                "Colectica",
                "Addins");

            // Set some options for the report.
            var options = new ReportOptions();
            options.IsAccessibilityMode = false;

            var builder = new ReportBuilder();

            // Use the custom codebook format.
            builder.Builders = new List<IDocumentationBuilder>();
            builder.Builders.Add(new SampleCustomCodebookBuilder());

            // Create English PDF.
            MultilingualString.CurrentCulture = "en";
            byte[] englishPdfBytes = builder.CreatePdf(dataset, resourcePath, addinsPath, options, null, null);
            string englishPdfFileName = $"{baseFileName}_en.pdf";
            File.WriteAllBytes(englishPdfFileName, englishPdfBytes);

            // Create English HTML.
            string englishHtml = builder.CreateHtml(dataset, resourcePath, addinsPath, options, null, null);
            string englishHtmlFileName = $"{baseFileName}_en.html";
            File.WriteAllText(englishHtmlFileName, englishHtml);

            // Create French PDF.
            MultilingualString.CurrentCulture = "fr";
            byte[] frenchPdfBytes = builder.CreatePdf(dataset, resourcePath, addinsPath, options, null, null);
            string frenchPdfFileName = $"{baseFileName}_fr.pdf";
            File.WriteAllBytes(frenchPdfFileName, frenchPdfBytes);

            // Create French HTML.
            string frenchHtml = builder.CreateHtml(dataset, resourcePath, addinsPath, options, null, null);
            string frenchHtmlFileName = $"{baseFileName}_fr.html";
            File.WriteAllText(frenchHtmlFileName, frenchHtml);
        }
    }
}
