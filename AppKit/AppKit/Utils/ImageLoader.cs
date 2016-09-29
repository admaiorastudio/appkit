namespace AdMaiora.AppKit.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;    
    using System.IO;
    using System.Linq;    
    using System.Threading.Tasks;
    using System.Net;    
    
    using AdMaiora.AppKit.IO;
    using AdMaiora.AppKit.Data;
    using AdMaiora.AppKit.Services;
    
    using RestSharp.Portable;
    using RestSharp.Portable.HttpClient;

    #pragma warning disable CS4014
    public class ImageLoader
    {
        #region Inner Classes

        class DownloadSubscribtion
        {
            public Uri SourceUri
            {
                get;
                private set;
            }

            public Action<FileUri> Success
            {
                get;
                private set;
            }

            public Action<FileUri> Error
            {
                get;
                private set;
            }

            public DownloadSubscribtion(Uri uri, Action<FileUri> success, Action<FileUri> error)
            {
                this.SourceUri = uri;
                this.Success = success;
                this.Error = error;
            }
        }

        #endregion

        #region Constants and Fields

        private IImageLoaderPlatform _loaderPlatform;

        private FileSystem _fileSystem;
        private Executor _executor;
        private ServiceClient _authorizator;

        private int _cacheMaxSize;

        private FolderUri _storageUri;

        private readonly Dictionary<int, string> _registeredSets;

        private int _nextImageViewId;

        private readonly LRUCache<string, object> _cache;

        private List<string> _downloading;
        private List<DownloadSubscribtion> _downloadedSubscriptions;

        #endregion

        #region Constructors

        public ImageLoader(IImageLoaderPlatform loaderPlatform, int cacheMaxSize = 10)
        {
            _loaderPlatform = loaderPlatform;
            _fileSystem = loaderPlatform.GetFileSystem();
            _executor = loaderPlatform.GetExecutor();

            _registeredSets = new Dictionary<int, string>();

            _cacheMaxSize = Math.Max(10, cacheMaxSize);
            _cache = new LRUCache<string, object>(_cacheMaxSize);

            _downloading = new List<string>();
            _downloadedSubscriptions = new List<DownloadSubscribtion>();
        }

        #endregion

        #region Properties

        public FolderUri StorageUri
        {
            get
            {
                if (_storageUri == null)
                    throw new InvalidOperationException("You must set a valid storage folder uri.");

                return _storageUri;
            }
            set
            {
                _storageUri = value;
            }
        }

        public int MaxCacheSize
        {
            get
            {
                return _cacheMaxSize;
            }
            set
            {
                _cacheMaxSize = Math.Max(10, value);
                _cache.Capacity = _cacheMaxSize;             
            }
        }

        public ServiceClient Authorizator
        {
            get
            {
                return _authorizator;
            }
            set
            {
                _authorizator = value;       
            }
        }

        #endregion

        #region Public Methods

        public void SetImageForView(FileUri imageUri, object imageView, object loaderView = null, int rotation = 0, bool hideWhileLoading = true, Action<string> done = null)
        {       
            if (!_fileSystem.FileExists(imageUri))
                return;
            
            bool isChanged = RegisterSetImageForView(_loaderPlatform.GetViewId(imageView, ref _nextImageViewId), imageUri.AbsolutePath);
            if (!isChanged
                && _loaderPlatform.GetImageViewHasContent(imageView))
            {
                return;
            }

            if (hideWhileLoading)
            {
                _loaderPlatform.SetViewIsVisible(imageView, false);

                if (loaderView != null)
                    _loaderPlatform.SetViewIsVisible(loaderView, true);
            }

            int targetWidth = 0;
            int targetHeight = 0;
            _loaderPlatform.GetImageViewSize(imageView, ref targetWidth, ref targetHeight);

            LoadImage(imageUri, targetWidth, targetHeight, rotation,
                (image) =>
                {
                    if (IsSetLastRequested(_loaderPlatform.GetViewId(imageView, ref _nextImageViewId), imageUri.AbsolutePath))
                    {                        
                        _loaderPlatform.SetImageViewContent(imageView, image);
                        if (hideWhileLoading)
                            _loaderPlatform.SetViewIsVisible(imageView, true);

                        if (loaderView != null)
                            _loaderPlatform.SetViewIsVisible(loaderView, false);

                        done?.Invoke(imageUri.AbsolutePath);
                    }
                },
                (error) =>
                {
                    // Do nothing?
                    System.Diagnostics.Debug.WriteLine(error);
                });
        }

        public void SetImageForView(Uri uri, FileUri noImageUri, object imageView, object loaderView = null, int rotation = 0, bool hideWhileLoading = true, Action<string> done = null)
        {            
            SetImageForView(noImageUri, imageView);

            DownlodImage(uri,
                // Success
                (imageUri) =>
                {
                    if (IsSetLastRequested(_loaderPlatform.GetViewId(imageView, ref _nextImageViewId), imageUri.AbsolutePath))
                            SetImageForView(imageUri, imageView, loaderView, rotation, hideWhileLoading, done);
                },
                // Error
                (imageUri) =>
                {
                    if (IsSetLastRequested(_loaderPlatform.GetViewId(imageView, ref _nextImageViewId), imageUri.AbsolutePath))
                            SetImageForView(noImageUri, imageView);
                });     
        }

        public void SetImageForView(Uri uri, string noImageName, object imageView, object loaderView = null, int rotation = 0, bool hideWhileLoading = true, Action<string> done = null)
        {            
            SetImageForView(noImageName, imageView);

            DownlodImage(uri,
                // Success
                (imageUri) =>
                {
                    if (IsSetLastRequested(_loaderPlatform.GetViewId(imageView, ref _nextImageViewId), imageUri.AbsolutePath))
                            SetImageForView(imageUri, imageView, loaderView, rotation, hideWhileLoading, done);                    
                },
                // Error
                (imageUri) =>
                {
                    if (IsSetLastRequested(_loaderPlatform.GetViewId(imageView, ref _nextImageViewId), imageUri.AbsolutePath))
                            SetImageForView(noImageName, imageView);
                });
        }

        public void SetImageForView(string imageName, object imageView)
        {
            UnregisterSetImageForView(_loaderPlatform.GetViewId(imageView, ref _nextImageViewId));
            object image = _loaderPlatform.GetImageFromBundle(imageName);
            _loaderPlatform.SetImageViewContent(imageView, image);            
        }

        public void InvalidateImage(FileUri imageUri)
        {
            List<int> toRemove = new List<int>();
            foreach (var kvp in _registeredSets)
            {
                if (kvp.Value == imageUri.AbsolutePath)
                    toRemove.Add(kvp.Key);
            }

            if (toRemove.Count > 0)
            {
                foreach (int key in toRemove)
                    UnregisterSetImageForView(key);
            }

            object image = _cache.Remove(imageUri.AbsolutePath);
            if (image != null)
                _loaderPlatform.ReleaseImage(image);
        }

        public void InvalidateImage(Uri uri)
        {
            string path = Path.Combine(this.StorageUri.Uri, Path.GetFileName(uri.AbsolutePath));
            FileUri cacheFileUri = _fileSystem.CreateFileUri(path);
            InvalidateImage(cacheFileUri);
        }

        #endregion

        #region Methods

        private async Task LoadImage(FileUri uri, int targetWidth, int targetHeight, int rotation, Action<object> success, Action<string> error)
        {
            try
            {
                object image = _cache.Get(uri.AbsolutePath);
                if (image == null)
                {
                    image = await (new TaskFactory<object>()).StartNew(
                        () => _loaderPlatform.GetImageFromUri(uri, targetWidth, targetHeight, rotation));

                    object removed = null;
                    _cache.Add(uri.AbsolutePath, image, out removed);
                    if (removed != null)
                        _loaderPlatform.ReleaseImage(removed);
                }

                if (image != null)
                {
                    if (success != null)
                        success(image);
                }
                else
                {
                    if (error != null)
                        error("Unknown error!");
                }
            }
            catch(Exception ex)
            {
                if (error != null)
                    error(ex.ToString());
            }
        }

        private async Task DownlodImage(Uri uri, Action<FileUri> success, Action<FileUri> error)
        {            
            if (!uri.IsWellFormedOriginalString())
                return;

            string path = Path.Combine(this.StorageUri.Uri, Path.GetFileName(uri.AbsoluteUri));
            FileUri localUri = _fileSystem.CreateFileUri(path);

            try
            {
                if (_fileSystem.FileExists(localUri))
                {
                    System.Diagnostics.Debug.WriteLine("LOCAL FILE >>> " + uri.AbsolutePath);
                    if (success != null)
                        success(localUri);                    
                }
                else
                {
                    if (_downloading.Contains(uri.AbsoluteUri))
                    {
                        _downloadedSubscriptions.Add(new DownloadSubscribtion(uri, success, error));
                        return;
                    }                   

                    _downloading.Add(uri.AbsoluteUri);

                    string host = String.Concat(uri.Scheme, "://", uri.Host);

                    RestClient client = null;
                    RestRequest request = null;
                    if (_authorizator == null)
                    {
                        client = new RestClient(host);
                        client.Timeout = TimeSpan.FromSeconds(5);
                        client.IgnoreResponseStatusCode = true;
                        request = new RestRequest(uri.AbsolutePath);
                    }
                    else
                    {
                        if(!_authorizator.IsAccessTokenValid)
                        {
                            if (error != null)
                                error(localUri);

                            return;
                        }
                            
                        client = _authorizator.GetRestClient();
                        request = _authorizator.GetRestRequest(uri.AbsolutePath, Method.GET);
                    }
                    
                    var response = await client.Execute(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        await (new TaskFactory()).StartNew(
                            () =>
                            {
                                if (!_fileSystem.FolderExists(this.StorageUri))
                                    _fileSystem.CreateFolder(this.StorageUri);

                                using (BinaryWriter w = new BinaryWriter(_fileSystem.OpenFile(localUri, UniversalFileMode.CreateNew, UniversalFileAccess.Write)))
                                    w.Write(response.RawBytes);
                            });

                        success?.Invoke(localUri);
                        NotifyDownloadSubscribers(uri, localUri, true);
                    }
                    else
                    {
                        error?.Invoke(localUri);
                        NotifyDownloadSubscribers(uri, localUri, false);
                    }
                }
            }
            catch (Exception ex)
            {
                error?.Invoke(localUri);
                NotifyDownloadSubscribers(uri, localUri, false);
            }
        }

        private bool RegisterSetImageForView(int viewId, string imagePath)
        {
            lock (((IDictionary)_registeredSets).SyncRoot)
            {
                if (!_registeredSets.ContainsKey(viewId))
                {
                    _registeredSets.Add(viewId, imagePath);
                    return true;
                }
                else
                {
                    string oldPath = _registeredSets[viewId];
                    _registeredSets[viewId] = imagePath;
                    return !(String.Compare(oldPath, imagePath, StringComparison.OrdinalIgnoreCase) == 0);
                }
            }
        }

        private void UnregisterSetImageForView(int viewId)
        {
            lock (((IDictionary)_registeredSets).SyncRoot)
            {
                if (_registeredSets.ContainsKey(viewId))
                    _registeredSets.Remove(viewId);
            }
        }

        private bool IsSetLastRequested(int viewId, string imagePath)
        {
            lock (((IDictionary)_registeredSets).SyncRoot)
            {
                string path = null;
                if (!_registeredSets.TryGetValue(viewId, out path))
                    return true;

                return String.Compare(imagePath, path, StringComparison.OrdinalIgnoreCase) == 0;
            }
        }

        private void NotifyDownloadSubscribers(Uri sourceUri, FileUri localUri, bool success = true)
        {
            _downloading.Remove(sourceUri.AbsoluteUri);

            var subscriptions = _downloadedSubscriptions
                .Where(s => s.SourceUri.AbsoluteUri == sourceUri.AbsoluteUri)
                .ToList();

            if (subscriptions == null || subscriptions.Count == 0)
                return;

            foreach (var s in subscriptions)
            {
                if (success)               
                    s.Success?.Invoke(localUri);                
                else                
                    s.Error?.Invoke(localUri);

                _downloadedSubscriptions.Remove(s);
            }
        }

        #endregion
    }
}
