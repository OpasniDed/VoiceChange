using AdminToys;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using Mirror;
using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UserSettings.ServerSpecific;
using VoiceChange.Enums;
using VoiceChat.Networking;
using Object = UnityEngine.Object;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using MessageType = Exiled.API.Enums.MessageType;

namespace VoiceChange
{
    public class EventHandlers
    {
        private readonly Config _config;
        private readonly Dictionary<Player, SpeakerToy> _toggledPlayers;
        private readonly HashSet<Player> _scpVoice = new();

        public EventHandlers(Config config)
        {
            _config = config;
            _toggledPlayers = new();
        }

        public void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestarting;
            Exiled.Events.Handlers.Player.VoiceChatting += OnVoiceChatting;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;

            switch (_config.ActivationType)
            {
                case ActivationType.ServerSpecificSettings:
                    ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
                    Exiled.Events.Handlers.Player.Verified += OnVerified;
                    break;
                case ActivationType.NoClip:
                    Exiled.Events.Handlers.Player.TogglingNoClip += OnTogglingNoClip;
                    break;
            }
        }

        public void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestarting;
            Exiled.Events.Handlers.Player.VoiceChatting -= OnVoiceChatting;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;

            switch (_config.ActivationType)
            {
                case ActivationType.ServerSpecificSettings:
                    ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSettingValueReceived;
                    Exiled.Events.Handlers.Player.Verified -= OnVerified;
                    break;
                case ActivationType.NoClip:
                    Exiled.Events.Handlers.Player.TogglingNoClip -= OnTogglingNoClip;
                    break;
            }
        }

        private void OnRoundRestarting()
        {
            _toggledPlayers.Clear();
        }

        private void OnVoiceChatting(VoiceChattingEventArgs ev)
        {
            Player player = ev.Player;
            if (player == null) return;

            if (_toggledPlayers.TryGetValue(player, out SpeakerToy speaker))
            {
                OpusHandler opusHandler = OpusHandler.Get(player);
                float[] decoded = new float[480];
                opusHandler.Component.Decoder.Decode(ev.VoiceMessage.Data, ev.VoiceMessage.DataLength, decoded);

                if (player.IsScp && _config.pitchSettings.TryGetValue(player.Role.Type, out float pitch))
                {
                    opusHandler.Component.PitchShift(pitch, decoded.Length, 48000f, decoded);
                    ApplyReverb(decoded);
                }

                for (int i = 0; i < decoded.Length; i++)
                    decoded[i] *= _config.volume;

                byte[] encoded = new byte[512];
                int dataLen = opusHandler.Component.Encoder.Encode(decoded, encoded);
                AudioMessage audioMessage = new AudioMessage(speaker.ControllerId, encoded, dataLen);

                foreach (Player target in Player.List)
                {
                    if (target.Role is not IVoiceRole voiceRole ||
                        voiceRole.VoiceModule.ValidateReceive(player.ReferenceHub, VoiceChat.VoiceChatChannel.Proximity) == VoiceChat.VoiceChatChannel.None)
                        continue;
                    if (_config.UseDefaultChat && target.IsScp)
                        continue;

                    target.ReferenceHub.connectionToClient.Send(audioMessage);
                }
                ev.IsAllowed = _config.UseDefaultChat;
            }
            else if (ev.VoiceMessage.Channel == VoiceChat.VoiceChatChannel.ScpChat)
            {
                //❤❤❤ Я пидор ❤❤❤//
            }
        }

        private void ApplyReverb(float[] pcmData)
        {
            float decay = 0.4f;
            int delaySamples = 2000;

            for (int i = 0; i < pcmData.Length - delaySamples; i++)
            {
                pcmData[i + delaySamples] += pcmData[i] * decay;
            }

            float max = Mathf.Max(pcmData.Max(), Mathf.Abs(pcmData.Min()));
            if (max > 1f)
            {
                for (int i = 0; i < pcmData.Length; i++)
                {
                    pcmData[i] /= max;
                }
            }
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            Player player = ev.Player;

            if (_toggledPlayers.ContainsKey(player))
                ToggleProximity(player);

            if (_config.allowedScps.Contains(ev.NewRole))
            {
                ShowRoleMessage(player, _config.ShowHint);
            }
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            ServerSpecificSettingsSync.SendToPlayer(ev.Player.ReferenceHub);
        }

        private void OnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase settingBase)
        {
            if (!Player.TryGet(hub, out Player player) || !_config.allowedScps.Contains(player.Role.Type))
                return;

            if (settingBase is SSKeybindSetting keybindSetting &&
                keybindSetting.SettingId == _config.KeybindId &&
                keybindSetting.SyncIsPressed)
            {
                ToggleProximity(player);
            }
        }

        private void OnTogglingNoClip(TogglingNoClipEventArgs ev)
        {
            Player player = ev.Player;

            if (FpcNoclip.IsPermitted(player.ReferenceHub) || !_config.allowedScps.Contains(player.Role.Type))
                return;

            ToggleProximity(player);
            ev.IsAllowed = false;
        }

        private void ToggleProximity(Player player)
        {
            if (_toggledPlayers.ContainsKey(player))
            {
                _scpVoice.Remove(player);
                NetworkServer.Destroy(_toggledPlayers[player].gameObject);
                _toggledPlayers.Remove(player);

                OpusHandler.Remove(player);

                ShowRoleMessage(player, _config.ProximityDisabled);
            }
            else
            {
                _scpVoice.Add(player);
                SpeakerToy speaker = Object.Instantiate(PrefabHelper.GetPrefab<SpeakerToy>(PrefabType.SpeakerToy), player.Transform, true);
                NetworkServer.Spawn(speaker.gameObject);
                speaker.NetworkControllerId = (byte)player.Id;
                speaker.NetworkMinDistance = _config.MinDistance;
                speaker.NetworkMaxDistance = _config.MaxDistance;
                speaker.transform.position = player.Position;

                _toggledPlayers.Add(player, speaker);

                ShowRoleMessage(player, _config.ProximityEnabled);
            }
        }

        private void ShowRoleMessage(Player player, Message message)
        {
            if (!message.Show) return;

            switch (message.Type)
            {
                case MessageType.Broadcast:
                    player.Broadcast(message.Duration, message.Content);
                    break;
                case MessageType.Hint:
                    player.ShowHint(message.Content, message.Duration);
                    break;
            }
        }
    }
}
