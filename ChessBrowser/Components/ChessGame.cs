using System;
using System.Collections;

namespace ChessBrowser.Components
{
    public class ChessGame
    {
        public string whitePlayer { get; set; }
        public string blackPlayer { get; set; }
        public string eventName { get; set; }
        public string site { get; set; }
        public string round { get; set; }
        public int whiteElo { get; set; }
        public int blackElo { get; set; }
        public string result { get; set; }
        public string stringDate { get; set; }
        public string moves { get; set; }

        public ChessGame(string whitePlayer, string blackPlayer, string eventName, string site, string round, int whiteElo, int blackElo, string result, string stringDate, string moves)
        {
            this.whitePlayer = whitePlayer;
            this.blackPlayer = blackPlayer;
            this.eventName = eventName;
            this.site = site;
            this.round = round;
            this.whiteElo = whiteElo;
            this.blackElo = blackElo;
            this.result = result;
            this.stringDate = stringDate;
            this.moves = moves;
        }
        public ChessGame()
        {

        }

        public string printEvent()
        {
            return this.eventName;
        }
    }
}
