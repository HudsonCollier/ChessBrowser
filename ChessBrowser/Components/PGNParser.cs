// Implementation by Hudson Collier and Ian Kerr
// Last Edited: Mach 7, 2025

using System.IO;
using System;

namespace ChessBrowser.Components
{
    /// <summary>
    /// Parses the uploaded PGN file  
    /// </summary>
	public class PGNParser
    {
        /// <summary>
        /// List of all of the Chess games in the file
        /// </summary>
		private List<ChessGame> chessGames;

        /// <summary>
        /// Constructs the PGNParser
        /// </summary>
        /// <param name="pgnFileArray">The lines from the selected file</param>
		public PGNParser(string[] pgnFileArray)
        {
            chessGames = new List<ChessGame>();
            ChessGame game = new ChessGame();
            for (int i = 0; i < pgnFileArray.Length; i++)
            {
                string line = pgnFileArray[i];

                if (line.StartsWith('['))
                    populateAtribute(line, game);

                else if (line.StartsWith("1."))
                {
                    string moves = "";
                    int j;
                    for (j = i; j < pgnFileArray.Length; j++)
                    {
                        if (pgnFileArray[j].StartsWith("["))
                            break;
                        moves += pgnFileArray[j];
                    }

                    i = j - 1;
                    game.moves = moves;

                    chessGames.Add(game);
                    game = new ChessGame();
                }
            }
        }

        /// <summary>
        /// Populates the attributes for the given game
        /// </summary>
        /// <param name="line">The line from the PGN file</param>
        /// <param name="game"> The game object</param>
        private void populateAtribute(string line, ChessGame game)
        {
            string dataType = line.Substring(1, 10); // all info on type will be contained in the first few characters of the striing
            if (dataType.Contains("Event "))
                game.eventName = getValue(line);
            else if (dataType.Contains("Site"))
                game.site = getValue(line);
            else if (dataType.Contains("EventDate"))
                game.stringDate = getValue(line);
            else if (dataType.Contains("Round"))
                game.round = getValue(line);
            else if (dataType.Contains("Result"))
                game.result = convertToResult(getValue(line));
            else if (dataType.Contains("White "))
                game.whitePlayer = getValue(line);
            else if (dataType.Contains("Black "))
                game.blackPlayer = getValue(line);
            else if (dataType.Contains("WhiteElo"))
                game.whiteElo = Int32.Parse(getValue(line));
            else if (dataType.Contains("BlackElo"))
                game.blackElo = Int32.Parse(getValue(line));
        }

        /// <summary>
        /// Converts the result of the game into either 'W', 'B', or 'D'
        /// </summary>
        /// <param name="line">The line of the PGN file</param>
        /// <returns>'W' if White won, 'B' if Black won, and 'D' if there was a draw</returns>
        private char convertToResult(string line)
        {
            if (line.StartsWith("1-"))
                return 'W';
            if (line.StartsWith("0"))
                return 'B';
            else
                return 'D';
        }

        /// <summary>
        /// Gets the actual value of the attribute 
        /// </summary>
        /// <param name="line">The line of the PGN file</param>
        /// <returns>The vlaue of the attribute as a string</returns>
        private string getValue(string line)
        {
            string dataType = line.Substring(1, 10);
            int index = 0;
            // finds the index of the first '"' character
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"') 
                {
                    index = i;
                    break;
                }
            }

            line = line.Substring(index + 1, line.Length - index - 3);

            return line;
        }

        /// <summary>
        /// Gets all of the chess games
        /// </summary>
        /// <returns>All of the chess games in a list</returns>
        public List<ChessGame> returnChessGames()
        {
            return chessGames;
        }
    }
}
