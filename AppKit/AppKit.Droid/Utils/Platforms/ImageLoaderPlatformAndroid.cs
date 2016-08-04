namespace AdMaiora.AppKit.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net;

    using Android.Graphics;
    using Android.Graphics.Drawables;
    using Android.Widget;
    using Android.Views;

    using AdMaiora.AppKit.IO;
    using AdMaiora.AppKit.Data;
    using AdMaiora.AppKit.Utils;

    using RestSharp;

    public class ImageLoaderPlatofrmiOS : IImageLoaderPlatform
    {
        public Executor GetExecutor()
        {
            return new Executor(new ExecutorPlatformAndroid());
        }

        public FileSystem GetFileSystem()
        {
            return new FileSystem(new FileSystemPlatformAndroid());
        }

        public int GetViewId(object view, ref int assignedId)
        {
            ImageView imageView = view as ImageView;
            if (imageView == null)
                return 0;

            if (imageView.Tag == null)
                imageView.Tag = ++assignedId;

            return (int)imageView.Tag;
        }

        public bool GetViewIsVisible(object view)
        {
            ImageView imageView = view as ImageView;
            if (imageView == null)
                return false;

            return imageView.Visibility == ViewStates.Visible;
        }

        public void SetViewIsVisible(object view, bool visibile)
        {
            ImageView imageView = view as ImageView;
            if (imageView == null)
                return;

            imageView.Visibility = visibile ? ViewStates.Visible : ViewStates.Invisible;
        }

        public void GetImageViewSize(object imageView, ref int width, ref int height)
        {
            ImageView iv = imageView as ImageView;
            if (iv == null)
                return;

            if (iv.Width != 0 && iv.Height != 0)
            {
                width  = iv.Width;
                height = iv.Height;
            }
            else
            {
                width  = iv.LayoutParameters.Width;
                height = iv.LayoutParameters.Height;
            }
        }

        public bool GetImageViewHasContent(object imageView)
        {
            ImageView iv = imageView as ImageView;
            if (iv == null)
                return false;

            return iv.Drawable != null /*&& ((BitmapDrawable)iv.Drawable).Bitmap != null*/;
        }

        public void SetImageViewContent(object imageView, object content)
        {
            ImageView iv = imageView as ImageView;
            if (iv == null)
                return;

            Bitmap image = content as Bitmap;
            if (image == null)
                return;

            iv.SetImageBitmap(image);
        }

        public object GetImageFromUri(FileUri imageUri, int targetWidth, int targetHeight, int rotation)
        {
            using (Stream stream = GetFileSystem().OpenFile(imageUri, UniversalFileMode.Open, UniversalFileAccess.Read, UniversalFileShare.Read))
            {
                BitmapFactory.Options options = new BitmapFactory.Options();

                options.InJustDecodeBounds = true;
                BitmapFactory.DecodeStream(stream, null, options);
                int imageWidth = options.OutWidth;
                int imageHeight = options.OutHeight;

                int scale = 1;
                while (imageWidth / scale / 2 >= targetWidth
                    && imageHeight / scale / 2 >= targetHeight)
                {
                    scale *= 2;
                }

                options.InJustDecodeBounds = false;
                if (scale != 1)
                    options.InSampleSize = scale;

                stream.Seek(0, SeekOrigin.Begin);
                Bitmap image = BitmapFactory.DecodeStream(stream, null, options);

                if (rotation != 0)
                {
                    Matrix transform = new Matrix();
                    transform.SetRotate(90, image.Width / 2, image.Height / 2);
                    Bitmap rotatedBitmap = Bitmap.CreateBitmap(image, 0, 0, image.Width, image.Height, transform, true);

                    image.Recycle();
                    image = rotatedBitmap;
                }

                return image;
            }
        }

        public object GetImageFromBundle(string imageName)
        {
            int resId = Android.App.Application.Context.Resources.GetIdentifier(
                imageName, "drawable", Android.App.Application.Context.PackageName);

            return BitmapFactory.DecodeResource(Android.App.Application.Context.Resources, resId);
        }

        public void ReleaseImage(object image)
        {
            Bitmap img = image as Bitmap;
            if (img == null)
                return;

            img.Recycle();
            img.Dispose();
        }
    }
}
