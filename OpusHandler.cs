using Exiled.API.Features;
using System;
using System.Collections.Generic;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;

namespace VoiceChange
{
    public class OpusHandler
    {
        private static readonly Dictionary<Player, OpusHandler> Handlers = new();

        public OpusComponent Component { get; private set; }

        private OpusHandler(Player player)
        {
            Component = OpusComponent.Get(player.ReferenceHub);
        }

        public static OpusHandler Get(Player player)
        {
            if (Handlers.TryGetValue(player, out OpusHandler opusHandler))
                return opusHandler;

            opusHandler = new OpusHandler(player);
            Handlers.Add(player, opusHandler);
            return opusHandler;
        }

        public static void Remove(Player player)
        {
            if (Handlers.TryGetValue(player, out OpusHandler opusHandler))
            {
                OpusComponent.Remove(player.ReferenceHub);
                Handlers.Remove(player);
            }
        }
    }
}