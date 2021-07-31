using AnimationExtensions;
using Windows.UI.Xaml;

namespace ComicBookReader.App.Animations
{
    public static class Extensions
    {
        public static Prototype PageHeaderOut(this FrameworkElement element)
        {
            return element
                .Fade(0, 500)
                .Move(-25, 0, 500, Eq.OutSine);
        }

        public static Prototype PageHeaderIn(this FrameworkElement element)
        {
            return element
                .Fade()
                .Move(100, 0)
                .Then()
                .Fade(1, 500)
                .Move(0, 0, 500, Eq.OutSine);
        }

        public static Prototype PageOut(this FrameworkElement element)
        {
            return element
                .Fade(0, 750)
                .Move(0, 100, 750, Eq.InBack);
        }

        public static Prototype MessageIn(this FrameworkElement element)
        {
            return element
                .Move(0, 0, 500, Eq.OutSine)
                .Fade(1, 500);
        }

        public static Prototype MessageOut(this FrameworkElement element)
        {
            return element
                .Move(0, 100, 500, Eq.InBack)
                .Fade(0, 500);
        }

    }
}
