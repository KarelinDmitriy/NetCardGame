using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.CardClasses
{
    public enum Suit 
    {
        Hearts=1,
        Diamonds,
        Clubs, 
        Spedes
    }

    public class Card
    {
        public Suit Suit { get; private set; }
        public int Value { get; private set; }

        public Card(Suit suit, int value)
        {
            Suit = suit;
            Value = value;
        }

        public byte[] Pack()
        {
            var res = new byte[2];
            res[0] = (byte)Suit;
            res[1] = (byte)Value;
            return res;
        }

        public static Card Unpack(byte[] arr)
        {
            if (arr.Length != 2)
                throw new InvalidOperationException("Не возможно распакавать карту");
            if (arr[0] == 0 && arr[1] == 0)
                return null;
            try
            {
                var suit = (Suit)arr[0];
                int val = arr[1];
                if (val < 6 || val > 14)
                    throw new InvalidOperationException("Не возможно распакавать карту");
                return new Card(suit, val);
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException("Не возможно распакавать карту");
            }
        }

        public override string ToString()
        {
            var res = string.Empty;
            switch (Suit)
            {
                case Suit.Clubs:
                    res += "C";
                    break;
                case Suit.Diamonds:
                    res += "D";
                    break;
                case Suit.Hearts:
                    res += "H";
                    break;
                case Suit.Spedes:
                    res += "S";
                    break;
            }
            return  Value+res;
        }

        public override int GetHashCode()
        {
            return Value + 20*(int) Suit;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(null, obj)) return false;
            var card = obj as Card;
            if (ReferenceEquals(card, null)) return false;
            if (card.Suit == Suit && card.Value == Value)
                return true;
            return false;
        }
    }
}
