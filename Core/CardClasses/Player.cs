using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.CardClasses
{
    public class Player
    {
        Queue<Card> cards = new Queue<Card>();
        List<Card> inHand = new List<Card>();
        public bool IsReady { get; set; }

        public Player()
        {
        }

        public void AddCard(Card card)
        {
            if (inHand.Count == 3)
                cards.Enqueue(card);
            else inHand.Add(card);
        }

        public Card ViewCard(int i)
        {
            return inHand[i];
        }

        public Card GetCard(int i)
        {
            var res = inHand[i];
            inHand.RemoveAt(i);
            if (cards.Any())
            {
                inHand.Add(cards.Dequeue());
            }
            return res;
        }

        public int Count()
        {
            return inHand.Count + cards.Count;
        }

        public bool IsLose()
        {
            return Count() == 0;
        }
    }
}
