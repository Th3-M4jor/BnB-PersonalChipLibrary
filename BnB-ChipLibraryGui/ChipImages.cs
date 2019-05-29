using System;
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

        private static readonly Lazy<ChipImages> lazy = new Lazy<ChipImages>(() => new ChipImages());

        private readonly BitmapImage[] images;

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
                return images[(int)elem];
            }
        }
    }
}