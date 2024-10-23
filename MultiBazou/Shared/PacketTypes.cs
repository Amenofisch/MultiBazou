namespace MultiBazou.Shared
{
    public enum PacketTypes
    {
        #region Lobby and Connection
            empty = 0,
            welcome = 1,
            keepAlive = 2,
            keepAliveConfirmed = 3,
            disconnect,
            readyState,
            playersInfo,
            playerInfo,
            startGame,
            spawnPlayer,
        #endregion

        #region In-Game
            playerInitialPos,
            playerPosition,
            playerRotation,
            playerSceneChange,
        #endregion
    }
}