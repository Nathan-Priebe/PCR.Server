using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PCRServer
{
    class AudioLibrary
    {
        public static void SetApplicationVolume(IAudioSessionControl2 name, float level)
        {
            var volume = name as ISimpleAudioVolume;
            var guid = Guid.Empty;
            volume?.SetMasterVolume(level / 100, ref guid);
        }

        public static int GetApplicationVolume(IAudioSessionControl2 name)
        {
            var volume = name as ISimpleAudioVolume;
            var level = 0f;
            volume?.GetMasterVolume(out level);
            return Convert.ToInt32(level * 100);
        }

        public static void SetMasterVolume(float level)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                    return;

                masterVol.SetMasterVolumeLevelScalar(level / 100, Guid.Empty);
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        public static int GetMasterVolume()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                    return -1;

                float volumeLevel;
                masterVol.GetMasterVolumeLevelScalar(out volumeLevel);
                return Convert.ToInt32(volumeLevel * 100);
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        public static void MuteApp(IAudioSessionControl2 name)
        {
            var app = name as ISimpleAudioVolume;

            var guid = Guid.Empty;
            var muteState = false;
            app?.GetMute(out muteState);
            app?.SetMute(!muteState, ref guid);
        }

        public static bool AppMuteState(IAudioSessionControl2 name)
        {
            var app = name as ISimpleAudioVolume;

            var muteState = false;
            app?.GetMute(out muteState);
            return muteState;
        }

        public static void MuteMaster()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();

                var muteState = false;
                masterVol.GetMute(out muteState);
                masterVol.SetMute(!muteState, Guid.Empty);
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        public static bool MasterMuteState()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();

                var muteState = false;
                masterVol.GetMute(out muteState);
                return muteState;
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        public enum EDataFlow
        {
            ERender,
            ECapture,
            EAll,
            EDataFlowEnumCount
        }

        public enum ERole
        {
            EConsole,
            EMultimedia,
            ECommunications,
            ERoleEnumCount
        }

        [ComImport]
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMMDeviceEnumerator
        {
            int NotImpl1();

            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMmDevice ppDevice);

            // the rest is not implemented
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMmDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

            // the rest is not implemented
        }

        [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionManager2
        {
            int NotImpl1();
            int NotImpl2();

            [PreserveSig]
            int GetSessionEnumerator(out IAudioSessionEnumerator sessionEnum);

            // the rest is not implemented
        }

        [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionEnumerator
        {
            [PreserveSig]
            int GetCount(out int sessionCount);

            [PreserveSig]
            int GetSession(int sessionCount, out IAudioSessionControl session);
        }

        [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionControl
        {
            int NotImpl1();

            [PreserveSig]
            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            // the rest is not implemented
        }

        [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ISimpleAudioVolume
        {
            [PreserveSig]
            int SetMasterVolume(float fLevel, ref Guid eventContext);

            [PreserveSig]
            int GetMasterVolume(out float pfLevel);

            [PreserveSig]
            int SetMute(bool bMute, ref Guid eventContext);

            [PreserveSig]
            int GetMute(out bool pbMute);
        }

        public static class MmDeviceEnumeratorFactory
        {
            private static readonly Guid MmDeviceEnumerator = new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");

            public static IMMDeviceEnumerator CreateInstance()
            {
                var type = Type.GetTypeFromCLSID(MmDeviceEnumerator);
                return (IMMDeviceEnumerator)Activator.CreateInstance(type);
            }
        }

        public static IEnumerable<IAudioSessionControl2> EnumerateApplications()
        {
            // get the speakers (1st render + multimedia) device
            var deviceEnumerator = MmDeviceEnumeratorFactory.CreateInstance();
            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.ERender, ERole.EMultimedia, out IMmDevice speakers);

            // activate the session manager. we need the enumerator
            var iidIAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
            speakers.Activate(ref iidIAudioSessionManager2, 0, IntPtr.Zero, out object o);
            var mgr = (IAudioSessionManager2)o;

            // enumerate sessions for on this device
            mgr.GetSessionEnumerator(out IAudioSessionEnumerator sessionEnumerator);
            sessionEnumerator.GetCount(out int count);

            for (var i = 0; i < count; i++)
            {
                sessionEnumerator.GetSession(i, out IAudioSessionControl ctl);

                var ctl2 = ctl as IAudioSessionControl2;

                if (ctl2 != null)
                {
                    yield return ctl2;
                }
                
                if (ctl != null)
                    Marshal.ReleaseComObject(ctl);

                if (ctl2 != null)
                    Marshal.ReleaseComObject(ctl2);
            }

            Marshal.ReleaseComObject(sessionEnumerator);
            Marshal.ReleaseComObject(mgr);
            Marshal.ReleaseComObject(speakers);
            Marshal.ReleaseComObject(deviceEnumerator);
        }

        public enum AudioSessionState
        {
            AudioSessionStateInactive = 0,
            AudioSessionStateActive = 1,
            AudioSessionStateExpired = 2
        }

        public enum AudioSessionDisconnectReason
        {
            DisconnectReasonDeviceRemoval = 0,
            DisconnectReasonServerShutdown = (DisconnectReasonDeviceRemoval + 1),
            DisconnectReasonFormatChanged = (DisconnectReasonServerShutdown + 1),
            DisconnectReasonSessionLogoff = (DisconnectReasonFormatChanged + 1),
            DisconnectReasonSessionDisconnected = (DisconnectReasonSessionLogoff + 1),
            DisconnectReasonExclusiveModeOverride = (DisconnectReasonSessionDisconnected + 1)
        }

        [Guid("24918ACC-64B3-37C1-8CA9-74A66E9957A8"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioSessionEvents
        {
            [PreserveSig]
            int OnDisplayNameChanged([MarshalAs(UnmanagedType.LPWStr)] string newDisplayName, Guid eventContext);
            [PreserveSig]
            int OnIconPathChanged([MarshalAs(UnmanagedType.LPWStr)] string newIconPath, Guid eventContext);
            [PreserveSig]
            int OnSimpleVolumeChanged(float newVolume, bool newMute, Guid eventContext);
            [PreserveSig]
            int OnChannelVolumeChanged(uint channelCount, IntPtr newChannelVolumeArray, uint changedChannel, Guid eventContext);
            [PreserveSig]
            int OnGroupingParamChanged(Guid newGroupingParam, Guid eventContext);
            [PreserveSig]
            int OnStateChanged(AudioSessionState newState);
            [PreserveSig]
            int OnSessionDisconnected(AudioSessionDisconnectReason disconnectReason);
        }

        [Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioSessionControl2
        {
            [PreserveSig]
            int GetState(out AudioSessionState state);
            [PreserveSig]
            int GetDisplayName([Out(), MarshalAs(UnmanagedType.LPWStr)] out string name);
            [PreserveSig]
            int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string value, Guid eventContext);
            [PreserveSig]
            int GetIconPath([Out(), MarshalAs(UnmanagedType.LPWStr)] out string path);
            [PreserveSig]
            int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string value, Guid eventContext);
            [PreserveSig]
            int GetGroupingParam(out Guid groupingParam);
            [PreserveSig]
            int SetGroupingParam(Guid Override, Guid eventcontext);
            [PreserveSig]
            int RegisterAudioSessionNotification(IAudioSessionEvents newNotifications);
            [PreserveSig]
            int UnregisterAudioSessionNotification(IAudioSessionEvents newNotifications);
            [PreserveSig]
            int GetSessionIdentifier([Out(), MarshalAs(UnmanagedType.LPWStr)] out string retVal);
            [PreserveSig]
            int GetSessionInstanceIdentifier([Out(), MarshalAs(UnmanagedType.LPWStr)] out string retVal);
            [PreserveSig]
            int GetProcessId(out uint retvVal);
            [PreserveSig]
            int IsSystemSoundsSession();
            [PreserveSig]
            int SetDuckingPreference(bool optOut);
        }

        private static IAudioEndpointVolume GetMasterVolumeObject()
        {
            IMMDeviceEnumerator deviceEnumerator = null;
            IMmDevice speakers = null;
            try
            {
                deviceEnumerator = MmDeviceEnumeratorFactory.CreateInstance();
                deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.ERender, ERole.EMultimedia, out speakers);

                var IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
                object o;
                speakers.Activate(ref IID_IAudioEndpointVolume, 0, IntPtr.Zero, out o);
                IAudioEndpointVolume masterVol = (IAudioEndpointVolume)o;

                return masterVol;
            }
            finally
            {
                if (speakers != null) Marshal.ReleaseComObject(speakers);
                if (deviceEnumerator != null) Marshal.ReleaseComObject(deviceEnumerator);
            }
        }

        [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioEndpointVolume
        {
            [PreserveSig]
            int NotImpl1();

            [PreserveSig]
            int NotImpl2();

            /// <summary>
            /// Gets a count of the channels in the audio stream.
            /// </summary>
            /// <param name="channelCount">The number of channels.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetChannelCount(
                [Out] [MarshalAs(UnmanagedType.U4)] out UInt32 channelCount);

            /// <summary>
            /// Sets the master volume level of the audio stream, in decibels.
            /// </summary>
            /// <param name="level">The new master volume level in decibels.</param>
            /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int SetMasterVolumeLevel(
                [In] [MarshalAs(UnmanagedType.R4)] float level,
                [In] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

            /// <summary>
            /// Sets the master volume level, expressed as a normalized, audio-tapered value.
            /// </summary>
            /// <param name="level">The new master volume level expressed as a normalized value between 0.0 and 1.0.</param>
            /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int SetMasterVolumeLevelScalar(
                [In] [MarshalAs(UnmanagedType.R4)] float level,
                [In] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

            /// <summary>
            /// Gets the master volume level of the audio stream, in decibels.
            /// </summary>
            /// <param name="level">The volume level in decibels.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetMasterVolumeLevel(
                [Out] [MarshalAs(UnmanagedType.R4)] out float level);

            /// <summary>
            /// Gets the master volume level, expressed as a normalized, audio-tapered value.
            /// </summary>
            /// <param name="level">The volume level expressed as a normalized value between 0.0 and 1.0.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetMasterVolumeLevelScalar(
                [Out] [MarshalAs(UnmanagedType.R4)] out float level);

            /// <summary>
            /// Sets the volume level, in decibels, of the specified channel of the audio stream.
            /// </summary>
            /// <param name="channelNumber">The channel number.</param>
            /// <param name="level">The new volume level in decibels.</param>
            /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int SetChannelVolumeLevel(
                [In] [MarshalAs(UnmanagedType.U4)] UInt32 channelNumber,
                [In] [MarshalAs(UnmanagedType.R4)] float level,
                [In] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

            /// <summary>
            /// Sets the normalized, audio-tapered volume level of the specified channel in the audio stream.
            /// </summary>
            /// <param name="channelNumber">The channel number.</param>
            /// <param name="level">The new master volume level expressed as a normalized value between 0.0 and 1.0.</param>
            /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int SetChannelVolumeLevelScalar(
                [In] [MarshalAs(UnmanagedType.U4)] UInt32 channelNumber,
                [In] [MarshalAs(UnmanagedType.R4)] float level,
                [In] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

            /// <summary>
            /// Gets the volume level, in decibels, of the specified channel in the audio stream.
            /// </summary>
            /// <param name="channelNumber">The zero-based channel number.</param>
            /// <param name="level">The volume level in decibels.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetChannelVolumeLevel(
                [In] [MarshalAs(UnmanagedType.U4)] UInt32 channelNumber,
                [Out] [MarshalAs(UnmanagedType.R4)] out float level);

            /// <summary>
            /// Gets the normalized, audio-tapered volume level of the specified channel of the audio stream.
            /// </summary>
            /// <param name="channelNumber">The zero-based channel number.</param>
            /// <param name="level">The volume level expressed as a normalized value between 0.0 and 1.0.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetChannelVolumeLevelScalar(
                [In] [MarshalAs(UnmanagedType.U4)] UInt32 channelNumber,
                [Out] [MarshalAs(UnmanagedType.R4)] out float level);

            /// <summary>
            /// Sets the muting state of the audio stream.
            /// </summary>
            /// <param name="isMuted">True to mute the stream, or false to unmute the stream.</param>
            /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int SetMute(
                [In] [MarshalAs(UnmanagedType.Bool)] Boolean isMuted,
                [In] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

            /// <summary>
            /// Gets the muting state of the audio stream.
            /// </summary>
            /// <param name="isMuted">The muting state. True if the stream is muted, false otherwise.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetMute(
                [Out] [MarshalAs(UnmanagedType.Bool)] out Boolean isMuted);

            /// <summary>
            /// Gets information about the current step in the volume range.
            /// </summary>
            /// <param name="step">The current zero-based step index.</param>
            /// <param name="stepCount">The total number of steps in the volume range.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetVolumeStepInfo(
                [Out] [MarshalAs(UnmanagedType.U4)] out UInt32 step,
                [Out] [MarshalAs(UnmanagedType.U4)] out UInt32 stepCount);

            /// <summary>
            /// Increases the volume level by one step.
            /// </summary>
            /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int VolumeStepUp(
                [In] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

            /// <summary>
            /// Decreases the volume level by one step.
            /// </summary>
            /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int VolumeStepDown(
                [In] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

            /// <summary>
            /// Queries the audio endpoint device for its hardware-supported functions.
            /// </summary>
            /// <param name="hardwareSupportMask">A hardware support mask that indicates the capabilities of the endpoint.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int QueryHardwareSupport(
                [Out] [MarshalAs(UnmanagedType.U4)] out UInt32 hardwareSupportMask);

            /// <summary>
            /// Gets the volume range of the audio stream, in decibels.
            /// </summary>
            /// <param name="volumeMin">The minimum volume level in decibels.</param>
            /// <param name="volumeMax">The maximum volume level in decibels.</param>
            /// <param name="volumeStep">The volume increment level in decibels.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetVolumeRange(
                [Out] [MarshalAs(UnmanagedType.R4)] out float volumeMin,
                [Out] [MarshalAs(UnmanagedType.R4)] out float volumeMax,
                [Out] [MarshalAs(UnmanagedType.R4)] out float volumeStep);
        }
    }
}
