using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;

namespace BnB_ChipLibraryGui
{
    public sealed class ChipImages
    {
        //public static readonly ChipImages instance = new ChipImages();
        private static readonly Lazy<ChipImages> lazy = new Lazy<ChipImages> (() => new ChipImages());
        public static ChipImages Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        private static readonly byte elementCount = 12;
        private static readonly string fireURL = "http://vignette.wikia.nocookie.net/megaman/images/3/38/BC_Element_Heat.png";
        private static readonly string aquaURL = "http://vignette.wikia.nocookie.net/megaman/images/f/fe/BC_Element_Aqua.png";
        private static readonly string elecURL = "http://vignette.wikia.nocookie.net/megaman/images/f/f6/BC_Element_Elec.png";
        private static readonly string woodURL = "http://vignette.wikia.nocookie.net/megaman/images/8/83/BC_Element_Wood.png";
        private static readonly string windURL = "http://vignette.wikia.nocookie.net/megaman/images/b/b1/BC_Attribute_Wind.png";
        private static readonly string swordURL = "http://vignette.wikia.nocookie.net/megaman/images/d/d5/BC_Attribute_Sword.png";
        private static readonly string breakURL = "http://vignette.wikia.nocookie.net/megaman/images/0/0e/BC_Attribute_Break.png";
        private static readonly string cursorURL = "http://vignette.wikia.nocookie.net/megaman/images/2/2b/TypeCursor.png";
        private static readonly string recoveryURL = "http://vignette.wikia.nocookie.net/megaman/images/8/81/TypeRecover.png";
        private static readonly string invisURL = "http://vignette.wikia.nocookie.net/megaman/images/e/e0/TypeInvis.png";
        private static readonly string objectURL = "http://vignette.wikia.nocookie.net/megaman/images/4/4c/TypeObstacle.png";
        private static readonly string nullURL = "http://vignette.wikia.nocookie.net/megaman/images/4/47/BC_Element_Null.png";

        private readonly BitmapImage[] images;

        private ChipImages()
        {
            
            images = new BitmapImage[elementCount];
            images[0] = new BitmapImage(new Uri(fireURL));
            images[1] = new BitmapImage(new Uri(aquaURL));
            images[2] = new BitmapImage(new Uri(elecURL));
            images[3] = new BitmapImage(new Uri(woodURL));
            images[4] = new BitmapImage(new Uri(windURL));
            images[5] = new BitmapImage(new Uri(swordURL));
            images[6] = new BitmapImage(new Uri(breakURL));
            images[7] = new BitmapImage(new Uri(cursorURL));
            images[8] = new BitmapImage(new Uri(recoveryURL));
            images[9] = new BitmapImage(new Uri(invisURL));
            images[10] = new BitmapImage(new Uri(objectURL));
            images[11] = new BitmapImage(new Uri(nullURL));
        }


        public BitmapImage this[Chip.ChipElements elem]
        {
            get
            {
                switch (elem)
                {
                    case Chip.ChipElements.Fire:
                        return images[0];
                    case Chip.ChipElements.Aqua:
                        return images[1];
                    case Chip.ChipElements.Elec:
                        return images[2];
                    case Chip.ChipElements.Wood:
                        return images[3];
                    case Chip.ChipElements.Wind:
                        return images[4];
                    case Chip.ChipElements.Sword:
                        return images[5];
                    case Chip.ChipElements.Break:
                        return images[6];
                    case Chip.ChipElements.Cursor:
                        return images[7];
                    case Chip.ChipElements.Recovery:
                        return images[8];
                    case Chip.ChipElements.Invis:
                        return images[9];
                    case Chip.ChipElements.Object:
                        return images[10];
                    case Chip.ChipElements.Null:
                    default:
                        return images[11];
                }
            }
        }
    }
}
