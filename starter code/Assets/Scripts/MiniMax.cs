using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimax : MonoBehaviour
{
    BoardManager board;
    GameManager gameManager;
    MoveData bestMove;
    int myScore = 0;
    int opponentScore = 0;
    int maxDepth;

    List<TileData> myPieces = new List<TileData>();
    List<TileData> opponentPieces = new List<TileData>();
    Stack<MoveData> moveStack = new Stack<MoveData>();
    MoveHeuristic weight = new MoveHeuristic();

    public static Minimax instance;
    public static Minimax Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null)        
            instance = this;        
        else if (instance != this)        
            Destroy(this);    
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

        MoveData CreateMove(TileData from, TileData to)
    {
        MoveData tempMove = new MoveData
        {
            firstPosition = from,
            pieceMoved = from.CurrentPiece,
            secondPosition = to
        };

        if (to.CurrentPiece != null)        
            tempMove.pieceKilled = to.CurrentPiece;        

        return tempMove;
    }


        List<MoveData> GetMoves(PlayerTeam team)
    {
        List<MoveData> turnMove = new List<MoveData>();
        List<TileData> pieces = (team == gameManager.playerTurn) ? myPieces : opponentPieces;      

        foreach (TileData tile in pieces)
        {
            MoveFunction movement = new MoveFunction(board);
            List<MoveData> pieceMoves = movement.GetMoves(tile.CurrentPiece, tile.Position);

            foreach (MoveData move in pieceMoves)
            {
                MoveData newMove = CreateMove(move.firstPosition, move.secondPosition);
                turnMove.Add(newMove);
            }
        }
        return turnMove;
    }


        void DoFakeMove(TileData currentTile, TileData targetTile)
    {
        targetTile.SwapFakePieces(currentTile.CurrentPiece);
        currentTile.CurrentPiece = null;
    }


        void UndoFakeMove()
    {
        MoveData tempMove = moveStack.Pop();
        TileData movedTo = tempMove.secondPosition;
        TileData movedFrom = tempMove.firstPosition;
        ChessPiece pieceKilled = tempMove.pieceKilled;
        ChessPiece pieceMoved = tempMove.pieceMoved;

        movedFrom.CurrentPiece = movedTo.CurrentPiece;
        movedTo.CurrentPiece = (pieceKilled != null) ? pieceKilled : null;      
    }


        int Evaluate()
    {
        int pieceDifference = myScore - opponentScore;            
        return pieceDifference;
    }


        void GetBoardState()
    {
        myPieces.Clear();
        opponentPieces.Clear();
        myScore = 0;
        opponentScore = 0;

        for (int y = 0; y < 8; y++)        
            for (int x = 0; x < 8; x++)
            {
                TileData tile = board.GetTileFromBoard(new Vector2(x, y));
                if(tile.CurrentPiece != null && tile.CurrentPiece.Type != ChessPiece.PieceType.NONE)
                {
                    if (tile.CurrentPiece.Team == gameManager.playerTurn)
                    {
                        myScore += weight.GetPieceWeight(tile.CurrentPiece.Type);
                        myPieces.Add(tile);
                    }
                    else
                    {
                        opponentScore += weight.GetPieceWeight(tile.CurrentPiece.Type);
                        opponentPieces.Add(tile);
                    }
                }
            }     
    }



        public MoveData GetMove()
        {
            board = BoardManager.Instance;
            gameManager = GameManager.Instance;
            bestMove = CreateMove(board.GetTileFromBoard(new Vector2(0, 0)), board.GetTileFromBoard(new Vector2(0, 0)));

            maxDepth = 3; //do something interesting here?
            CalculateMinMax(maxDepth, true);

            return bestMove;
        } 


        //original
        //int CalculateMinMax(int depth, bool max)
        int CalculateMinMax(int depth, int alpha, int beta, bool max)
        {
            GetBoardState();

            if (depth == 0)        
                return Evaluate();

            if (max)
            {
                //int maxScore = int.MinValue;
                List<MoveData> allMoves = GetMoves(gameManager.playerTurn);
                allMoves = Shuffle(allMoves);
                foreach (MoveData move in allMoves)
            
                {
                    moveStack.Push(move);

                    DoFakeMove(move.firstPosition, move.secondPosition);
                    int score = CalculateMinMax(depth - 1, false);
                    UndoFakeMove();            

                    if (score > alpha)
                    {
                        alpha = score;
                        move.score = score;

                        if (score > bestMove.score && depth == maxDepth)                                                                
                            bestMove = move;                                                            
                    }
                    
                }
                //return maxScore;
                return alpha;
            }
            else
            {
                PlayerTeam opponent = gameManager.playerTurn == PlayerTeam.WHITE ? PlayerTeam.BLACK : PlayerTeam.WHITE;
                //int minScore = int.MaxValue;
                List<MoveData> allMoves = GetMoves(opponent);
                allMoves = Shuffle(allMoves);
                foreach (MoveData move in allMoves)
                {
                    moveStack.Push(move);

                    DoFakeMove(move.firstPosition, move.secondPosition);
                    int score = CalculateMinMax(depth - 1, true);
                    UndoFakeMove();

                    //if(score < minScore)                
                    //    minScore = score;         
                    if (score < beta)                
                    beta = score;   

                }
                //return minScore;
                return beta;
            }
        }


        public List<T> Shuffle<T>(List<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = Random.Range(0,n);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
        return list;
    }

}
 
