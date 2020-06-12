using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Utility;
using Colectica.Reporting;
using ColecticaAddinSamples.Utility;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColecticaAddinSamples.Reports
{
    [Export(typeof(IDocumentationBuilder))]
    public class SampleCustomCodebookBuilder : IDocumentationBuilder
    {
        private Section tocSection;

        public string Name => "Sample Custom Codebook";

        public Guid ItemType => DdiItemType.PhysicalInstance;

        public object Options
        {
            get => null;
            set { }
        }

        public ItemBuilderBase Helper { get; protected set; }

        public void AddContent(IVersionable item, Document document, ReportContext context)
        {
            Helper = new ItemBuilderBase(document, context);
            InitializeStyles(document);

            // Add the cover, table of contents, and set up the headers and footers.
            string title = item.GetHeader();

            document.Info.Title = string.Format("{0} data dictionary (version {1})", title, item.Version);

            var coverSection = AddCover(title, context.AddinsPath, document);
            Helper.IncreaseHeaderLevel();

            this.tocSection = AddTocPlaceholder(document);
            AddHeaderAndFooter(tocSection);

            // Main content, filled in by a derived class.
            var mainSection = document.AddSection();
            Helper.AddHeader(title);
            Helper.IncreaseHeaderLevel();

            var datasetModel = GetDatasetModelFromPhysicalInstance(item as PhysicalInstance);
            AddVariableTables(datasetModel.Yield().ToList(), document);

            Helper.DecreaseHeaderLevel();

            // Fill in the table of contents.
            AddTableOfContents(document, tocSection, coverSection);

            Helper.DecreaseHeaderLevel();
        }

        public void InitializeOptions(IVersionable item)
        {

        }

        public static void InitializeStyles(Document document)
        {
            document.DefaultPageSetup.PageFormat = PageFormat.A4;
            //document.LastSection.PageSetup.PageFormat = PageFormat.A4;

            Style normal = document.Styles["Normal"];
            normal.Font.Name = "Arial";
            normal.Font.Size = Unit.FromPoint(8);
            //normal.ParagraphFormat.LineSpacing = Unit.FromPoint();

            Style h1 = document.Styles["Heading1"];
            h1.Font.Name = "Arial";
            h1.Font.Size = Unit.FromPoint(26);
            h1.Font.Bold = false;

            Style h2 = document.Styles["Heading2"];
            h2.Font.Name = "Arial";
            h2.Font.Size = Unit.FromPoint(24);
            h2.Font.Bold = false;
            h2.ParagraphFormat.SpaceBefore = 0;
            h2.ParagraphFormat.KeepWithNext = true;

            Style h3 = document.Styles["Heading3"];
            h3.Font.Name = "Arial";
            h3.Font.Size = Unit.FromPoint(16);
            h3.Font.Bold = false;
            h3.ParagraphFormat.SpaceBefore = 20;
            h3.ParagraphFormat.KeepWithNext = true;

            Style h4 = document.Styles["Heading4"];
            h4.Font.Name = "Arial";
            h4.Font.Size = Unit.FromPoint(12);
            h4.Font.Bold = false;
            h4.ParagraphFormat.SpaceBefore = 10;
            h4.ParagraphFormat.KeepWithNext = true;

            Style h5 = document.Styles["Heading5"];
            h5.Font.Name = "Arial";
            h5.Font.Size = Unit.FromPoint(10);
            h5.Font.Bold = true;
            h5.Font.Italic = false;
            h5.ParagraphFormat.KeepWithNext = true;

            Style h6 = document.Styles["Heading6"];
            h6.Font.Name = "Arial";
            h6.Font.Size = Unit.FromPoint(10);
            h6.Font.Bold = false;
            h6.Font.Italic = true;
            h6.ParagraphFormat.KeepWithNext = true;

            Style h7 = document.Styles["Heading7"];
            h7.Font.Name = "Arial";
            h7.Font.Size = Unit.FromPoint(8);
            h7.Font.Bold = false;
            h7.Font.Italic = true;
            h7.ParagraphFormat.KeepWithNext = true;

            Style list = document.Styles["List"];
            list.ParagraphFormat.AddTabStop(Unit.FromCentimeter(.25));
        }

        public static Section AddCover(string title, string addinsPath, Document document)
        {
            // Add the cover with image and title.
            var coverSection = document.AddSection();

            coverSection.PageSetup.LeftMargin = 0;
            coverSection.PageSetup.TopMargin = 0;
            coverSection.PageSetup.RightMargin = 0;
            coverSection.PageSetup.BottomMargin = 0;
            coverSection.PageSetup.DifferentFirstPageHeaderFooter = true;

            coverSection.AddParagraph();

            string imagePath = System.IO.Path.Combine(addinsPath, "cover-image.png");

            var image = coverSection.AddImage(imagePath);
            image.RelativeVertical = RelativeVertical.Page;
            image.RelativeHorizontal = RelativeHorizontal.Page;
            image.WrapFormat.Style = WrapStyle.Through;
            image.Left = 0;
            image.Top = 0;

            var titleParagraph = coverSection.AddParagraph(title, "Heading1");
            titleParagraph.Format.LeftIndent = Unit.FromCentimeter(5);
            titleParagraph.Format.Borders.DistanceFromTop = Unit.FromCentimeter(7.35);

            return coverSection;
        }

        public static Section AddTocPlaceholder(Document document)
        {
            // Add table of contents.
            var tocSection = document.AddSection();
            var tocHeader = tocSection.AddParagraph("Table of Contents", "Heading2");

            tocSection.PageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            tocSection.PageSetup.TopMargin = Unit.FromCentimeter(2.5);
            tocSection.PageSetup.RightMargin = Unit.FromCentimeter(2.5);
            tocSection.PageSetup.BottomMargin = Unit.FromCentimeter(2.5);
            tocSection.PageSetup.DifferentFirstPageHeaderFooter = false;

            return tocSection;
        }

        public static void AddHeaderAndFooter(Section section)
        {
            string footerText = string.Format("Published by SampleAgency on {0}",
                DateTime.Now.ToString("D"));

            // Header.
            var header = section.Headers.Primary.AddParagraph();
            header.Format.Borders.Bottom.Width = 1;
            header.Format.Alignment = ParagraphAlignment.Right;
            header.AddInfoField(InfoFieldType.Title);

            // Footer.
            var footer = section.Footers.Primary.AddParagraph();
            footer.Format.Borders.Top.Width = 1;
            footer.Format.Alignment = ParagraphAlignment.Right;
            footer.AddPageField();
            footer.AddSpace(1);
            footer.AddText("of");
            footer.AddSpace(1);
            footer.AddNumPagesField();

            section.Footers.Primary.AddParagraph(footerText);
        }

        public static void AddTableOfContents(Document document, Section tocSection, Section coverSection)
        {
            OutlineLevel[] okLevels = new[] { OutlineLevel.Level1, OutlineLevel.Level2, OutlineLevel.Level3, OutlineLevel.Level4 };

            foreach (Section section in document.Sections)
            {
                if (section == coverSection ||
                    section == tocSection)
                {
                    continue;
                }

                foreach (var paragraph in section.Elements.OfType<Paragraph>().ToList())
                {
                    var style = document.Styles[paragraph.Style];
                    if (style != null &&
                        okLevels.Contains(style.ParagraphFormat.OutlineLevel))
                    {
                        ProcessTocParagraph(document, tocSection, paragraph);
                    }
                }
            }
        }

        static void ProcessTocParagraph(Document document, Section tocSection, Paragraph paragraph)
        {
            if (paragraph.Tag == null) return;

            Paragraph tocParagraph = tocSection.AddParagraph();
            tocParagraph.Style = "TOC";

            var style = document.Styles[paragraph.Style];
            int level = 0;
            switch (style.ParagraphFormat.OutlineLevel)
            {
                case OutlineLevel.Level1: level = 1; break;
                case OutlineLevel.Level2: level = 2; break;
                case OutlineLevel.Level3: level = 3; break;
                case OutlineLevel.Level4: level = 4; break;
            }
            tocParagraph.Format.LeftIndent = level * 10;

            string text = GetTextForParagraph(paragraph);

            string bookmark = paragraph.Tag.ToString();
            Hyperlink hyperlink = tocParagraph.AddHyperlink(bookmark);

            //hyperlink.AddText(text + "\t");
            var formattedText = hyperlink.AddFormattedText(text, TextFormat.Underline);
            formattedText.Font.Color = Colors.Blue;
            hyperlink.AddTab();

            hyperlink.AddPageRefField(bookmark);
        }

        static string GetTextForParagraph(Paragraph paragraph)
        {
            var textElement = paragraph.Elements.OfType<Text>().FirstOrDefault();
            return textElement == null ? string.Empty : textElement.Content;
        }

        public static SampleDatasetModel GetDatasetModelFromPhysicalInstance(PhysicalInstance physicalInstance)
        {
            var variables = physicalInstance.DataRelationships
                    .SelectMany(x => x.LogicalRecords)
                    .SelectMany(x => x.VariablesInRecord);

            // Try the DDI 3.1 structure, too.
            if (variables.Count() == 0)
            {
                variables = physicalInstance.RecordLayouts
                    .SelectMany(x => x.DataItems)
                    .Where(x => x.Variable != null)
                    .Select(x => x.Variable);
            }

            var datasetModel = new SampleDatasetModel();
            datasetModel.Title = physicalInstance.DisplayLabel;
            foreach (var variable in variables)
            {
                datasetModel.Variables.Add(variable);
            }

            return datasetModel;
        }

        public void AddVariableTables(List<SampleDatasetModel> datasets, Document document)
        {
            foreach (var dataset in datasets)
            {
                Helper.AddHeader(dataset.Title);

                if (dataset.Variables.Count() == 0)
                {
                    Helper.AddBodyText("The dataset contains no variables.");
                }
                else
                {
                    var table = document.LastSection.AddTable();
                    table.Borders.Width = 0;
                    table.Borders.Visible = false;

                    var nameColumn = table.AddColumn(Unit.FromCentimeter(4));

                    int nameColumnIndex = 0;
                    int descriptionColumnIndex = 1;
                    int rangeColumnIndex = 2;
                    int rangeColumnWidth = 3;
                    rangeColumnIndex = 1;
                    rangeColumnWidth = 6;

                    MigraDoc.DocumentObjectModel.Tables.Column descriptionColumn = null;
                    descriptionColumn = table.AddColumn(Unit.FromCentimeter(9));
                    var rangeColumn = table.AddColumn(Unit.FromCentimeter(rangeColumnWidth));

                    var headerRow = table.AddRow();
                    headerRow.HeadingFormat = true;
                    headerRow.Format.Font.Bold = true;
                    headerRow.Format.KeepWithNext = true;

                    headerRow.Cells[nameColumnIndex].AddParagraph("Name");
                    headerRow.Cells[descriptionColumnIndex].AddParagraph("Description");
                    headerRow.Cells[rangeColumnIndex].AddParagraph("Range");

                    int i = 1;

                    foreach (var variable in dataset.Variables)
                    {
                        var row = table.AddRow();

                        // Name cell.
                        string name = variable.ItemName.Best;
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            name = variable.Label.Best;
                        }

                        // If there are no spaces, Migradoc will not perform wrapping.
                        // Insert some soft hyphens to help out.
                        if (!name.Contains(" "))
                        {
                            name = StringHelpers.AddWordBreaks(name);
                        }

                        row.Cells[0].AddParagraph(name);

                        // Description cell.
                        var par = row.Cells[descriptionColumnIndex].AddParagraph();

                        if (!variable.Label.IsEmpty)
                        {
                            par.AddFormattedText(variable.Label.Best, TextFormat.Bold);
                            par.AddLineBreak();
                        }

                        string truncatedDescription = StringHelpers.TruncateWords(variable.Description.Best, 400, "...");
                        //par.AddText(truncatedDescription);
                        Helper.TransformAndAddText(row.Cells[descriptionColumnIndex], truncatedDescription);

                        par = row.Cells[descriptionColumnIndex].AddParagraph();
                        par.AddLineBreak();

                        foreach (var question in variable.SourceQuestions)
                        {
                            par.AddFormattedText("Source Question: ", TextFormat.Italic);
                            par.AddText(question.ItemName.Best);
                            par.AddLineBreak();
                            par.AddText(question.QuestionText.Best);
                            par.AddLineBreak();
                        }

                        if (variable.Concept != null)
                        {
                            par.AddFormattedText("Concept: ", TextFormat.Italic);
                            par.AddText(variable.Concept.DisplayLabel);
                            par.AddLineBreak();
                        }

                        foreach (var universe in variable.Universes)
                        {
                            par.AddFormattedText("Universe: ", TextFormat.Italic);
                            par.AddText(variable.Label.Best);
                        }
                    

                        // Representation cell.
                        var representationParagraph = row.Cells[rangeColumnIndex].AddParagraph(GetRepresentationText(variable));

                        if (variable.CodeRepresentation?.Codes?.OtherMaterials.Count > 0)
                        {
                            representationParagraph.AddLineBreak();

                            var otherMaterial = variable.CodeRepresentation.Codes.OtherMaterials[0];

                            if (otherMaterial.UrlReference != null)
                            {
                                var link = representationParagraph.AddHyperlink(otherMaterial.UrlReference.ToString(), HyperlinkType.Web);
                                link.AddText(otherMaterial.DublinCoreMetadata.Title.Best);
                            }
                            else
                            {
                                representationParagraph.AddText(otherMaterial.DublinCoreMetadata.Title.Best);
                            }
                        }

                        i++;
                    }
                }
            }
        }

        public string GetRepresentationText(Variable variable)
        {
            if (variable == null)
            {
                return string.Empty;
            }

            if (variable.RepresentationType == RepresentationType.Text)
            {
                return "Text";
            }
            else if (variable.RepresentationType == RepresentationType.Numeric)
            {
                return "Numeric";
            }
            else if (variable.RepresentationType == RepresentationType.DateTime)
            {
                return "Date/Time";
            }
            else if (variable.RepresentationType == RepresentationType.Code)
            {
                if (variable.CodeRepresentation == null ||
                    variable.CodeRepresentation.Codes == null ||
                    variable.CodeRepresentation.Codes.Codes == null)
                {
                    return "Code";
                }

                StringBuilder builder = new StringBuilder();
                foreach (var code in variable.CodeRepresentation.Codes.Codes)
                {
                    if (code == null ||
                        code.Category == null ||
                        code.Category.Label == null)
                    {
                        continue;
                    }

                    string line = string.Format("{0} {1}", code.Value, code.Category.Label.Best);
                    builder.AppendLine(line);
                }

                if (variable.CodeRepresentation.Codes.Codes.Count > 20)
                {
                    builder.AppendLine(".\n.\n.");
                }

                return builder.ToString();
            }

            return string.Empty;
        }


    }

    public class SampleDatasetModel
    {
        public string Title { get; set; }

        public List<Variable> Variables { get; set; }

        public SampleDatasetModel()
        {
            Variables = new List<Variable>();
        }

    }

}
