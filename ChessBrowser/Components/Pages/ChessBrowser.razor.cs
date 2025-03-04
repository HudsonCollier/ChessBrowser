using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Collections.Specialized;

namespace ChessBrowser.Components.Pages
{
  public partial class ChessBrowser
  {
    /// <summary>
    /// Bound to the Unsername form input
    /// </summary>
    private string Username = "u1424286";

    /// <summary>
    /// Bound to the Password form input
    /// </summary>
    private string Password = "March2500";

    /// <summary>
    /// Bound to the Database form input
    /// </summary>
    private string Database = "Team155Chess";

    /// <summary>
    /// Represents the progress percentage of the current
    /// upload operation. Update this value to update 
    /// the progress bar.
    /// </summary>
    private int    Progress = 0;


        private List<ChessGame> chessGames;
    /// <summary>
    /// This method runs when a PGN file is selected for upload.
    /// Given a list of lines from the selected file, parses the 
    /// PGN data, and uploads each chess game to the user's database.
    /// </summary>
    /// <param name="PGNFileLines">The lines from the selected file</param>
    private async Task InsertGameData(string[] PGNFileLines)
    {

       

      // This will build a connection string to your user's database on atr,
      // assuimg you've filled in the credentials in the GUI
      string connection = GetConnectionString();

            // TODO:
            //   Parse the provided PGN data
            //   We recommend creating separate libraries to represent chess data and load the file
            PGNParser pgnData = new PGNParser(PGNFileLines);
            chessGames = pgnData.returnChessGames();

      using (MySqlConnection conn = new MySqlConnection(connection))
      {
        try
        {
          // Open a connection
          conn.Open();
                    MySqlCommand command;
                    float size = chessGames.Count;
                    float index = 0;

                    // TODO:
                    //   Iterate through your data and generate appropriate insert commands
                    foreach (var game in chessGames) {

                        //add events
                        command = conn.CreateCommand();
                        command.CommandText = "INSERT IGNORE INTO Events(Name, Site, Date) VALUES(@name, @site, @date);";
                        command.Parameters.AddWithValue("@site", game.site);
                        command.Parameters.AddWithValue("@name", game.eventName);
                        command.Parameters.AddWithValue("@date", game.stringDate);
                        command.ExecuteNonQuery();

                        
                       

                        //add white player
                        command = conn.CreateCommand();
                        command.CommandText = "INSERT INTO Players(Name, Elo) VALUES(@name, @elo) " +
                                              "ON DUPLICATE KEY UPDATE Elo = GREATEST(Elo, @elo);";
                        command.Parameters.AddWithValue("@name", game.whitePlayer);
                        command.Parameters.AddWithValue("@elo", game.whiteElo);
                        command.ExecuteNonQuery();

                        // Add black player
                        command = conn.CreateCommand();
                        command.CommandText = "INSERT INTO Players(Name, Elo) VALUES(@name, @elo) " +
                                              "ON DUPLICATE KEY UPDATE Elo = GREATEST(Elo, @elo);";
                        command.Parameters.AddWithValue("@name", game.blackPlayer);
                        command.Parameters.AddWithValue("@elo", game.blackElo);
                        command.ExecuteNonQuery();

                       

                        command = conn.CreateCommand();
                        command.CommandText = "SELECT eID FROM Events WHERE Name = @name AND Site = @site AND Date = @date;";
                        command.Parameters.AddWithValue("@name", game.eventName);
                        command.Parameters.AddWithValue("@site", game.site);
                        command.Parameters.AddWithValue("@date", game.stringDate);

                        object result = command.ExecuteScalar();
                        int eID = (result != null) ? Convert.ToInt32(result) : 0;

                        command = conn.CreateCommand();
                        command.CommandText = "SELECT pID FROM Players WHERE Name = @name;";
                        command.Parameters.AddWithValue("@name", game.blackPlayer);

                        result = command.ExecuteScalar();
                        int bID = (result != null) ? Convert.ToInt32(result) : 0;


                        command = conn.CreateCommand();
                        command.CommandText = "SELECT pID FROM Players WHERE Name = @name;";
                        command.Parameters.AddWithValue("@name", game.whitePlayer);

                        result = command.ExecuteScalar();
                        int wID = (result != null) ? Convert.ToInt32(result) : 0;

                        Console.WriteLine(wID);

                        // adding games
                        command = conn.CreateCommand();
                        command.CommandText = "INSERT INTO Games (Round, Result, Moves, BlackPlayer, WhitePlayer, eID) VALUES(@round, @result, @moves, @blackPlayer, @whitePlayer, @eID);";
                        command.Parameters.AddWithValue("@round", game.round);
                        command.Parameters.AddWithValue("@result", game.result);
                        command.Parameters.AddWithValue("@moves", game.moves);
                        command.Parameters.AddWithValue("@blackPlayer", bID);
                        command.Parameters.AddWithValue("@whitePlayer", wID);
                        command.Parameters.AddWithValue("@eId", eID);
                        command.ExecuteNonQuery();
                        index++;

                      

                        Progress = (int)((index / size) * 100);
                    
                    }
           
                  

          // This tells the GUI to redraw after you update Progress (this should go inside your loop)
          await InvokeAsync(StateHasChanged);
          

        }
        catch (Exception e)
        {
          System.Diagnostics.Debug.WriteLine(e.Message);
        }
      }

    }


    /// <summary>
    /// Queries the database for games that match all the given filters.
    /// The filters are taken from the various controls in the GUI.
    /// </summary>
    /// <param name="white">The white player, or "" if none</param>
    /// <param name="black">The black player, or "" if none</param>
    /// <param name="opening">The first move, e.g. "1.e4", or "" if none</param>
    /// <param name="winner">The winner as "W", "B", "D", or "" if none</param>
    /// <param name="useDate">true if the filter includes a date range, false otherwise</param>
    /// <param name="start">The start of the date range</param>
    /// <param name="end">The end of the date range</param>
    /// <param name="showMoves">true if the returned data should include the PGN moves</param>
    /// <returns>A string separated by newlines containing the filtered games</returns>
    private string PerformQuery(string white, string black, string opening,
      string winner, bool useDate, DateTime start, DateTime end, bool showMoves)
    {
      // This will build a connection string to your user's database on atr,
      // assuimg you've typed a user and password in the GUI
      string connection = GetConnectionString();

      // Build up this string containing the results from your query
      string parsedResult = "";

      string query = "SELECT  ";

      // Use this to count the number of rows returned by your query
      // (see below return statement)
      int numRows = 0;

      using (MySqlConnection conn = new MySqlConnection(connection))
      {
        try
        {
                    MySqlCommand command;
          // Open a connection
          conn.Open();
          if(white != "" && black != "")
                    {
                        command = conn.CreateCommand();
                        command.CommandText = "SELECT * FROM Games JOIN Events ON Games.eID = Events.eID WHERE WhitePlayer IN (SELECT pID FROM Players WHERE Name = @whiteName) " +
                                              "OR BlackPlayer IN (SELECT pID FROM Players WHERE Name = @blackName)";
                        command.Parameters.AddWithValue("@whiteName", white);
                        command.Parameters.AddWithValue("@blackName", black);
                        if (opening != "")
                        {
                            command.CommandText += " AND Moves LIKE @openingMove";
                            command.Parameters.AddWithValue("@openingMove", opening + "%");
                        }
                        if (winner != "" && winner != "Any")
                        {
                            command.CommandText += " AND Result = @result";
                            command.Parameters.AddWithValue("@result", winner);
                        }
                        if (useDate)
                        {
                            command.CommandText += " AND Date > @startDate AND Date < @endDate";
                            command.Parameters.AddWithValue("@startDate", start);
                            command.Parameters.AddWithValue("@endDate", end);
                        }
                        command.CommandText += ";";
                        MySqlDataReader reader = command.ExecuteReader();
                      

                        while(reader.Read())
                        {
                            parsedResult += "Event: " + reader["Name"] + "\n";
                            parsedResult += "Site: " + reader["Site"] + "\n";
                            parsedResult += "Date: " + reader["Date"] + "\n";
                            parsedResult += "White: " + white + " (" + reader["WhitePlayer"] + ") " + "\n";
                            parsedResult += "Black: " + black + " (" + reader["BlackPlayer"] + ") " + "\n";
                            parsedResult += "Result: " + reader["result"] + "\n";
                            if (showMoves)
                                parsedResult += reader["moves"] + "\n";
                            parsedResult += "\n";
                            numRows++;

                        }
       
                    }

                   else if (white != "" )
                    {
                        command = conn.CreateCommand();
                        command.CommandText = "SELECT *, Players.Name AS pName FROM Games JOIN Events ON Events.eID = Games.eID JOIN Players ON Games.BlackPlayer = Players.pID WHERE WhitePlayer IN (SELECT pID FROM Players WHERE Name = @whiteName)";
                                             
                        command.Parameters.AddWithValue("@whiteName", white);
                       
                        if (opening != "")
                        {
                            command.CommandText += " AND Moves LIKE @openingMove";
                            command.Parameters.AddWithValue("@openingMove", opening + "%");
                        }
                        if (winner != "Any" && winner != "")
                        {
                            command.CommandText += " AND Result = @result";
                            command.Parameters.AddWithValue("@result", winner);
                        }
                        
                        if(useDate)
                        {
                            command.CommandText += " AND Date > @startDate AND Date < @endDate";
                            command.Parameters.AddWithValue("@startDate", start);
                            command.Parameters.AddWithValue("@endDate", end);
                        }
                        command.CommandText += ";";
                        MySqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            parsedResult += "Event: " + reader["Name"] + "\n";
                            parsedResult += "Site: " + reader["Site"] + "\n";
                            parsedResult += "Date: " + reader["Date"] + "\n";
                            parsedResult += "White: " + white + " (" + reader["WhitePlayer"] + ") " + "\n";
                            parsedResult += "Black: " + reader["pName"] + " (" + reader["BlackPlayer"] + ") " + "\n";
                            parsedResult += "Result: " + reader["result"] + "\n\n";
                            if (showMoves)
                                parsedResult += reader["moves"] + "\n";
                            parsedResult += "\n";
                            numRows++;

                        }

                    }

                    else if (black != "")
                    {
                        command = conn.CreateCommand();
                        command.CommandText = "SELECT *, Players.Name AS pName FROM Games JOIN Events ON Events.eID = Games.eID JOIN Players ON Games.WhitePlayer = Players.pID WHERE BlackPlayer IN (SELECT pID FROM Players WHERE Name = @blackName)";

                        command.Parameters.AddWithValue("@blackName", black);

                        if (opening != "")
                        {
                            command.CommandText += " AND Moves LIKE @openingMove";
                            command.Parameters.AddWithValue("@openingMove", opening + "%");
                        }
                        if (winner != "Any" && winner != "")
                        {
                            command.CommandText += " AND Result = @result";
                            command.Parameters.AddWithValue("@result", winner);
                        }
                        if (useDate)
                        {
                            command.CommandText += " AND Date > @startDate AND Date < @endDate";
                            command.Parameters.AddWithValue("@startDate", start);
                            command.Parameters.AddWithValue("@endDate", end);
                        }
                        command.CommandText += ";";
                        MySqlDataReader reader = command.ExecuteReader();


                        while (reader.Read())
                        {
                            parsedResult += "Event: " + reader["Name"] + "\n";
                            parsedResult += "Site: " + reader["Site"] + "\n";
                            parsedResult += "Date: " + reader["Date"] + "\n";
                            parsedResult += "White: " + reader["pName"] + " (" + reader["WhitePlayer"] + ") " + "\n";
                            parsedResult += "Black: " + black + " (" + reader["BlackPlayer"] + ") " + "\n";
                            parsedResult += "Result: " + reader["result"] + "\n\n";
                            if (showMoves)
                                parsedResult += reader["moves"] + "\n";
                            parsedResult += "\n";
                            numRows++;

                        }

                    }







                    // TODO:
                    //   Generate and execute an SQL command,
                    //   then parse the results into an appropriate string and return it.


                }
        catch (Exception e)
        {
          System.Diagnostics.Debug.WriteLine(e.Message);
        }
      }

      return numRows + " results\n\n" + parsedResult + "\n";
    }


     
        private string getPlayerName(string pID, MySqlConnection conn) 
        {
            
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "SELECT Name FROM Players WHERE pID = " + pID + ";";
            MySqlDataReader reader = command.ExecuteReader();
            return (string)reader["Name"];

        }

        private string GetConnectionString()
    {
      return "server=atr.eng.utah.edu;database=" + Database + ";uid=" + Username + ";password=" + Password;
    }


    /// <summary>
    /// This method will run when the file chooser is used.
    /// It loads the files contents as an array of strings,
    /// then invokes the InsertGameData method.
    /// </summary>
    /// <param name="args">The event arguments, which contains the selected file name</param>
    private async void HandleFileChooser(EventArgs args)
    {
      try
      {
        string fileContent = string.Empty;

        InputFileChangeEventArgs eventArgs = args as InputFileChangeEventArgs ?? throw new Exception("unable to get file name");
        if (eventArgs.FileCount == 1)
        {
          var file = eventArgs.File;
          if (file is null)
          {
            return;
          }

          // load the chosen file and split it into an array of strings, one per line
          using var stream = file.OpenReadStream(1000000); // max 1MB
          using var reader = new StreamReader(stream);                   
          fileContent = await reader.ReadToEndAsync();
          string[] fileLines = fileContent.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

          // insert the games, and don't wait for it to finish
          // _ = throws away the task result, since we aren't waiting for it
          _ = InsertGameData(fileLines);
        }
      }
      catch (Exception e)
      {
        Debug.WriteLine("an error occurred while loading the file..." + e);
      }
    }

  }

}
