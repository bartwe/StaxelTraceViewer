using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace Staxel.Trace {
    [Obfuscation(Exclude = true)]
    public sealed class TraceKeys {
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
        public static TraceKey StaxelGame_Present = new TraceKey(Color.Cyan);
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
        public static TraceKey WorldRenderer_RenderChunks_Render = new TraceKey(Color.Blue);
        public static TraceKey WorldRenderer_RenderChunks_Prepare = new TraceKey(Color.DarkGoldenrod);
        public static TraceKey SkyBoxRenderer_PreDraw = new TraceKey(Color.DeepSkyBlue);
        public static TraceKey SkyBoxRenderer_PostDraw = new TraceKey(Color.LightSkyBlue);
        public static TraceKey UniverseRenderer_DrawOverlay = new TraceKey(Color.MediumPurple);
        public static TraceKey Universe_TimeClone = new TraceKey(Color.DarkOrange);
        public static TraceKey Universe_Deallocate = new TraceKey(Color.DarkKhaki);
        public static TraceKey WorldRenderer_FetchRenderChunks_FetchChunks = new TraceKey(Color.Orange);
        public static TraceKey WorldRenderer_FetchRenderChunks_FetchChunks_Bind = new TraceKey(Color.Blue);
        public static TraceKey WorldRenderer_FetchRenderChunks_BuildDeltaList = new TraceKey(Color.Red);
        public static TraceKey Server_Update = new TraceKey(Color.DeepSkyBlue);
        public static TraceKey LodServer_Update = new TraceKey(Color.Orange);
        public static TraceKey Server_RequestReload = new TraceKey(Color.Red);
        public static TraceKey Server_Update_Packets = new TraceKey(Color.LawnGreen);
        public static TraceKey Server_Update_ConnectionsClosed = new TraceKey(Color.Blue);
        public static TraceKey Server_Update_Multiverse = new TraceKey(Color.LightGreen);
        public static TraceKey Server_Update_Behavior = new TraceKey(Color.Red);
        public static TraceKey Server_Update_Connections = new TraceKey(Color.White);
        public static TraceKey ClientServerChunkManager_Update = new TraceKey(Color.Brown);
        public static TraceKey LodViewController_Update = new TraceKey(Color.MediumPurple);
        public static TraceKey ClientServerChunkManager_ProcessPackets = new TraceKey(Color.Blue);
        public static TraceKey LodViewController_SetPosition = new TraceKey(Color.DarkGoldenrod);
        public static TraceKey LivingWorld_Update = new TraceKey(Color.DarkGoldenrod);
        public static TraceKey Entities_Update = new TraceKey(Color.Blue);
        public static TraceKey World_Update = new TraceKey(Color.White);
        public static TraceKey Universe_Update_Packets = new TraceKey(Color.LawnGreen);
        public static TraceKey EntityPhysics_Update = new TraceKey(Color.Yellow);
        public static TraceKey PlayerEntityLogic_Update = new TraceKey(Color.Tomato);
        public static TraceKey ItemEntityLogic_Update = new TraceKey(Color.Red);
        public static TraceKey PlayerEntityLogic_Update_TerrainRayIntersect = new TraceKey(Color.MediumPurple);
        public static TraceKey PlayerEntityLogic_Update_EntityRayIntersect = new TraceKey(Color.Aqua);
        public static TraceKey PlayerEntityLogic_Update_Camera = new TraceKey(Color.White);
        public static TraceKey EntityUniverseFacade_InnerTerrainRayIntersect_ReadTiles = new TraceKey(Color.Brown);
        public static TraceKey Multiverse_Update_Bedrock = new TraceKey(Color.DarkKhaki);
        public static TraceKey Multiverse_Update_Catchup = new TraceKey(Color.Red);
        public static TraceKey Multiverse_Update_Previous = new TraceKey(Color.SaddleBrown);
        public static TraceKey Multiverse_Update_Next = new TraceKey(Color.SkyBlue);
        public static TraceKey LodClient_Draw = new TraceKey(Color.GreenYellow);
        public static TraceKey LodClient_Update = new TraceKey(Color.Blue);
        public static TraceKey LodClient_RebuildVisibility = new TraceKey(Color.Red);
        public static TraceKey UniverseRenderer_DrawGeometry = new TraceKey(Color.LightGreen);
        public static TraceKey UniverseRenderer_DrawGeometryShadow = new TraceKey(Color.DarkGreen);
        public static TraceKey ClientMainLoop_ProcessPackets_ReceiveLodChunk = new TraceKey(Color.MediumPurple);
        public static TraceKey UniverseRenderer_Update = new TraceKey(Color.MediumPurple);
        public static TraceKey StaxelGame_Draw_Gearset = new TraceKey(Color.Aqua);
        public static TraceKey RenderChunksManager_FrustumValidate = new TraceKey(Color.Tomato);
        public static TraceKey RenderChunksManager_RebuildCache = new TraceKey(Color.GreenYellow);
        public static TraceKey RenderChunksManager_BuildSortedViewRadiusOptions = new TraceKey(Color.Blue);
        public static TraceKey GpuResources = new TraceKey(Color.Red);
        public static TraceKey GpuResources_StartOfFrame = new TraceKey(Color.Blue);
        public static TraceKey GpuResources_FrameQuery = new TraceKey(Color.GreenYellow);
        public static TraceKey GpuResources_PostPresent = new TraceKey(Color.Blue);
        public static TraceKey LongLivedVertexStorage_Flush = new TraceKey(Color.Red);
        public static TraceKey LongLivedVertexStorage_Compact = new TraceKey(Color.MediumPurple);
        public static TraceKey ShortLivedVertexStorage_SetData = new TraceKey(Color.Red);
        public static TraceKey PrepareIndexBuffer = new TraceKey(Color.Red);
        public static TraceKey UniverseRenderer_DrawSelectionCursor = new TraceKey(Color.White);


        public static TraceKey A = new TraceKey(Color.LightYellow);
        public static TraceKey B = new TraceKey(Color.LightYellow);
        public static TraceKey C = new TraceKey(Color.LightYellow);
        public static TraceKey D = new TraceKey(Color.LightYellow);
        public static TraceKey E = new TraceKey(Color.LightYellow);
        public static TraceKey F = new TraceKey(Color.LightYellow);
        public static TraceKey G = new TraceKey(Color.LightYellow);
        public static TraceKey H = new TraceKey(Color.LightYellow);
        public static TraceKey I = new TraceKey(Color.LightYellow);
        public static TraceKey J = new TraceKey(Color.LightYellow);
        public static TraceKey K = new TraceKey(Color.LightYellow);
        public static TraceKey L = new TraceKey(Color.LightYellow);
        public static TraceKey M = new TraceKey(Color.LightYellow);
        public static TraceKey N = new TraceKey(Color.LightYellow);
        public static TraceKey O = new TraceKey(Color.LightYellow);
        public static TraceKey P = new TraceKey(Color.LightYellow);
        public static TraceKey Q = new TraceKey(Color.LightYellow);
        public static TraceKey R = new TraceKey(Color.LightYellow);
        public static TraceKey S = new TraceKey(Color.LightYellow);
        public static TraceKey T = new TraceKey(Color.LightYellow);
        public static TraceKey U = new TraceKey(Color.LightYellow);
        public static TraceKey V = new TraceKey(Color.LightYellow);
        public static TraceKey W = new TraceKey(Color.LightYellow);
        public static TraceKey X = new TraceKey(Color.LightYellow);
        public static TraceKey Y = new TraceKey(Color.LightYellow);
        public static TraceKey Z = new TraceKey(Color.LightYellow);


        static TraceKeys() {
            var props = typeof(TraceKeys).GetFields(BindingFlags.Public | BindingFlags.Static);
            var id = 1;
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
