namespace BookStore.Permissions
{
    public static class BookStorePermissions
    {
        public const string BookStoreGroup = "BookStore";

        public static class Books
        {
            public const string Default = BookStoreGroup + ".Books";
            public const string Create = Default + ".Create";
            public const string Update = Default+ ".Update";
            public const string Delete = Default + ".Delete";
        }
    }
}