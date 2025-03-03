using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace ChessBrowser.Components.Pages
{
  public partial class ChessBrowser
  {
    /// <summary>
    /// Bound to the Unsername form input
    /// </summary>
    private string Username = "";

    /// <summary>
    /// Bound to the Password form input
    /// </summary>
    private string Password = "";

    /// <summary>
    /// Bound to the Database form input
    /// </summary>
    private string Database = "";

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
                    int size = chessGames.Count;
                    int index = 0;
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

                        //add white players
                        command = conn.CreateCommand();
                        command.CommandText = "INSERT IGNORE INTO Events(Name, Elo) VALUES(@name, @elo) WHERE Elo < @elo;";
                        command.Parameters.AddWithValue("@name", game.whitePlayer);
                        command.Parameters.AddWithValue("@elo", game.whiteElo);
                        command.ExecuteNonQuery();

                        //add black players
                        command = conn.CreateCommand();
                        command.CommandText = "INSERT IGNORE INTO Events(Name, Elo) VALUES(@name, @elo) WHERE Elo < @elo;";
                        command.Parameters.AddWithValue("@name", game.blackPlayer);
                        command.Parameters.AddWithValue("@elo", game.blackElo);
                        command.ExecuteNonQuery();



                        // adding games
                        command = conn.CreateCommand();
                        command.CommandText = "INSERT IGNORE INTO Games (Round, Result, Moves, BlackPlayer, WhitePlayer, eID) VALUES(@round, @result, @moves, @blackPlayer, @whitePlayer, @eID);";
                        command.Parameters.AddWithValue("@round", game.round);
                        command.Parameters.AddWithValue("@result", game.result);
                        command.Parameters.AddWithValue("@moves", game.moves);
                        command.Parameters.AddWithValue("@blackPlayer", game.blackPlayer);
                        command.Parameters.AddWithValue("@whitePlayer", game.whitePlayer);
                        //command.Parameters.AddWithValue("@eId", game.eID);
                        command.ExecuteNonQuery();
                        index++;

                        Progress = (index / size);
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

      // Use this to count the number of rows returned by your query
      // (see below return statement)
      int numRows = 0;

      using (MySqlConnection conn = new MySqlConnection(connection))
      {
        try
        {
          // Open a connection
          conn.Open();

          // TODO:
          //   Generate and execute an SQL command,
          //   then parse the results into an appropriate string and return it.
        }
        catch (Exception e)
        {
          System.Diagnostics.Debug.WriteLine(e.Message);
        }
      }

      return numRows + " results\n" + parsedResult;
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
