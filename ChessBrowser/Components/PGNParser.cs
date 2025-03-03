
using System.IO;
using System;

namespace ChessBrowser.Components
{
	public class PGNParser
	{
		public List<ChessGame> chessGames;
		public PGNParser(string pgnFilePath)
		{
			chessGames = new List<ChessGame>();
			using (StreamReader reader = new StreamReader(pgnFilePath))
			{
                ChessGame game = new ChessGame();
                string line;
				while ((line = reader.ReadLine()) != null)
				{
                    if (line.StartsWith('['))
                        populateAtribute(line, game);
  
					if(line.StartsWith("1."))
                    {
                        string moves = "";
                        string moveLine;
                        while ((moveLine = reader.ReadLine()) != "")
                            moves += moveLine;
                        
                        game.moves = moves;

                        chessGames.Add(game);
                        game = new ChessGame();
                    }
                   
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
            else if (dataType.Contains("White "))
                game.whitePlayer = getValue(line);
            else if (dataType.Contains("Black "))
                game.blackPlayer = getValue(line);
            else if (dataType.Contains("WhiteElo"))
                game.whiteElo = Int32.Parse(getValue(line));
            else if (dataType.Contains("BlackElo"))
                game.blackElo = Int32.Parse(getValue(line));
           



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
	}
}
