using BookStore.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace BookStore.Permissions
{
    public class BookStorePermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {

            var bookStoreGroup = context.AddGroup(BookStorePermissions.BookStoreGroup, L("Permission:BookStoreGroup"));
            
            var booksPermission = bookStoreGroup.AddPermission(BookStorePermissions.Books.Default, L("Permission:Books"));
            booksPermission.AddChild(BookStorePermissions.Books.Create, L("Permission:Books:Create"));
            booksPermission.AddChild(BookStorePermissions.Books.Update, L("Permission:Books:Update"));
            booksPermission.AddChild(BookStorePermissions.Books.Delete, L("Permission:Books:Delete"));




        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<BookStoreResource>(name);
        }
    }
}
