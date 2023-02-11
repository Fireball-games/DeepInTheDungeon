using System.Collections.Generic;

namespace Scripts.Localization
{
    public static class Keys
    {
        private const string TooltipPrefix = "Tooltip/";
        private const string TooltipDefault = "Tooltip is missing";
        
        public const string AddCampaign = "AddCampaign";
        public const string AddEndPoint = "AddEndPoint";
        public const string AddMap = "AddMap";
        public const string AddNewSubscriber = "AddNewSubscriber";
        public const string AddOpenMap = "AddOpenMap";
        public const string AddWaypoint = "AddWaypoint";
        public const string AvailablePrefabs = "AvailablePrefabs";
        public const string Campaign = "Campaign";
        public const string Cancel = "Cancel";
        public const string Close = "Close";
        public const string Columns = "Columns";
        public const string Confirm = "Confirm";
        public const string ContinueCampaign = "ContinueCampaign";
        public const string Count = "Count";
        public const string CreateMap = "CreateMap";
        public const string CreateNewCharacter = "CreateNewCharacter";
        public const string CreateOppositePath = "CreateOppositePath";
        public const string CustomCampaign = "CustomCampaign";
        public const string Default = "Default";
        public const string DefaultCampaignName = "DefaultCampaignName";
        public const string Delete = "Delete";
        public const string DontSave = "DontSave";
        public const string Down = "Down";
        public const string East = "East";
        public const string EmptyEditorPrompt = "EmptyEditorPrompt";
        public const string EndPoint = "EndPoint";
        public const string EnterCampaignName = "EnterCampaignName";
        public const string EnterName = "EnterName";
        public const string ErrorBuildingPrefab = "ErrorBuildingPref";
        public const string ExistingPrefabs = "ExistingPrefabs";
        public const string Exit = "Exit";
        public const string ExitGame = "ExitGame";
        public const string Floors = "Floors";
        public const string IsWalkable = "IsWalkable";
        public const string InvalidNumberOfPrefabsFound = "InvalidNumberOfPrefabsFound";
        public const string Load = "Load";
        public const string LoadingFileFailed = "LoadingFileFailed";
        public const string LoadLastEditedMap = "LoadLastEditedMap";
        public const string Map = "Map";
        public const string MapAlreadyExists = "MapAlreadyExists";
        public const string MapSaved = "MapSaved";
        public const string MapSelection = "MapSelection";
        public const string NewMap = "NewMap";
        public const string NewMapDialogTitle = "NewMapDialogTitle";
        public const string NewMapName = "NewMapName";
        public const string NewMapNamePrompt = "NewMapNamePrompt";
        public const string NoChangesToSave = "NoChangesToSave";
        public const string NoFilesToShow = "NoFilesToShow";
        public const string NoMapToPlayLoaded = "NoMapToPlayLoaded";
        public const string NoPrefabsAvailable = "NoPrefabsAvailable";
        public const string NothingToEditForConfiguration = "NothingToEditForConfiguration";
        public const string North = "North";
        public const string OpenMapEditor = "OpenMapEditor";
        public const string Offset = "Offset";
        public const string Point = "Point";
        public const string Position = "Position";
        public const string ReturnToEditor = "ReturnToEditor";
        public const string ReturnToMainScene = "ReturnToMainScene";
        public const string Rotate = "Rotate";
        public const string Rows = "Rows";
        public const string Save = "Save";
        public const string SaveFailed = "SaveFailed";
        public const string SaveMap = "SaveMap";
        public const string SaveEditedMapPrompt = "SaveEditedMapPrompt";
        public const string SelectCampaignPrompt = "SelectCampaignPrompt";
        public const string SelectCampaignToLoad = "SelectCampaignToLoad";
        public const string SelectConfiguration = "SelectConfiguration";
        public const string SelectMap = "SelectMap";
        public const string SelectMapPrompt = "SelectMapPrompt";
        public const string SelectMapToLoad = "SelectMapToLoad";
        public const string SelectPrefab = "SelectPrefab";
        public const string SelectSubscriber = "SelectSubscriber";
        public const string Settings = "Settings";
        public const string South = "South";
        public const string SubscribedReceivers = "SubscribedReceivers";
        public const string SpeedTowardsPoint = "SpeedTowardsPoint";
        public const string StartPoint = "StartPoint";
        public const string StartPosition = "StartPosition";
        public const string StartNewCampaign = "StartNewCampaign";
        public const string Step = "Step";
        public const string TriggerType = "TriggerType";
        public const string Up = "Up";
        public const string WaypointEditor = "WaypointEditor";
        public const string West = "West";
        
        private static readonly Dictionary<string, string> Tooltips;

        static Keys()
        {
            // Tooltips = new Dictionary<string, string> { { Default, "Tooltip is missing" } };
            //
            // FieldInfo[] fields = typeof(Keys).GetFields(BindingFlags.Public | BindingFlags.Static);
            //
            // foreach (FieldInfo field in fields)
            // {
            //     if (field.FieldType != typeof(string)) continue;
            //     
            //     string key = field.Name;
            //
            //     string value = (string)field.GetValue(null);
            //     
            //     if (value.Contains("Tooltip/")) Tooltips.Add(key, value);
            // }
        }

        public static string GetTooltipText(string key)
        {
            string text = t.Get($"{TooltipPrefix}{key}");
            return string.IsNullOrEmpty(text) ? TooltipDefault : text;
        }
    }
}