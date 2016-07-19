namespace AdMaiora.AppKit.Utils
{
    using System;
    
    using AdMaiora.AppKit.IO;

    public interface IImageLoaderPlatform
    {
        Executor GetExecutor();

        FileSystem GetFileSystem();

        int GetViewId(object view, ref int assignedId);        

        bool GetViewIsVisible(object view);

        void SetViewIsVisible(object view, bool visibile);

        void GetImageViewSize(object imageView, ref int width, ref int height);

        bool GetImageViewHasContent(object imageView);

        void SetImageViewContent(object imageView, object content);

        object GetImageFromUri(FileUri imageUri, int targetWidth, int targetHeight, int rotation = 0);       

        object GetImageFromBundle(string imageName);

        void ReleaseImage(object image);
    }
}
