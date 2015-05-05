using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.CardClasses;

namespace CardGame.Net
{
    internal static class NetExtentions
    {
        public static byte[] PackPlayer(this PlayerInfo player, string name)
        {
            byte msgLength = 0;
            var nameUtf8 = Encoding.UTF8.GetBytes(name);
            var nameLength = (byte) nameUtf8.Length;
            var inHand = player.Player.GetCards4Send();
            var inHandLength = (byte) inHand.Length;
            var inGame = player.CardInGame.SafeGetTopCard();
            var status = new[] {player.IsLose ? (byte) 2 : (byte) 1};
            var isDispute = new[] {player.InDispute ? (byte) 2 : (byte) 1};
            var isWinInStep = new[] {player.IsWinInStep ? (byte) 2 : (byte) 1};
            var isLose = new[] {player.IsLose ? (byte) 2 : (byte) 1};
            var cardCount = new[] {(byte) player.Player.Count()};
            msgLength = (byte) (1 + (1 + nameLength) + (1 + inHandLength) 
                + 2 + 1 + 1 + 1 + 1 + 1);
            var result = new Byte[3];
            result[0] = msgLength;
            result[1] = nameLength;
            result[2] = inHandLength;
            return result.Concat(nameUtf8).Concat(inHand)
                .Concat(inGame).Concat(status)
                .Concat(isDispute).Concat(isWinInStep)
                .Concat(isLose).Concat(cardCount).ToArray();
        }

        public static byte[] PackGameToPlayers(this IDictionary<string, PlayerInfo> info)
        {
            return info.SelectMany(x => x.Value.PackPlayer(x.Key)).ToArray();
        }

        public static byte[] PackPlayerName(this string name)
        {
            var bName = Encoding.UTF8.GetBytes(name);
            return new[] {(byte) bName.Length}.Concat(bName).ToArray();
        }

        public static List<PlayerData> UnpackPlayerInfo(this byte[] array)
        {
            var result = new List<PlayerData>();
            var firstMsgLength = array[1];
            result.Add(array.GetSubArray(1, firstMsgLength).GetPlayerData());
            var idx = 1 + firstMsgLength;
            var secondMsgLength = array[idx];
            result.Add(array.GetSubArray(idx, secondMsgLength).GetPlayerData());
            idx += secondMsgLength;
            var thridMsgLength = array[idx];
            result.Add(array.GetSubArray(idx, thridMsgLength).GetPlayerData());
            return result;
        }

        private static PlayerData GetPlayerData(this byte[] array)
        {
            var result = new PlayerData {Cards = new List<Card>()};
            var nameLength = array[1];
            var name = Encoding.UTF8.GetString(array, 3, nameLength);
            result.Name = name;
            var cardCount = array[2]/2;
            for (var i = 0; i < cardCount; i++)
            {
                var cr = array.GetSubArray(3 + nameLength + i*2, 2);
                var card = Card.Unpack(cr);
                result.Cards.Add(card);
            }
            var inG = array.GetSubArray(3 + nameLength + cardCount*2, 2);
            result.CardInGame = Card.Unpack(inG);
            result.IsReady = array[3 + nameLength + cardCount*2 + 2] == 2;
            result.InDispute = array[3 + nameLength + cardCount*2 + 3] == 2;
            result.IsWinInStep = array[3 + nameLength + cardCount*2 + 4] == 2;
            result.IsLose = array[3 + nameLength + cardCount*2 + 5] == 2;
            result.CardCount = array[3 + nameLength + cardCount*2 + 6];
            return result;
        }

        private static byte[] GetSubArray(this byte[] array, int start, int count)
        {
            var result = new List<byte>();
            var lCount = count;
            for (var i = start; lCount > 0; i++, lCount--)
            {
                result.Add(array[i]);
            }
            return result.ToArray();
        }

        private static IEnumerable<byte> SafeGetTopCard(this Stack<Card> stack)
        {
            return stack.Any() ? stack.Peek().Pack() : new byte[2];
        }
    }
}