using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing; // чтобы исплоьзовать Graphics
using System.Drawing.Imaging;

namespace WindowsFormsApp13
{
    public class Particle
    {
        public int Radius; // радуис частицы
        public float X; // X координата положения частицы в пространстве
        public float Y; // Y координата положения частицы в пространстве

        public float Direction; // направление движения
        public float Speed; // скорость перемещения

        public float Life; // запас здоровья частицы

        // добавили генератор случайных чисел
        public static Random rand = new Random();

        // метод генерации частицы
        public static Particle Generate()
        {
            // я не заполняю координаты X, Y потому что хочу, чтобы все частицы возникали из одного места
            return new Particle
            {
                Direction = rand.Next(360),
                Speed = 1 + rand.Next(10),
                Radius = 2 + rand.Next(10),
                Life = 20 + rand.Next(100),
            };
        }

        public virtual void Draw(Graphics g)
        {
            // рассчитываем коэффициент прозрачности по шкале от 0 до 1.0
            float k = Math.Min(1f, Life / 100);

            // рассчитываем значение альфа канала в шкале от 0 до 255
            int alpha = (int)(k * 255);

            // создаем цвет из уже существующего, но привязываем к нему еще и значение альфа канала
            var color = Color.FromArgb(alpha, Color.Black);
            var b = new SolidBrush(color);

            // нарисовали залитый кружок радиусом Radius с центром в X, Y
            g.FillEllipse(b, X - Radius, Y - Radius, Radius * 2, Radius * 2);

            // удалили кисть из памяти
            b.Dispose();
        }
    }

    // новый класс для цветных частиц
    public class ParticleColorful : Particle
    {
        // два новых поля под цвет начальный и конечный
        public Color FromColor;
        public Color ToColor;

        // для смеси цветов
        public static Color MixColor(Color color1, Color color2, float k)
        {
            return Color.FromArgb(
                (int)(color2.A * k + color1.A * (1 - k)),
                (int)(color2.R * k + color1.R * (1 - k)),
                (int)(color2.G * k + color1.G * (1 - k)),
                (int)(color2.B * k + color1.B * (1 - k))
            );
        }

        // подменяем метод генерации на новый, который будет возвращать ParticleColorful
        public new static ParticleColorful Generate()
        {
            return new ParticleColorful
            {
                Direction = rand.Next(360),
                Speed = 1 + rand.Next(10),
                Radius = 2 + rand.Next(10),
                Life = 20 + rand.Next(100),
            };
        }

        public override void Draw(Graphics g)
        {
            float k = Math.Min(1f, Life / 100);

            // так как k уменшается от 1 до 0, то порядок цветов обратный
            var color = MixColor(ToColor, FromColor, k);
            var b = new SolidBrush(color);

            g.FillEllipse(b, X - Radius, Y - Radius, Radius * 2, Radius * 2);

            b.Dispose();
        }
    }

    public class ParticleImage : Particle
    {
        public Image image;
        public Color FromColor;
        public Color ToColor;

        public new static ParticleImage Generate()
        {
            return new ParticleImage
            {
                Direction = rand.Next(360),
                Speed = 1 + rand.Next(10),
                Radius = 2 + rand.Next(10),
                Life = 20 + rand.Next(100),
            };
        }

        public override void Draw(Graphics g)
        {
            float k = Math.Min(1f, Life / 100);

            var color = ParticleColorful.MixColor(ToColor, FromColor, k);

            // матрица преобразования цвета
            // типа аналога матрицы трансформации, но для цвета
            ColorMatrix matrix = new ColorMatrix(new float[][]{
            new float[] {0, 0, 0, 0, 0}, // умножаем текущий красный цвет на 0
            new float[] {0, 0, 0, 0, 0}, // умножаем текущий зеленый цвет на 0
            new float[] {0, 0, 0, 0, 0}, // умножаем текущий синий цвет на 0
            new float[] {0, 0, 0, k, 0}, // тут подставляем k который прозрачность задает
            new float[] {(float)color.R / 255, (float)color.G / 255, (float)color.B/255, 0, 1F}}); // а сюда пихаем рассчитанный цвет переведенный из шкалы от 0 до 255 в шкалу от 0 до 1

            // эту матрицу пихают в атрибуты
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(matrix);

            g.DrawImage(image,
                // куда рисовать
                new Rectangle((int)(X - Radius), (int)(Y - Radius), Radius * 2, Radius * 2),
                // и какую часть исходного изображения брать, в нашем случае все изображения
                0, 0, image.Width, image.Height,
                GraphicsUnit.Pixel, // надо передать
                imageAttributes // наши атрибуты с матрицей преобразования
               );
        }
    }

}
