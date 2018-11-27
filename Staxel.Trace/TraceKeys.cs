﻿using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace Staxel.Trace {
    [Obfuscation(Exclude = true)]
    public sealed class TraceKeys {
        static readonly List<TraceKey> Keys = new List<TraceKey>();
        public static string[] KeyNames;

        // Hardcoded for now, open to improvement
        public static TraceKey ClientMainLoop_Draw = new TraceKey(Color.LightGreen);
        public static TraceKey UniverseManager_Update = new TraceKey(Color.Purple);
        public static TraceKey Universe_Update = new TraceKey(Color.Blue);
        public static TraceKey Universe_RenderUpdate = new TraceKey(Color.Red);
        public static TraceKey ClientMainLoop_Update = new TraceKey(Color.LightYellow);
        public static TraceKey ClientMainLoop_Update_SendPackets = new TraceKey(Color.LightGreen);
        public static TraceKey ClientMainLoop_ProcessPackets = new TraceKey(Color.LightSlateGray);
        public static TraceKey ClientMainLoop_ReceiveActivateBundle = new TraceKey(Color.Blue);
        public static TraceKey ClientMainLoop_ReceiveBundleResource = new TraceKey(Color.Yellow);
        public static TraceKey ClientMainLoop_ReceiveChunkData = new TraceKey(Color.Green);
        public static TraceKey ClientMainLoop_ReceiveChunkDenied = new TraceKey(Color.Red);
        public static TraceKey Recorder_Flush = new TraceKey(Color.DeepPink);
        public static TraceKey UniverseRenderer_Draw = new TraceKey(Color.LightSteelBlue);
        public static TraceKey UniverseRenderer_Post = new TraceKey(Color.Red);
        public static TraceKey ParticleRenderer_Render = new TraceKey(Color.Yellow);
        public static TraceKey UniverseRenderer_DrawInWorld = new TraceKey(Color.Tomato);
        public static TraceKey StaxelGame_Draw = new TraceKey(Color.Green);
        public static TraceKey StaxelGame_Update = new TraceKey(Color.Magenta);
        public static TraceKey StaxelGame_Present = new TraceKey(Color.Cyan);
        public static TraceKey EntityRenderer_Draw = new TraceKey(Color.MediumOrchid);
        public static TraceKey ChunkRenderer_Render = new TraceKey(Color.Brown);
        public static TraceKey ChunkRenderer_PendingBuildQueue = new TraceKey(Color.White);
        public static TraceKey WorldRenderer_FetchRenderChunks = new TraceKey(Color.Blue);
        public static TraceKey WorldRenderer_PrepareRenderChunks = new TraceKey(Color.Blue);
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
        public static TraceKey Server_Update_UniverseManager = new TraceKey(Color.LightGreen);
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
        public static TraceKey UniverseManager_Update_Universe = new TraceKey(Color.DarkKhaki);
        public static TraceKey LodClient_Draw = new TraceKey(Color.GreenYellow);
        public static TraceKey LodClient_Update = new TraceKey(Color.Blue);
        public static TraceKey LodClient_RebuildVisibility = new TraceKey(Color.Red);
        public static TraceKey UniverseRenderer_BeforeDrawGeometry = new TraceKey(Color.Aquamarine);
        public static TraceKey UniverseRenderer_DrawGeometry = new TraceKey(Color.LightGreen);
        public static TraceKey UniverseRenderer_DrawGeometryShadow = new TraceKey(Color.DarkGreen);
        public static TraceKey UniverseRenderer_DrawGeometryInside = new TraceKey(Color.LightBlue);
        public static TraceKey ClientMainLoop_ProcessPackets_ReceiveLodChunk = new TraceKey(Color.MediumPurple);
        public static TraceKey UniverseRenderer_Update = new TraceKey(Color.MediumPurple);
        public static TraceKey StaxelGame_Draw_Gearset = new TraceKey(Color.Aqua);
        public static TraceKey RenderChunksManager_FrustumValidate = new TraceKey(Color.Tomato);
        public static TraceKey RenderChunksManager_RebuildCache = new TraceKey(Color.GreenYellow);
        public static TraceKey RenderChunksManager_BuildSortedViewRadiusOptions = new TraceKey(Color.Blue);
        public static TraceKey GpuResources = new TraceKey(Color.Red);
        public static TraceKey GpuResources_StartOfFrame = new TraceKey(Color.Blue);
        public static TraceKey GpuResources_FrameQuery = new TraceKey(Color.GreenYellow);
        public static TraceKey GpuResources_BeforePresent = new TraceKey(Color.Blue);
        public static TraceKey LongLivedVertexStorage_Flush = new TraceKey(Color.Red);
        public static TraceKey LongLivedVertexStorage_Compact = new TraceKey(Color.MediumPurple);
        public static TraceKey ShortLivedVertexStorage_SetData = new TraceKey(Color.Red);
        public static TraceKey ShortLivedVertexStorage_SetDataHead = new TraceKey(Color.Orange);
        public static TraceKey PrepareIndexBuffer = new TraceKey(Color.Red);
        public static TraceKey UniverseRenderer_DrawSelectionCursor = new TraceKey(Color.White);
        public static TraceKey UserInput_Update = new TraceKey(Color.Blue);
        public static TraceKey StaxelGame_Memory = new TraceKey(Color.MediumPurple);
        public static TraceKey Client_Update_AvatarController = new TraceKey(Color.Blue);
        public static TraceKey InputSource_ScanDevices = new TraceKey(Color.Orange);
        public static TraceKey BrowserRenderSurface_UpdateTexture_SetData = new TraceKey(Color.Orange);
        public static TraceKey VertexManager_StartOfFrame = new TraceKey(Color.Orange);
        public static TraceKey VertexManager_ReleaseLostSurfaces = new TraceKey(Color.MediumPurple);
        public static TraceKey ActiveChunkTracker_UpdatePlayerPosition = new TraceKey(Color.MediumPurple);
        public static TraceKey ChunkActivityDatabase_Update = new TraceKey(Color.MediumPurple);
        public static TraceKey ServerMainLoop_SendStepPackets = new TraceKey(Color.Orange);
        public static TraceKey Entities_SendEntityPackets = new TraceKey(Color.Red);
        public static TraceKey SoundManager_Update = new TraceKey(Color.Blue);

        public static TraceKey WorkerManagerExecute = new TraceKey(Color.Blue);
        public static TraceKey WorkerManagerInnerExecute = new TraceKey(Color.Blue);
        public static TraceKey WorkerManagerFireAndForget = new TraceKey(Color.Blue);
        public static TraceKey LightingBatchProcess = new TraceKey(Color.Yellow);
        public static TraceKey LodServerBuildNLodChunk = new TraceKey(Color.Purple);
        public static TraceKey LodServerBuild0LodChunk = new TraceKey(Color.MediumPurple);
        public static TraceKey ServerWorldManagerProcessRequest = new TraceKey(Color.GreenYellow);
        public static TraceKey PathFinderFindPath = new TraceKey(Color.GreenYellow);
        public static TraceKey DykstraGridProcess = new TraceKey(Color.Orange);
        public static TraceKey PathFinderFindPathMesh = new TraceKey(Color.White);

        public static TraceKey PathFindCoroutine = new TraceKey(Color.Tomato);
        public static TraceKey VillagerScript = new TraceKey(Color.Thistle);
        public static TraceKey DialogueStateMoveTo = new TraceKey(Color.Navy);

        // usable for temporary tracescopes, use fully named ones when using them longterm

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

        //always append to end to not break existing tracefiles

        public static TraceKey CounterManagerUpdate = new TraceKey(Color.Navy);
        public static TraceKey CounterManagerMemory = new TraceKey(Color.Navy);
        public static TraceKey BrowserRenderSurface_Update = new TraceKey(Color.Navy);
        public static TraceKey ChunkDatabaseInitialize = new TraceKey(Color.Coral);
        public static TraceKey ServerWorldManagerTryQuickComplete = new TraceKey(Color.HotPink);
        public static TraceKey ServerWorldManagerGenerateChunk = new TraceKey(Color.SaddleBrown);
        public static TraceKey EffectRenderer_Render = new TraceKey(Color.Gold);
        public static TraceKey AnimatedTilePainter_Render = new TraceKey(Color.DarkOliveGreen);
        public static TraceKey LongLiveVertexStorageBacking_Bind = new TraceKey(Color.Gold);
        public static TraceKey ChunkEntitiesManager_LoadEntity = new TraceKey(Color.Red);
        public static TraceKey CefInitialize = new TraceKey(Color.Gray);
        public static TraceKey CefInitializeWait = new TraceKey(Color.Indigo);
        public static TraceKey BrowserCoreInitialization = new TraceKey(Color.Gold);
        public static TraceKey StreamInitialization = new TraceKey(Color.Navy);
        public static TraceKey WorkerManagerForeach = new TraceKey(Color.HotPink);
        public static TraceKey Entities_BuildCaches = new TraceKey(Color.HotPink);
        public static TraceKey ServerWorldManager_DatabaseWriteWorker = new TraceKey(Color.DarkGreen);
        public static TraceKey SoundManager_LoadSound = new TraceKey(Color.Gold);
        
        
        
        

        static TraceKeys() {
            var props = typeof(TraceKeys).GetFields(BindingFlags.Public | BindingFlags.Static);
            var id = 1;
            var maxId = -1;
            foreach (var key in props) {
                if (key.FieldType == typeof(TraceKey)) {
                    var scope = (TraceKey)key.GetValue(null);
                    scope.Code = key.Name;
                    if (scope.Id == 0)
                        scope.Id = id++;
                    Keys.Add(scope);
                    if (scope.Id > maxId)
                        maxId = scope.Id;
                }
            }
            KeyNames = new string[maxId + 1];
            foreach (var entry in Keys)
                KeyNames[entry.Id] = entry.Code;
        }

        public static IEnumerable<TraceKey> All() {
            return Keys;
        }
    }
}
