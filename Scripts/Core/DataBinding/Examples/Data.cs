namespace Scuti.Examples.DataBinding {
    public class Data {
        // NOTE : Made static for the sake of simplicity
        static public Profile Profile { get; } = new Profile();

        static public ListBinding<string> GUIDs = new ListBinding<string>();
    }
}
