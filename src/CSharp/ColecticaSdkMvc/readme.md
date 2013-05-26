# Colectica SDK MVC Examples

This project demonstrates several methods available in Colectica SDK, using the ASP.NET MVC 4 framework to show how information from Colectica Repository can be integrated into a Web application.

These samples were prepared during a Colectica Workshop at Statistics Denmark.

The Home page displays the results of a basic search. The search simply lists all StudyUnits, CategorySchemes, and CodeSchemes that are present in the repository.

The search results link to a page for each item. The item pages show some basic information about the item.

For all item types, two types of information are displayed:

* the version history 
* a list of items that reference the specified item

For StudyUnits, the study's title is displayed, along with a list of all Questions that are referenced in the study.

## The Code

### *M:* Models/

In MVC, the Model classes represent the information that is displayed on a Web page.

The HomeModel class contains a list of SearchResults.

The ItemModel class contains two lists of RepositoryItemMetadata objects: one representing the version history of an item, and one representing items that reference the current item.

The CategorySchemeModel, CodeSchemeModel, and StudyUnitModel are all derived from ItemModel, and may add properties to display information about the specific item types.

The RepositoryItemMetadata class is part of the SDK, and contains basic information about a DDI item, including the item's full identification and summary text, but not the full contents of the object. This allows large numbers of search results to be returned efficiently. When the full contents of an item are required, they can be requested separately.

### *V*: Views/

In MVC, the View classes are the templates that are transformed into HTML pages to be displayed. This sample uses Razor template language, which is a combination of HTML and C#. The templates use the Model classes to put the appropriate information into the Web pages.

Home/Index.cshtml is the template for the main page, which shows a list of search results.

The Item/*.cshtml files provide templates for each of the corresponding item types.

Shared/_Layout.cshtml provides the main layout for all pages, and can be customized to change the branding, look, and feel of the Web application.

Shared/_Item.cshtml provides the layout for the item pages. In addition to providing the general look and feel, it also adds the version history and referencing items lists, so we don't need to manually add those to each page.

### *C*: Controllers/

In MVC, the Controller classes create instances of the Model classes, populate them with data, and send the Model object to the appropriate View.

In these samples, the Controller classes use Colectica SDK to retrieve information from the Repository in order to build the Model objects.

### Utility/

The ClientHelper class contains a single method, GetClient(), that configures and returns a RepositoryClient object that can be used to communicate with Colectica Repository. By default it is configured to communicate with a locally-running instance of Colectica Repository Developer Edition, but you can change the URL to point to a remote repository if you like.

### Other MVC-related content

The App_Start, Content, and Scripts directories are created by the ASP.NET MVC 4 framework and are not critical for understanding the Colectica SDK.