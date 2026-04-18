using Bloxstrap.Enums.FlagPresets;
using Bloxstrap.Enums.GBSPresets;
using Bloxstrap.Models.SettingTasks;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class GlobalSettingsViewModel : NotifyPropertyChangedViewModel
    {
        private static readonly string[] LODLevels = { "L0", "L12", "L23", "L34" };
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
            ["DFFlagForceCaptureEnabled"] = "False",
            ["FFlagEnableCapturesHotkeyExperiment_v4"] = "False",
            ["FIntVideoCaptureMaxLongSide"] = "0",
            ["FIntVideoCaptureMaxShortSide"] = "0"
        };
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
