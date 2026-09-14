using CurioClerk.Core.Workbench;
using UnityEngine;

namespace CurioClerk.Presentation
{
    internal static class WorkbenchArtwork
    {
        private static readonly Sprite[] States = new Sprite[6];
        private static readonly Sprite[] WatchStates = new Sprite[3];
        private static Texture2D _atlas;
        private static Texture2D _watchAtlas;

        public static Sprite ForSession(WorkbenchSession session)
        {
            switch (session.Puzzle.Id)
            {
                case "ice-01-crack": return session.IsComplete ? State(0) : null;
                case "ice-02-spread":
                    if (session.IsComplete) return WatchState(0);
                    return State(session.HasCompleted("lift-base") ? 1 : 0);
                case "ice-03-tomorrow":
                    return WatchState(session.HasCompleted("free-leaf") ? 2 : session.HasCompleted("clear-hinge") ? 1 : 0);
                case "ice-04-frozen-seal": return session.HasCompleted("lift-back") && !session.IsComplete ? State(3) : null;
                case "rain-03-unsent-letter": return session.HasCompleted("unfold-fish") ? State(4) : null;
                case "rain-05-testimony": return session.HasCompleted("return-reply") ? State(5) : null;
                default: return null;
            }
        }

        private static Sprite WatchState(int index)
        {
            if (WatchStates[index] != null) return WatchStates[index];
            if (_watchAtlas == null) _watchAtlas = Resources.Load<Texture2D>("Art/Workbench/watch-discovery-states");
            if (_watchAtlas == null) return null;
            var width = _watchAtlas.width / 3f;
            WatchStates[index] = Sprite.Create(_watchAtlas, new Rect(index * width, 0, width, _watchAtlas.height),
                new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            WatchStates[index].name = "WatchDiscovery" + index;
            return WatchStates[index];
        }

        private static Sprite State(int index)
        {
            if (States[index] != null) return States[index];
            if (_atlas == null) _atlas = Resources.Load<Texture2D>("Art/Workbench/workbench-states");
            if (_atlas == null) return null;
            var rect = new Rect(index % 3 * 512, index < 3 ? 512 : 0, 512, 512);
            // The authored base ornament crosses the regular row boundary. Preserve it,
            // and leave its ten-pixel tail out of the letter below.
            if (index == 1) rect = new Rect(512, 492, 512, 532);
            if (index == 4) rect = new Rect(512, 0, 512, 488);
            var scale = new Vector2(_atlas.width / 1536f, _atlas.height / 1024f);
            rect = new Rect(Vector2.Scale(rect.position, scale), Vector2.Scale(rect.size, scale));
            States[index] = Sprite.Create(_atlas, rect, new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            States[index].name = "WorkbenchState" + index;
            return States[index];
        }
    }
}
