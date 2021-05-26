namespace Scuti.Examples.DataBinding {
    [System.Serializable]
    public class Profile : GroupBinding {
        public StringBinding name = new StringBinding();
        public FloatBinding rating = new FloatBinding();
        public IntBinding ordersCompleted = new IntBinding();

        public Profile() {
            name.OnValueChanged += value => Notify("name");
            rating.OnValueChanged += value => Notify("rating");
            ordersCompleted.OnValueChanged += value => Notify("ordersCompleted");
        }
    }
}