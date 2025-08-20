namespace CustomAuth.Filters
{
    public class ChangeTracker<T> where T : class
    {
        private readonly Dictionary<string, (object OldValue, object NewValue)> _changes =
            new Dictionary<string, (object, object)>();

        public void TrackChange(string propertyName, object oldValue, object newValue)
        {
            if (!Equals(oldValue, newValue))
            {
                _changes[propertyName] = (oldValue, newValue);
            }
        }

        public bool HasChanges => _changes.Count > 0;

        public Dictionary<string, (object OldValue, object NewValue)> GetChanges() => _changes;

        public string[] GetChangedPropertyNames() => _changes.Keys.ToArray();
    }
}
