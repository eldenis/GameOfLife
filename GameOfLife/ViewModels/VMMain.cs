using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GameOfLife.Utils;

namespace GameOfLife.ViewModels
{
    class VMMain : ObjetoObservable
    {
        const int TamanoCelulaPredeterminado = 50;
        const int MargenPredeterminado = 2;

        const int TamanoCelulaMinimo = 1;
        const int TamanoCelulaMaximo = 100;
        const int MargenMinimo = 0;
        const int MargenMaximo = 9;


        private VMTablero _tablero;
        public VMTablero Tablero
        {
            get { return _tablero; }
            set
            {
                _tablero = value;
                OnPropertyChanged();
            }
        }


        private int _velocidad = 200;
        public int Velocidad
        {
            get { return _velocidad; }
            set
            {
                _velocidad = value;
                OnPropertyChanged();
            }
        }


        private int[] _listaViveCon = Enumerable.Range(0, 9).ToArray();
        public int[] ListaViveCon
        {
            get { return _listaViveCon; }
            set
            {
                _listaViveCon = value;
                OnPropertyChanged();
            }
        }


        private int[] _listaResucitaCon = Enumerable.Range(0, 9).ToArray();
        public int[] ListaResucitaCon
        {
            get { return _listaResucitaCon; }
            set
            {
                _listaResucitaCon = value;
                OnPropertyChanged();
            }
        }


        private string _resucitaCon = "3";
        public string ResucitaCon
        {
            get { return _resucitaCon; }
            set
            {
                _resucitaCon = value;
                OnPropertyChanged();
            }
        }


        private string _viveCon = "2,3";
        public string ViveCon
        {
            get { return _viveCon; }
            set
            {
                _viveCon = value;
                OnPropertyChanged();
            }
        }



        private Rect _tamanoTablero;
        public Rect TamanoTablero
        {
            get { return _tamanoTablero; }
            set
            {
                _tamanoTablero = value;
                if (Tablero == null) GenerarTablero.Execute(null);
                else Tablero.Tamano = value;
                OnPropertyChanged();
            }
        }

        private int _tamanoCelula = TamanoCelulaPredeterminado;
        public int TamanoCelula
        {
            get { return _tamanoCelula; }
            set
            {
                _tamanoCelula = value;
                OnPropertyChanged();
            }
        }

        private int _margen = MargenPredeterminado;
        public int Margen
        {
            get { return _margen; }
            set
            {
                _margen = value;
                OnPropertyChanged();
            }
        }


        private int[] _listaMargenes = Enumerable.Range(MargenMinimo, MargenMaximo - MargenMinimo + 1).ToArray();
        public int[] ListaMargenes
        {
            get { return _listaMargenes; }
            set
            {
                _listaMargenes = value;
                OnPropertyChanged();
            }
        }



        private int[] _listaTamanos = Enumerable.Range(TamanoCelulaMinimo, TamanoCelulaMaximo - TamanoCelulaMinimo + 1).ToArray();
        public int[] ListaTamanos
        {
            get { return _listaTamanos; }
            set
            {
                _listaTamanos = value;
                OnPropertyChanged();
            }
        }

        private bool _esAleatorio = true;
        public bool EsAleatorio
        {
            get { return _esAleatorio; }
            set
            {
                _esAleatorio = value;
                OnPropertyChanged();
            }
        }


        private RelayCommand<MouseEventArgs> _movimientoMouse;
        public RelayCommand<MouseEventArgs> MovimientoMouse
        {
            get
            {
                return _movimientoMouse ?? (_movimientoMouse = new RelayCommand<MouseEventArgs>(
                    p => Tablero.MovimientoMouse(p, Mouse.GetPosition((IInputElement)p.Source))));
            }
        }


        private RelayCommand _generarTablero;
        public RelayCommand GenerarTablero
        {
            get
            {
                return _generarTablero ?? (_generarTablero = new RelayCommand(
                    () => Tablero = VMTablero.CrearTablero(TamanoTablero, TamanoCelula, Margen + 1, EsAleatorio),
                    () => Tablero == null || !Tablero.EstaActivo));
            }
        }



        private RelayCommand _iniciarAutomata;
        public RelayCommand IniciarAutomata
        {
            get
            {
                return _iniciarAutomata ?? (_iniciarAutomata = new RelayCommand(
                    () => Tablero.IniciarAutomata(ViveCon, ResucitaCon, Velocidad),
                    () => Tablero != null && !Tablero.EstaActivo));
            }
        }

        private RelayCommand _detenerAutomata;
        public RelayCommand DetenerAutomata
        {
            get
            {
                return _detenerAutomata ?? (_detenerAutomata = new RelayCommand(
                    () => Tablero.DetenerAutomata(),
                    () => Tablero != null && Tablero.EstaActivo));
            }
        }


        private RelayCommand<VMCelula> _cambiarEstadoCelula;
        public RelayCommand<VMCelula> CambiarEstadoCelula
        {
            get
            {
                return _cambiarEstadoCelula ?? (_cambiarEstadoCelula = new RelayCommand<VMCelula>(
                    p => p.EstaViva = !p.EstaViva,
                    p => Tablero != null && !Tablero.EstaActivo));
            }
        }


    }
}
