using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTackToe;

namespace TickTackToe
{
    public static class Ai
    {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="board">Array of Board Squares that represent the ticktacktoe board</param>
		/// <param name="difficulty">Difficulty level of the AI. Ranges from "Easy" to "Hard"</param>
		/// <param name="player">The int representing the player that the AI will make a move as.</param>
		/// <returns>An index relating to a legal move that the AI can make on the board.</returns>
		public static int AIMove(BoardTile[] board, string difficulty, int player)
        {
            int move;

            switch (difficulty)
            {
                case "Hard":
                    move = smartMove(board, player);
                    break;
                case "Easy":
                default:
                    move = randomMove(board);
                    break;
            }

            return move;

        }
        
        /// <summary>
        /// Method for picking a random move to make on the board.
        /// </summary>
        /// <param name="board">Array of Board Squares that represent the ticktacktoe board.</param>
        /// <returns>Index of a random un-claimed tile on the board that is a valid move to make.</returns>
        private static int randomMove(BoardTile[] board)
        {
            Random random = new Random();
            int randMove;

            board = board.Where(b => b.status == 0).ToArray();
            //choose place on board

            randMove = board[random.Next(0, board.Length - 1)].index;

            return randMove;
        }

		/// <summary>
		/// A more complex method for choosing a legal move to make.
        /// Contains rules for first four turns of the game to ensure optimal start conditions.
        /// After the first four turns, it relies on logic that checks the board for any row or column with two of the same symbol in a row.
        /// If it finds two symbols in a row, this means that one of the players is about to win, so it will place it's move in the third slot of that set.
        /// It will prioritize taking a winning move over a blocking move, and will fall back on making a random move if there is neither.
		/// </summary>
		/// <param name="board">Array of Board Squares that represent the ticktacktoe board.</param>
		/// <param name="playerNumber">An integer representing the player that the AI is making a move as.</param>
		/// <returns></returns>
		private static int smartMove(BoardTile[] board, int playerNumber)
        {
            int smartMove = -1;
            int turnCount = board.Where(b => b.status != 0).Count();
            int[] corners = { 0, 2, 6, 8 };
            int[] edges = { 1, 3, 4, 7 };
            int[] antiCorners = { 8, 6, 2, 0 };
            Random random = new Random();

            if(turnCount < 4)
            {
                if (turnCount == 0) //If it's turn 0, take a corner.
                {
                    //Pick a random corner.
                    return corners[random.Next(0, 3)];
                }
                else if (turnCount == 1)    //If it's turn 1, either take the middle, or a corner if they took the middle.
                {
                    if (board[4].status == 0)
                    {
                        return 4;
                    }
                    else
                    {
                        return randomMove(board);
                    }
                }
                else if (turnCount == 2) // If it's turn two and they didn't take middle or the opposite corner, set up winning move.
                {
                    int previousMove = board.First(b => b.status == playerNumber).index;

                    int oppositeCorner = antiCorners[Array.IndexOf(corners, previousMove)];

                    if (board[oppositeCorner].status == 0)
                    {
                        //Take opposite corner if middle is clear
                        return oppositeCorner;
                    }
                    else
                    {
                        //otherwise set up Scratch.

                        int scratchCorner = corners.Where(c => c != previousMove && c != oppositeCorner).ToArray()[random.Next(0, 1)];

                        return scratchCorner;
                    }
                }
                else if (turnCount == 3) // If it's turn three, force a draw.
                {
                    int previousMove = board.First(b => b.status == playerNumber).index;

                    int[] scratchEdge = edges.Where(c => board[c].status == 0).ToArray();

                    if (corners.Where(c => board[c].status != 0 && board[c].status != playerNumber).Count() > 1)
                    {
                        return scratchEdge[random.Next(0, scratchEdge.Length)];
                    }
                }
            }

            int blockMove;
            int nextBestMove = -1;

            for (int i = 0; i < 3; i++) //check each row and column
            {
                List<BoardTile> tiles = new List<BoardTile>()   //Rows
                {
                    board[i * 3],board[(i * 3) + 1],board[(i * 3) + 2]
                };

                //Always takes winning moves, or blocks opponent's winning moves.
                if (CheckForSmartMove(tiles, playerNumber, out smartMove, out blockMove))
                {
                    if (smartMove != -1)
                    {
                        return smartMove;
                    }
                    else
                    {
                        nextBestMove = blockMove;
                    }
                }

                tiles = new List<BoardTile>() //Columns
                {
                    board[i],board[i + 3],board[i + 6]
                };

                if (CheckForSmartMove(tiles, playerNumber, out smartMove, out blockMove))
                {
                    if (smartMove != -1)
                    {
                        return smartMove;
                    }
                    else
                    {
                        nextBestMove = blockMove;
                    }
                }


            }
            
            //check both diagonals
            for (int i = 0; i < 4; i += 2)
            {

                List<BoardTile> tiles = new List<BoardTile>()
                {
                    board[0 + i],board[4],board[8-i]
                };

                if (CheckForSmartMove(tiles, playerNumber, out smartMove, out blockMove))
                {
                    if (smartMove != -1)
                    {
                        return smartMove;
                    }
                    else
                    {
                        nextBestMove = blockMove;
                    }
                }
            }

            //If the AI found two in a row that wasn't a win, it will take that move.
            if(nextBestMove != -1)
            {
                return nextBestMove;
            }

            //If no "smart" move is possible, AI will fall back on making a random move.
            return randomMove(board);
        }


		/// <summary>
		/// Checks a set of tiles to see if two out of three are claimed by the same player.
		/// Returns false if there is no "desireable" move to be made. (One that either wins or blocks a win.)
		/// If a winning/blocking move is found, it is written to one of the two Out params.
		/// </summary>
		/// <param name="tiles">A set of three tiles for the method to compare to eachother.</param>
		/// <param name="playerNumber">The integer that tells the AI which player it is playing as.</param>
		/// <param name="winMove">Out param for if the method finds a move that wins.</param>
		/// <param name="blockMove">Out param for if the method finds a move that blocks an opponent's win.</param>
		/// <returns></returns>
		private static bool CheckForSmartMove(List<BoardTile> tiles,int playerNumber, out int winMove, out int blockMove)
        {
            winMove = -1;
            blockMove = -1;

            //Break if all tiles are taken
            if (tiles.All(b => b.status != 0))
            {
                return false;
            }

            var twoTileCheck = tiles.Where(t => t.status != 0).ToArray();
            //Take tile if you can win, or if it blocks the opponent's win.
            if (twoTileCheck.Length == 2 && twoTileCheck[0].status == twoTileCheck[1].status)
            {
                int move = tiles.Where(t => t.status == 0).First().index;

                if (twoTileCheck[0].status == playerNumber)
                {
                    winMove = move;
                }
                else
                {
                    blockMove = move;
                }
                
                return true;
            }


            return false;
        }

    }
}
