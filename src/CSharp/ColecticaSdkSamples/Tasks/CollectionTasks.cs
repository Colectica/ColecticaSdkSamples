using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Serialization;
using Algenta.Colectica.Model.Ddi.Utility;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Repository.Client;
using Colectica.Reporting;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace ColecticaSdkSamples.SampleTasks
{
    public class CollectionTasks
    {
        public static async Task MainAsync()
        {
            VersionableBase.DefaultAgencyId = "int.example";

            // Test ID 001 and 002
            // Create a Survey
            var survey = CreateSurveyInstrument();

            // Test ID 003 and 004
            // Register Items with Repository
            await RegisterItems(survey);

            // Test ID 005 and 006
            // Ingest from Nesstar DDI Codebook (DDI 2)
            await ImportNesstar("nesstar-ddi2.xml");

            // Test ID 007 and 008
            // Ingest EQDT - DDI Lifecycle 2.2
            var ddi33 = ImportDDI33("EQDT-ddi-3.3.xml");

            // Test ID 009 and 010
            // Edit/Version Questionnaire Metadata
            // We will update the survey from test 001
            await ModifyAndVersionSurvey(survey);

            // Test ID 011 and 012
            // Ability to Assign an Administrative Status
            AssignAdministrativeStatus(survey);

            // Test ID 013
            // Sample permissions
            AssignPermissions(survey);

            // Test ID 014, 015, 016, 017
            // Render PDF and HTML
            CreateHtmlAndPdf(survey);
        }




        /// <summary>
        /// Test ID 001 and 002
        /// Create a Survey
        /// </summary>
        /// <returns></returns>
        public static Instrument CreateSurveyInstrument()
        {
            // create a survey
            var survey = new Instrument();
            survey.Label["en"] = "Survey of Names";
            survey.Label["fr"] = "Sondage des noms";

            // create a question construct (usage in an survey) 
            var questionConstruct = new QuestionActivity();
            questionConstruct.ItemName.Current = "q1";

            // create a question (reusable question)
            var question1 = new Question();
            question1.ItemName.Current = "q1";
            question1.QuestionText["en"] = "What is your first name?";
            question1.QuestionText["fr"] = "Quel est votre prénom?";
            question1.QuestionResponse = RepresentationType.Text;

            // link the question usage to the question
            questionConstruct.Question = question1;

            // add the usage to the survey
            survey.Sequence.AddChild(questionConstruct);

            // Continue building the survey with these additional activity items as needed
            // ActionActivity
            // CustomLoopActivity
            // CustomUntilActivity
            // CustomWhileActivity
            // CustomIfElseActivity
            // CustomSequenceActivity
            // MeasurementActivity
            // QuestionActivity
            // StatementActivity

            return survey;
        }

        /// <summary>
        /// Test ID 003 and 004
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
        /// Test ID 005 and 006
        /// Import Nesstar
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static async Task ImportNesstar(string filename)
        {
            // Create the DDI 2, DDI 2.5, NESSTAR reader
            var importer = new Ddi2Deserializer(filename,
                                                VersionableBase.DefaultAgencyId,
                                                MultilingualString.CurrentCulture);

            // get the DDI Lifecycle instance from the importer
            DdiInstance ddiInstance = importer.Import();

            // Add the imported NESSTAR to the Repository
            // We use the same method from Test ID 003 and 004
            await RegisterItems(ddiInstance);
        }


        /// <summary>
        /// Test ID 007 and 008
        /// Import EQDT - DDI Lifecycle 2.2
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static FragmentInstance ImportDDI33(string filename)
        {
            // Create the EQDT, DDI 3.3 reader
            DdiReader reader = DdiReader.Create(filename);

            // get the DDI Lifecycle items from the importer in the fragmentInstance
            bool success = reader.Read(out FragmentInstance ddi33, out Collection<ResultMessage> messages);

            // return the DDI 3.3 container
            return ddi33;
        }


        /// <summary>
        /// Test ID 009 and 010
        /// Edit/Version Questionnaire Metadata
        /// </summary>
        /// <param name="survey"></param>
        /// <returns></returns>
        public static async Task ModifyAndVersionSurvey(Instrument survey)
        {
            // update the questionniare label
            survey.Label["en"] = "Survey of Names 2";
            survey.Label["fr"] = "Sondage des noms 2";

            // create another question construct (usage in an survey) and another question (reusable question)
            var questionConstruct = new QuestionActivity();
            questionConstruct.ItemName.Current = "q2";

            var question2 = new Question();
            question2.ItemName.Current = "q2";
            question2.QuestionText["en"] = "What is your last name?";
            question2.QuestionText["fr"] = "Quel est ton nom de famille??";
            question2.QuestionResponse = RepresentationType.Text;

            // link the question usage to the question
            questionConstruct.Question = question2;

            // add the usage to the survey
            survey.Sequence.AddChild(questionConstruct);

            // The test from 003 and 004 will find the changed items and add them to the Repository
            // The Repository will record new versions of existing items, and update all the references to the correct version
            await RegisterItems(survey);
        }


        /// <summary>
        /// Test ID 011 and 012
        /// Ability to Assign an Administrative Status
        /// </summary>
        /// <param name="survey"></param>
        /// <returns></returns>
        public static void AssignAdministrativeStatus(Instrument survey)
        {
            // Get the Repository API
            var api = GetRepositoryApiUsername();

            // Set the AdministrativeStatus on a specific version of the survey
            api.CreateTag(survey.CompositeId, "TheCurrentAdministrativeState");
        }


        /// <summary>
        /// Test ID 013
        /// Sample permissions
        /// </summary>
        /// <param name="survey"></param>
        public static void AssignPermissions(Instrument survey)
        {
            // Get the Repository API
            var api = GetRepositoryApiUsername();

            // create a new item permission for the survey
            var permission = new ItemPermission();
            permission.Permission = PermissionType.ExclusiveWrite;
            permission.RoleName = "user@example.org";
            //permission.RoleName = "yourdomain\\username";
            permission.Identifier = survey.Identifier;
            permission.AgencyId = survey.AgencyId;

            // apply the permission on the Repository
            var roles = new RepositorySecurityContext();
            roles.IdentifierPermissions.Add(permission);
            api.AddPermissions(roles);
        }

        /// <summary>
        /// Test ID 014, 015, 016, 017
        /// Render PDF and HTML
        /// </summary>
        /// <param name="survey"></param>
        public static void CreateHtmlAndPdf(Instrument survey)
        {
            string resourcePath = Path.Combine("Resources", "icons", "types");
            string baseFileName = "survey-report";

            // Set some options for the report.
            var options = new ReportOptions();
            options.IsPaperForm = false;
            options.IsAccessibilityMode = false;

            var builder = new ReportBuilder();

            // Create English PDF.
            MultilingualString.CurrentCulture = "en";
            byte[] englishPdfBytes = builder.CreatePdf(survey, resourcePath, null, options, null, null);
            string englishPdfFileName = $"{baseFileName}_en.pdf";
            File.WriteAllBytes(englishPdfFileName, englishPdfBytes);

            // Create English HTML.
            string englishHtml = builder.CreateHtml(survey, resourcePath, null, options, null, null);
            string englishHtmlFileName = $"{baseFileName}_en.html";
            File.WriteAllText(englishHtmlFileName, englishHtml);

            // Create French PDF.
            MultilingualString.CurrentCulture = "fr";
            byte[] frenchPdfBytes = builder.CreatePdf(survey, resourcePath, null, options, null, null);
            string frenchPdfFileName = $"{baseFileName}_fr.pdf";
            File.WriteAllBytes(frenchPdfFileName, frenchPdfBytes);

            // Create French HTML.
            string frenchHtml = builder.CreateHtml(survey, resourcePath, null, options, null, null);
            string frenchHtmlFileName = $"{baseFileName}_fr.html";
            File.WriteAllText(frenchHtmlFileName, frenchHtml);
        }


        // helpers
        public static WcfRepositoryClient GetRepositoryApiWindows()
        {
            var info = new RepositoryConnectionInfo()
            {
                AuthenticationMethod = RepositoryAuthenticationMethod.Windows,
                Url = "localhost:19893",
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
