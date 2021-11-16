using System;

namespace Medior.BaseTypes
{
    public static class AppModuleIds
    {
        public static Guid About { get; } = new("1bb0d8a3-73b5-4075-a2ce-2b0232d2fc81");
        public static Guid Dashboard { get; } = new("c54b3911-ea21-471e-849d-3c6729b7b232");
        public static Guid GuidGenerator { get; } = new("160d6d04-d41d-446d-96da-84d8d045bca0");
        public static Guid PhotoSorter { get; } = new("83e0a286-eb2d-4a0b-9fa6-61e75cbabffd");
        public static Guid QrCodeCreator { get; } = new("651411e0-4fa5-4a7b-ad61-b69e0168b88a");
        public static Guid RemoteHelp { get; } = new("0c17e529-96c8-47b1-8b50-e16820eda87e");
        public static Guid ScreenCapture { get; } = new("4fedef05-0ccc-48f8-bc80-7f520556eef9");
        public static Guid Settings { get; } = new("4bd293f9-b568-4be3-9b8e-9eb60765cd7d");
    }
}
