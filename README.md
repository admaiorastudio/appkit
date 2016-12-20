# AppKit
App accelerator pack to bring common core functionality easy with Xamarin

## What is AppKit?
**AppKit** is a cross-platform PCL library built over Xamarin and its purpose is to make developer life easier. Under the hood it does nothing _special_, it consumes standard **Xamarin.iOS** & **Xamarin.Android** libraries, plus it uses some common and famous external libraries (via Nuget) like: SQLite (portable), Restharp (portable). The fun part is that it abstracts for you common specific tasks like: writing to the database, call REST services, receiving push notifications, send emails, execute async tasks, syncronize tasks to the main thread and much more. In short, it simply adds an abstraction layer which allows you to write code once for all this stuff (usually in a PCL library which represents your business core of your app).

## What is not AppKit?
**AppKit** is not a "write-once" library like **Xamarin.Forms**. It not brings a unique programming model which allows you to write UI views and UI logic once for each platform you want to target. You still need to create separate UI views (.xib or .axml) and UI code logic to wire up widgets events. 

## Do I need it?
No, if you like to write boring code from the very base, countless times in any app you develop. 
Yes, if you want to speed up the development of your apps, leaving **AppKit** to do basics tasks for you.

## How it helps me?
**AppKit** helps you in a very simple way. It allows you to develop in a _happy_ and very _easy-porting_ mode. You still have full control over the _native platform_ you want to target, with the only exception that you will have at you disposal many enanched or helper classes that will _unify_ business core logic.

Steps involved to do this are the following:
 1. First you create a Cross-Platform -> Blank App (Native Portable) 
 2. Add the **AppKit.UI** Nuget package to your solution
 3. Then choose an "initial" platform (could be **iOS** or **Android**, it doesn't matter) and start to design and create your UI views
 4. When the "bones" are ready you wire up widgets events to gather input or execute actions 
 5. Create a global "AppController" class which holds all the business core logic (actions) of your app (**AppKit** will help you here)
 6. Use the newly created _AppController actions_ in your UI (mostly in the widgets events)
 7. Then you _port_ the app to the other platform, but a this point you just need to create a "copy" of the _original_ UI you designed, wire up events and consume your _AppController actions_ as the same you did before.

## An important thing to note!
Developing cross-platform apps with **AppKit** leads to one major limitation. You can't use any specific platform feature that is not "abstractable" by the library itself. At least, you can, but you will lose the _easy-porting_ mode. So for example, you can't use a **Storyboard** to design your UI in **iOS**, as a matter of fact, there's nothign similar in **Android**.

Think about **AppKit** as a _facilitator_ than a real tool, to develop Xamarin apps. **Remember** if you need a real tool to create cross-platform apps, you should consider using other stuff like **Xamarin.Forms** or **MvvmCross**.

## Ok, I want to try it!
Let's start with some basic stuff. 

Remember that **AppKit** is more an _accelerator_ than a _real tool_, so keep in mind that this is **NOT** the _only_ way to create apps with **Xamarin**.

After you create your _Cross-Platform -> Blank App (Native Portable)_ solution add the **AppKit** Nuget package to all projects.

Then you are ready to begin!

### The App Structure
First of all we propose a "structure model" for your app. 

A good strategy colud be to "divide" your app in two macro blocks: core logic and UI logic. The first serves as the business logic of your app, while the second incapsulates all UI implementations.

This comes naturally due to the fact that your _Blank App (Native Portable)_ solutions is already organized like this.
In your newly created solution you will find:
- A PCL project which is ideal to use as a central core logic implementation
- A **Xamarin.iOS** app project which is where you create the UI logic for **iOS**
- A **Xamarin.Android** app project which is where you create the UI logic for **Android**

##### Core Logic:
_This class is implemented in the **PCL project** in your solution_
```           
 AppController // static class you need to create which holds all the common business logic of your app
      |
      |
      +--FunnyAction1() // static method which do logic stuff. appkit will come handy here!
      +--FunnyAction2()
      +--FunnyAction3()
      ...
      +--AnotherFunnyAction()
```

##### UI Logic:
_All these classes are implemented in the **platform specific project** in your solution_
```
 AppEntryPoint: class you need to implement
      |           
      |
      +-MainAppScreen: this is usually a UINavigationViewController class or an Activity class
               |
               |
               +-SubAppScreen1: this is usually a UIViewController class or a Fragment class
               +-SubAppScreen2  here you consume your AppController.Action methods
               +-SubAppScreen3
               ...
               +-SubAppScreenN    
```

### Core Logic: The AppController
An _AppController_ is nothing more than a static class full of static useful methods which represent the business core logic of your app. Usually these methods do things like: write to a database, call a rest service, write/read files to/from the internal or external memory. 

To accomplish this you should use specific platform API or integrate other external PCL libraries. There's a lot of fragmentation out there (speaking of Nuget), choosing can be difficult, so why not let's **AppKit** takes the most used ones for you, wrap all together and expose them via some useful and simpler objects?

As described above, we can design our _AppController_ class as a central unit for our business logic. 
Using **AppKit** this become very easy because it tries to abstract anything you basically need to create _very fancy logic methods_!

```cs
    // This is an example of AppController implementation
    public static class AppController
    {
        // This fantastic method will do magical things in your app, maybe querying a SQLite database?
        public static void CustomLogicFancyMethod1()
        
        // This fantastic method will do magical things in your app, maybe consuming some REST services?
        public static void CustomLogicFancyMethod2()
        
        // This fantastic method will do magical things in your app, maybe manipulating the file system?
        public static void CustomLogicFancyMethod3()
    }
```

Ok! now it's time to see what's in store! How can **AppKit** really helps you? 
Down below a full list of all classes exposed by the kit!

#### Handling Common Tasks
___
_Need to open the browser? Open a phone call? Execute some async task in a fancy way?_

To accomplish this **AppKit** provides you the **AdMaiora.AppKit.Utils.Executor** class. 
This is a simple object which will executes very common basic actions as described below:

##### _Executor class_
```cs
    public class Executor
    {
        // Start a phone call using the phone number you provide
        public void OpenPhoneCall(string phoneNumber)

        // Open the default web browser and navigate to the url you provide
        public void OpenBrowser(string url)

        // Open the default store app and navigate to the app via the app ID you provide
        public void OpenStore(string appId = null)

        // Open the default email app to send an email
        public void SendEmail(string[] toRecipients, 
            string subject, 
            string text = null, 
            FileUri[] attachments = null)

        // Execute an action delegate on the main thread (usually you use it in an async method)
        public void ExecuteOnMainThread(Action action)

        // Execute a "cancellable" function delegate in an awaitable Task using the async pattern, provides also 
        // action delegates to manage "after execution" and "exception handling" in the main thread
        public async Task ExecuteOnAsyncTask<TResult>(CancellationToken ctk, 
            Func<TResult> function, 
            Action<TResult> whenFinished = null, 
            Action<Exception> whenException = null)
        
        // Execute a "cancellable" action delegate in an awaitable Task using the async pattern, provides also 
        // action delegates to manage "after execution" and "exception handling" in the main thread
        public async Task ExecuteOnAsyncTask(CancellationToken ctk, 
            Action action, 
            Action whenFinished = null, 
            Action<Exception> whenException = null)

        // Execute a delayed "cancellable" action delegate in an awaitable Task using the async pattern, provides
        // also action delegates to manage "after execution" and "exception handling" in the main thread
        public async Task ExecuteOnAsyncTaskDelayed(int millisecondsDelay, CancellationToken ctk, 
            Action action, 
            Action whenFinished = null, 
            Action<Exception> whenException = null)

        // Execute a delayed action delegate in the main thread, provides also 
        // action delegates to manage "exception handling" in the main thread
        public async Task ExecuteDelayedAction(int millisecondsDelay, CancellationToken ctk,
            Action action = null, 
            Action<Exception> whenException = null)
        
        // Get app context info like: OS version, Build name and Build number
        public void GetContextInfo(out string osVersion, out string appVersion, out string build)

        // Output a message in the develop console, with tagging and formatting 
        public void DebugOutput(string tag, string format, params object[] args)

        // Output a message in the develop console 
        public void DebugOutput(string message)
    }
```


#### Handling File System
___
_Need to write or read files or folders?_

To accomplish this **AppKit** provides you the **AdMaiora.AppKit.IO.FileSystem** class.
This is a simple object which handles files or folders creation/reading as described below:

The **AdMaiora.AppKit.IO.FileSystem** class methods are intended to be consumed via the **FileUri** and the **FolderUri** classes. These classes are used to _abstract_ file location. You can create these classes directly, instead you will use two factory methods in the **AdMaiora.AppKit.IO.FileSystem** class.

_Why this level of abstraction?_
An app can handle files and folder from three main locations: _bundle_, _internal_ or _external_. 

Each location is handled in a different way on each platoform you target, but they serve for specific case that are described below:
- _bundle_ files or folders are part of the _app bundle_. These resources are in a ReadOnly mode and cannot be modified.
- _internal_ files or folders are stored in a _private_ way. These resources are readable or writable, but only the app can access them.
- _external_ files or folders are stored in a _public_ way. These resources are readable or writable, users can get them using external tools.

Let's see some examples:
- _"bundle://readme.txt"_ is a **FileUri** which points to a file located inside the app bundle. The exact location of the file depends on which platform you are targeting. In **iOS** will be _"Resources/readme.txt"_ while in **Android** corresponds to _"Assets/readme.txt"_
 
- _"internal://config.xml"_ is a **FileUri** which points to a file located in a _private_ store which usually corresponds to the root folder where the app is installed by the OS. Only the app can access the file. The exact (absolute) location of the file depends on which platform you are targeting.

- "external://data.sql"_ is a **FileUri** which points to a file located in a _public_ store which usually corresponds to a specific folder created by the OS when your app is installed. The file is publicy exposed, so the user can reach it using a specific external tool. The exact (absolute) location of the file depends on which platform you are targeting.
 
- "external://cache"_ is a **FolderUri** which points to a folder located in a _public_ store which usually corresponds to a specific folder created by the OS when your app is installed. The folder is publicy exposed, so the user can reach it and all its content, using a specific external tool. The exact (absolute) location of the folder depends on which platform you are targeting.

##### _FileSystem class_
```cs
    public class FileSystem
    {
        // Create a FileUri instance via the uri you provide 
        public FileUri CreateFileUri(string uri)
        
        // Create a FileUri instance vie the path and the storage location you provide
        public FileUri CreateFileUri(string path, StorageLocation location)

        // Create a FolderUri instance vie the uri you provide
        public FolderUri CreateFolderUri(string uri)

        // Create a FolderUri instance vie the path and the storage location you provide
        public FolderUri CreateFolderUri(string path, StorageLocation location)
        
        // Get available disk space for a specific folder (in bytes)
        public ulong GetAvailableDiskSpace(FolderUri uri)

        // Get the size of a specific file (in bytes)
        public ulong GetFileSize(FileUri uri)

        // Get the info of a specific file
        public UniversalFileInfo GetFileInfo(FileUri uri)

        // Check if a folder exists
        public bool FolderExists(FolderUri uri)

        // Create a folder
        public void CreateFolder(FolderUri uri)

        // Delete a folder
        public void DeleteFolder(FolderUri uri)

        // Get a list of FileUri, one for each file continaed in a folder. You can
        // filter files via the searchPattern you provide. You can also specify to search recursively
        public FileUri[] GetFolderFiles(FolderUri uri, string searchPattern, bool recursive)

        // Check if a file exists
        public bool FileExists(FileUri uri)

        // Copy a file to a new location. You can specify to overwrite any existing file
        public void CopyFile(FileUri sourceUri, FileUri destinationUri, bool overwrite)
    
        // Move a file to a new location
        public void MoveFile(FileUri sourceUri, FileUri destinationUri)

        // Delete a file
        public void DeleteFile(FileUri uri)
        
        // Get a stream object that points to a specific file. You can specify open file mode, access and share 
        public Stream OpenFile(FileUri uri, UniversalFileMode mode, UniversalFileAccess access, UniversalFileShare share)
        
        // Get a stream object that points to a specific file. You can specify open file mode and access. No share.
        public Stream OpenFile(FileUri uri, UniversalFileMode mode, UniversalFileAccess access)

        // Get a stream object that points to a specific file. File open mode is open for read access.
        public Stream OpenFile(FileUri uri)
        
        // Get the mime type for a specific file or extension
        public string GetMimeType(string fileUriOrExt)
    }
```

#### Handling User Settings
___
_Need to store simple values which configure your app behavior?_

To accomplish this **AppKit** provides you the **AdMaiora.AppKit.Data.UserSettings** class.
This is a simple object which allows you to read/write simple values in a private app storage as described below:

The storage will exists until the app is installed on the device. Once the app is removed form the device, the settings will be wiped out. 

No "cloud stuff" involved here!

##### _UserSettings class_
```cs
    public class UserSettings
    {
        // Get an int value from the settings via the specified key
        public int GetIntValue(string key)

        // Get a string value from the settings via the specified key
        public string GetStringValue(string key)

        // Get a bool value from the settings via the specified key
        public bool GetBoolValue(string key)

        // Get a DateTime value from the settings via the specified key
        public DateTime? GetDateTimeValue(string key)

        // Set an int value into the settings for a specified key
        public void SetIntValue(string key, int value)

        // Set a string value into the settings for a specified key
        public void SetStringValue(string key, string value)

        // Set a bool value into the settings for a specified key
        public void SetBoolValue(string key, bool value)

        // Set a DateTime value into the settings for a specified key
        public void SetDateTimeValue(string key, DateTime? value)
    }
```

#### Handling Database
___
_Need to store data into a SQLite database? Want to get rid of SQL queries and be a "code first" boy?_

To accomplish this **AppKit** provides you the **AdMaiora.AppKit.Data.DataStorage** class.
This is a simple object which allows you to read/write and query data stored in a SQLite database as described below:

The **AdMaiora.AppKit.Data.DataStorage** class methods expose a simple ORM interface and are intended to be consumed using POCO objects (_entities_) as a _model_ for your data.

This functionality is based on **SQLite-net**, as reference you can read more here:
https://github.com/praeclarum/sqlite-net

##### _DataStorage class_
```cs
    public class DataStorage
    {
        // Bulk insert a list of entities
        public void InsertAll<TEntity>(IEnumerable<TEntity> entities)

        // Single insert of an entity
        public void Insert<TEntity>(TEntity entity)

        // Get a single entity by id (int property marked as PrimaryKey)
        public TEntity GetById<TEntity>(int objectId)

        // Get the full list of entities
        public List<TEntity> FindAll<TEntity>()

        // Get a list of entities filtered by the selector
        public List<TEntity> FindAll<TEntity>(Func<TEntity, bool> selector) 

        // Get a single entity filtered by the selector
        public TEntity FindSingle<TEntity>(Expression<Func<TEntity, bool>> selector)

        // Bulk update a list of entities
        public void UpdateAll<TEntity>(IEnumerable<TEntity> entities)

        // Single update of an entity
        public void Update<TEntity>(TEntity entity)

        // Bulk delete a list of entities
        public void DeleteAll<TEntity>()

        // Bulk delete a list of entities filtered by the selector
        public void DeleteAll<TEntity>(Func<TEntity, bool> selector)

        // Single delete of an entity
        public void Delete<TEntity>(TEntity entity)

        // Wraps and executes CRUD operations in a single transaction. Cancellable.
        public void RunInTransaction(CancellationToken ct, Action<DataStorage> action)
        
        // Wraps and executes CRUD operations in a single transaction.
        public void RunInTransaction(Action<DataStorage> action)

        // Create an old plain SQL command query.
        public SQLiteCommand CreateCommand(string sqlCommand, params object[] args)

        // Execute an old plain SQL command query
        public void ExecuteNonQueryCommand(SQLiteCommand cmd)

        // Execute an old plain SQL scalar command query
        public TValue ExecuteScalarCommand<TValue>(SQLiteCommand cmd)

        // Check if a table exists
        public bool TableExists<TEntity>()
    }
```

#### Handling REST Services
___
_Need to consume a REST service? Need to serialize/deserialize data? Need to track timing and results of endpoint?_

To accomplish this **AppKit** provides you the **AdMaiora.AppKit.Services.ServiceClient** class.
This is a simple object which allows you to consume REST services as described below:

The **AdMaiora.AppKit.Services.ServiceClient** class methods expose a simple interface to make HTTP requests and are intended to be consumed using POCO objects (_entities_) as a _data transfer objects_ for your data.

It can manage requests for you via the _Request()_ methods or can be used as a factory class to create _RestClient_ and _RestRequest_ objects to consume endpoints.

Each _Request()_ method support standard HTTP verbs (GET, POST, PUT, DELETE). For each request you can send data parmeters in different falvours, specifing a parameters handling mode:

- _Default_: Will let RestSharp choose which is right for the request
- _Body_: Will add paremters as request body 
- _MultipartFormData_: This means multipart/form-data, this allow file uploads BUT you can send form parameters only

It supports OAuth 2.0 authentication. It can be configured to handle _Access Token_ parametrization and validity.

All the JSON serialization/deserialization operations are handled internally.

This functionality is based on **RestSharp Portable**, as reference you can read more here:
https://github.com/FubarDevelopment/restsharp.portable

##### _ServiceClient class_
```cs
    public class ServiceClient
    {
        // Get or Set default request timeout
        public double RequestTimeout

        // Get or Set current access token value used for requests
        public string AccessToken

        // Get or Set current access token header parameter name used for requests
        public string AccessTokenName

        // Get or Set current access token expiration date used for requests
        public DateTime? AccessTokenExpiration
        
        // Get or Set json data field in multipart requests
        public string MultipartJsonField

        // Get or Set http errors handling. If set exceptions are thrown in place of HTTP errors
        public bool HandleHttpErrors
        
        // Get current active network connection: not connected, wifi, mobile or others
        public NetworkConnection NetworkConnection

        // Check if the current access token is expired, monitoring the AccessTokenExpiration date
        public bool IsAccessTokenValid

        // Check if a network connection is available
        public bool IsNetworkAvailable
        
        // Make a cancellable request for a specific resource, using a specific HTTP method
        public async Task<IRestResponse> Request(string resource, Method method,
            CancellationToken t = default(CancellationToken),
            ParametersHandling ct = ParametersHandling.Default,
            object parameters = null)

        // Make a cancellable typed request for a specific resource, using a specific HTTP method
        public async Task<IRestResponse<T>> Request<T>(string resource, Method method,
            CancellationToken t = default(CancellationToken),
            ParametersHandling ct = ParametersHandling.Default,
            object parameters = null)

        // Make a cancellable request for a specific resource, using a specific HTTP method
        // Configuration of the client and the request is exposed via
        // the configure client action delegate and the configure request action delegate
        public async Task<IRestResponse> Request(string resource, Method method,
            Action<RestClient> configureClient,
            Action<RestRequest> configureRequest,
            CancellationToken t = default(CancellationToken),
            ParametersHandling ct = ParametersHandling.Default,
            object parameters = null)

        // Make a cancellable typed request for a specific resource, using a specific HTTP method
        // Configuration of the client and the request is exposed via
        // the configure client action delegate and the configure request action delegate
        public async Task<IRestResponse<T>> Request<T>(string resource, Method method,
            Action<RestClient> configureClient = null,
            Action<RestRequest> configureRequest = null,
            CancellationToken t = default(CancellationToken),
            ParametersHandling ct = ParametersHandling.Default,
            object parameters = null)

        // Make a cancellable tracked request for a specific resource, using a specific HTTP method
        // Tracking can be handled via the track action delegate. Information related 
        // to the tracked request are passed to the action delegate via a TrackedRestRequest object
        public async Task<IRestResponse> TrackedRequest(string resource, Method method, 
            Action<TrackedRestRequest> track,
            CancellationToken t = default(CancellationToken),
            ParametersHandling ct = ParametersHandling.Default,
            object parameters = null)

        // Make a cancellable typed request for a specific resource, using a specific HTTP method
        // Tracking can be handled via the track action delegate. Information related 
        // to the tracked request are passed to the action delegate via a TrackedRestRequest object
        public async Task<IRestResponse<T>> TrackedRequest<T>(string resource, Method method, 
            Action<TrackedRestRequest> track,
            CancellationToken t = default(CancellationToken),
            ParametersHandling ct = ParametersHandling.Default,
            object parameters = null)

        // Make a cancellable tracked request for a specific resource, using a specific HTTP method
        // Configuration of the client and the request is exposed via
        // the configure client action delegate and the configure request action delegate
        // Tracking can be handled via the track action delegate. Information related 
        // to the tracked request are passed to the action delegate via a TrackedRestRequest object
        public async Task<IRestResponse> TrackedRequest(string resource, Method method, 
            Action<RestClient> configureClient,
            Action<RestRequest> configureRequest,
            Action<TrackedRestRequest> track,
            CancellationToken t = default(CancellationToken),
            ParametersHandling ct = ParametersHandling.Default,
            object parameters = null)

        // Make a cancellable typed request for a specific resource, using a specific HTTP method
        // Configuration of the client and the request is exposed via
        // the configure client action delegate and the configure request action delegate
        // Tracking can be handled via the track action delegate. Information related 
        // to the tracked request are passed to the action delegate via a TrackedRestRequest object
        public async Task<IRestResponse<T>> TrackedRequest<T>(string resource, Method method,
            Action<RestClient> configureClient,
            Action<RestRequest> configureRequest,
            Action<TrackedRestRequest> track,
            CancellationToken t = default(CancellationToken),
            ParametersHandling ct = ParametersHandling.Default,
            object parameters = null)
            
        // Store internally the Access-Token value and its expiration date
        // Once set the Access-Token will be passed with to the request
        public void RefreshAccessToken(string accessToken, DateTime expirationDate)

        // Invalidate the locally stored Access-Token
        // Once invalidate the Access-Token will be NOT passed to the request
        public void InvalidateAccessToken()

        // Factory method to create a preconfigured RestSharp.RestClient object
        // Preconfiguration is done via ServiceClient properties
        public RestClient GetRestClient()
        
        // Factory method to create a preconfigured RestSharp.RestRequest object
        // Preconfiguration is done via ServiceClient properties
        public RestRequest GetRestRequest(string resource, Method method, 
            ParametersHandling ph = ParametersHandling.Default, 
            object parameters = null)
    }
```

#### Handling Image Loading
___
_Need to safely and easly load image into views asynchronously? Need to load them into a long list, without any memory issue?_

To accomplish this **AppKit** provides you the **AdMaiora.AppKit.Utils.ImageLoader** class.
This is a simple object which allows you to load images as described below:

The **AdMaiora.AppKit.Utils.ImageLoader** class exposes a simple interface to asynchronously load images into views fetching them from local resources (_bundle_) or remote resources (_http_). It gracefully handle memory using a simple _LRU Policy Cache_ (last recently used) of images.  

##### _ImageLoader class_
```cs
    public class ImageLoader
    {
        // Get or Set where the loader will store physically loaded images
        public FolderUri StorageUri

        // Get or Set the size limit of the LRU cache used to store images
        public int MaxCacheSize
        
        // Get or Set the authenticator used for protected resources.
        // Usually you set this with the same ServiceClient instance
        // used by your app for consuming REST services
        public ServiceClient Authorizator

        // Get or Set if the loader will send the MIME type
        // as header parameter in remote resource requests
        public bool IsMimeTypeMandatory
        
        // Set a local resource image for a View
        // You can specify how image loading is handled configuring: rotation, hiding of the view
        // You can be notified when the load is done via the done action delegate
        public void SetImageForView(FileUri imageUri, object imageView, 
            object loaderView = null, 
            int rotation = 0, 
            bool hideWhileLoading = true,
            Action<string> done = null)

        // Set a remote resource image for a View. You can sepcify a "no-image" 
        // as a fall back when resource is not available (for eg. network error)
        // You can specify how image loading is handled configuring: rotation, hiding of the view
        // You can be notified when the load is done via the done action delegate
        public void SetImageForView(Uri uri, FileUri noImageUri, object imageView, 
            object loaderView = null, 
            int rotation = 0,
            bool hideWhileLoading = true, 
            Action<string> done = null)

        // Set a remote resource image for a View. You can sepcify a "no-image" 
        // as a fall back when resource is not available (for eg. network error)
        // You can specify how image loading is handled configuring: rotation, hiding of the view
        // You can be notified when the load is done via the done action delegate
        public void SetImageForView(Uri uri, string noImageName, object imageView, 
            object loaderView = null, 
            int rotation = 0, 
            bool hideWhileLoading = true, 
            Action<string> done = null)

        // Set a local resource image for a View.
        public void SetImageForView(string imageName, object imageView)

        // Invalidate a locally loaded image resource, this will force 
        // a full reload of the image into the LRU cache
        public void InvalidateImage(FileUri imageUri)

        // Invalidate a remotly loaded image resource, this will force 
        // a full reload of the image into the LRU cache
        public void InvalidateImage(Uri uri)
    }
```

#### Handling Localization
___
_Need to localize your app? Want to do it in a simply and unique way on each platform?_

To accomplish this **AppKit** provides you the **AdMaiora.AppKit.Localization.Localizator** class.
This is a simple object which allows you to manage and apply translations into UI as described below:

The **AdMaiora.AppKit.Localization.Localizator** class is nothing more than an "enached localized dictionary" to store strings values, translated in each language you need. Each "culture" handled by the dictionary is defined by implementing a **AdMaiora.AppKit.Localization.LocalizationDictionary** class marked with special attribute which associate the implementation to a specific culture. Localized strings are defined in a key-value pair mode via a simple _fluent api_ exposed by the base class. Each implemented class self contains translations for one specific culture.

The **AdMaiora.AppKit.Localization.Localizator** class will handle automatic selection of the specific culture to use to localize strings in your application. The choice is based via a combination of _language_ and _region_ exposed by the targeted platform, that is, because any device can be configured with a non standard combination of _language_ and _region_ (for eg. you can have your deviced localized in english with italian regional settings "en-IT")  A runtime change of the current culture can be effectuated any time, but you have to refresh the UI in turn to refresh localized string with the new selected culture.

For comodity the enanched dictionary also exposes some methods to facilitate DateTime and Currency value formatting.

##### _Localizator class_
```cs
    // Get the current culture name used by the dictionary (for eg. en-US or it-IT)
    public string Culture
    
    // Get the current culture language used by the dictionary (for eg. en or it)
    public CultureInfo LanguageCulture
    
    // Get the current culture region used by the dictionary (for eg. US or IT)
    public CultureInfo RegionalCulture

    // Get the current localized dictionary loaded for the current culture
    public LocalizationDictionary Dictionary
    
    // Get a localized string for a specific key
    public string GetString(string key)
    
    // Get a custom formatted localized string for a specific key
    public string GetString(string key, params object[] args)

    // Get a currency formatted localized string for a specific currency value 
    public string FormatCurrency(decimal value)

    // Get a datetime formatted localized string for a specific datetime value 
    public string FormatDate(DateTime value, string format)

    // Get a date formatted localized string for a specific datetime value
    public string FormatShortDate(DateTime value)

    // Get a short date and time formatted localized string for a specific datetime value
    public string FormatShortDateAndTime(DateTime value)

    // Get a long date formatted localized string for a specific datetime value
    public string FormatLongDate(DateTime value)

    // Get a long date and time formatted localized string for a specific datetime value
    public string FormatLongDateAndTime(DateTime value)

    // Get a time formatted localized string for a specific datetime value
    public string FormatTime(DateTime value)
```

#### Handling Logging
___
_Need to add simple text file loggin in your app?_

To accomplish this **AppKit** provides you the **AdMaiora.AppKit.Utils.Logger** class.
This is a simple object which allows you to write simple textual log files as described below:

The **AdMaiora.AppKit.Utils.Logger** exposes a really simple interface to output diagnostic messages into console output (when in _DEBUG_ mode) and into a specific log file. Before any log operations happens, you need to specify where to store the log file, configuring the logger to your needs.

The logger will handle automatically the recycling operation of the log file, this to avoid memory consumption!

##### _Localizator class_
```cs
    public class Logger
    {
        // Get or Set where the log file is stored
        public FileUri LogUri
    
        // Get or Set a log tag used for console output
        public string LogTag
    
        // Get or Set the log size limit
        public ulong MaxLogSize
    
        // Get or Set if logger will output in console (when in DEBUG mode)
        public bool EchoInConsole
        
        // Log an INFO message, formatted
        public void Info(string message, params object[] prms)
        
        // Log an INFO message
        public void Info(string message)
    
        // Log an ERROR message, formatted
        public void Error(string message, params object[] prms)
    
        // Log an ERROR message with related Execption informations
        public void Error(string error, Exception ex = null)
    }
```

#### Let's wrap all together
___
Below a simple _AppController class_ basic implementation using **AppKit**. We are incorporating some helper objects described above, that later we will se in action.

```cs
    // This is an example of AppController implementation
    public static class AppController
    {
        private static UserSettings _settings;
        private static Executor _utility;
        private static FileSystem _filesystem;
        private static DataStorage _database;
        private static ServiceClient _services;
        
        public static void EnableSettings(IUserSettingsPlatform userSettingsPlatform)
        {
            _settings = new UserSettings(userSettingsPlatform);
        }

        public static void EnableUtilities(IExecutorPlatform utiltiyPlatform)
        {
            _utility = new Executor(utiltiyPlatform);
        }

        public static void EnableFileSystem(IFileSystemPlatform fileSystemPlatform)
        {
            _filesystem = new FileSystem(fileSystemPlatform);
        }
        
        public static void EnableDataStorage(IDataStoragePlatform dataStoragePlatform)
        {
            FileUri storageUri = _filesystem.CreateFileUri("external://data/database.dat");
            _database = new DataStorage(dataStoragePlatform, storageUri);
        }

        public static void EnableServices(IServiceClientPlatform servicePlatform)
        {
            _services = new ServiceClient(servicePlatform, "http://my-superfancy-api.azurewebsites.net/");
            _services.RequestTimeout = 30;
            _services.AccessTokenName = "Authorization";
        }
    }
```

As you can see each **AppKit** helper object is instanced via the _Constructor()_. The first parameter of each _Constructor()_ is a special "platform" object and is passed via a _platform agnostic interface_. This is needed to allow each helper object to act correctly when targeting different platforms (iOS or Android). This is called the *IOC dependency constructor injection* tequinque.

It may seem _strange_ and _tricky_ but believe me, you don't have to worry. IOC is a well known pattern and **AppKit** will provide to you all those special "platform" objects for you. 

Now it's time to show you how to implement some magic!

```cs
    // This is an example of AppController implementation
    public static class AppController
    {
        private static DataStorage _database;
        private static ServiceClient _services;
        
        /* 
         ***** How to use the DataStorage class *******
         */        
        
        public static void EnableDataStorage(IDataStoragePlatform dataStoragePlatform)
        {
            FileUri storageUri = _filesystem.CreateFileUri("external://data/database.dat");
            _database = new DataStorage(dataStoragePlatform, storageUri);
        }
        
        // Here an example to make a simple query to the database
        // The code is quite self explanatory
        public static DataItem[] GetItems()
        {
            List<DataItem> items = _database.FindAll<DataItem>();
            if (items == null)
                return null;

            return items
                // Apply any pre ordering rule here
                .ToArray();
        }        
        
        public static void AddItem(DataItem newItem)
        {
            // Here you can add any custom logic before the insertion
            //
            
            _database.Insert(newItem);
        }
        
        public static void DeleteItem(DataItem oldItem)
        {
            // Here you can add any custom logic before the deletion
            //
            
            _database.Delete(oldItem);
        }        
        
        /* 
         ***** How to use the ServiceClient class *******
         */
        
        public static void EnableServices(IServiceClientPlatform servicePlatform)
        {
            _services = new ServiceClient(servicePlatform, "http://my-superfancy-api.azurewebsites.net/");
            _services.RequestTimeout = 30;
            _services.AccessTokenName = "Authorization";
        }
        
        // Here an example to make a REST request ot a login endpoint
        // The code is quite self explanatory
        public static async Task LoginUser(CancellationTokenSource cts,
            string email,
            string password,
            Action<Poco.User> success,
            Action<string> error,
            Action finished)
        {
            try
            {
                var response = await _services.Request<Dto.Response<Poco.User>>(
                    // Resource to call
                    "users/login",
                    // HTTP method
                    Method.POST,
                    // Cancellation token
                    cts.Token,
                    // Parameters handling
                    ParametersHandling.Body,
                    // Payload
                    new
                    {
                        email = email,
                        password = password
                    });

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string accessToken = response.Data.Content.AuthAccessToken;
                    DateTime accessExpirationDate = response.Data.Content.AuthExpirationDate.GetValueOrDefault().ToLocalTime();

                    // Refresh access token for further service calls
                    _services.RefreshAccessToken(accessToken, accessExpirationDate);
                    
                    success?.Invoke(response.Data.Content);
                }
                else
                {
                    error?.Invoke(
                        response.Data.ExceptionMessage ?? response.Data.Message ?? response.StatusDescription);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                
                error?.Invoke("Internal error :(");
            }
            finally
            {                
                finished?.Invoke();
            }
        }      
    }
```

### UI Logic: The AppEntryPoint               
Usually your _AppEntryPoint_ is an implementation of these classes:
- **UIKit.UIApplicationDelegate** class for **iOS**
- **Android.App.Application** class for **Android**

(Android hides it for you, but believe me you can create one. Read more here https://developer.android.com/reference/android/app/Application.html)

With **AppKit** you need to implement your _AppEntryPoint_ starting from: 
- **AdMaiora.AppKit.UIAppKitApplicationDelegate** class for **iOS**
- **AdMaiora.AppKit.AppKitApplication** class for **Android**

These precooked class are the _first step_ of the abstraction and allow you have an unified model for some app status and events. Always wondered how to know if your app is in foreground status? Easy! Now you gained a property like this:

```cs
    public bool IsApplicationInForeground
    {
        get;
    }
```

in **iOS** you usually have these overridable methods to intercept app background/foreground stauts:

```cs
    public override void DidEnterBackground(UIApplication application)
    {
    }

    public override void WillEnterForeground(UIApplication application)
    {
    }
```

so now in **Android** you will have:

```cs
    public virtual void OnPause()
    {
    }

    public virtual void OnResume()
    {
    }
```

All the rest is quite similar to old times when you were using plain **Xamarin Classic** libraries.
The _AppEntryPoint class_ is a good point where to configure all the **AppKit** stuff we want to use.

In **iOS** you still need to override the _FinishedLaunching()_ method:

```cs
    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // Do common setup here, initialize common stuff like database, rest services, 
        // file system manager or external libraries like Facebook.
        AppController.EnableSettings(new AdMaiora.AppKit.Data.UserSettingsPlatformiOS());
        AppController.EnableUtilities(new AdMaiora.AppKit.Utils.ExecutorPlatformiOS());
        AppController.EnableFileSystem(new AdMaiora.AppKit.IO.FileSystemPlatformiOS());
        AppController.EnableDataStorage(new AdMaiora.AppKit.Data.DataStoragePlatformiOS());
        AppController.EnableServices(new AdMaiora.AppKit.Services.ServiceClientPlatformiOS());
        

        // NOTICE THIS!
        //
        // This method tells the app which is the _MainViewController_ to show when app starts.
        // Automatically handles the creation of the Window class and assign its root view controller.
        return RegisterMainLauncher(new MainViewController(), launchOptions);            
    }
```
 
while in **Android** you still need to override the _OnCreate()_ method:

```cs
    public override void OnCreate()
    {
        base.OnCreate();
        
        // Do common setup here, initialize common stuff like database, rest services, 
        // file system manager or external libraries like Facebook. 
        AppController.EnableSettings(new AdMaiora.AppKit.Data.UserSettingsPlatformAndroid());
        AppController.EnableUtilities(new AdMaiora.AppKit.Utils.ExecutorPlatformAndroid());
        AppController.EnableFileSystem(new AdMaiora.AppKit.IO.FileSystemPlatformAndroid());
        AppController.EnableDataStorage(new AdMaiora.AppKit.Data.DataStoragePlatformAndroid());
        AppController.EnableServices(new AdMaiora.AppKit.Services.ServiceClientPlatformAndroid());                                    
        // NOTICE THIS!
        //
        // As you can see there is no _RegisterMainLauncher()_ method like in iOS
        // In Android you need to mark your initial MainActivity with the _Activity_ 
        // attribute, and have its _MainLauncher_ attribute set to _true_.
    }
```

### UI Logic: The MainAppScreen  
A **MainAppScreen** usually represents your main UI _holder_ which will contains all others **SubAppScreen** elements. 
This object will be responsible for few major things in your app, one of the most important is how you will _navigate_ between the various _sections_ of your app. Each _section_ can be represented by a single **SubAppScreen** or a set of them, one of which will represent the "root" of the _section_.

In **iOS** you could have something like this:

```cs
    public partial class MainViewController : UINavigationViewController
    {
        #region Constructors

        public MainViewController()
            : base("MainViewController", null)
        {
        }

        #endregion

        #region ViewController Methods

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            #region Designer Stuff
            #endregion
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();
        }

        #endregion
    }
```

while in **Android** you will have something like this:

```cs
    [Activity(
        Label = "YourAppNameHere",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges =
            ConfigChanges.Orientation | ConfigChanges.ScreenSize |
            ConfigChanges.KeyboardHidden | ConfigChanges.Keyboard
    )]
    public class MainActivity : Activity
    {
        #region Constructors

        public MainActivity()
        {
        }

        #endregion
        
        #region Activity Methods

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            #region Desinger Stuff

            SetContentView(Resource.Layout.ActivityMain);

            #endregion
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        
        #endregion
    }
```

### UI Logic: The SubAppScreen
A **SubAppScreen** usually represent a single section of your app or a part of it. Each **SubAppScreen** will contain one or more UI widgets and UI logic to let the user do some stuff. Remember to put minimal code in the UI logic and let the _AppController_ class do the heavy stuff, this will be useful when porting the app from one platform to another.

Here is a good point where to consume your _AppController_ methods.

In **iOS** you could have something like this:

```cs
    public partial class SubController : UIViewController
    {
        #region Constructors

        public SubController()
            : base("SubController", null)
        {
        }

        #endregion

        #region Properties
        #endregion

        #region ViewController Methods

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            #region Designer Stuff
            #endregion
            
            // NOTICE THIS!
            //
            var items = AppController.GetDatatmes();
            
            // Bind logic to your UITableView
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        #endregion
    }
```

while in **Android** you will have something like this:

```cs
    public class SubFragment : Fragment
    {
        #region Constructors

        public SubFragment()
        {
        }

        #endregion

        #region Fragment Methods

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            #region Desinger Stuff

            View view = inflater.Inflate(Resource.Layout.FragmentSub, container, false);          

            #endregion     
            
            // NOTICE THIS!
            //
            var items = AppController.GetDataItmes();
            
            // Bind logic to your UITableView
            
            return view;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
        }

        #endregion    }
```

## In Conclusion
This is a basic documentation. We suggest to you to check our app examples which fully implement the **AppKit** library. 

All source code is available, fully forkable, modificable and playable in **GitHub**

Have a look to:
- [Chatty](https://github.com/admaiorastudio/chatty) a simple chat application
- [Listy](https://github.com/admaiorastudio/listy) a simple todo list application
- [Bugghy](https://github.com/admaiorastudio/bugghy) a simple bug tracking application