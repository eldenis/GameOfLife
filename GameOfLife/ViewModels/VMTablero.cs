using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GameOfLife.Utils;

namespace GameOfLife.ViewModels
{
    public class VMTablero : ObjetoObservable
    {
        //readonly TimeSpan _intervaloEntreGeneraciones = TimeSpan.FromMilliseconds(83);
        static readonly Random Randomizer = new Random();

        internal static VMTablero CrearTablero(Rect tamanoTablero, int tamanoCelula, int margenCelula, bool aleatorio)
        {
            return new VMTablero
            {
                Aleatorio = aleatorio,
                TamanoCelula = tamanoCelula,
                MargenCelula = margenCelula,
                Tamano = tamanoTablero
            };
        }


        public void IniciarAutomata(string viveCon, string resucitaCon, int velocidad)
        {
            EstaActivo = true;

            ViveCon = viveCon.Split(',').Select(int.Parse).ToArray();
            ResucitaCon = resucitaCon.Split(',').Select(int.Parse).ToArray();

            new Task(() => Proceso(velocidad),
               _cancelToken = (_cancelTokenSource = new CancellationTokenSource()).Token).Start();
        }

        public void DetenerAutomata()
        {
            _cancelTokenSource.Cancel();
            EstaActivo = false;
        }

        CancellationTokenSource _cancelTokenSource;
        CancellationToken _cancelToken;
        void Proceso(int velocidad)
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            while (!_cancelToken.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
                ComputarGeneracion();
                sw.Stop();
                Thread.Sleep(velocidad);
            }
        }

        private void ComputarGeneracion()
        {
            var celVivas = ComputarCelulasVivas(CelulasVivas, ViveCon, ResucitaCon, OperacionCelulas.Automata);

            Application.Current.Dispatcher.Invoke(() =>
            {
                CelulasVivas = celVivas;
                Vista = ObtenerVistaActual();
                Generacion++;
            }, DispatcherPriority.Send);
        }


        void EscribirVista()
        {
            if (!(Tamano.Width > 0) || !(Tamano.Height > 0)) return;

            var filas = (int)Math.Floor(Tamano.Height / TamanoCelula);
            var columnas = (int)Math.Floor(Tamano.Width / TamanoCelula);
            _celulasAfectadas = new bool[filas, columnas];

            var celulasVivas = new bool[filas, columnas];
            CelulasVivas = ComputarCelulasVivas(celulasVivas,
                null, null, Aleatorio ? OperacionCelulas.Aleatorio : OperacionCelulas.Vacio);

            Vista = ObtenerVistaActual();
        }

        bool[,] _celulasAfectadas;
        internal void MovimientoMouse(MouseEventArgs args, Point posicion)
        {
            if (CelulasVivas == null) return;
            if (args.LeftButton == MouseButtonState.Released)
            {
                var filas = (int)Math.Floor(Tamano.Height / TamanoCelula);
                var columnas = (int)Math.Floor(Tamano.Width / TamanoCelula);
                _celulasAfectadas = new bool[filas, columnas];
                return;
            }

            var fila = (int)Math.Floor(posicion.Y / TamanoCelula);
            var columna = (int)Math.Floor(posicion.X / TamanoCelula);

            if (_celulasAfectadas == null ||
                _celulasAfectadas.GetUpperBound(0) < fila ||
                _celulasAfectadas.GetUpperBound(1) < columna ||
                _celulasAfectadas[fila, columna]) return;

            _celulasAfectadas[fila, columna] = true;
            CelulasVivas[fila, columna] = !CelulasVivas[fila, columna];
            Vista = ObtenerVistaActual();
        }

        enum OperacionCelulas { Vacio = 0, Aleatorio = 1, Automata = 2 }
        static bool[,] ComputarCelulasVivas(bool[,] actuales, int[] viveCon, int[] resucitaCon, OperacionCelulas operacion)
        {
            if (actuales == null) return null;
            var filas = actuales.GetUpperBound(0) + 1;
            var columnas = actuales.GetUpperBound(1) + 1;
            var retval = new bool[filas, columnas];

            //var sw = Stopwatch.StartNew();
            if (operacion != OperacionCelulas.Vacio)
                for (var i = 0; i < filas; i++)
                    for (var j = 0; j < columnas; j++)
                        switch (operacion)
                        {
                            case OperacionCelulas.Aleatorio:
                                retval[i, j] = Randomizer.Next(3) == 1;
                                break;
                            case OperacionCelulas.Automata:
                                retval[i, j] = EstaraViva(i, j, actuales, viveCon, resucitaCon);
                                break;
                        }

            //sw.Stop();

            return retval;
        }

        static bool EstaraViva(int fila, int col, bool[,] actuales, int[] viveCon, int[] resucitaCon)
        {
            var vecinos = 0;
            var estaViva = actuales[fila, col];
            var filas = actuales.GetUpperBound(0) + 1;
            var columnas = actuales.GetUpperBound(1) + 1;

            for (var i = fila - 1; i <= fila + 1; i++)
            {
                if (i < 0 || i > filas - 1) continue;
                for (var j = col - 1; j <= col + 1; j++)
                {
                    if ((fila == i && col == j) || j < 0 || j > columnas - 1) continue;

                    vecinos += actuales[i, j] ? 1 : 0;
                }
            }

            //return
            //    /*
            //        Any live cell with fewer than two live neighbours dies, as if caused by under-population.
            //        Any live cell with two or three live neighbours lives on to the next generation.
            //        Any live cell with more than three live neighbours dies, as if by overcrowding.                
            //    */
            //    (estaViva && vecinos >= 2 && vecinos <= 3) ||
            //    //Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
            //    (!estaViva && vecinos == 3);

            return (estaViva && viveCon.Contains(vecinos)) ||
                (!estaViva && resucitaCon.Contains(vecinos));
        }

        WriteableBitmap ObtenerVistaActual()
        {
            var altura = (int)Tamano.Height;
            var ancho = (int)Tamano.Width;

            if (altura == 0 || ancho == 0) return null;
            var bitmap = BitmapFactory.New(ancho, altura);

            bitmap.Lock();
            bitmap.Clear(Colors.Black);

            if (CelulasVivas != null)
            {
                var tamanoRealCelula = TamanoCelula - MargenCelula;
                var filas = CelulasVivas.GetUpperBound(0) + 1;
                var columnas = CelulasVivas.GetUpperBound(1) + 1;

                for (var i = 0; i < filas; i++)
                    for (var j = 0; j < columnas; j++)
                        if (CelulasVivas[i, j])
                        {
                            var coordX = (TamanoCelula * j) + MargenCelula;
                            var coordY = (TamanoCelula * i) + MargenCelula;

                            bitmap.FillRectangle(
                                coordX,
                                coordY,
                                coordX + tamanoRealCelula,
                                coordY + tamanoRealCelula,
                                Colors.Green);
                        }
            }

            bitmap.Unlock();

            return bitmap;
        }


        private int[] _resucitaCon;
        public int[] ResucitaCon
        {
            get { return _resucitaCon; }
            set
            {
                _resucitaCon = value;
                OnPropertyChanged();
            }
        }



        private int[] _viveCon;
        public int[] ViveCon
        {
            get { return _viveCon; }
            set
            {
                _viveCon = value;
                OnPropertyChanged();
            }
        }



        private bool[,] _celulasVivas;
        public bool[,] CelulasVivas
        {
            get { return _celulasVivas; }
            set
            {
                _celulasVivas = value;
                OnPropertyChanged();
            }
        }



        private int _tamanoCelula;
        public int TamanoCelula
        {
            get { return _tamanoCelula; }
            set
            {
                _tamanoCelula = value;
                OnPropertyChanged();
            }
        }

        private int _margenCelula;
        public int MargenCelula
        {
            get { return _margenCelula; }
            set
            {
                _margenCelula = value;
                OnPropertyChanged();
            }
        }


        private bool _aleatorio;
        public bool Aleatorio
        {
            get { return _aleatorio; }
            set
            {
                _aleatorio = value;
                OnPropertyChanged();
            }
        }

        private Rect _tamano;
        public Rect Tamano
        {
            get { return _tamano; }
            set
            {
                _tamano = value;
                CelulasVivas = null;
                EscribirVista();
                OnPropertyChanged();
            }
        }

        private WriteableBitmap _vista;
        public WriteableBitmap Vista
        {
            get { return _vista; }
            set
            {
                _vista = value;
                OnPropertyChanged();
            }
        }


        private int _generacion;
        public int Generacion
        {
            get { return _generacion; }
            set
            {
                _generacion = value;
                OnPropertyChanged();
            }
        }


        private bool _estaActivo;
        public bool EstaActivo
        {
            get { return _estaActivo; }
            set
            {
                _estaActivo = value;
                OnPropertyChanged();
            }
        }



    }
}
