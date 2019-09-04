using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace BnB_ChipLibraryGui
{
    public sealed class ChipImages
    {
        public const byte elementCount = 12;

        private const string aquaURL = "http://vignette.wikia.nocookie.net/megaman/images/f/fe/BC_Element_Aqua.png";

        private const string breakURL = "http://vignette.wikia.nocookie.net/megaman/images/0/0e/BC_Attribute_Break.png";

        private const string cursorURL = "http://vignette.wikia.nocookie.net/megaman/images/2/2b/TypeCursor.png";

        private const string elecURL = "http://vignette.wikia.nocookie.net/megaman/images/f/f6/BC_Element_Elec.png";

        private const string fireURL = "http://vignette.wikia.nocookie.net/megaman/images/3/38/BC_Element_Heat.png";

        private const string invisURL = "http://vignette.wikia.nocookie.net/megaman/images/e/e0/TypeInvis.png";

        private const string nullURL = "http://vignette.wikia.nocookie.net/megaman/images/4/47/BC_Element_Null.png";

        private const string objectURL = "http://vignette.wikia.nocookie.net/megaman/images/4/4c/TypeObstacle.png";

        private const string recoveryURL = "http://vignette.wikia.nocookie.net/megaman/images/8/81/TypeRecover.png";

        private const string swordURL = "http://vignette.wikia.nocookie.net/megaman/images/d/d5/BC_Attribute_Sword.png";

        private const string windURL = "http://vignette.wikia.nocookie.net/megaman/images/b/b1/BC_Attribute_Wind.png";

        private const string woodURL = "http://vignette.wikia.nocookie.net/megaman/images/8/83/BC_Element_Wood.png";

        private static readonly string[] URLs =
        {
            fireURL, aquaURL, elecURL, woodURL, windURL, swordURL, breakURL, cursorURL, recoveryURL, invisURL, objectURL, nullURL
        };

        private static readonly string[] imageFileNames =
        {
            "Fire.png", "Aqua.png", "Elec.png", "Wood.png", "Wind.png", "Sword.png", "Break.png",
            "Cursor.png", "Recovery.png", "Invis.png", "Object.png", "Null.png"
        };

        private static readonly Lazy<ChipImages> lazy = new Lazy<ChipImages>(() => new ChipImages());

        private readonly BitmapImage[] images;

        private readonly Dictionary<Chip.ChipElements[], BitmapImage> combinedImages;

        public static ChipImages Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        private ChipImages()
        {
            images = new BitmapImage[elementCount];
            LoadImages();

            combinedImages = new Dictionary<Chip.ChipElements[], BitmapImage>();
        }

        public BitmapImage this[Chip.ChipElements[] elem]
        {
            get
            {
                if (elem.Length == 1)
                {
                    return images[(int)elem[0]];
                }
                else
                {
                    return GetCombinedImage(elem);
                }
            }
        }

        public BitmapImage this[Chip.ChipElements elem]
        {
            get
            {
                return images[(int)elem];
            }
        }

        private BitmapImage GetCombinedImage(Chip.ChipElements[] elem)
        {
            if (combinedImages.TryGetValue(elem, out BitmapImage toReturn))
            {
                return toReturn;
            }
            else
            {
                bool cacheResult = true;
                Bitmap[] imagesToCombine = new Bitmap[elem.Length];
                int width = 0;
                int height = 0;
                for (int i = 0; i < elem.Length; i++)
                {
                    if (images[(int)elem[i]].IsDownloading)
                    {
                        cacheResult = false;
                    }
                    imagesToCombine[i] = BitmapImage2Bitmap(images[(int)elem[i]]);
                    width += imagesToCombine[i].Width;
                    if (imagesToCombine[i].Height > height)
                    {
                        height = imagesToCombine[i].Height;
                    }
                }

                Bitmap img3 = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(img3);
                g.Clear(Color.Transparent);
                width = 0;
                for (int i = 0; i < elem.Length; i++)
                {
                    g.DrawImage(imagesToCombine[i], new System.Drawing.Point(width, 0));
                    width += imagesToCombine[i].Width;
                }
                g.Dispose();
                BitmapImage finalResult = ToBitmapImage(img3);
                if (cacheResult)
                {
                    combinedImages.Add(elem, finalResult);
                }
                return finalResult;
            }
        }

        private static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private void LoadImages()
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {

                for(int i = 0; i < images.Length; i++)
                {
                    if(isoStore.FileExists(imageFileNames[i]))
                    {
                        var stream = isoStore.OpenFile(imageFileNames[i], FileMode.Open);
                        images[i] = MakeBitmapFromStream(stream);
                        stream.Close();
                    }
                    else
                    {
                        images[i] = DownloadBitmap(URLs[i]);
                        SaveBitmap(images[i], imageFileNames[i]);
                    }
                }
                    
            }
        }

        private void SaveBitmap(BitmapImage toSave, string filename)
        {
            var bitmap = BitmapImage2Bitmap(toSave);
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                var stream = isoStore.OpenFile(filename, FileMode.Create);
                bitmap.Save(stream, ImageFormat.Png);
                stream.Close();
            }
            bitmap.Dispose();
        }

        private BitmapImage MakeBitmapFromStream(Stream streamSource)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = streamSource;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private BitmapImage DownloadBitmap(string Url)
        {
            byte[] data;
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                data = wc.DownloadData(Url);
            }
            using (var stream = new MemoryStream(data))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }
    }
}