using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GameOfLife.Utils;

namespace GameOfLife.ViewModels
{
    public class VMCelula : ObjetoObservable
    {

        private bool _estaViva;

        public bool EstaViva
        {
            get { return _estaViva; }
            set
            {
                if (_estaViva == value) return;
                _estaViva = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Método que define si una célula específica estará viva para la siguiente generación
        /// </summary>
        /// <param name="fila">Fila de la célula</param>
        /// <param name="col">Columna de la célula</param>
        /// <param name="estados">Estados de la generación actual</param>
        /// <returns>Un valor indicando si estará viva la siguiente generación</returns>
        internal static bool EstaraViva(int fila, int col, bool[][] estados)
        {
            var vecinos = 0;
            var filas = estados.Length;
            var columnas = estados[0].Length;
            var estaViva = estados[fila][col];

            for (var i = fila - 1; i <= fila + 1; i++)
            {
                if (i < 0 || i > filas - 1) continue;
                for (var j = col - 1; j <= col + 1; j++)
                {
                    if ((fila == i && col == j) || j < 0 || j > columnas - 1) continue;

                    vecinos += estados[i][j] ? 1 : 0;
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
