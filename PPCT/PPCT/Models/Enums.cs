namespace PPCT.Models
{
    public static class Enums
    {
        public enum AssembliesMode
        {
            Dll,
            Nuget
        }

        public enum PPCTTask
        {
            None,
            Init,
            Deploy,
            Extract
        }

        public enum PPCTAssembliesTask
        {
            Add,
            Init,
            Deploy,
            Decorate
        }

        public enum PPCTModelBuilderTask
        {
            Init,
            Run
        }

        public enum PPCTMonitoringTask
        {
            Get
        }

        public enum PPCTWebresourcesTask
        {
            Init
        }

        public enum ProcessingAction
        {
            Unknown,
            Create,
            Update,
            Delete
        }
    }
}
