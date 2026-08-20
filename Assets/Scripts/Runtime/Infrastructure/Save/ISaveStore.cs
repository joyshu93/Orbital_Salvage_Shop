using CurioClerk.Core.Progression;

namespace CurioClerk.Infrastructure.Save
{
    public interface ISaveStore
    {
        PlayerSaveData LoadOrDefault();

        void Save(PlayerSaveData data);
    }
}

