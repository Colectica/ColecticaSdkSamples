using Algenta.Colectica.Commands.Import;
using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Serialization;
using Algenta.Colectica.Model.Ddi.Utility;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Repository.Client;
using Colectica.Reporting;
using Sasr.Data;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ColecticaSdkSamples.SampleTasks
{
    public class ProcessingTasks
    {
        public static async Task MainAsync()
        {
            VersionableBase.DefaultAgencyId = "int.example";

            // Processing Test ID 001, 002, and 003
            // Create processing metadata
            var processing = CreateProcessingMetatata();

            // Processing Test ID 001, 002, and 003
            // Register Items with Repository
            await RegisterItems(processing);


            // Test ID 008, 009, 010, 011
            // Document instance variables of data assets stemming from administrative data sources
            var documentedSAS = ImportSASDataset("nyts2014_dataset.sas7bdat", "nyts2014_formats.sas7bcat");

            // Test ID 009 and 010
            // Edit/Version Questionnaire Metadata
            // We will update the survey from test 001
            await ModifyAndVersionDataset(documentedSAS);

            // Test ID 022, 023
            // Ability to Assign an Administrative Status
            AssignAdministrativeStatus(documentedSAS);

            // Test ID 024
            // Sample permissions for use with states
            AssignPermissions(documentedSAS);

            // Test ID 025, 026, 027, 028
            // Render PDF and HTML
            CreateHtmlAndPdf(documentedSAS);
        }




        /// <summary>
        /// Processing Test ID 001, 002, and 003
        /// Register processing metadata
        /// </summary>
        /// <returns></returns>
        public static ProcessingEvent CreateProcessingMetatata()
        {
            // create a processing event
            var processing = new ProcessingEvent();
            processing.Label["en"] = "Sample processing";
            processing.Label["fr"] = "Traitement des échantillons";

            processing.OtherAppraisalProcess["en"] = "Describe the appraisal process";
            processing.OtherAppraisalProcess["fr"] = "Décrire le processus d'évaluation";

            // create a description of the organization doing the processing
            var statCan = new Organization();
            statCan.ItemName["en"] = "Statistics Canada";
            statCan.ItemName["fr"] = "Statistique Canada";

            // Describe a cleaning operation
            processing.CleaningOperation.Organizations.Add(statCan);
            processing.CleaningOperation.Description["en"] = "Description of the data cleaning.";
            processing.CleaningOperation.Description["fr"] = "Description du nettoyage des données.";
            

            // Describe a control operation
            processing.ControlOperation.Organizations.Add(statCan);
            processing.ControlOperation.Description["en"] = "Description of the data control method.";
            processing.ControlOperation.Description["fr"] = "Description de la méthode de contrôle des données.";


            return processing;
        }

        /// <summary>
        /// Processing Test ID 001, 002, and 003
        /// Register Items with Repository
        /// </summary>
        /// <returns></returns>
        public static async Task RegisterItems(IVersionable item)
        {
            // First find all the items that should be registered.
            // The dirty item finder can collect new and changed items in a model
            var dirtyItemGatherer = new DirtyItemGatherer();
            item.Accept(dirtyItemGatherer);

            // Get an API client, windows or username
            var api = GetRepositoryApiWindows();

            // start a transaction
            var transaction = await api.CreateTransactionAsync();

            // Add all of the items to register to a transaction
            var addItemsRequest = new RepositoryTransactionAddItemsRequest();
            foreach (var itemToRegister in dirtyItemGatherer.DirtyItems)
            {
                addItemsRequest.Items.Add(VersionableToRepositoryItem(itemToRegister));
            }
            await api.AddItemsToTransactionAsync(addItemsRequest);

            // commit the transaction, the Repository will handle the versioning
            var options = new RepositoryTransactionCommitOptions()
            {
                TransactionId = transaction.TransactionId,
                TransactionType = RepositoryTransactionType.CommitAsLatestWithLatestChildrenAndPropagateVersions
            };
            var results = await api.CommitTransactionAsync(options);
        }


        /// <summary>
        // Test ID 008, 009, 010, 011
        // Document instance variables of data assets stemming from administrative data sources
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static PhysicalInstance ImportSASDataset(string sas7bdatFilename, string sas7bcatFilename = null)
        {
            // Create the Colectica SAS reader
            using (SasDataReader sasReader = new SasDataReader(sas7bdatFilename, sas7bcatFilename))
            {
                // show a couple fields available on the reader
                var dataSetLabel = sasReader.Label;
                var recordCount = sasReader.CasesCount;
                var variableNames = sasReader.Variables.Select(x => x.Name).ToArray();

                // use the metadata import to create DDI Lifecycle metadata description of the dataset
                var ddi = SasImporter.ImportMetadata(sasReader);

                // The data can also be read, using the reader
                //while (sasReader.Read()) { }

                // return the DDI for the first dataset
                return ddi.PhysicalInstances.FirstOrDefault();
            }
        }



        /// <summary>
        /// Test ID 018, 019, 020
        /// Document edit/version processing metdata 
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static async Task ModifyAndVersionDataset(PhysicalInstance dataset)
        {
            // update the dataset description
            dataset.Description["en"] = "Description of the dataset";
            dataset.Description["fr"] = "Description de l'ensemble de données";

            // add descriptions to variables. All the same content in this sample
            var variables = dataset.DataRelationships.SelectMany(x => x.LogicalRecords).SelectMany(l => l.VariablesInRecord);
            int position = 1;
            foreach (Variable variable in variables)
            {
                variable.Description["en"] = $"This variable is in position {position}.";
                variable.Description["fr"] = $"Cette variable est en position {position}.";
                position++;
            }

            // The test from 001, 002, and 003 will find the changed items and add them to the Repository
            // The Repository will record new versions of existing items, and update all the references to the correct version
            await RegisterItems(dataset);
        }


        /// <summary>
        /// Test ID 022, 023
        /// Ability to Assign an Administrative Status
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static void AssignAdministrativeStatus(PhysicalInstance dataset)
        {
            // Get the Repository API
            var api = GetRepositoryApiWindows();

            // Set the AdministrativeStatus on a specific version of the survey
            api.CreateTag(dataset.CompositeId, "TheCurrentAdministrativeState");
        }


        /// <summary>
        /// Test ID 024
        /// Sample permissions for administrative statuses
        /// </summary>
        /// <param name="dataset"></param>
        public static void AssignPermissions(PhysicalInstance dataset)
        {
            // Get the Repository API
            var api = GetRepositoryApiWindows();

            // create a new item permission for the survey
            var permission = new ItemPermission();
            permission.Permission = PermissionType.ExclusiveWrite;
            permission.RoleName = "user@example.org";
            //permission.RoleName = "yourdomain\\username";
            permission.Identifier = dataset.Identifier;
            permission.AgencyId = dataset.AgencyId;

            // apply the permission on the Repository
            var roles = new RepositorySecurityContext();
            roles.IdentifierPermissions.Add(permission);
            api.AddPermissions(roles);
        }

        /// <summary>
        /// Test ID 025, 026, 027, 028
        /// Render PDF and HTML
        /// </summary>
        /// <param name="dataset"></param>
        public static void CreateHtmlAndPdf(PhysicalInstance dataset)
        {
            string resourcePath = Path.Combine("Resources", "icons", "types");
            string baseFileName = "codebook";

            // Set some options for the report.
            var options = new ReportOptions();
            options.IsAccessibilityMode = false;

            var builder = new ReportBuilder();

            // Create English PDF.
            MultilingualString.CurrentCulture = "en";
            byte[] englishPdfBytes = builder.CreatePdf(dataset, resourcePath, null, options, null, null);
            string englishPdfFileName = $"{baseFileName}_en.pdf";
            File.WriteAllBytes(englishPdfFileName, englishPdfBytes);

            // Create English HTML.
            string englishHtml = builder.CreateHtml(dataset, resourcePath, null, options, null, null);
            string englishHtmlFileName = $"{baseFileName}_en.html";
            File.WriteAllText(englishHtmlFileName, englishHtml);

            // Create French PDF.
            MultilingualString.CurrentCulture = "fr";
            byte[] frenchPdfBytes = builder.CreatePdf(dataset, resourcePath, null, options, null, null);
            string frenchPdfFileName = $"{baseFileName}_fr.pdf";
            File.WriteAllBytes(frenchPdfFileName, frenchPdfBytes);

            // Create French HTML.
            string frenchHtml = builder.CreateHtml(dataset, resourcePath, null, options, null, null);
            string frenchHtmlFileName = $"{baseFileName}_fr.html";
            File.WriteAllText(frenchHtmlFileName, frenchHtml);
        }


        // helpers
        public static WcfRepositoryClient GetRepositoryApiWindows()
        {
            var info = new RepositoryConnectionInfo()
            {
                AuthenticationMethod = RepositoryAuthenticationMethod.Windows,
                Url = "localhost",
                TransportMethod = RepositoryTransportMethod.NetTcp,
            };

            var api = new WcfRepositoryClient(info);
            return api;
        }

        public static WcfRepositoryClient GetRepositoryApiUsername()
        {
            var info = new RepositoryConnectionInfo()
            {
                AuthenticationMethod = RepositoryAuthenticationMethod.UserName,
                Url = "localhost:19893",
                TransportMethod = RepositoryTransportMethod.NetTcp,
                UserName = "username@example.org",
                Password = "password"
            };

            var api = new WcfRepositoryClient(info);
            return api;
        }

        private static RepositoryItem VersionableToRepositoryItem(IVersionable versionable)
        {
            Collection<Note> emptyNotes = new Collection<Note>();

            RepositoryItem ri = new RepositoryItem()
            {
                CompositeId = versionable.CompositeId,
                Item = versionable.GetDdi33FragmentRepresentation().ToString(),
                ItemType = versionable.ItemType,
                Notes = emptyNotes,
                ItemFormat = RepositoryFormats.Ddi33,
                IsPublished = versionable.IsPublished,
                IsProvisional = true,
                VersionDate = versionable.VersionDate,
                VersionRationale = versionable.VersionRationale,
                VersionResponsibility = versionable.VersionResponsibility
            };
            return ri;

        }

        private static void TestUserIdSearch()
        {
            RepositoryConnectionInfo info = new RepositoryConnectionInfo();
            info.AuthenticationMethod = RepositoryAuthenticationMethod.Windows;
            info.Url = "localhost:19893";
            info.TransportMethod = RepositoryTransportMethod.NetTcp;
            WcfRepositoryClient client = new WcfRepositoryClient(info);

            Category c = new Category() { AgencyId = "int.example" };
            c.ItemName.Current = "Test Category";
            c.Label.Current = "Test Category Label";
            c.Description.Current = "TestCategoryDesc";
            string userId = Guid.NewGuid().ToString();
            c.UserIds.Add(new UserId("sometype", userId));
            client.RegisterItem(c, new CommitOptions());

            var facet = new SearchFacet();
            facet.SearchTargets.Add(DdiStringType.UserId);
            //facet.SearchTerms.Add("TestCategoryDesc");
            facet.SearchTerms.Add(userId);

            var response = client.Search(facet);

            Console.WriteLine("Found " + response.Results.Count + " items");
        }


    }
}
