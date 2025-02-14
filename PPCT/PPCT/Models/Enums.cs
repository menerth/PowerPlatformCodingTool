namespace PPCT.Models
{
    public static class Enums
    {
        public enum PPCTTask
        {
            None,
            Init,
            Deploy,
            Extract
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
