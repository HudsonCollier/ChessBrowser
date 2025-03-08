// Implementation by Hudson Collier and Ian Kerr
// Last Edited: Mach 7, 2025

using System;
using System.Collections;

namespace ChessBrowser.Components
{
    /// <summary>
    /// Holds all of the attributes for a chess game 
    /// </summary>
    public class ChessGame
    {
        public string whitePlayer { get; set; } = "";
        public string blackPlayer { get; set; } = "";
        public string eventName { get; set; } = "";
        public string site { get; set; } = "";
        public string round { get; set; } = "";
        public int whiteElo { get; set; } = 0;
        public int blackElo { get; set; } = 0;
        public char result { get; set; }
        public string stringDate { get; set; } = "";
        public string moves { get; set; } = "";

        /// <summary>
        /// Constructor for Chessgame object. All values set to 0/empty strings when exiting.
        /// </summary>
        public ChessGame() {}
    }
}
