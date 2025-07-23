using Exiled.API.Features;
using Exiled.API.Features.Core.UserSettings;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoiceChange.Enums;
using VoiceChat.Networking;

namespace VoiceChange
{
    public class Plugin : Plugin<Config>
    {
        // Прочитай!! Основная логика изменения голоса находится в OpusComponent, OpusHandler нужен тупо для того чтобы разговаривать за сцп. Это уже все используется в EventHandlers
        public override string Author => "OpasniDed";
        public override string Name => "VoiceChange";
        public override string Prefix => "VoiceChange";
        public static Plugin plugin;
        private EventHandlers _eventHandler;

        public Dictionary<ReferenceHub, OpusComponent> Encoders = new();
        public List<Exiled.API.Features.Player> scpVoice = new();
        public List<Exiled.API.Features.Player> scp999Player = new();

        public override void OnEnabled()
        {
            plugin = this;
            _eventHandler = new EventHandlers(Config);
            _eventHandler.RegisterEvents();
            if (Config.ActivationType == ActivationType.ServerSpecificSettings)
            {
                HeaderSetting header = new HeaderSetting(Config.SettingHeaderLabel);
                IEnumerable<SettingBase> settingBases = new SettingBase[]
                {
                    header,
                    new KeybindSetting(Config.KeybindId, Config.KeybindLabel, default, hintDescription: Config.KeybindHintLabel)
                };
                SettingBase.Register(settingBases);
                SettingBase.SendToAll();
            }
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            _eventHandler.UnregisterEvents();
            plugin = null;
            _eventHandler = null;
            base.OnDisabled();
        }

        //public void OnVoiceChatting(VoiceChattingEventArgs ev)
        //{
        //    if (ev.Player == null) return;

        //    if (ev.Player.IsScp)
        //    {
        //        if (!Config.pitchSettings.TryGetValue(ev.Player.Role.Type, out float pitch))
        //        {
        //            return;
        //        }
        //        if (!scpVoice.Contains(ev.Player)) return;

        //        var modifiedMessage = new VoiceMessage
        //        {
        //            Speaker = ev.VoiceMessage.Speaker,
        //            Channel = ev.VoiceMessage.Channel,
        //            Data = new byte[ev.VoiceMessage.Data.Length]
        //        };
        //        Array.Copy(ev.VoiceMessage.Data, modifiedMessage.Data, ev.VoiceMessage.Data.Length);

        //        OpusComponent opusComponent = OpusComponent.Get(ev.Player.ReferenceHub);
        //        if (opusComponent == null) return;

        //        float[] pcmData = new float[480];

        //        int decodedLegth = opusComponent.Decoder.Decode(ev.VoiceMessage.Data, ev.VoiceMessage.DataLength, pcmData);
        //        opusComponent.PitchShift(pitch, pcmData.Length, 48000f, pcmData);
        //        ApplyReverb(pcmData);

        //        modifiedMessage.DataLength = opusComponent.Encoder.Encode(pcmData, modifiedMessage.Data, 480);
        //        ev.VoiceMessage = modifiedMessage;
        //    }
        //}

        //private void ApplyReverb(float[] pcmData)
        //{
        //    float decay = 0.4f;
        //    int delaySamples = 2000;

        //    for (int i = 0; i < pcmData.Length - delaySamples; i++)
        //    {
        //        pcmData[i + delaySamples] += pcmData[i] * decay;
        //    }

        //    float max = Mathf.Max(pcmData.Max(), Mathf.Abs(pcmData.Min()));
        //    if (max > 1f)
        //    {
        //        for (int i = 0; i < pcmData.Length; i++)
        //        {
        //            pcmData[i] /= max;
        //        }
        //    }
        //}


    }
}