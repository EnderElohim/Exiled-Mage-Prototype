using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrGameData : MonoBehaviour
{
    private static soGameData _gameData;
    public static soGameData values
    {
        get
        {
            if (_gameData == null)
            {
                _gameData = Resources.Load("Game Data") as soGameData; // Resources.Load("Game Data") as soGameData;
            }

            return _gameData;
        }
        set
        {
            _gameData = value;
        }
    }
}
