using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TickTackToe
{
    /// <summary>
    /// Represents a tile on the 3x3 ticktacktoe board. Has an index, Magic Square number, status of who the tile is currently claimed by, and a button asscociated with it.
    /// Also contains a method for multiplying the status by the magic square number to get the Magic Number for that tile.
    /// </summary>
    public class BoardTile
    {
        /// <summary>
        /// Array of symbols representing tile state.
        /// </summary>
		public static readonly string[] symbols = { "?", "X", "O" };

		//Properties
		/// <summary>
		/// The index of the tile on the board.
		/// </summary>
		public int index;
        /// <summary>
        /// Magic Square number associated with this tile.
        /// </summary>
        public int magicSquare;
        /// <summary>
        /// Int that indicates who currently has claim of the tile.
        /// </summary>
        public int status = 0;         //Status
        /// <summary>
        /// The button that is associated with the tile on the board.
        /// </summary>
        public Button button;          //Button


        //Methods
        /// <summary>
        /// Method that multiplies the status (current owner's player number), by the Magic Square number of this tile.
        /// </summary>
        /// <returns>An int from the magic square multiplied by the number of the tile owner.</returns>
        public int getMagic()
        {
            return status * magicSquare;
        }

        /// <summary>
        /// Resets a tile to a status and symbol of 0.
        /// </summary>
        public void Reset()
        {
			status = 0;
			button.Content = symbols[0];
		}

        /// <summary>
        /// Sets the status of the tile as being claimed by a player, and updates the symbol to the related symbol.
        /// </summary>
        /// <param name="playerNumber">Represents the number related to the player claiming the tile.</param>
        public void Claim(int playerNumber)
        {
			button.Content = symbols[playerNumber];
			button.IsEnabled = false;
			status = playerNumber;
		}

    }

}
