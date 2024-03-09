using BookStore.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace BookStore.Permissions;

public class BookStorePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        // var myGroup = context.AddGroup(BookStorePermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(BookStorePermissions.MyPermission1, L("Permission:MyPermission1"));


        var bookStoreGroup = context.AddGroup(BookStorePermissions.GroupName, L("Permission:BookStoreGroup"));
        //"Permission:BookStoreGroup": "BookStore management",

        var booksPermission = bookStoreGroup.AddPermission(BookStorePermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(BookStorePermissions.Books.Create, L("Permission:Books:Create"));
        booksPermission.AddChild(BookStorePermissions.Books.Update, L("Permission:Books:Update"));
        booksPermission.AddChild(BookStorePermissions.Books.Delete, L("Permission:Books:Delete"));
        
        //"Permission:Books": "Books management",
        //"Permission:Books:Create": "Creating Books",
        //"Permission:Books:Update": "Editing Books",
        //"Permission:Books:Delete": "Deleting Books",
        
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BookStoreResource>(name);
    }
}
