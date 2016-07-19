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

    using Foundation;
    using UIKit;    
    using CoreGraphics;
    using ImageIO;
    
    using AdMaiora.AppKit.IO;
    using AdMaiora.AppKit.Data;
    using AdMaiora.AppKit.Utils;    

    using RestSharp;

    public class ImageLoaderPlatofrmiOS : IImageLoaderPlatform
    {
        public Executor GetExecutor()
        {
            return new Executor(new ExecutorPlatformiOS());
        }

        public FileSystem GetFileSystem()
        {
            return new FileSystem(new FileSystemPlatformiOS());
        }

        public int GetViewId(object view, ref int assignedId)
        {
            UIImageView imageView = view as UIImageView;
            if (imageView == null)            
                return 0;

            if (imageView.Tag == 0)
                imageView.Tag = ++assignedId;

            return (int)imageView.Tag;
        }

        public bool GetViewIsVisible(object view)
        {
            UIImageView imageView = view as UIImageView;
            if (imageView == null)
                return false;

            return !imageView.Hidden;
        }

        public void SetViewIsVisible(object view, bool visibile)
        {
            UIImageView imageView = view as UIImageView;
            if (imageView == null)
                return;

            imageView.Hidden = !visibile;
        }

        public void GetImageViewSize(object imageView, ref int width, ref int height)
        {
            UIImageView iv = imageView as UIImageView;
            if (iv == null)
                return;

            width = (int)iv.Frame.Width;
            height = (int)iv.Frame.Height;            
        }

        public bool GetImageViewHasContent(object imageView)
        {
            UIImageView iv = imageView as UIImageView;
            if (iv == null)
                return false;

            return iv.Image != null;
        }

        public void SetImageViewContent(object imageView, object content)
        {
            UIImageView iv = imageView as UIImageView;
            if (iv == null)
                return;

            UIImage image = content as UIImage;
            if (image == null)
                return;

            iv.Image = image;
        }

        public object GetImageFromUri(FileUri imageUri, int targetWidth, int targetHeight, int rotation)
        {
            using (Stream stream = GetFileSystem().OpenFile(imageUri, UniversalFileMode.Open, UniversalFileAccess.Read, UniversalFileShare.Read))
            {                
                CGImageSource source = CGImageSource.FromUrl(imageUri.ToNSUrl());

                CGImageOptions options = new CGImageOptions();
                options.ShouldCache = false;

                var props = source.CopyProperties(options, 0);

                int imageWidth = ((NSNumber)props["PixelWidth"]).Int32Value;
                int imageHeight = ((NSNumber)props["PixelHeight"]).Int32Value;

                int scale = 1;
                while (imageWidth / scale / 2 >= (nfloat)targetWidth
                    && imageHeight / scale / 2 >= (nfloat)targetHeight)
                {
                    scale *= 2;
                }

                stream.Seek(0, SeekOrigin.Begin);
                UIImage image = UIImage.LoadFromData(NSData.FromUrl(imageUri.ToNSUrl()), scale);

                if (rotation != 0)
                {
                    float radians = rotation * (float)Math.PI / 180f;

                    CGRect imageFrame = new CGRect(0, 0, image.Size.Width, image.Size.Height);
                    CGAffineTransform t = CGAffineTransform.MakeRotation(radians);
                    imageFrame = t.TransformRect(imageFrame);

                    CGSize rotatedSize = new CGSize(imageFrame.Width, imageFrame.Height);

                    UIGraphics.BeginImageContext(rotatedSize);

                    using (CGContext context = UIGraphics.GetCurrentContext())
                    {
                        context.TranslateCTM(rotatedSize.Width / 2, rotatedSize.Height / 2);
                        context.RotateCTM(-radians);
                        context.ScaleCTM(1.0f, -1.0f);
                        image.Draw(new CGRect(-image.Size.Width / 2, -image.Size.Height / 2, image.Size.Width, image.Size.Height));

                        image = UIGraphics.GetImageFromCurrentImageContext();
                    }

                    UIGraphics.EndImageContext();
                }

                return image;
            }
        }

        public object GetImageFromBundle(string imageName)
        {
            return UIImage.FromBundle(imageName);
        }

        public void ReleaseImage(object image)
        {
            UIImage img = image as UIImage;
            if (img == null)
                return;

            img.Dispose();
        }        
    }
}
