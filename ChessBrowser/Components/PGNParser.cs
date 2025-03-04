
using System.IO;
using System;

namespace ChessBrowser.Components
{
	public class PGNParser
	{
		private List<ChessGame> chessGames;
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
      
		private void populateAtribute(string line, ChessGame game)
		{
			

			string dataType = line.Substring(1, 10);
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

        private char convertToResult(string line)
        {
            if (line.StartsWith("1-"))
                return 'W';
            if (line.StartsWith("0"))
                return 'B';
            else
                return 'D';


        }

		private string getValue(string line)
		{

            string dataType = line.Substring(1, 10);
          
                int index = 0;

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

        public List<ChessGame> returnChessGames()
        {
            return chessGames;
        }
	}
}
