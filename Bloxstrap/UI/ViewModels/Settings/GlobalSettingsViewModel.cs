using Bloxstrap.Enums.FlagPresets;
using Bloxstrap.Enums.GBSPresets;
using Bloxstrap.Models.SettingTasks;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class GlobalSettingsViewModel : NotifyPropertyChangedViewModel
    {
        private readonly NvidiaProfileTask _nvidiaProfileTask = new();
        private static readonly string[] LODLevels = { "L0", "L12", "L23", "L34" };
        private static readonly IReadOnlyDictionary<string, string> PoddoksFastFlags = new Dictionary<string, string>
        {
            ["FFlagTaskSchedulerLimitTargetFpsTo2402"] = "False",
            ["DFIntTaskSchedulerTargetFps"] = "99999",
            ["FFlagHandleAltEnterFullscreenManually"] = "False",
            ["FFlagUserEnablePerformanceScaling"] = "True",
            ["FFlagUserGraphicsPreferD3D11"] = "True",
            ["FFlagUserEnableFrameRateCapOverride"] = "True",
            ["FFlagUserAllowHighFrameRates"] = "True",
            ["FFlagUserFastFlagUpdates"] = "True",
            ["FFlagGraphicsPreferHardwareRendering"] = "True",
            ["FFlagNetworkUseThrottle"] = "True",
            ["FFlagNetworkPingThrottle"] = "True",
            ["FFlagUserEnableNetworkResendOptimization"] = "True",
            ["FFlagUserEnableNetworkLatencyPrediction"] = "True",
            ["FFlagUserEnableNetworkSmoothing"] = "True",
            ["FFlagUserEnableNetworkOwnershipFix"] = "True",
            ["FFlagUserEnablePacketReliabilityImprovement"] = "True",
            ["FFlagUserEnableNetworkDeprioritizeOutOfRange"] = "True",
            ["FFlagUserFastLuaGC"] = "True",
            ["FFlagUserLowMemoryMode"] = "True",
            ["FFlagUserEnableNewMemoryTracker"] = "True",
            ["FFlagDebugGraphicsPreferD3D11"] = "True",
            ["FFlagDebugGraphicsPreferD3D11FL10"] = "True",
            ["FFlagDebugGraphicsDisableVSync"] = "True",
            ["DFFlagDisableDPIScale"] = "True",
            ["DFFlagDebugRenderForceTechnologyVoxel"] = "True",
            ["FFlagDisablePostFx"] = "True",
            ["FFlagNoFog"] = "True",
            ["FFlagRenderSkipReadingShaderData"] = "True",
            ["FFlagFastGPULightCulling3"] = "True",
            ["FFlagOcclusionCullingBetaFeature"] = "True",
            ["FFlagRenderEnableGlobalInstancingD3D10"] = "True",
            ["FFlagGraphicsEnableD3D10Compute"] = "True",
            ["FFlagSortKeyOptimization"] = "True",
            ["FFlagFasterPreciseTime4"] = "True",
            ["FFlagLuauCodegen"] = "True",
            ["FFlagEnableParallelPhysicsUpdates"] = "True",
            ["FFlagEnableParallelShadowMapping"] = "True",
            ["DFFlagUseMultiThreadedTextureUpload"] = "True",
            ["DFFlagOptimizeVideoMemory"] = "True",
            ["DFFlagStreamingTargetMinimizeStutter"] = "True",
            ["DFFlagUseBackgroundLoadingForRendering"] = "True",
            ["DFFlagSimOptimizeSetSize"] = "True",
            ["DFFlagJointIrregularityOptimization"] = "True",
            ["FFlagMessageBusCallOptimization"] = "True",
            ["FFlagOptimizeCFrameUpdates"] = "True",
            ["FIntDebugForceMSAASamples"] = "1",
            ["DFIntTextureQualityOverride"] = "0",
            ["DFFlagTextureQualityOverrideEnabled"] = "True",
            ["DFIntDebugFRMQualityLevelOverride"] = "1",
            ["DFIntPerformanceControlFrameTimeMax"] = "1",
            ["DFIntPerformanceControlReportingPeriodInMs"] = "700",
            ["FIntDebugTextureManagerSkipMips"] = "5",
            ["FIntRenderShadowIntensity"] = "0",
            ["FIntRenderShadowmapBias"] = "0",
            ["FIntSSAOMipLevels"] = "0",
            ["FIntRenderMaxShadowAtlasUsageBeforeDownscale"] = "0",
            ["FIntRenderLocalLightFadeInMs"] = "0",
            ["FIntRenderLocalLightUpdatesMax"] = "1",
            ["FIntRenderLocalLightUpdatesMin"] = "1",
            ["FIntDirectionalAttenuationMaxPoints"] = "0",
            ["FIntRenderGrassDetailStrands"] = "0",
            ["FIntRenderGrassHeightScaler"] = "0",
            ["FIntGrassMovementReducedMotionFactor"] = "0",
            ["FIntTerrainArraySliceSize"] = "0",
            ["FIntFRMMinGrassDistance"] = "0",
            ["FIntFRMMaxGrassDistance"] = "0",
            ["FIntRobloxGuiBlurIntensity"] = "0",
            ["DFIntCSGLevelOfDetailSwitchingDistance"] = "0",
            ["DFIntCSGLevelOfDetailSwitchingDistanceL12"] = "0",
            ["DFIntCSGLevelOfDetailSwitchingDistanceL23"] = "0",
            ["DFIntCSGLevelOfDetailSwitchingDistanceL34"] = "0",
            ["FStringTerrainMaterialTable2022"] = "",
            ["FStringTerrainMaterialTablePre2022"] = "",
            ["FFlagDebugSkyGray"] = "True",
            ["FFlagDebugDisableOTAMaterialTexture"] = "True",
            ["FIntNetworkMaxPacketSize"] = "1200",
            ["FIntNetworkRetryCount"] = "5",
            ["FIntNetworkClientTimeout"] = "60",
            ["FIntRakNetResendBufferArrayLength"] = "128",
            ["DFIntRakNetResendRttMultiple"] = "1",
            ["DFIntRakNetLoopMs"] = "1",
            ["DFIntRakNetSelectTimeoutMs"] = "1",
            ["DFIntRakNetNakResendDelayMs"] = "1",
            ["DFIntRakNetNakResendDelayMsMax"] = "1",
            ["DFIntRakNetPingFrequencyMillisecond"] = "1",
            ["DFIntClientPacketMaxDelayMs"] = "1",
            ["DFIntClientPacketMaxFrameMicroseconds"] = "200",
            ["DFIntMaxAcceptableUpdateDelay"] = "1",
            ["DFIntMaxReceiveToDeserializeLatencyMilliseconds"] = "10",
            ["DFIntMaxProcessPacketsStepsPerCyclic"] = "5000",
            ["DFIntMaxProcessPacketsStepsAccumulated"] = "0",
            ["DFIntMaxProcessPacketsJobScaling"] = "10000",
            ["DFIntClusterCompressionLevel"] = "0",
            ["DFIntBufferCompressionLevel"] = "0",
            ["DFFlagClampIncomingReplicationLag"] = "True",
            ["DFFlagSampleAndRefreshRakPing"] = "True",
            ["DFFlagRakNetEnablePoll"] = "True",
            ["DFFlagRakNetUnblockSelectOnShutdownByWritingToSocket"] = "True",
            ["DFFlagNetworkUseZstdWrapper"] = "False",
            ["DFIntTaskSchedulerJobInitThreads"] = "20",
            ["DFIntTaskSchedulerJobInGameThreads"] = "20",
            ["DFIntRuntimeConcurrency"] = "16",
            ["DFIntMegaReplicatorNumParallelTasks"] = "20",
            ["DFIntReplicationDataCacheNumParallelTasks"] = "20",
            ["DFIntNetworkClusterPacketCacheNumParallelTasks"] = "20",
            ["DFIntPlayerNetworkUpdateQueueSize"] = "20",
            ["DFIntPlayerNetworkUpdateRate"] = "60",
            ["FIntTaskSchedulerAutoThreadLimit"] = "4",
            ["FIntSmoothClusterTaskQueueMaxParallelTasks"] = "20",
            ["FIntLuaGcParallelMinMultiTasks"] = "20",
            ["FIntGCStepSizeKb"] = "512",
            ["DFIntDebugRestrictGCDistance"] = "1",
            ["DFIntMemoryUtilityCurveBaseHundrethsPercent"] = "10000",
            ["DFIntMemoryUtilityCurveNumSegments"] = "100",
            ["DFIntMemoryUtilityCurveTotalMemoryReserve"] = "0",
            ["DFFlagEnableTexturePreloading"] = "True",
            ["DFFlagEnableSoundPreloading"] = "True",
            ["FFlagPreloadTextureItemsOption4"] = "True",
            ["FFlagPreloadAllFonts"] = "True",
            ["DFIntNumAssetsMaxToPreload"] = "9999999",
            ["DFIntAssetPreloading"] = "2147483647",
            ["DFIntSendGameServerDataMaxLen"] = "9999999",
            ["DFIntMaxDataPacketPerSend"] = "100000",
            ["FFlagDebugDisableTelemetryPoint"] = "True",
            ["FFlagDebugDisableTelemetryEphemeralStat"] = "True",
            ["FFlagDebugDisableTelemetryEphemeralCounter"] = "True",
            ["FFlagDebugDisableTelemetryEventIngest"] = "True",
            ["FFlagDebugDisableTelemetryV2Counter"] = "True",
            ["FFlagDebugDisableTelemetryV2Event"] = "True",
            ["FFlagDebugDisableTelemetryV2Stat"] = "True",
            ["FFlagAdServiceEnabled"] = "False"
        };
        private static readonly IReadOnlyDictionary<string, string> PoddoksExtremeFastFlags = new Dictionary<string, string>
        {
            ["FFlagTaskSchedulerLimitTargetFpsTo2402"] = "False",
            ["DFIntTaskSchedulerTargetFps"] = "99999",
            ["FFlagHandleAltEnterFullscreenManually"] = "False",
            ["FFlagUserEnablePerformanceScaling"] = "True",
            ["FFlagUserGraphicsPreferD3D11"] = "True",
            ["FFlagUserEnableFrameRateCapOverride"] = "True",
            ["FFlagUserAllowHighFrameRates"] = "True",
            ["FFlagUserFastFlagUpdates"] = "True",
            ["FFlagGraphicsPreferHardwareRendering"] = "True",
            ["FFlagNetworkUseThrottle"] = "True",
            ["FFlagNetworkPingThrottle"] = "True",
            ["FFlagUserEnableNetworkResendOptimization"] = "True",
            ["FFlagUserEnableNetworkLatencyPrediction"] = "True",
            ["FFlagUserEnableNetworkSmoothing"] = "True",
            ["FFlagUserEnableNetworkOwnershipFix"] = "True",
            ["FFlagUserEnablePacketReliabilityImprovement"] = "True",
            ["FFlagUserEnableNetworkDeprioritizeOutOfRange"] = "True",
            ["FFlagUserFastLuaGC"] = "True",
            ["FFlagUserLowMemoryMode"] = "True",
            ["FFlagUserEnableNewMemoryTracker"] = "True",
            ["FFlagDebugGraphicsPreferD3D11"] = "True",
            ["FFlagDebugGraphicsPreferD3D11FL10"] = "True",
            ["FFlagDebugGraphicsDisableVSync"] = "True",
            ["DFFlagDisableDPIScale"] = "True",
            ["FFlagDisablePostFx"] = "True",
            ["FFlagRenderSkipReadingShaderData"] = "True",
            ["FFlagFastGPULightCulling3"] = "True",
            ["FFlagOcclusionCullingBetaFeature"] = "True",
            ["FFlagRenderEnableGlobalInstancingD3D10"] = "True",
            ["FFlagGraphicsEnableD3D10Compute"] = "True",
            ["FFlagSortKeyOptimization"] = "True",
            ["FFlagFasterPreciseTime4"] = "True",
            ["FFlagLuauCodegen"] = "True",
            ["FFlagEnableParallelPhysicsUpdates"] = "True",
            ["FFlagEnableParallelShadowMapping"] = "True",
            ["FFlagEnableParallelLightCulling"] = "True",
            ["FFlagEnableParallelLightProcessing"] = "True",
            ["FFlagEnableParallelLightUpdates"] = "True",
            ["FFlagEnableParallelLightingCalculations"] = "True",
            ["FFlagEnableParallelRenderThreads"] = "True",
            ["FFlagEnableParallelJobScheduler"] = "True",
            ["FFlagEnableParallelJobExecution"] = "True",
            ["FFlagEnableParallelJobProcessing"] = "True",
            ["FFlagEnableParallelTextureUpload"] = "True",
            ["FFlagEnableParallelTextureStreaming"] = "True",
            ["FFlagEnableParallelTextureProcessing"] = "True",
            ["FFlagEnableParallelTextureCompression"] = "True",
            ["FFlagEnableParallelTextureLoading"] = "True",
            ["FFlagEnableParallelMeshProcessing"] = "True",
            ["FFlagEnableParallelMeshUpdates"] = "True",
            ["FFlagEnableParallelLuaExecution"] = "True",
            ["FFlagEnableParallelLuaGC"] = "True",
            ["FFlagEnableParallelLuaCoroutines"] = "True",
            ["FFlagEnableParallelCoroutines"] = "True",
            ["FFlagEnableParallelTaskScheduler"] = "True",
            ["FFlagEnableParallelAnimationUpdates"] = "True",
            ["FFlagEnableParallelPathfinding"] = "True",
            ["FFlagEnableParallelTerrainUpdates"] = "True",
            ["FFlagEnableGPUAcceleration"] = "True",
            ["FFlagEnableGPUComputing"] = "True",
            ["FFlagEnableGPUVertexProcessing"] = "True",
            ["FFlagEnableGPUTextureProcessing"] = "True",
            ["FFlagEnableGPUTextureStreaming"] = "True",
            ["FFlagEnableGPUTextureCompression"] = "True",
            ["FFlagEnableGPUTextureUpload"] = "True",
            ["FFlagEnableGPUParticles"] = "True",
            ["FFlagEnableGPUPhysics"] = "True",
            ["DFFlagUseMultiThreadedTextureUpload"] = "True",
            ["DFFlagUseMultiThreadedShadowMapping"] = "True",
            ["DFFlagUseMultiThreadedTextureProcessing"] = "True",
            ["DFFlagUseMultiThreadedTextureCompression"] = "True",
            ["DFFlagUseMultiThreadedTextureStreaming"] = "True",
            ["DFFlagUseMultiThreadedTextureLoading"] = "True",
            ["DFFlagUseMultiThreadedRendering"] = "True",
            ["DFFlagUseBackgroundLoadingForRendering"] = "True",
            ["DFFlagUseBackgroundThreadsForRendering"] = "True",
            ["DFFlagUseThreadPoolForNetworkJobs"] = "True",
            ["DFFlagOptimizeVideoMemory"] = "True",
            ["DFFlagStreamingTargetMinimizeStutter"] = "True",
            ["DFFlagStreamingTargetStableFramerate"] = "True",
            ["DFFlagStreamingTargetFixedFps"] = "True",
            ["DFFlagStreamingTargetPerformance"] = "True",
            ["DFFlagStreamingTargetLowLatency"] = "True",
            ["DFFlagSimOptimizeSetSize"] = "True",
            ["DFFlagJointIrregularityOptimization"] = "True",
            ["FFlagMessageBusCallOptimization"] = "True",
            ["FFlagOptimizeCFrameUpdates"] = "True",
            ["FFlagOptimizeRenderingPerformance"] = "True",
            ["FFlagOptimizeNetwork"] = "True",
            ["FFlagOptimizeNetworkRouting"] = "True",
            ["FFlagOptimizeNetworkTransport"] = "True",
            ["FIntDebugForceMSAASamples"] = "1",
            ["DFIntTextureQualityOverride"] = "0",
            ["DFFlagTextureQualityOverrideEnabled"] = "True",
            ["DFIntDebugFRMQualityLevelOverride"] = "1",
            ["DFIntPerformanceControlFrameTimeMax"] = "1",
            ["DFIntPerformanceControlReportingPeriodInMs"] = "700",
            ["DFIntPerformanceControlTextureQualityBestUtility"] = "-1",
            ["DFIntPerformanceControlFrameTimeMaxUtility"] = "-1",
            ["FIntDebugTextureManagerSkipMips"] = "5",
            ["FIntRenderShadowIntensity"] = "0",
            ["FIntRenderShadowmapBias"] = "0",
            ["FIntSSAOMipLevels"] = "0",
            ["FIntRenderMaxShadowAtlasUsageBeforeDownscale"] = "0",
            ["FIntRenderLocalLightFadeInMs"] = "0",
            ["FIntRenderLocalLightUpdatesMax"] = "1",
            ["FIntRenderLocalLightUpdatesMin"] = "1",
            ["FIntDirectionalAttenuationMaxPoints"] = "0",
            ["FIntRenderGrassDetailStrands"] = "0",
            ["FIntGrassMovementReducedMotionFactor"] = "0",
            ["FIntTerrainArraySliceSize"] = "0",
            ["FIntFRMMinGrassDistance"] = "0",
            ["FIntFRMMaxGrassDistance"] = "0",
            ["DFIntCSGLevelOfDetailSwitchingDistance"] = "0",
            ["DFIntCSGLevelOfDetailSwitchingDistanceL12"] = "0",
            ["DFIntCSGLevelOfDetailSwitchingDistanceL23"] = "0",
            ["DFIntCSGLevelOfDetailSwitchingDistanceL34"] = "0",
            ["FStringTerrainMaterialTable2022"] = "",
            ["FStringTerrainMaterialTablePre2022"] = "",
            ["FIntNetworkMaxPacketSize"] = "1200",
            ["FIntNetworkRetryCount"] = "5",
            ["FIntNetworkClientTimeout"] = "60",
            ["FIntRakNetResendBufferArrayLength"] = "128",
            ["DFIntConnectionMTUSize"] = "900",
            ["DFIntRakNetResendRttMultiple"] = "1",
            ["DFIntRakNetLoopMs"] = "1",
            ["DFIntRakNetSelectTimeoutMs"] = "1",
            ["DFIntRakNetNakResendDelayMs"] = "1",
            ["DFIntRakNetNakResendDelayMsMax"] = "1",
            ["DFIntRakNetPingFrequencyMillisecond"] = "1",
            ["DFIntClientPacketMaxDelayMs"] = "1",
            ["DFIntClientPacketMaxFrameMicroseconds"] = "200",
            ["DFIntClientPacketExcessMicroseconds"] = "1000",
            ["DFIntMaxAcceptableUpdateDelay"] = "1",
            ["DFIntMaxReceiveToDeserializeLatencyMilliseconds"] = "10",
            ["DFIntMaxProcessPacketsStepsPerCyclic"] = "5000",
            ["DFIntMaxProcessPacketsStepsAccumulated"] = "0",
            ["DFIntMaxProcessPacketsJobScaling"] = "10000",
            ["DFIntClusterCompressionLevel"] = "0",
            ["DFIntBufferCompressionLevel"] = "0",
            ["DFFlagClampIncomingReplicationLag"] = "True",
            ["DFFlagSampleAndRefreshRakPing"] = "True",
            ["DFFlagRakNetEnablePoll"] = "True",
            ["DFFlagRakNetUnblockSelectOnShutdownByWritingToSocket"] = "True",
            ["DFFlagNetworkUseZstdWrapper"] = "False",
            ["DFIntTaskSchedulerJobInitThreads"] = "20",
            ["DFIntTaskSchedulerJobInGameThreads"] = "20",
            ["DFIntRuntimeConcurrency"] = "16",
            ["DFIntMegaReplicatorNumParallelTasks"] = "20",
            ["DFIntReplicationDataCacheNumParallelTasks"] = "20",
            ["DFIntNetworkClusterPacketCacheNumParallelTasks"] = "20",
            ["DFIntPlayerNetworkUpdateQueueSize"] = "20",
            ["DFIntPlayerNetworkUpdateRate"] = "60",
            ["FIntTaskSchedulerAutoThreadLimit"] = "4",
            ["FIntSmoothClusterTaskQueueMaxParallelTasks"] = "20",
            ["FIntLuaGcParallelMinMultiTasks"] = "20",
            ["FIntGCStepSizeKb"] = "512",
            ["DFIntDebugRestrictGCDistance"] = "1",
            ["DFIntMemoryUtilityCurveBaseHundrethsPercent"] = "10000",
            ["DFIntMemoryUtilityCurveNumSegments"] = "100",
            ["DFIntMemoryUtilityCurveTotalMemoryReserve"] = "0",
            ["DFFlagEnableTexturePreloading"] = "True",
            ["DFFlagEnableSoundPreloading"] = "True",
            ["FFlagPreloadTextureItemsOption4"] = "True",
            ["FFlagPreloadAllFonts"] = "True",
            ["DFIntNumAssetsMaxToPreload"] = "9999999",
            ["DFIntAssetPreloading"] = "2147483647",
            ["DFIntSendGameServerDataMaxLen"] = "9999999",
            ["DFIntMaxDataPacketPerSend"] = "100000",
            ["FFlagDebugDisableTelemetryPoint"] = "True",
            ["FFlagDebugDisableTelemetryEphemeralStat"] = "True",
            ["FFlagDebugDisableTelemetryEphemeralCounter"] = "True",
            ["FFlagDebugDisableTelemetryEventIngest"] = "True",
            ["FFlagDebugDisableTelemetryV2Counter"] = "True",
            ["FFlagDebugDisableTelemetryV2Event"] = "True",
            ["FFlagDebugDisableTelemetryV2Stat"] = "True",
            ["FFlagAdServiceEnabled"] = "False"
        };
        private static readonly IReadOnlyDictionary<string, string> AbsoluteMaxFpsMinDelayExtraFlags = new Dictionary<string, string>
        {
            ["DFFlagForceCaptureEnabled"] = "False",
            ["FFlagVideoReportHardwareBufferMetrics"] = "False",
            ["FFlagVideoServiceAddHardwareCodecMetrics"] = "False"
        };
        private static readonly IReadOnlyDictionary<string, string> NetworkOptimizedFlags = new Dictionary<string, string>
        {
            ["FIntRakNetResendBufferArrayLength"] = "128",
            ["DFIntRakNetResendRttMultiple"] = "1",
            ["DFIntRakNetLoopMs"] = "1",
            ["DFIntRakNetSelectTimeoutMs"] = "1",
            ["DFIntClientPacketMaxDelayMs"] = "1",
            ["DFIntMaxAcceptableUpdateDelay"] = "1",
            ["DFIntMaxProcessPacketsStepsPerCyclic"] = "5000",
            ["DFFlagClampIncomingReplicationLag"] = "True",
            ["DFFlagSampleAndRefreshRakPing"] = "True"
        };

        private static readonly IReadOnlyDictionary<string, string> MemoryOptimizedFlags = new Dictionary<string, string>
        {
            ["FIntGCStepSizeKb"] = "512",
            ["DFIntDebugRestrictGCDistance"] = "1",
            ["DFIntMemoryUtilityCurveBaseHundrethsPercent"] = "10000",
            ["DFIntMemoryUtilityCurveNumSegments"] = "100",
            ["DFIntMemoryUtilityCurveTotalMemoryReserve"] = "0"
        };

        private static readonly IReadOnlyDictionary<string, string> CullingOptimizedFlags = new Dictionary<string, string>
        {
            ["DFIntCSGLevelOfDetailSwitchingDistance"] = "0",
            ["DFIntCSGLevelOfDetailSwitchingDistanceL12"] = "0",
            ["DFIntCSGLevelOfDetailSwitchingDistanceL23"] = "0",
            ["DFIntCSGLevelOfDetailSwitchingDistanceL34"] = "0",
            ["FFlagFastGPULightCulling3"] = "True",
            ["FFlagOcclusionCullingBetaFeature"] = "True"
        };

        private static readonly IReadOnlyDictionary<string, string> TextureDegradedFlags = new Dictionary<string, string>
        {
            ["FIntDebugTextureManagerSkipMips"] = "5",
            ["FStringTerrainMaterialTable2022"] = "",
            ["FStringTerrainMaterialTablePre2022"] = "",
            ["FIntRenderGrassDetailStrands"] = "0"
        };

        private static readonly IReadOnlyDictionary<string, string> TerrainDecorationFlags = new Dictionary<string, string>
        {
            ["FIntTerrainArraySliceSize"] = "0",
            ["FIntFRMMinGrassDistance"] = "0",
            ["FIntFRMMaxGrassDistance"] = "0",
            ["FIntGrassMovementReducedMotionFactor"] = "0"
        };

        private static readonly IReadOnlyDictionary<string, string> ParallelPhysicsFlags = new Dictionary<string, string>
        {
            ["DFIntTaskSchedulerJobInitThreads"] = "20",
            ["DFIntTaskSchedulerJobInGameThreads"] = "20",
            ["DFIntRuntimeConcurrency"] = "16",
            ["DFFlagSimOptimizeSetSize"] = "True",
            ["DFFlagJointIrregularityOptimization"] = "True"
        };

        private static readonly IReadOnlyDictionary<string, string> VisualEffectsDisabledFlags = new Dictionary<string, string>
        {
            ["FFlagDisablePostFx"] = "True",
            ["FIntRenderShadowIntensity"] = "0",
            ["FIntRenderShadowmapBias"] = "0",
            ["FIntSSAOMipLevels"] = "0"
        };

        private static readonly IReadOnlyDictionary<string, string> D3D11OptimizedFlags = new Dictionary<string, string>
        {
            ["FFlagDebugGraphicsPreferD3D11"] = "True",
            ["FFlagDebugGraphicsPreferD3D11FL10"] = "True"
        };

        private static readonly IReadOnlyDictionary<string, string> TelemetryReducedFlags = new Dictionary<string, string>
        {
            ["FFlagDebugDisableTelemetryPoint"] = "True",
            ["FFlagDebugDisableTelemetryEphemeralStat"] = "True",
            ["FFlagDebugDisableTelemetryEphemeralCounter"] = "True",
            ["FFlagDebugDisableTelemetryEventIngest"] = "True",
            ["FFlagDebugDisableTelemetryV2Counter"] = "True",
            ["FFlagDebugDisableTelemetryV2Event"] = "True",
            ["FFlagDebugDisableTelemetryV2Stat"] = "True"
        };

        private static readonly IReadOnlyDictionary<string, string> VSyncDisabledFlags = new Dictionary<string, string>
        {
            ["FFlagDebugGraphicsDisableVSync"] = "True"
        };
        private static readonly IReadOnlyDictionary<string, string> CaptureFeaturesDisabledFlags = new Dictionary<string, string>
        {
            ["DFFlagForceCaptureEnabled"] = "False"
        };
        private static readonly IReadOnlyDictionary<string, string> UltraLowDelayFlags = new Dictionary<string, string>
        {
            ["FFlagTaskSchedulerLimitTargetFpsTo2402"] = "False",
            ["DFIntTaskSchedulerTargetFps"] = "99999",
            ["FFlagDebugGraphicsDisableVSync"] = "True",
            ["FFlagUserAllowHighFrameRates"] = "True",
            ["FFlagUserEnableFrameRateCapOverride"] = "True",
            ["FFlagDebugGraphicsPreferD3D11"] = "True",
            ["FFlagDebugGraphicsPreferD3D11FL10"] = "True",
            ["FFlagNetworkUseThrottle"] = "True",
            ["FFlagNetworkPingThrottle"] = "True",
            ["FFlagUserEnableNetworkResendOptimization"] = "True",
            ["FFlagUserEnableNetworkLatencyPrediction"] = "True",
            ["FFlagUserEnableNetworkSmoothing"] = "True",
            ["FFlagUserEnablePacketReliabilityImprovement"] = "True",
            ["DFIntRakNetResendRttMultiple"] = "1",
            ["DFIntRakNetLoopMs"] = "1",
            ["DFIntRakNetSelectTimeoutMs"] = "1",
            ["DFIntRakNetNakResendDelayMs"] = "1",
            ["DFIntRakNetNakResendDelayMsMax"] = "1",
            ["DFIntRakNetPingFrequencyMillisecond"] = "1",
            ["DFIntClientPacketMaxDelayMs"] = "1",
            ["DFIntClientPacketMaxFrameMicroseconds"] = "200",
            ["DFIntClientPacketExcessMicroseconds"] = "1000",
            ["DFIntMaxAcceptableUpdateDelay"] = "1",
            ["DFIntMaxReceiveToDeserializeLatencyMilliseconds"] = "10",
            ["DFIntMaxProcessPacketsStepsPerCyclic"] = "5000",
            ["DFIntMaxProcessPacketsJobScaling"] = "10000",
            ["DFFlagClampIncomingReplicationLag"] = "True",
            ["DFFlagSampleAndRefreshRakPing"] = "True",
            ["DFFlagRakNetEnablePoll"] = "True",
            ["DFFlagRakNetUnblockSelectOnShutdownByWritingToSocket"] = "True",
            ["DFFlagStreamingTargetLowLatency"] = "True",
            ["DFFlagStreamingTargetPerformance"] = "True",
            ["FFlagMessageBusCallOptimization"] = "True",
            ["FFlagOptimizeNetwork"] = "True",
            ["FFlagOptimizeNetworkRouting"] = "True",
            ["FFlagOptimizeNetworkTransport"] = "True",
            ["FFlagDebugDisableTelemetryPoint"] = "True",
            ["FFlagDebugDisableTelemetryEventIngest"] = "True"
        };

        public ICommand ApplyAggressivePresetCommand => new RelayCommand(ApplyAggressivePreset);
        public ICommand ApplyUltraLowDelayPresetCommand => new RelayCommand(ApplyUltraLowDelayPreset);
        public ICommand ApplyPoddoksFastFlagsCommand => new RelayCommand(ApplyAbsoluteMaxFpsMinDelay);
        public ICommand ApplyAbsoluteMaxFpsMinDelayCommand => new RelayCommand(ApplyAbsoluteMaxFpsMinDelay);

        public bool ReadOnly
        {
            get => App.GlobalSettings.GetReadOnly();
            set => App.GlobalSettings.SetReadOnly(value);
        }

        public bool UseFastFlagManager
        {
            get => App.Settings.Prop.UseFastFlagManager;
            set => App.Settings.Prop.UseFastFlagManager = value;
        }

        public string FramerateCap
        {
            get => App.GlobalSettings.GetPreset("Rendering.FramerateCap")!;
            set => App.GlobalSettings.SetPreset("Rendering.FramerateCap", value);
        }

        public string GraphicsQuality
        {
            get => App.GlobalSettings.GetPreset("Rendering.SavedQualityLevel")!;
            set
            {
                App.GlobalSettings.SetPreset("Rendering.SavedQualityLevel", value);
                OnPropertyChanged(nameof(GraphicsQuality));
            }
        }

        public bool ReducedMotion
        {
            get => App.GlobalSettings.GetPreset("UI.ReducedMotion")?.ToLower() == "true";
            set => App.GlobalSettings.SetPreset("UI.ReducedMotion", value);
        }

        public bool EnableBetterMatchmaking
        {
            get => App.Settings.Prop.EnableBetterMatchmaking;
            set => App.Settings.Prop.EnableBetterMatchmaking = value;
        }

        public bool EnableBetterMatchmakingRandomization
        {
            get => App.Settings.Prop.EnableBetterMatchmakingRandomization;
            set => App.Settings.Prop.EnableBetterMatchmakingRandomization = value;
        }

        public bool DisableTerrainTextures
        {
            get => App.Settings.Prop.DisableTerrainTextures;
            set => App.Settings.Prop.DisableTerrainTextures = value;
        }

        public bool ExtremeStripMode
        {
            get => App.Settings.Prop.ExtremeStripMode;
            set => App.Settings.Prop.ExtremeStripMode = value;
        }

        public bool AutoRepairBrokenRobloxInstallBeforeLaunch
        {
            get => App.Settings.Prop.AutoRepairBrokenRobloxInstallBeforeLaunch;
            set => App.Settings.Prop.AutoRepairBrokenRobloxInstallBeforeLaunch = value;
        }

        public bool CleanupStaleRobloxFilesBeforeLaunch
        {
            get => App.Settings.Prop.CleanupStaleRobloxFilesBeforeLaunch;
            set => App.Settings.Prop.CleanupStaleRobloxFilesBeforeLaunch = value;
        }

        public bool NvidiaProfileTweaksEnabled
        {
            get => _nvidiaProfileTask.NewState;
            set
            {
                _nvidiaProfileTask.NewState = value;
                OnPropertyChanged(nameof(NvidiaProfileTweaksEnabled));
            }
        }

        public bool NetworkOptimized
        {
            get => HasFlagSet(NetworkOptimizedFlags);
            set => SetFlagSet(NetworkOptimizedFlags, value, nameof(NetworkOptimized));
        }

        public bool MemoryOptimized
        {
            get => HasFlagSet(MemoryOptimizedFlags);
            set => SetFlagSet(MemoryOptimizedFlags, value, nameof(MemoryOptimized));
        }

        public bool CullingOptimized
        {
            get => HasFlagSet(CullingOptimizedFlags);
            set => SetFlagSet(CullingOptimizedFlags, value, nameof(CullingOptimized));
        }

        public bool TextureDegraded
        {
            get => HasFlagSet(TextureDegradedFlags);
            set => SetFlagSet(TextureDegradedFlags, value, nameof(TextureDegraded));
        }

        public bool TerrainDecorationsDisabled
        {
            get => HasFlagSet(TerrainDecorationFlags);
            set => SetFlagSet(TerrainDecorationFlags, value, nameof(TerrainDecorationsDisabled));
        }

        public bool ParallelPhysicsOptimized
        {
            get => HasFlagSet(ParallelPhysicsFlags);
            set => SetFlagSet(ParallelPhysicsFlags, value, nameof(ParallelPhysicsOptimized));
        }

        public bool VisualEffectsDisabled
        {
            get => HasFlagSet(VisualEffectsDisabledFlags);
            set => SetFlagSet(VisualEffectsDisabledFlags, value, nameof(VisualEffectsDisabled));
        }

        public bool D3D11Optimized
        {
            get => HasFlagSet(D3D11OptimizedFlags);
            set => SetFlagSet(D3D11OptimizedFlags, value, nameof(D3D11Optimized));
        }

        public bool TelemetryReduced
        {
            get => HasFlagSet(TelemetryReducedFlags);
            set => SetFlagSet(TelemetryReducedFlags, value, nameof(TelemetryReduced));
        }

        public bool VSyncDisabled
        {
            get => HasFlagSet(VSyncDisabledFlags);
            set => SetFlagSet(VSyncDisabledFlags, value, nameof(VSyncDisabled));
        }

        public bool CaptureFeaturesDisabled
        {
            get => HasFlagSet(CaptureFeaturesDisabledFlags);
            set => SetFlagSet(CaptureFeaturesDisabledFlags, value, nameof(CaptureFeaturesDisabled));
        }

        public IReadOnlyDictionary<MSAAMode, string?> MSAALevels => FastFlagManager.MSAAModes;

        public MSAAMode SelectedMSAALevel
        {
            get => MSAALevels.FirstOrDefault(x => x.Value == App.FastFlags.GetPreset("Rendering.MSAA")).Key;
            set => App.FastFlags.SetPreset("Rendering.MSAA", MSAALevels[value]);
        }

        public IReadOnlyDictionary<TextureQuality, string?> TextureQualities => FastFlagManager.TextureQualityLevels;

        public TextureQuality SelectedTextureQuality
        {
            get => TextureQualities.Where(x => x.Value == App.FastFlags.GetPreset("Rendering.TextureQuality.Level")).FirstOrDefault().Key;
            set
            {
                if (value == TextureQuality.Default)
                {
                    App.FastFlags.SetPreset("Rendering.TextureQuality", null);
                }
                else
                {
                    App.FastFlags.SetPreset("Rendering.TextureQuality.OverrideEnabled", "True");
                    App.FastFlags.SetPreset("Rendering.TextureQuality.Level", TextureQualities[value]);
                }
            }
        }

        public bool FRMQualityOverrideEnabled
        {
            get => App.FastFlags.GetPreset("Rendering.FRMQualityOverride") != null;
            set
            {
                if (value)
                    FRMQualityOverride = 1;
                else
                    App.FastFlags.SetPreset("Rendering.FRMQualityOverride", null);

                OnPropertyChanged(nameof(FRMQualityOverride));
                OnPropertyChanged(nameof(FRMQualityOverrideEnabled));
            }
        }

        public int FRMQualityOverride
        {
            get => int.TryParse(App.FastFlags.GetPreset("Rendering.FRMQualityOverride"), out var x) ? x : 1;
            set
            {
                App.FastFlags.SetPreset("Rendering.FRMQualityOverride", value);
                OnPropertyChanged(nameof(FRMQualityOverride));
            }
        }

        public bool MeshQualityEnabled
        {
            get => App.FastFlags.GetPreset("Geometry.MeshLOD.Static") != null;
            set
            {
                if (value)
                {
                    MeshQuality = 0;
                }
                else
                {
                    foreach (string level in LODLevels)
                        App.FastFlags.SetPreset($"Geometry.MeshLOD.{level}", null);

                    App.FastFlags.SetPreset("Geometry.MeshLOD.Static", null);
                }

                OnPropertyChanged(nameof(MeshQualityEnabled));
            }
        }

        public int MeshQuality
        {
            get => int.TryParse(App.FastFlags.GetPreset("Geometry.MeshLOD.Static"), out var x) ? x : 0;
            set
            {
                int clamped = Math.Clamp(value, 0, LODLevels.Length - 1);

                for (int i = 0; i < LODLevels.Length; i++)
                {
                    int lodValue = Math.Clamp(clamped - i, 0, 3);
                    string lodLevel = LODLevels[i];

                    App.FastFlags.SetPreset($"Geometry.MeshLOD.{lodLevel}", lodValue);
                }

                App.FastFlags.SetPreset("Geometry.MeshLOD.Static", clamped);
                OnPropertyChanged(nameof(MeshQuality));
                OnPropertyChanged(nameof(MeshQualityEnabled));
            }
        }

        public IReadOnlyDictionary<FontSize, string?> FontSizes => GlobalSettingsManager.FontSizes;

        public FontSize SelectedFontSize
        {
            get => FontSizes.FirstOrDefault(x => x.Value == App.GlobalSettings.GetPreset("UI.FontSize")).Key;
            set => App.GlobalSettings.SetPreset("UI.FontSize", FontSizes[value]);
        }

        private void ApplyAggressivePreset()
        {
            UseFastFlagManager = true;
            GraphicsQuality = "1";
            ReducedMotion = true;
            FramerateCap = "9999";
            EnableBetterMatchmaking = true;
            EnableBetterMatchmakingRandomization = false;
            DisableTerrainTextures = true;
            NetworkOptimized = true;
            MemoryOptimized = true;
            CullingOptimized = true;
            TextureDegraded = true;
            TerrainDecorationsDisabled = true;
            ParallelPhysicsOptimized = true;
            VisualEffectsDisabled = true;
            D3D11Optimized = true;
            TelemetryReduced = true;
            VSyncDisabled = true;
            CaptureFeaturesDisabled = true;
            SelectedTextureQuality = TextureQuality.Level0;
            SelectedMSAALevel = MSAAMode.Default;
            FRMQualityOverrideEnabled = true;
            FRMQualityOverride = 1;
            MeshQualityEnabled = true;
            MeshQuality = 0;

            OnPropertyChanged(nameof(UseFastFlagManager));
            OnPropertyChanged(nameof(GraphicsQuality));
            OnPropertyChanged(nameof(ReducedMotion));
            OnPropertyChanged(nameof(FramerateCap));
            OnPropertyChanged(nameof(EnableBetterMatchmaking));
            OnPropertyChanged(nameof(EnableBetterMatchmakingRandomization));
            OnPropertyChanged(nameof(DisableTerrainTextures));
            OnPropertyChanged(nameof(NetworkOptimized));
            OnPropertyChanged(nameof(MemoryOptimized));
            OnPropertyChanged(nameof(CullingOptimized));
            OnPropertyChanged(nameof(TextureDegraded));
            OnPropertyChanged(nameof(TerrainDecorationsDisabled));
            OnPropertyChanged(nameof(ParallelPhysicsOptimized));
            OnPropertyChanged(nameof(VisualEffectsDisabled));
            OnPropertyChanged(nameof(D3D11Optimized));
            OnPropertyChanged(nameof(TelemetryReduced));
            OnPropertyChanged(nameof(VSyncDisabled));
            OnPropertyChanged(nameof(CaptureFeaturesDisabled));
            OnPropertyChanged(nameof(SelectedTextureQuality));
            OnPropertyChanged(nameof(SelectedMSAALevel));
        }

        private void ApplyUltraLowDelayPreset()
        {
            UseFastFlagManager = true;
            FramerateCap = "9999";
            EnableBetterMatchmaking = true;
            EnableBetterMatchmakingRandomization = false;

            foreach (var flag in UltraLowDelayFlags)
                App.FastFlags.SetValue(flag.Key, flag.Value);

            D3D11Optimized = true;
            VSyncDisabled = true;
            CaptureFeaturesDisabled = true;
            TelemetryReduced = true;
            SelectedTextureQuality = TextureQuality.Level0;
            SelectedMSAALevel = MSAAMode.Default;

            OnPropertyChanged(nameof(UseFastFlagManager));
            OnPropertyChanged(nameof(FramerateCap));
            OnPropertyChanged(nameof(EnableBetterMatchmaking));
            OnPropertyChanged(nameof(EnableBetterMatchmakingRandomization));
            OnPropertyChanged(nameof(NetworkOptimized));
            OnPropertyChanged(nameof(D3D11Optimized));
            OnPropertyChanged(nameof(TelemetryReduced));
            OnPropertyChanged(nameof(VSyncDisabled));
            OnPropertyChanged(nameof(CaptureFeaturesDisabled));
            OnPropertyChanged(nameof(SelectedTextureQuality));
            OnPropertyChanged(nameof(SelectedMSAALevel));

            Frontend.ShowMessageBox("Ultra Low Delay preset applied. Click Save to keep the launch, network, and latency-focused changes.", MessageBoxImage.Information);
        }

        private void ApplyAbsoluteMaxFpsMinDelay()
        {
            UseFastFlagManager = true;
            AutoRepairBrokenRobloxInstallBeforeLaunch = true;
            CleanupStaleRobloxFilesBeforeLaunch = true;
            EnableBetterMatchmaking = true;
            EnableBetterMatchmakingRandomization = false;
            DisableTerrainTextures = true;
            GraphicsQuality = "1";
            FramerateCap = "9999";
            ReducedMotion = true;

            foreach (var flag in PoddoksExtremeFastFlags)
                App.FastFlags.SetValue(flag.Key, flag.Value);

            foreach (var flag in AbsoluteMaxFpsMinDelayExtraFlags)
                App.FastFlags.SetValue(flag.Key, flag.Value);

            D3D11Optimized = true;
            VSyncDisabled = true;
            CaptureFeaturesDisabled = true;
            TelemetryReduced = true;
            TextureDegraded = true;
            TerrainDecorationsDisabled = true;
            VisualEffectsDisabled = true;
            SelectedTextureQuality = TextureQuality.Level0;
            SelectedMSAALevel = MSAAMode.x1;
            FRMQualityOverrideEnabled = true;
            FRMQualityOverride = 1;
            MeshQualityEnabled = true;
            MeshQuality = 0;

            OnPropertyChanged(nameof(UseFastFlagManager));
            OnPropertyChanged(nameof(AutoRepairBrokenRobloxInstallBeforeLaunch));
            OnPropertyChanged(nameof(CleanupStaleRobloxFilesBeforeLaunch));
            OnPropertyChanged(nameof(EnableBetterMatchmaking));
            OnPropertyChanged(nameof(EnableBetterMatchmakingRandomization));
            OnPropertyChanged(nameof(DisableTerrainTextures));
            OnPropertyChanged(nameof(GraphicsQuality));
            OnPropertyChanged(nameof(FramerateCap));
            OnPropertyChanged(nameof(ReducedMotion));
            OnPropertyChanged(nameof(NetworkOptimized));
            OnPropertyChanged(nameof(MemoryOptimized));
            OnPropertyChanged(nameof(CullingOptimized));
            OnPropertyChanged(nameof(TextureDegraded));
            OnPropertyChanged(nameof(TerrainDecorationsDisabled));
            OnPropertyChanged(nameof(ParallelPhysicsOptimized));
            OnPropertyChanged(nameof(VisualEffectsDisabled));
            OnPropertyChanged(nameof(D3D11Optimized));
            OnPropertyChanged(nameof(TelemetryReduced));
            OnPropertyChanged(nameof(VSyncDisabled));
            OnPropertyChanged(nameof(CaptureFeaturesDisabled));
            OnPropertyChanged(nameof(SelectedTextureQuality));
            OnPropertyChanged(nameof(SelectedMSAALevel));
            OnPropertyChanged(nameof(MeshQualityEnabled));
            OnPropertyChanged(nameof(MeshQuality));
            OnPropertyChanged(nameof(FRMQualityOverrideEnabled));
            OnPropertyChanged(nameof(FRMQualityOverride));

            Frontend.ShowMessageBox("Poddoks Fast Flags have been applied. Click Save to keep the most aggressive FPS and latency-focused configuration.", MessageBoxImage.Information);
        }

        private static bool HasFlagSet(IReadOnlyDictionary<string, string> flags)
        {
            return flags.All(flag => String.Equals(App.FastFlags.GetValue(flag.Key), flag.Value, StringComparison.OrdinalIgnoreCase));
        }

        private void SetFlagSet(IReadOnlyDictionary<string, string> flags, bool enabled, string propertyName)
        {
            foreach (var flag in flags)
                App.FastFlags.SetValue(flag.Key, enabled ? flag.Value : null);

            OnPropertyChanged(propertyName);
        }
    }
}
