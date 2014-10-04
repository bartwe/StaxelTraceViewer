using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace Staxel.Trace {
    public class TraceKeys {
        static readonly List<TraceKey> Keys = new List<TraceKey>();

        // Hardcoded for now, open to improvement
        public static TraceKey ClientMainLoop_Draw = new TraceKey(Color.LightGreen);
        public static TraceKey Multiverse_Update = new TraceKey(Color.Purple);
        public static TraceKey Universe_Update = new TraceKey(Color.Blue);
        public static TraceKey ClientMainLoop_Update = new TraceKey(Color.LightYellow);
        public static TraceKey ClientMainLoop_ProcessPackets = new TraceKey(Color.LightSlateGray);
        public static TraceKey Recorder_Flush = new TraceKey(Color.DeepPink);
        public static TraceKey UniverseRenderer_Draw = new TraceKey(Color.LightSteelBlue);
        public static TraceKey ParticleRenderer_Render = new TraceKey(Color.Yellow);
        public static TraceKey UniverseRenderer_DrawInWorld = new TraceKey(Color.Tomato);
        public static TraceKey StaxelGame_Draw = new TraceKey(Color.Green);
        public static TraceKey StaxelGame_Update = new TraceKey(Color.Magenta);
        public static TraceKey EntityRenderer_Draw = new TraceKey(Color.MediumOrchid);
        public static TraceKey ChunkRenderer_Render = new TraceKey(Color.Brown);
        public static TraceKey ChunkRenderer_PendingBuildQueue = new TraceKey(Color.White);
        public static TraceKey WorldRenderer_FetchRenderChunks = new TraceKey(Color.Blue);
        public static TraceKey WorldRenderer_BuildInline = new TraceKey(Color.Red);
        public static TraceKey ChunkRendererHelper_Prepare = new TraceKey(Color.Red);
        public static TraceKey WorldRenderer_Cleanup = new TraceKey(Color.Green);
        public static TraceKey ChunkRendererHelper_Render = new TraceKey(Color.Yellow);
        public static TraceKey ChunkRendererHelper_RenderUnprepared = new TraceKey(Color.LawnGreen);
        public static TraceKey WorldRenderer_NeedsBuilding = new TraceKey(Color.Orange);
        public static TraceKey WorldRenderer_RenderChunks = new TraceKey(Color.LightSkyBlue);

        static TraceKeys() {
            var props = typeof(TraceKeys).GetFields(BindingFlags.Public | BindingFlags.Static);
            int id = 1;
            foreach (var key in props) {
                var scope = (TraceKey)key.GetValue(null);
                scope.Code = key.Name;
                scope.Id = id++;
                Keys.Add(scope);
            }
        }

        public static IEnumerable<TraceKey> All() {
            return Keys;
        }
    }
}
