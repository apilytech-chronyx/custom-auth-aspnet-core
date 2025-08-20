namespace CustomAuth.Filters
{
    public static class ChangeTrackerExtensions
    {
        private const string ChangeTrackerKey = "ChangeTracker";

        public static void SetChangeTracker(this HttpContext httpContext, Dictionary<string, (object OldValue, object NewValue)> changes)
        {
            httpContext.Items[ChangeTrackerKey] = changes;
        }

        public static Dictionary<string, (object OldValue, object NewValue)> GetChangeTracker(this HttpContext httpContext)
        {
            if (httpContext.Items.TryGetValue(ChangeTrackerKey, out var tracker))
            {
                return (Dictionary<string, (object OldValue, object NewValue)>)tracker;
            }

            // Create and store a new tracker if none exists
            var newTracker = new Dictionary<string, (object OldValue, object NewValue)>();
            httpContext.Items[ChangeTrackerKey] = newTracker;
            return newTracker;
        }

        public static void TrackChange(this HttpContext httpContext, string propertyName, object oldValue, object newValue)
        {
            if (!Equals(oldValue, newValue))
            {
                var tracker = httpContext.GetChangeTracker();
                tracker[propertyName] = (oldValue, newValue);
            }
        }

        public static bool HasChanges(this HttpContext httpContext)
        {
            return httpContext.GetChangeTracker().Count > 0;
        }
    }
}
