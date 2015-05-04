using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Core.CardClasses;

namespace CardGame.Net
{
    public class Painter : IDisposable
    {
        private Dictionary<Card, Image> images;
        private readonly Control playerControl;
        private readonly Control boardControl;
        private List<PlayerData> dataEnemys;
        private PlayerData myData;
        private string name;

        public Painter(Control playerControl, Control boardControl)
        {
            this.playerControl = playerControl;
            this.boardControl = boardControl;
            images = new Dictionary<Card, Image>();
            for (var i = 1; i < 5; i++)
            {
                for (var j = 6; j < 15; j++)
                {
                    var card = new Card((Suit)i, j);
                    var path = "Picture/" + card + ".png";
                    var img = Image.FromFile(path);
                    images.Add(card, img);
                }
            }
            this.playerControl.Paint += playerControl_Paint;
            this.boardControl.Paint += boardControl_Paint;
        }

        private void boardControl_Paint(object sender, PaintEventArgs e)
        {
            //создаем буффер и контекст для него
            //определяем контекст
            var context = BufferedGraphicsManager.Current;
            //определяем размер контекста
            context.MaximumBuffer = new Size(boardControl.Width + 1, boardControl.Height + 1);
            var rec = new Rectangle(0, 0, boardControl.Width, boardControl.Height);
            //на основе контекста создаем буфер
            var buffer = context.Allocate(e.Graphics, rec);
            var brush = new SolidBrush(Color.Black);
            buffer.Graphics.FillRectangle(brush, rec);
            //отрисовка чего то там
            if (myData != null && dataEnemys != null)
            {
                var first = dataEnemys.First().CardInGame;
                if (first != null)
                    buffer.Graphics.DrawImage(images[first], 2, 70);
                var second = dataEnemys.Last().CardInGame;
                if (second != null)
                    buffer.Graphics.DrawImage(images[second], 450, 70);
                var my = myData.CardInGame;
                if (my != null)
                    buffer.Graphics.DrawImage(images[my], 226, 90);
            }
            //закончили отрисовку
            //Выводим буффер на экран
            buffer.Render(e.Graphics);
            //и очищаем память
            buffer.Dispose();
        }

        private void playerControl_Paint(object sender, PaintEventArgs e)
        {
            //создаем буффер и контекст для него
            //определяем контекст
            var context = BufferedGraphicsManager.Current;
            //определяем размер контекста
            context.MaximumBuffer = new Size(playerControl.Width + 1, playerControl.Height + 1);
            var rec = new Rectangle(0, 0, playerControl.Width, playerControl.Height);
            //на основе контекста создаем буфер
            var buffer = context.Allocate(e.Graphics, rec);
            //отрисовка чего то там
            if (myData != null)
            {
                for (var i = 0; i < myData.Cards.Count; i++)
                {
                    var y = 2;
                    var x = 2 + i * 224;
                    var card = myData.Cards[i];
                    buffer.Graphics.DrawImage(images[card], x, y);
                }
            }
            //закончили отрисовку
            //Выводим буффер на экран
            buffer.Render(e.Graphics);
            //и очищаем память
            buffer.Dispose();
        }

        public void SetData(List<PlayerData> data, string name)
        {
            dataEnemys = data.Where(x => x.Name != name).ToList();
            myData = data.First(x => x.Name == name);
            this.name = name;
            playerControl.Invalidate();
            boardControl.Invalidate();
        }

        public void Dispose()
        {
            playerControl.Paint -= playerControl_Paint;
            boardControl.Paint -= boardControl_Paint;
        }
    }
}