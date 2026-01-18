using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Attic.DI;
using Attic.Utilities;
using Edgegap;
using KILLGRID.Networking.Edgegap;
using Mirror;
using UnityEngine;

namespace KILLGRID.Networking.Mirror
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected
    }

    [Injectable]
    public class MirrorRelaySessionController : CardboardCoreBehaviour
    {
        [SerializeField] private NetworkManager networkManager;

        [Header("Edgegap Relay Settings")]
        [SerializeField] private string devRelayProfileToken = "DEV_RELAY_PROFILE_TOKEN";
        [SerializeField] private string stagingRelayProfileToken = "STAGING_RELAY_PROFILE_TOKEN";
        [SerializeField] private string prodRelayProfileToken = "PROD_RELAY_PROFILE_TOKEN";

        private ConnectionState serverConnectionState;
        private ConnectionState clientConnectionState;

        protected override InjectTiming MyInjectTiming => InjectTiming.Start;

        /// <summary>
        /// Only available after when hosting a session
        /// </summary>
        public string SessionId { get; private set; }
        public bool IsHost { get; private set; }

        protected override void OnInjected()
        {
            string relayProfileToken = devRelayProfileToken;

#if Dev
            relayProfileToken = devRelayProfileToken;
#elif Staging
            relayProfileToken = stagingRelayProfileToken;
#elif Prod
            relayProfileToken = prodRelayProfileToken;
#endif

            Log.Write($"Using Edgegap Relay Profile Token: {relayProfileToken}");

            EdgegapRelayService.Initialize(relayProfileToken);
        }

        protected override void OnReleased()
        {
        }

        public async Task<ApiResponse> CreateSession()
        {
            if (serverConnectionState != ConnectionState.Disconnected)
            {
                return new ApiResponse();
            }

            Log.Write("Creating Edgegap Relay Session");

            serverConnectionState = ConnectionState.Connecting;

            try
            {
                ApiResponse data = await EdgegapRelayService.CreateSessionAsync(networkManager.maxConnections);

                Log.Write("Created Edgegap Relay Session");
                Log.Write($"Session id: {data.session_id}");

                EdgegapKcpTransport transport = networkManager.transport as EdgegapKcpTransport;

                transport.relayAddress = data.relay.ip;
                transport.relayGameServerPort = data.relay.ports.server.port;
                transport.relayGameClientPort = data.relay.ports.client.port;
                transport.sessionId = data.authorization_token ?? 0;
                transport.userId = data.session_users?[0].authorization_token ?? 0;

                networkManager.StartHost();

                serverConnectionState = ConnectionState.Connected;

                SessionId = data.session_id;

                IsHost = true;

                return data;
            }
            catch (Exception e)
            {
                serverConnectionState = ConnectionState.Disconnected;

                ApiResponse apiResponse = new ApiResponse
                {
                    error = e.Message
                };

                return apiResponse;
            }
        }

        public async Task<ApiResponse> FindSession()
        {
            Log.Write("Finding Edgegap Relay Session");

            try
            {
                string ignoredSessionIdsString = PlayerPrefs.GetString("IgnoredSessionIds", "");
                List<string> ignoredSessionIds = new List<string>(ignoredSessionIdsString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

                ApiResponse data = await EdgegapRelayService.FindSessionAsync(ignoredSessionIds);

                if (data == null || string.IsNullOrEmpty(data.session_id))
                {
                    Log.Error("No Edgegap Relay Session found.");
                    return new ApiResponse { error = "No Edgegap Relay Session found." };
                }

                Log.Write($"Found Edgegap Relay Session: {data.session_id}");

                CacheSessionId(data.session_id);

                return data;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to find session - {e.Message}");
                return new ApiResponse { error = e.Message };
            }
        }

        public async Task<ApiResponse> JoinSession(string sessionId)
        {
            if (clientConnectionState != ConnectionState.Disconnected)
            {
                return new ApiResponse();
            }

            Log.Write("Joining Edgegap Relay Session");

            clientConnectionState = ConnectionState.Connecting;

            try
            {
                ApiResponse data = await EdgegapRelayService.JoinSessionAsync(sessionId);

                Log.Write("Joined Edgegap Relay Session");
                Log.Write($"Session id: {data.session_id}");

                if (networkManager.transport is not EdgegapKcpTransport transport)
                {
                    Log.Error("NetworkManager transport is not EdgegapKcpTransport. Cannot join session.");
                    clientConnectionState = ConnectionState.Disconnected;
                    return new ApiResponse { error = "NetworkManager transport is not EdgegapKcpTransport." };
                }

                transport.relayAddress = data.relay.ip;
                transport.relayGameServerPort = data.relay.ports.server.port;
                transport.relayGameClientPort = data.relay.ports.client.port;
                transport.sessionId = data.authorization_token ?? 0;
                transport.userId = data.session_users?[1].authorization_token ?? 0;

                networkManager.StartClient();

                clientConnectionState = ConnectionState.Connected;

                IsHost = false;

                return data;
            }
            catch (Exception e)
            {
                clientConnectionState = ConnectionState.Disconnected;

                ApiResponse apiResponse = new ApiResponse
                {
                    error = e.Message
                };

                return apiResponse;
            }
        }

        public async Task<ApiResponse> DeleteSession(string sessionId)
        {
            Log.Write("Deleting Edgegap Relay Session");

            try
            {
                ApiResponse data = await EdgegapRelayService.DeleteSessionAsync(sessionId);

                Log.Write($"Deleted Edgegap Relay Session {data.session_id}");

                return data;
            }
            catch (Exception e)
            {
                // Throws every time, even if the session is deleted successfully
                // Log.Error($"Failed to delete session {sessionId} - {e.Message}");
                return new ApiResponse();
            }
        }

        public async void Disconnect(Action callback = null)
        {
            if (networkManager.transport is not EdgegapKcpTransport transport)
            {
                Log.Error("NetworkManager transport is not EdgegapKcpTransport. Cannot disconnect.");
                return;
            }

            switch (serverConnectionState)
            {
                case ConnectionState.Connected:

                    networkManager.StopHost();

                    await DeleteSession(SessionId);

                    transport.ServerStop();

                    ClearSessionId();

                    serverConnectionState = ConnectionState.Disconnected;

                    callback?.Invoke();

                    break;
            }

            switch (clientConnectionState)
            {
                case ConnectionState.Connected:

                    transport.ClientDisconnect();
                    networkManager.StopClient();

                    ClearSessionId();

                    clientConnectionState = ConnectionState.Disconnected;

                    callback?.Invoke();
                    break;
            }
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        public void CacheSessionId(string sessionId)
        {
            if (!string.IsNullOrEmpty(SessionId))
            {
                Log.Warn($"You are trying to cache a session id when it is already cached. Current session id: {SessionId}");
            }

            SessionId = sessionId;
        }

        public void AddCurrentSessionIdToIgnoreList()
        {
            // Add to playerprefs so it persists between sessions
            string ignoredSessionIdsString = PlayerPrefs.GetString("IgnoredSessionIds", "");
            ignoredSessionIdsString += SessionId + ";";
            PlayerPrefs.SetString("IgnoredSessionIds", ignoredSessionIdsString);
        }

        public void ClearSessionId()
        {
            SessionId = null;
            IsHost = false;
        }
    }
}
