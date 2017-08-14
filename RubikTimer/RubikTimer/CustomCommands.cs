using System.Windows.Input;

namespace RubikTimer
{
    public static class CustomCommands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit", "Exit", typeof(CustomCommands), new InputGestureCollection() { new KeyGesture(Key.F4, ModifierKeys.Alt) });
        public static readonly RoutedUICommand Edit = new RoutedUICommand("Edit", "Edit", typeof(CustomCommands), new InputGestureCollection() { new KeyGesture(Key.E, ModifierKeys.Control) });
        public static readonly RoutedUICommand About = new RoutedUICommand("About", "About", typeof(CustomCommands), new InputGestureCollection() { new KeyGesture(Key.F2) });
        public static readonly RoutedUICommand Generate = new RoutedUICommand("Generate", "Generate", typeof(CustomCommands));
    }
}
