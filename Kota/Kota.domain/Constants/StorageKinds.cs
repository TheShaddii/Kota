// Kota.Domain/Constants/StorageKinds.cs
namespace Kota.Domain.Constants
{
    public static class StorageKinds
    {
        public const string Container = "container";
        public const string Compartment = "compartment";
        public const string Bin = "bin";
        public const string Slot = "slot";
    }

    public static class StorageTypes
    {
        public const string Cabinet = "cabinet";
        public const string Shelf = "shelf";
        public const string Rack = "rack";
        public const string DrawerUnit = "drawer_unit";
        public const string Pegboard = "pegboard";
        public const string Other = "other";
    }

    public static class ReasonCodes
    {
        public const string Add = "add";
        public const string Remove = "remove";
        public const string BulkAdd = "bulk_add";
        public const string BulkRemove = "bulk_remove";
        public const string EditAdjust = "edit_adjust";
        public const string DeleteItem = "delete_item";
        public const string InitialLoad = "initial_load";
    }

    public static class UserRoles
    {
        public const string Admin = "admin";
        public const string User = "user";
    }

    public static class AuditActions
    {
        public const string Create = "create";
        public const string Update = "update";
        public const string Delete = "delete";
        public const string Login = "login";
        public const string PasswordReset = "password_reset";
    }
}