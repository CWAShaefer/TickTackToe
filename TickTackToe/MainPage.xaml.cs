using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TickTackToe
{
    // Name: Cameron Shaefer
    // Done for: PROG1442 Project 2
    // Last Modified: 2023-04-12
    // Description: A TickTackToe program that also includes an "Ai" to play against.
    public sealed partial class MainPage : Page
    {

		//Properties
        //int and string for keeping track of player turn
        private int currentPlayer = 1;
        private int player1;
        //int counters for tracking wins
        private int xWins = 0;
        private int oWins = 0;
        private int scratch = 0;
        /// <summary>
        /// AI Difficulty
        /// </summary>
        string difficulty = "None";
        /// <summary>
        /// Array of board tiles representing the ticktacktoe board.
        /// </summary>
        public BoardTile[] board;


        public MainPage()
        {
            this.InitializeComponent();

			// A Magic Square is used for determining wins https://en.wikipedia.org/wiki/Magic_square
			board = new BoardTile[9]
            {
				new BoardTile(){ index = 0, magicSquare = 2, button = btn0 },
				new BoardTile(){ index = 1, magicSquare = 7, button = btn1 },
				new BoardTile(){ index = 2, magicSquare = 6, button = btn2 },
				new BoardTile(){ index = 3, magicSquare = 9, button = btn3 },
				new BoardTile(){ index = 4, magicSquare = 5, button = btn4 },
				new BoardTile(){ index = 5, magicSquare = 1, button = btn5 },
				new BoardTile(){ index = 6, magicSquare = 4, button = btn6 },
				new BoardTile(){ index = 7, magicSquare = 3, button = btn7 },
				new BoardTile(){ index = 8, magicSquare = 8, button = btn8 }
			};

            //Combo box set up for AI Level.
            cboAILevel.Items.Add("Easy");
            cboAILevel.Items.Add("Hard");
            cboAILevel.Items.Add("None");
            cboAILevel.SelectedIndex = 0;

            //Combo box set up for Player Select.
            cboPlayerSelect.Items.Add("X");
            cboPlayerSelect.Items.Add("O");
            cboPlayerSelect.SelectedIndex = 0;

        }

        /// <summary>
        /// Disables all remaining board tiles and determines who won, increasing their score by one and displaying it as such.
        /// </summary>
        /// <param name="winner">Player number representing the player who's score will increase. 0 is a scratch game.</param>
        private void UpdateScore(int winner)
        {
            //disable all remaining buttons.
            foreach (var tile in board.Where(t => t.button.IsEnabled))
            {
                tile.button.IsEnabled = false;
            }
            
            switch (winner) //Switch case for updating score
            {
                case 0:
                    scratch++;
                    txtScratch.Text = $"Scratch Games: {scratch}";
                    break;
                case 1:
                    xWins++;
                    txtXWins.Text = $"Player X wins: {xWins}";
                    break;
                case 2:
                    oWins++;
                    txtOWins.Text = $"Player O wins: {oWins}";
                    break;
            }
            //Display winner/scratch game
            txtStatus.Text = winner == 0 ? "Scratch Game!" : $"Player {BoardTile.symbols[currentPlayer]} wins!";
        }

		/// <summary>
		/// Checks board for any wins by using a Magic Square.
		/// Each row, column, and diagonal in a magic square is equal to exactly 15
		/// Therefor, if a row is all 1 (Player X) it will equal exactly 15, and if it's all 2s (Player O) it will equal exactly 30 when added together.
		/// </summary>
		private void BoardCheck()  //Checks board for wins
        {
            int winner = 0;
            bool scratch = board.All(b => b.status != 0);

            for (int i = 0; i < 3; i++) //check each row and column
            {
                int row = board[i * 3].getMagic() + board[(i * 3) + 1].getMagic() + board[(i * 3) + 2].getMagic(); //Rows

                //if it equals 15 or 30, someone won.
                if (row == 15 || row == 30)
                {
                    winner = board[i * 3].status;
                    break;
                }
                int column = board[i].getMagic() + board[i + 3].getMagic() + board[i + 6].getMagic(); //Columns
                if (column == 15 || column == 30)
                {
                    winner = board[i].status;
                    break;
                }
            }

            //check both diagonals
            for (int i = 0; i < 4; i += 2)
            {
                int diagonal = board[0 + i].getMagic() + board[4].getMagic() + board[8 - i].getMagic();
                if (diagonal == 15 || diagonal == 30)
                {
                    winner = board[4].status;
                }
            }

            if (winner != 0 || scratch)
            {
                UpdateScore(winner);
            }
            else    //If no wins, swap players for next turn
            {
                SwapPlayer(currentPlayer);
            }
        }

        /// <summary>
        /// Generic method that is applied to all board buttons. 
        /// Determines which button was pressed and passes the index to the MakeMove method.
        /// </summary>
        private void btn_Click(object sender, RoutedEventArgs e)
        {  
            //Get button
            Button btn = sender as Button;

            //Find matching button in board array and get index.
			int btnIndex = board.FirstOrDefault(b => b.button == btn).index; 
            //Make move
            MakeMove(btnIndex);     
        }

        /// <summary>
        /// Reset board and show main menu.
        /// </summary>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            foreach(BoardTile tile in board)
            {
                tile.Reset();
				tile.button.IsEnabled = false;
				tile.button.Visibility = Visibility.Collapsed;
			}

            currentPlayer = 1;
            txtStatus.Text = $"Ready Player '{BoardTile.symbols[currentPlayer]}'...";

			btnGameStart.IsEnabled = true;
			btnReset.IsEnabled = false;

			ShowGame(false);
        }

        //Code for AI turn

        /// <summary>
        /// Button that starts the game. Reveals the board and sets the player and difficulty of the AI.
        /// If the AI is set to "None", the AI will not play and the user can play with themselves or someone else.
        /// </summary>
        private void btnGameStart_Click(object sender, RoutedEventArgs e)
        {
            
            btnGameStart.IsEnabled = false;
			btnReset.IsEnabled = true;

			ShowGame(true);

            difficulty = cboAILevel.SelectedValue.ToString();
            player1 = cboPlayerSelect.SelectedValue.ToString() == "O" ? 2 : 1;

            foreach (var tile in board)
            {
                tile.button.IsEnabled = true;
            }

            if (difficulty != "None" && currentPlayer != player1)
            {
                int aiMove = Ai.AIMove(board, difficulty, currentPlayer);
                MakeMove(aiMove);
            }

            
        }

        /// <summary>
        /// Changes the symbol in the given board square to that of the current player's, disables it, and sets it's status as claimed by that player.
        /// Then runs the board check method to see if someone won the game.
        /// </summary>
        /// <param name="index">Index of the board square that the move is being made on.</param>
        private void MakeMove(int index)
        {
            board[index].Claim(currentPlayer);
            BoardCheck();
        }

		/// <summary>
		/// Swaps between the two players to change turns.
		/// If the AI is currently enabled and it is the AI's turn, instead of swapping players it will have the AI make a move, then return control to the player.
		/// If the AI is set to "None", the AI will not play and the user can play with themselves or someone else.
		/// </summary>
		/// <param name="player">Integer representing the current player.</param>
		private void SwapPlayer(int player)
        {
            currentPlayer = (player == 1) ? 2 : 1;

            if (difficulty != "None" && currentPlayer != player1)
            {
                MakeMove(Ai.AIMove(board, difficulty, currentPlayer));
                return;
            }

            txtStatus.Text = $"Ready Player '{BoardTile.symbols[currentPlayer]}'...";

        }

        /// <summary>
        /// Toggles the visibility of either the game board or the main menu depending on if the game is being played or not.
        /// </summary>
        /// <param name="showGame">Bool for toggling if the game or the main menu is shown.</param>
        private void ShowGame(bool showGame)
        {

            Visibility gameVisible = showGame ? Visibility.Visible : Visibility.Collapsed;
            Visibility menuVisible = (!showGame) ? Visibility.Visible : Visibility.Collapsed;

            btnGameStart.Visibility = menuVisible;
            cboAILevel.Visibility = menuVisible;
            txtAILevelLabel.Visibility = menuVisible;
            cboPlayerSelect.Visibility = menuVisible;
            txtPlayerSelectLabel.Visibility = menuVisible;

            foreach (var tile in board)
            {
                tile.button.IsEnabled = true;

                tile.button.Visibility = gameVisible;
            }
        }


    }
}

