using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GameOfLife.ViewModels;
using Brushes = System.Windows.Media.Brushes;


namespace GameOfLife.Views
{
    public class TableroCanvas : Canvas
    {
        public int Generacion
        {
            get { return (int)GetValue(GeneracionProperty); }
            set { SetValue(GeneracionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Generacion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GeneracionProperty =
            DependencyProperty.Register("Generacion", typeof(int), typeof(TableroCanvas), new PropertyMetadata(0, CambioGeneracion));


        private static void CambioGeneracion(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var celulasVivas = (List<Tuple<int, int>>)e.NewValue;
            //canvas._celulasVivas = celulasVivas;
            var canvas = (d as TableroCanvas);
            canvas._generacionAPintar = ((int)e.NewValue);
            canvas.InvalidateVisual();
        }


        const int TamanoCelula = 10;
        const int MargenCelula = 5;
        int? _ultimaGeneracion;
        int _generacionAPintar;
        List<Tuple<int, int>> _celulasVivas;
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var filas = (int)Math.Floor(ActualHeight / TamanoCelula);
            var columnas = (int)Math.Floor(ActualWidth / TamanoCelula);
            //var filas = 200;
            //var columnas = 200;

            var sw = Stopwatch.StartNew();
            _celulasVivas = ComputarCelulasVivas(filas, columnas,
                _ultimaGeneracion.HasValue && _generacionAPintar > _ultimaGeneracion.Value ? _celulasVivas : null);

            var pen = new Pen(Brushes.Black, 1);
            var rect = new Rect { Width = TamanoCelula - MargenCelula, Height = TamanoCelula - MargenCelula };

            foreach (var celulaViva in _celulasVivas)
            {
                rect.Y = celulaViva.Item1 * TamanoCelula;
                rect.X = celulaViva.Item2 * TamanoCelula;

                dc.DrawRectangle(Brushes.Black, pen, rect);
                //dc.DrawEllipse(Brushes.Black, pen, new Point
                //{
                //    X = rect.X - (TamanoCelula/2),
                //    Y = rect.Y - (TamanoCelula / 2)
                //}, TamanoCelula-MargenCelula, TamanoCelula-MargenCelula);
            }
            sw.Stop();
            if (_generacionAPintar > 0) _ultimaGeneracion = _generacionAPintar;
        }


        static readonly Random Randomizer = new Random();
        static List<Tuple<int, int>> ComputarCelulasVivas(int filas, int columnas, List<Tuple<int, int>> actuales)
        {
            if (actuales == null)
            {
                var celulasVivasAleatorias = (int)Math.Floor((filas * columnas * .50));
                return Enumerable.Range(0, celulasVivasAleatorias)
                        .Select(x => Tuple.Create(Randomizer.Next(filas), Randomizer.Next(columnas)))
                        .ToList();
            }

            var retval = new List<Tuple<int, int>>();
            for (var i = 0; i < filas; i++)
                for (var j = 0; j < columnas; j++)
                    if (EstaraViva(i, j, actuales)) retval.Add(Tuple.Create(i, j));

            return retval;
        }

        static bool EstaraViva(int fila, int col, List<Tuple<int, int>> actuales)
        {
            var vecinos = 0;
            var estaViva = actuales.Any(x => x.Item1 == fila && x.Item2 == col);

            for (var i = fila - 1; i <= fila + 1; i++)
            {
                if (i < 0 /*|| i > filas - 1*/) continue;
                for (var j = col - 1; j <= col + 1; j++)
                {
                    if ((fila == i && col == j) || j < 0 /*|| j > columnas - 1*/) continue;

                    vecinos += actuales.Any(x => x.Item1 == i && x.Item2 == j) ? 1 : 0;
                }
            }

            return
                /*
                    Any live cell with fewer than two live neighbours dies, as if caused by under-population.
                    Any live cell with two or three live neighbours lives on to the next generation.
                    Any live cell with more than three live neighbours dies, as if by overcrowding.                
                */
                (estaViva && vecinos >= 2 && vecinos <= 3) ||
                //Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
                (!estaViva && vecinos == 3);
        }
    }
}
