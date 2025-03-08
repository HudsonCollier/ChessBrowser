// Implemented by Hudson Collier and Ian Kerr
// Last edited: March 7, 2025

using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Collections.Specialized;
using System.Reflection;

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
        private int Progress = 0;

        /// <summary>
        /// List of all of the Chess Games
        /// </summary>
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

                    foreach (var game in chessGames)
                    {
                        // Insert into events
                        command = conn.CreateCommand();
                        command.CommandText = "INSERT IGNORE INTO Events(Name, Site, Date) VALUES(@eName, @eSite, @eDate);";
                        
                        // Add white player
                        command.CommandText += "INSERT IGNORE INTO Players(Name, Elo) VALUES(@wName, @wElo) " +
                                              "ON DUPLICATE KEY UPDATE Elo = GREATEST(Elo, @wElo);";
                       

                        // Add black player
                        command.CommandText += "INSERT IGNORE INTO Players(Name, Elo) VALUES(@bName, @bElo) " +
                                              "ON DUPLICATE KEY UPDATE Elo = GREATEST(Elo, @bElo);";
                        
                     
                        // adding games
                        command.CommandText += "INSERT IGNORE INTO Games (Round, Result, Moves, BlackPlayer, WhitePlayer, eID) VALUES(@round, @result, @moves, (SELECT pID FROM Players WHERE Name = @bName), (SELECT pID FROM Players WHERE Name = @wName), (SELECT eID FROM Events WHERE Name = @eName AND Site = @eSite AND Date = @eDate));";
                        // Parameters for adding basic game info
                        command.Parameters.AddWithValue("@round", game.round);
                        command.Parameters.AddWithValue("@result", game.result);
                        command.Parameters.AddWithValue("@moves", game.moves);
                        // parameters for adding player info
                        command.Parameters.AddWithValue("@bName", game.blackPlayer);
                        command.Parameters.AddWithValue("@wName", game.whitePlayer);
                        command.Parameters.AddWithValue("@wElo", game.whiteElo);
                        command.Parameters.AddWithValue("@bElo", game.blackElo);
                        // parameters for finding eID
                        command.Parameters.AddWithValue("@eSite", game.site);
                        command.Parameters.AddWithValue("@eDate", game.stringDate);
                        command.Parameters.AddWithValue("@eName", game.eventName);
                        

                        command.ExecuteNonQuery();
                        index++;

                        Progress = (int)((index / size) * 100);
                        await InvokeAsync(StateHasChanged);
                    }
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
                    MySqlCommand command;
                    // Open a connection
                    conn.Open();
                    if (white != "" && black != "") // Case when user requests data for the white and the black player
                    {
                        command = conn.CreateCommand();
                        command.CommandText = "SELECT * FROM Games JOIN Events ON Games.eID = Events.eID WHERE WhitePlayer IN (SELECT pID FROM Players WHERE Name = @whiteName) " +
                                              "AND BlackPlayer IN (SELECT pID FROM Players WHERE Name = @blackName)";
                        command.Parameters.AddWithValue("@whiteName", white);
                        command.Parameters.AddWithValue("@blackName", black);
                        checkModifiers(opening, command, winner, useDate, start, end);
                        MySqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            parsedResult += buildQueryResult(reader, showMoves, white, black, ref numRows);
                        }

                    }

                    else if (white != "") // Case when user requests data for the white player only
                    {
                        command = conn.CreateCommand();
                        command.CommandText = "SELECT *, Players.Name AS pName FROM Games JOIN Events ON Events.eID = Games.eID JOIN Players ON Games.BlackPlayer = Players.pID WHERE WhitePlayer IN (SELECT pID FROM Players WHERE Name = @whiteName)";

                        command.Parameters.AddWithValue("@whiteName", white);

                        checkModifiers(opening, command, winner, useDate, start, end);
                        MySqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            parsedResult += buildQueryResult(reader, showMoves, white, reader["pName"].ToString(), ref numRows);
                        }

                    }

                    else if (black != "") // Case when user requests data for the black player only
                    {
                        command = conn.CreateCommand();
                        command.CommandText = "SELECT *, Players.Name AS pName FROM Games JOIN Events ON Events.eID = Games.eID JOIN Players ON Games.WhitePlayer = Players.pID WHERE BlackPlayer IN (SELECT pID FROM Players WHERE Name = @blackName)";

                        command.Parameters.AddWithValue("@blackName", black);

                        checkModifiers(opening, command, winner, useDate, start, end);

                        
                        MySqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            parsedResult += buildQueryResult(reader, showMoves, reader["pName"].ToString(), black, ref numRows);
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
            return numRows + " results\n\n" + parsedResult + "\n";
        }

       
        /// <summary>
        /// Builds the result of the selected query
        /// </summary>
        /// <param name="reader">Current data reader</param>
        /// <param name="showMoves">The users choice of if they want the output to contain the moves of the game</param>
        /// <param name="white">Name of white player</param>
        /// <param name="black">Name of black player</param>
        /// <param name="numRows">The number of rows returned</param>
        /// <returns>One entry of the result as a string</returns>
        private string buildQueryResult(MySqlDataReader reader, bool showMoves, string white, string black, ref int numRows)
        {
            string result = "";

            
                result += "Event: " + reader["Name"] + "\n";
                result += "Site: " + reader["Site"] + "\n";
                result += "Date: " + reader["Date"] + "\n";
                result += "White: " + white + " (" + reader["WhitePlayer"] + ") " + "\n";
                result += "Black: " + black + " (" + reader["BlackPlayer"] + ") " + "\n";
                result += "Result: " + reader["result"] + "\n\n";
                if (showMoves)
                    result += reader["moves"] + "\n";
                result += "\n";
                numRows++;
            

            return result;
        }


        /// <summary>
        /// Checks the modifiers of user query. Modifies the sql query to reflect the modifier choices
        /// </summary>
        /// <param name="opening">Opening move</param>
        /// <param name="command">Current command</param>
        /// <param name="winner">Selected winner</param>
        /// <param name="useDate">choice to filter by date</param>
        /// <param name="start">Start date of filter (if applicable)</param>
        /// <param name="end">End date of filter (if applicable)</param>
        private void checkModifiers(string opening, MySqlCommand command, string winner, bool useDate, DateTime start, DateTime end)
        {
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
        }

        /// <summary>
        /// Gets the connection string for the database
        /// </summary>
        /// <returns>The connection string</returns>
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
